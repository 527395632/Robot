using System.ComponentModel;

namespace Robot
{

    /// <summary>
    /// 启动画面面板,填充到目标表单上,通过外部提供的绘制委托进行自绘。
    /// </summary>
    internal class SplashScreen : Panel
    {
        /// <summary>
        /// 绘制委托,在面板可见时于 <see cref="OnPaint"/> 中调用。
        /// </summary>
        private readonly Action<PaintEventArgs> _drawAction;

        /// <summary>
        /// 目标表单(宿主控件)。
        /// </summary>
        protected Form TargetControl { get; }
        /// <summary>
        /// 缓存的绘制图像,设计器中不可见。
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image? CachedImage { get; private set; }

        //private Rectangle CanvasBounds {
        //    get {
        //        User32.GetClientRect(TargetControl.Handle, out var rect);

        //        return rect;
        //    }
        //}



        /// <summary>
        /// 初始化 <see cref="SplashScreen"/> 实例,启用双缓冲自绘并填充到父表单。
        /// </summary>
        /// <param name="parent">宿主表单。</param>
        /// <param name="drawAction">绘制委托,在面板可见时于 <see cref="OnPaint"/> 中调用。</param>
        public SplashScreen(Form parent, Action<PaintEventArgs> drawAction)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            TargetControl = parent;
            BackColor = Color.White;
            _drawAction = drawAction;

            Dock= DockStyle.Fill;

            Margin = Padding.Empty;
        }



        //private void PaintRequest()
        //{
        //    if (Visible == false) return;


        //    var bounds = CanvasBounds;

        //    var width = bounds.Width;
        //    var height = bounds.Height;

        //    using var bitmap = new Bitmap(width, height);
        //    var bitmapData = bitmap.LockBits(new Rectangle(Point.Empty, new Size(width, height)), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        //    using var surface = SKSurface.Create(new SKImageInfo
        //    {
        //        ColorType = SKColorType.Bgra8888,
        //        AlphaType = SKAlphaType.Premul,
        //        Width = width,
        //        Height = height,
        //    }, bitmapData.Scan0, bitmapData.Stride);
        //    using var canvas = surface.Canvas;

        //    await Task.Run(() => _drawAction(canvas));

        //    bitmap.UnlockBits(bitmapData);

        //    var ms = new MemoryStream();
        //    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

        //    var oldImage = CachedImage;

        //    CachedImage = Image.FromStream(ms);



        //    BackgroundImage = CachedImage;

        //    if (oldImage != null)
        //    {
        //        oldImage.Dispose();
        //    }
        //}


        /// <summary>
        /// 绘制面板,面板可见时调用绘制委托。
        /// </summary>
        /// <param name="e">包含绘制信息的 <see cref="PaintEventArgs"/>。</param>
        protected override void OnPaint(PaintEventArgs e)
        {

            if (Visible)
            {
                _drawAction.Invoke(e);
            }

        }


    }
}
