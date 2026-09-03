using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Vanara.PInvoke;

using static Vanara.PInvoke.User32;

namespace Robot.Forms
{

    /// <summary>
    /// 窗口 DPI 适配器: 通过 NativeWindow 接管目标窗口的消息循环, 监听 WM_DPICHANGED 并按新 DPI 重排窗口与控件。
    /// </summary>
    public class WindowDpiAdapter : NativeWindow
    {
        /// <summary>
        /// 被适配的目标窗口。
        /// </summary>
        Form _targetForm;

        /// <summary>
        /// 初始化窗口 DPI 适配器, 接管目标窗口的句柄并订阅其创建/销毁事件。
        /// </summary>
        /// <param name="form">需要适配 DPI 的目标窗口。</param>
        public WindowDpiAdapter(Form form)
        {
            _targetForm = form;


            _targetForm.HandleCreated += (_, _) =>
            {
                AssignHandle(form.Handle);

                SystemDpiManager.InitializeDpiManager();
                _deviceDpi = SystemDpiManager.DeviceDpi;


                ScaleFactor = SystemDpiManager.GetScaleFactorForWindow(Handle);

                if (SystemDpiManager.IsScalingRequirementMet && ScaleFactor != 1.0f)
                {
                    form.Scale(new SizeF(ScaleFactor, ScaleFactor));
                }


            };


            _targetForm.HandleDestroyed += (_, _) => ReleaseHandle();
        }


        /// <summary>
        /// 重写消息处理: 在 .NET 8 之前拦截 WM_DPICHANGED 消息, 交由 <see cref="WmDpiChanged"/> 处理。
        /// </summary>
        /// <param name="m">待处理的窗口消息。</param>
        protected override void WndProc(ref Message m)
        {
    #if !NET8_0_OR_GREATER
            if (m.Msg == (int)WindowMessage.WM_DPICHANGED)
            {
                if (WmDpiChanged(ref m))
                {
                    return;
                }
            }
    #endif


            base.WndProc(ref m);
        }

        /// <summary>
        /// 当前设备 DPI。
        /// </summary>
        private int _deviceDpi;


        /// <summary>
        /// 当前窗口的缩放因子。
        /// </summary>
        internal protected float ScaleFactor { get; private set; } = 1.0f;

        /// <summary>
        /// 当前窗口的有效 DPI: 支持 PerMonitorV2 时取窗口自身 DPI, 否则取系统 DPI。
        /// </summary>
        internal protected int CurrentDpi => SystemDpiManager.IsPerMonitorV2Awareness ? _deviceDpi : SystemDpiManager.DeviceDpi;



        /// <summary>
        /// 是否正在锁定窗口边界变更, 用于避免 DPI 重排期间触发额外的布局回调。
        /// </summary>
        internal bool IsBoundsChangingLocked { get; private set; } = false;

        /// <summary>
        /// 窗口 DPI 变化事件。
        /// </summary>
        public event EventHandler<WindowDpiChangedEventArgs>? WindowDpiChanged;


        /// <summary>
        /// 通过反射重置窗口的自动缩放基准尺寸, 使其按指定 DPI 重新计算。
        /// </summary>
        /// <param name="force">为 true 时强制重置, 否则不做任何处理。</param>
        protected void CheckResetDPIAutoScale(bool force = false)
        {
            if (force)
            {
                var fi = typeof(ContainerControl).GetField("currentAutoScaleDimensions", BindingFlags.NonPublic | BindingFlags.Instance);
                var dpi = _targetForm.IsHandleCreated ? SystemDpiManager.GetDpiForWindow(Handle) : 96;
                if (fi != null)
                    fi.SetValue(this, new SizeF(dpi, dpi));
            }
        }

        /// <summary>
        /// 是否正在执行 DPI 变化处理, 用于防止重入。
        /// </summary>
        bool _isPerformDpiChanged = false;

        /// <summary>
        /// 处理 WM_DPICHANGED 消息: 按新 DPI 缩放窗口尺寸、重排控件并触发 <see cref="WindowDpiChanged"/> 事件。
        /// </summary>
        /// <param name="m">携带新 DPI 与建议矩形的窗口消息。</param>
        /// <returns>消息已被处理返回 true, 否则返回 false。</returns>
        private bool WmDpiChanged(ref Message m)
        {
            if (_isPerformDpiChanged) return false;

            _isPerformDpiChanged = true;

            var oldDeviceDpi = _deviceDpi;
            var newDeviceDpi = Macros.SignedHIWORD(m.WParam);
            var suggestedRect = Marshal.PtrToStructure<RECT>(m.LParam);



            if (m.HWnd == (nint)0) return false;

            var hWnd = m.HWnd;

            ScaleFactor = SystemDpiManager.GetScaleFactorForWindow(hWnd);



            _deviceDpi = newDeviceDpi;

            var maxSizeState = _targetForm.MaximumSize;
            var minSizeState = _targetForm.MinimumSize;
            _targetForm.MinimumSize = Size.Empty;
            _targetForm.MaximumSize = Size.Empty;

            var scaleFactor = (float)newDeviceDpi / oldDeviceDpi;

            GetWindowRect(hWnd, out var lpCurrentRect);

    //#if NET8_0_OR_GREATER

    //        if (scaleFactor != 1.0f && lpCurrentRect == suggestedRect)
    //        {
    //            suggestedRect.Size = new Size((int)(suggestedRect.Width * scaleFactor), (int)(suggestedRect.Height * scaleFactor));
    //        }

    //        System.Diagnostics.Debug.WriteLine($"{scaleFactor} {lpCurrentRect.Location} {lpCurrentRect.Size} -> {suggestedRect.Location} {suggestedRect.Size}");

    //#endif



            SetWindowPos(hWnd, HWND.NULL, suggestedRect.left, suggestedRect.top, suggestedRect.Width, suggestedRect.Height, SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOACTIVATE);



            _targetForm.MinimumSize = SystemDpiManager.CalcScaledSize(minSizeState, new SizeF(scaleFactor, scaleFactor));
            _targetForm.MaximumSize = SystemDpiManager.CalcScaledSize(maxSizeState, new SizeF(scaleFactor, scaleFactor));


            IsBoundsChangingLocked = true;

            _targetForm.PerformLayout();
            _targetForm.Update();

            IsBoundsChangingLocked = false;

            WindowDpiChanged?.Invoke(this, new WindowDpiChangedEventArgs(oldDeviceDpi, newDeviceDpi));

            _isPerformDpiChanged = false;
            return true;
        }




    }
}
