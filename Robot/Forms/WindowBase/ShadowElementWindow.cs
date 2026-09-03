
using System;
using System.Drawing;
using System.Runtime.InteropServices;

using Vanara.PInvoke;
using static Vanara.PInvoke.User32;
using static Vanara.PInvoke.Gdi32;
using SkiaSharp;

namespace Robot.Forms
{

    /// <summary>
    /// 阴影元素窗口: 一个分层 (layered) 无边框子窗口, 用于在主窗口某一侧绘制阴影。
    /// 通过 UpdateLayeredWindow 更新阴影位图, 并跟随父窗口位置与 Z 序。
    /// </summary>
    internal partial class ShadowElementWindow
    {
        /// <summary>
        /// 阴影元素窗口类名前缀。
        /// </summary>
        const string SHADOW_WND_NAME = "ShivaShadowElementWindow";

        /// <summary>
        /// 阴影元素窗口句柄。
        /// </summary>
        public SafeHWND WND { get; private set; }

        /// <summary>
        /// 父窗口 (目标窗口) 句柄。
        /// </summary>
        public SafeHWND ParentWND { get; }

        /// <summary>
        /// 窗口类 (注册后用于创建窗口)。
        /// </summary>
        private WindowClass _windowClass;

        /// <summary>
        /// 是否置顶 (跟随目标窗口的 TopMost)。
        /// </summary>
        internal bool IsTopMost { get; }


        /// <summary>
        /// 阴影元素方位 (上/下/左/右)。
        /// </summary>
        public ShadowElementPosition Position { get; }

        /// <summary>
        /// 窗口阴影装饰器 (提供目标窗口与阴影配置)。
        /// </summary>
        public WindowShadowDecorator ShadowDecorator { get; }

        /// <summary>
        /// 阴影效果配置 (来自装饰器)。
        /// </summary>
        public ShadowEffectConfiguration ShadowConfiguration => ShadowDecorator.ShadowElementConfiguration;

        /// <summary>
        /// 初始化阴影元素窗口: 注册窗口类并创建分层无边框子窗口。
        /// </summary>
        public ShadowElementWindow(ShadowElementPosition elemetPossition, WindowShadowDecorator windowShadowDecorator)
        {
            Position = elemetPossition;
            ShadowDecorator = windowShadowDecorator;

            ParentWND = new SafeHWND(ShadowDecorator.TargetWindow.Handle);

            IsTopMost = ShadowDecorator.TargetWindow.TopMost;


            var className = $"{SHADOW_WND_NAME}_{ShadowDecorator.TargetWindow.Handle}_{Position}";

            _windowClass = new WindowClass(className, HINSTANCE.NULL, WndProc, hbrBkgd: HBRUSH.NULL);

            var exStyles = WindowStylesEx.WS_EX_LAYERED | WindowStylesEx.WS_EX_NOACTIVATE | WindowStylesEx.WS_EX_TRANSPARENT | WindowStylesEx.WS_EX_NOREDIRECTIONBITMAP;

            var styles = WindowStyles.WS_CLIPCHILDREN | WindowStyles.WS_POPUP | WindowStyles.WS_CLIPSIBLINGS;

            WND = CreateWindowEx(dwExStyle: exStyles, lpClassName: _windowClass.ClassName, lpWindowName: SHADOW_WND_NAME, dwStyle: styles, X: 0, Y: 0, nWidth: 0, nHeight: 0, hWndParent: HWND.NULL, hMenu: HMENU.NULL, hInstance: HINSTANCE.NULL, lpParam: (nint)0);


        }

        /// <summary>
        /// 阴影位图缓存状态 (尺寸变化时重新生成位图)。
        /// </summary>
        ShadowBitmapState? _bitmapBuff = null;

        /// <summary>
        /// 更新阴影位图: 尺寸变化或强制刷新时重新生成位图并更新分层窗口, 否则仅更新矩形位置。
        /// </summary>
        public void UpdateBitmap(RECT windowRect, bool isActivated, bool forceRefresh = false)
        {
            var tpl = ShadowDecorator.ShadowElementRender;
            var shadowConfig = ShadowConfiguration;

            if (tpl == null) return;

            var shadowRect = windowRect;
            InflateRect(ref shadowRect, shadowConfig.Size, shadowConfig.Size);

            var elementRect = GetShadowElementRect(windowRect, shadowRect);

            var shouldRedraw = forceRefresh || _bitmapBuff == null || _bitmapBuff.Width != elementRect.Width || _bitmapBuff.Height != elementRect.Height;


            if (shouldRedraw)
            {
                var width = elementRect.Width;
                var height = elementRect.Height;


                var imgInfo = new SKImageInfo()
                {
                    AlphaType = SKAlphaType.Premul,
                    ColorType = SKColorType.Bgra8888,
                    Height = height,
                    Width = width
                };

                using var bitmap = new Bitmap(width, height);

                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                using var surface = SKSurface.Create(imgInfo, bitmapData.Scan0, bitmapData.Stride);
                using var canvas = surface.Canvas;

                tpl.DrawShadowBitmap(canvas, Position, isActivated);

                bitmap.UnlockBits(bitmapData);


                _bitmapBuff = new ShadowBitmapState(elementRect);

                UpdateLayer(elementRect, bitmap);



                SetWindowPos(WND, ParentWND, _bitmapBuff.X, _bitmapBuff.Y, _bitmapBuff.Width, _bitmapBuff.Height, SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOMOVE);

            }
            else if (_bitmapBuff != null)
            {
                _bitmapBuff.UpdateRectangle(elementRect);


                SetWindowPos(WND, ParentWND, _bitmapBuff.X, _bitmapBuff.Y, _bitmapBuff.Width, _bitmapBuff.Height, SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOSIZE);
            }

        }



        /// <summary>
        /// 根据方位计算阴影元素矩形 (窗口矩形与阴影矩形之间的过渡区域)。
        /// </summary>
        private RECT GetShadowElementRect(RECT windowRect, RECT shadowRect)
        {
            var shadowConfig = ShadowConfiguration;

            var insideOffset = shadowConfig.InsideOffset;

            switch (Position)
            {
                case ShadowElementPosition.Top:
                    return new RECT(shadowRect.left, shadowRect.top, shadowRect.right, windowRect.top + insideOffset);
                case ShadowElementPosition.Right:
                    return new RECT(windowRect.right - insideOffset, windowRect.top, shadowRect.right, windowRect.bottom);
                case ShadowElementPosition.Bottom:
                    return new RECT(shadowRect.left, windowRect.bottom - insideOffset, shadowRect.right, shadowRect.bottom);
                case ShadowElementPosition.Left:
                    return new RECT(shadowRect.left, windowRect.top, windowRect.left + insideOffset, windowRect.bottom);
            }

            throw new ArgumentOutOfRangeException("position");
        }

        /// <summary>
        /// 窗口过程: 直接交由默认窗口过程处理。
        /// </summary>
        private nint WndProc(HWND hWnd, uint umsg, nint wParam, nint lParam)
        {
            return DefWindowProc(hWnd, umsg, wParam, lParam);
        }



        /// <summary>
        /// 更新分层窗口: 将阴影位图通过 UpdateLayeredWindow 绘制到窗口, 并释放 GDI 资源。
        /// </summary>
        private void UpdateLayer(RECT rect, Bitmap bitmap)
        {
            var windowRect = rect;

            if (windowRect.Width <= 0 || windowRect.Height <= 0) return;

            var screenDC = GetDC();
            var memDC = CreateCompatibleDC(screenDC);

            var hBitmap = HBITMAP.NULL;

            var hOldBitmap = HBITMAP.NULL;

            try
            {
                hBitmap = new HBITMAP(bitmap.GetHbitmap(Color.FromArgb(0x00, 0x00, 0x00, 0x00)));
                hOldBitmap = SelectObject(memDC, hBitmap);


                var location = new POINT(windowRect.X, windowRect.Y);
                var size = new SIZE(windowRect.Width, windowRect.Height);


                UpdateLayeredWindow(WND, screenDC, location, size, memDC, POINT.Empty, COLORREF.Default, ShadowDecorator.DefaultBlendFunciton, UpdateLayeredWindowFlags.ULW_ALPHA);
            }
            finally
            {
                if (hBitmap != HBITMAP.NULL)
                {
                    SelectObject(memDC, hOldBitmap);
                    DeleteObject(hBitmap);
                }

                if (memDC != HDC.NULL)
                {
                    DeleteDC(memDC);
                }

                if (screenDC != HDC.NULL)
                {
                    ReleaseDC(HWND.NULL, screenDC);
                }

                bitmap.Dispose();

                System.GC.SuppressFinalize(bitmap);

            }




        }


        /// <summary>
        /// 32 位 SetWindowLong 导入。
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]

        static extern nint SetWindowLong32(nint hWnd, int nIndex, nint dwNewLong);

        /// <summary>
        /// 64 位 SetWindowLongPtr 导入。
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]

        static extern nint SetWindowLong64(nint hWnd, int nIndex, nint dwNewLong);

        /// <summary>
        /// 按指针大小选择 32/64 位 API 设置窗口长 (Window Long)。
        /// </summary>
        internal static nint SetWindowLongPtr(HWND hWnd, WindowLongFlags nIndex, nint dwNewLong)
        {
            if (IntPtr.Size == 8)
            {
                return SetWindowLong64(hWnd.DangerousGetHandle(), (int)nIndex, dwNewLong);
            }
            else
            {
                return SetWindowLong32(hWnd.DangerousGetHandle(), (int)nIndex, dwNewLong);
            }
        }

        /// <summary>
        /// 设置窗口所有者 (父窗口)。
        /// </summary>
        public void SetOwner(HWND owner)
        {
            var retval = SetWindowLongPtr(WND, WindowLongFlags.GWL_HWNDPARENT, owner.DangerousGetHandle());
        }

        /// <summary>
        /// 更新 Z 序: 按是否置顶调整窗口层级, 并保持在父窗口之上。
        /// </summary>
        public void UpdateZOrder()
        {
            if (ParentWND == (nint)0) return;

            SetWindowPos(WND, IsTopMost ? HWND.HWND_TOPMOST : HWND.HWND_NOTOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOACTIVATE);

            SetWindowPos(WND, ParentWND, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOACTIVATE);
        }

        /// <summary>
        /// 关闭阴影元素窗口 (解除父级并关闭窗口)。
        /// </summary>
        public void Close()
        {
            SetParent(WND, HWND.NULL);
            CloseWindow(WND);
        }

        /// <summary>
        /// 是否已释放。
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 释放阴影元素窗口资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            System.GC.SuppressFinalize(this);

        }

        /// <summary>
        /// 释放阴影元素窗口: 销毁窗口句柄。
        /// </summary>
        private void Dispose(bool isDisposed)
        {

            if (_disposed)
                return;

            DestroyWindow(WND);

            _disposed = isDisposed;
        }

    }
}
