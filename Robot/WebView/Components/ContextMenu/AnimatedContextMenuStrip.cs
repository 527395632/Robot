// Robot 桌面软件 — 动画右键菜单
// 带淡入动画效果的右键菜单,支持深色模式

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Robot.Browser.ContextMenu
{

    /// <summary>
    /// 动画右键菜单:带淡入动画效果的右键菜单,支持深色模式。
    /// </summary>
    internal class AnimatedContextMenuStrip : ContextMenuStrip
    {
        /// <summary>
        /// 动画方向:水平正向。
        /// </summary>
        private const uint AW_HOR_POSITIVE = 0x1;

        /// <summary>
        /// 动画方向:水平负向。
        /// </summary>
        private const uint AW_HOR_NEGATIVE = 0x2;

        /// <summary>
        /// 动画方向:垂直正向。
        /// </summary>
        private const uint AW_VER_POSITIVE = 0x4;

        /// <summary>
        /// 动画方向:垂直负向。
        /// </summary>
        private const uint AW_VER_NEGATIVE = 0x8;

        /// <summary>
        /// 动画方向:居中。
        /// </summary>
        private const uint AW_CENTER = 0x10;

        /// <summary>
        /// 动画效果:隐藏。
        /// </summary>
        private const uint AW_HIDE = 0x10000;

        /// <summary>
        /// 动画效果:激活。
        /// </summary>
        private const uint AW_ACTIVATE = 0x20000;

        /// <summary>
        /// 动画效果:滑动。
        /// </summary>
        private const uint AW_SLIDE = 0x40000;

        /// <summary>
        /// 动画效果:混合。
        /// </summary>
        private const uint AW_BLEND = 0x80000;

        /// <summary>
        /// 是否启用深色模式。
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DarkMode { get; set; } = false;

        /// <summary>
        /// 初始化 <see cref="AnimatedContextMenuStrip"/> 实例。
        /// </summary>
        public AnimatedContextMenuStrip()
        {
        }

        /// <summary>
        /// 初始化 <see cref="AnimatedContextMenuStrip"/> 实例并加入容器。
        /// </summary>
        /// <param name="container">组件容器。</param>
        public AnimatedContextMenuStrip(IContainer container) : this()
        {
            if (container == null)
            {
                throw new ArgumentNullException("container is null");
            }

            container.Add(this);
        }

        /// <summary>
        /// 菜单打开前回调:设置初始透明度并触发淡入动画。
        /// </summary>
        /// <param name="e">取消事件参数。</param>
        protected override void OnOpening(CancelEventArgs e)
        {
            base.OnOpening(e);

            Opacity = 0;

            FadeOut();

            //User32.AnimateWindow(Handle, 50, AW_SLIDE | AW_VER_POSITIVE);
        }

        /// <summary>
        /// 淡入动画:逐步提升透明度直至完全显示。
        /// </summary>
        async void FadeOut()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(FadeOut));
                return;
            }

            Opacity += 0.1d;
            await Task.Delay(10);

            if (Opacity >= 1)
            {
                return;
            };

            FadeOut();
        }
    }
}
