using System.Runtime.InteropServices;

using Xilium.CefGlue;

using Robot.Browser;
using Robot.JavaScript;
using System.Reflection;

namespace Robot.App.Forms
{

    /// <summary>
    /// 将宿主窗口能力(最小化、最大化、移动、缩放、发消息等)绑定到前端 JavaScript 环境的窗口绑定对象。
    /// </summary>
    internal class RobotFormWindowBindingObject : JavaScriptWindowBindingObject
    {
        /// <summary>
        /// 从 wininet.dll 导入,用于查询系统网络连接状态。
        /// </summary>
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(int Description, int ReservedValue);


        /// <summary>
        /// 绑定对象名称,固定为 "Robot/RobotFormWindowBindingObject"。
        /// </summary>
        public override string Name { get; } = "Robot/RobotFormWindowBindingObject";
        /// <summary>
        /// 注入到前端窗口的 JavaScript 绑定代码,取自资源文件 RobotFormWindowBindingObject.js。
        /// </summary>
        public override string JavaScriptWindowBindingCode { get; } = Properties.Resources.RobotFormWindowBindingObject;

        /// <summary>
        /// 初始化 <see cref="RobotFormWindowBindingObject"/> 实例,注册所有同步与异步原生函数。
        /// </summary>
        public RobotFormWindowBindingObject()
        {
            RegisterSynchronousNativeFunction(GetRobotVersion);
            RegisterSynchronousNativeFunction(GetChromiumVersion);
            RegisterSynchronousNativeFunction(GetApplicationVersion);

            RegisterSynchronousNativeFunction(nameof(GetCurrentCulture), (owner, args) => GetCurrentCulture((RobotWindow)owner, args));


        // HostWindow 对象函数
        // - 方法
        RegisterSynchronousNativeFunction(nameof(Minimize), (owner, args) => Minimize((RobotWindow)owner, args));
        RegisterSynchronousNativeFunction(nameof(Maximize), (owner, args) => Maximize((RobotWindow)owner, args));
        RegisterSynchronousNativeFunction(nameof(Restore), (owner, args) => Restore((RobotWindow)owner, args));
        RegisterSynchronousNativeFunction(nameof(Close), (owner, args) => Close((RobotWindow)owner, args));
        RegisterSynchronousNativeFunction(nameof(FullScreen), (owner, args) => FullScreen((RobotWindow)owner, args));
        RegisterSynchronousNativeFunction(nameof(MoveTo), (owner, args) => MoveTo((RobotWindow)owner, args));
        RegisterSynchronousNativeFunction(nameof(MoveBy), (owner, args) => MoveBy((RobotWindow)owner, args));
        RegisterSynchronousNativeFunction(nameof(SizeTo), (owner, args) => SizeTo((RobotWindow)owner, args));
        RegisterSynchronousNativeFunction(nameof(SizeBy), (owner, args) => SizeBy((RobotWindow)owner, args));
        RegisterSynchronousNativeFunction(nameof(Active), (owner, args) => Active((RobotWindow)owner, args));
        RegisterSynchronousNativeFunction(nameof(Center), (owner, args) => Center((RobotWindow)owner, args));

        // - 属性
        RegisterSynchronousNativeFunction(nameof(GetWindowState), (owner, args) => GetWindowState((RobotWindow)owner, args));
        RegisterSynchronousNativeFunction(nameof(GetWindowLocation), (owner, args) => GetWindowLocation((RobotWindow)owner, args));
        RegisterSynchronousNativeFunction(nameof(GetWindowSize), (owner, args) => GetWindowSize((RobotWindow)owner, args));
        RegisterSynchronousNativeFunction(nameof(GetWindowRectangle), (owner, args) => GetWindowRectangle((RobotWindow)owner, args));

            RegisterSynchronousNativeFunction(nameof(IsFramelessWindow), (owner, args) => IsFramelessWindow((RobotWindow)owner, args));
            RegisterSynchronousNativeFunction(nameof(IsEmbeddedInControl), (owner, args) => IsEmbeddedInControl((RobotWindow)owner, args));

            RegisterSynchronousNativeFunction(nameof(PostHostWindowMessage), (owner, args) => PostHostWindowMessage((RobotWindow)owner, args));
            RegisterSynchronousNativeFunction(nameof(SendHostWindowMessageRequest), (owner, args) => SendHostWindowMessageRequest((RobotWindow)owner, args));

            RegisterAsynchronousNativeFunction(nameof(SendHostWindowMessageRequestAsync), (owner, args, promise) => SendHostWindowMessageRequestAsync((RobotWindow)owner, args, promise));
        }

        /// <summary>
        /// 异步向宿主窗口发送消息请求,通过 <see cref="JavaScriptPromise"/> 稍后解析或拒绝。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="arguments">参数数组,须为两个参数:消息文本与任意 JavaScript 值。</param>
        /// <param name="promise">用于稍后解析或拒绝请求的 Promise。</param>
        private void SendHostWindowMessageRequestAsync(RobotWindow owner, JavaScriptArray arguments, JavaScriptPromise promise)
        {

            if (arguments.Count != 2 || arguments.First().ValueType != JavaScriptValueType.String)
            {
                promise.Reject("It only accepts two parameters, one is the message text and the other is any JavaScript value type.");
                return;
            }

            var message = arguments[0].GetString();
            var data = arguments[1];

            if (message == null || string.IsNullOrEmpty(message)) {
                promise.Reject("The first argument should be message text.");
                return;
            }

            owner.OnBrowserRequestAsync(message, data, promise);


        }

        /// <summary>
        /// 同步向宿主窗口发送消息请求,返回宿主处理结果。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="arguments">参数数组,须为两个参数:消息文本与任意 JavaScript 值。</param>
        /// <returns>宿主处理结果;参数不合法时返回 null。</returns>
        private JavaScriptValue? SendHostWindowMessageRequest(RobotWindow owner, JavaScriptArray arguments)
        {

            if (arguments.Count != 2) return null;

            if (arguments.First().ValueType != JavaScriptValueType.String) return null;

            var message = arguments[0].GetString();
            var data = arguments[1];

            if (message == null || string.IsNullOrEmpty(message)) return null;

            return owner.OnBrowserRequest(message, data);
        }

        /// <summary>
        /// 向宿主窗口发送消息(无返回值),触发宿主的消息处理器。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="arguments">参数数组,须为两个参数:消息文本与任意 JavaScript 值。</param>
        /// <returns>参数不合法时返回 null,否则始终返回 null。</returns>
        private JavaScriptValue? PostHostWindowMessage(RobotWindow owner, JavaScriptArray arguments)
        {

            if (arguments.Count != 2) return null;

            if (arguments.First().ValueType != JavaScriptValueType.String) return null;

            var message = arguments[0].GetString();
            var data = arguments[1];

            if (message == null || string.IsNullOrEmpty(message)) return null;

            owner.OnBrowserMessage(message, data);

            return null;
        }


        /// <summary>
        /// 获取当前区域设置信息,包含名称、显示名/英文名数组与 LCID。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="args">参数数组(未使用)。</param>
        /// <returns>包含 name、cultureName、lcid 字段的 JavaScript 对象。</returns>
        private JavaScriptValue GetCurrentCulture(RobotWindow owner, JavaScriptArray args)
        {

            var retval = new JavaScriptObject
            {
                { "name", $"{Application.CurrentCulture}" },
                { "cultureName", new JavaScriptArray()
                    {
                        $"{Thread.CurrentThread.CurrentCulture.DisplayName}",
                        $"{Thread.CurrentThread.CurrentCulture.EnglishName}",
                    }
                },
                { "lcid", Application.CurrentCulture.LCID}
            };

            return retval;
        }


        /// <summary>
        /// 获取 Robot 应用程序集的版本号。
        /// </summary>
        /// <param name="_">参数数组(未使用)。</param>
        /// <returns>版本号字符串;无法获取时返回 "UNKNOWN"。</returns>
        public JavaScriptValue GetRobotVersion(JavaScriptArray _)
        {

            var version = typeof(Program).Assembly?.GetName()?.Version;

            if (version == null) return "UNKNOWN";

            return $"{version}";
        }

        /// <summary>
        /// 获取 Chromium 内核的版本号。
        /// </summary>
        /// <param name="_">参数数组(未使用)。</param>
        /// <returns>Chromium 版本号字符串。</returns>
        public JavaScriptValue GetChromiumVersion(JavaScriptArray _)
        {
            return $"{CefRuntime.ChromeVersion}";
        }

        /// <summary>
        /// 获取宿主应用程序(入口程序集)的版本号。
        /// </summary>
        /// <param name="_">参数数组(未使用)。</param>
        /// <returns>版本号字符串;无法获取时返回 "UNKNOWN"。</returns>
        public JavaScriptValue GetApplicationVersion(JavaScriptArray _)
        {
            var version = Assembly.GetEntryAssembly()?.GetName()?.Version;
            if (version == null) return "UNKNOWN";

            return $"{version}";
        }

        #region HostWindow

        /// <summary>
        /// 最小化宿主窗口(仅当窗口可最小化时)。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="_">参数数组(未使用)。</param>
        /// <returns>始终返回 null。</returns>
        private JavaScriptValue? Minimize(RobotWindow owner, JavaScriptArray _)
        {

            if (!owner.Minimizable)
                return null;

            owner.InvokeOnUIThread(() =>
            {
                owner.WindowState = RobotFormWindowState.Minimized;
            });

            return null;
        }

        /// <summary>
        /// 最大化或还原宿主窗口(仅当窗口可最大化时,在最大化与正常状态间切换)。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="_">参数数组(未使用)。</param>
        /// <returns>始终返回 null。</returns>
        private JavaScriptValue? Maximize(RobotWindow owner, JavaScriptArray _)
        {

            if (!owner.Maximizable)
                return null;

            if (owner.WindowState != RobotFormWindowState.Maximized)
            {
                owner.InvokeOnUIThread(() => owner.WindowState = RobotFormWindowState.Maximized);
            }
            else
            {
                owner.InvokeOnUIThread(() => owner.WindowState = RobotFormWindowState.Normal);
            }

            return null;
        }

        /// <summary>
        /// 还原宿主窗口到正常状态(仅当当前非正常状态时)。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="_">参数数组(未使用)。</param>
        /// <returns>始终返回 null。</returns>
        private JavaScriptValue? Restore(RobotWindow owner, JavaScriptArray _)
        {

            if (owner.WindowState != RobotFormWindowState.Normal)
            {
                owner.InvokeOnUIThread(() => owner.WindowState = RobotFormWindowState.Normal);
            }

            return null;
        }

        /// <summary>
        /// 将宿主窗口切换为全屏状态(仅当允许全屏时)。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="_">参数数组(未使用)。</param>
        /// <returns>始终返回 null。</returns>
        private JavaScriptValue? FullScreen(RobotWindow owner, JavaScriptArray _)
        {

            if (owner.AllowFullScreen)
            {
                owner.InvokeOnUIThread(() => owner.WindowState = RobotFormWindowState.FullScreen);
            }
            return null;
        }

        /// <summary>
        /// 关闭宿主窗口。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="_">参数数组(未使用)。</param>
        /// <returns>始终返回 null。</returns>
        private JavaScriptValue? Close(RobotWindow owner, JavaScriptArray _)
        {

            owner.InvokeOnUIThread(owner.Close);
            return null;
        }



        /// <summary>
        /// 将宿主窗口移动到指定坐标(仅当窗口处于正常状态时)。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="arguments">参数数组,两个整数参数:目标 x、y 坐标。</param>
        /// <returns>始终返回 null。</returns>
        private JavaScriptValue? MoveTo(RobotWindow owner, JavaScriptArray arguments)
        {

            if (owner.WindowState != RobotFormWindowState.Normal)
                return null;

            var x = arguments.Count == 2 ? arguments[0].GetInt() : 0;
            var y = arguments.Count == 2 ? arguments[1].GetInt() : 0;

            owner.InvokeOnUIThread(() =>
            {
                owner.Left = x;
                owner.Top = y;
            });

            return null;
        }

        /// <summary>
        /// 将宿主窗口按指定偏移量移动(仅当窗口处于正常状态时)。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="arguments">参数数组,两个整数参数:x、y 方向偏移量。</param>
        /// <returns>始终返回 null。</returns>
        private JavaScriptValue? MoveBy(RobotWindow owner, JavaScriptArray arguments)
        {


            if (owner.WindowState != RobotFormWindowState.Normal)
                return null;

            var x = arguments.Count == 2 ? arguments[0].GetInt() : 0;
            var y = arguments.Count == 2 ? arguments[1].GetInt() : 0;

            owner.InvokeOnUIThread(() =>
            {
                owner.Left += x;
                owner.Top += y;
            });

            return null;
        }

        /// <summary>
        /// 将宿主窗口尺寸调整为指定宽高(仅当窗口处于正常状态时,宽高非正值时保留原值)。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="arguments">参数数组,两个整数参数:目标宽度、高度。</param>
        /// <returns>始终返回 null。</returns>
        private JavaScriptValue? SizeTo(RobotWindow owner, JavaScriptArray arguments)
        {

            if (owner.WindowState != RobotFormWindowState.Normal)
                return null;

            var width = arguments.Count == 2 ? arguments[0].GetInt() : 0;
            var height = arguments.Count == 2 ? arguments[1].GetInt() : 0;

            owner.InvokeOnUIThread(() =>
            {


                if (width > 0 && height > 0)
                {
                    owner.Size = new Size(width, height);
                }
                else if (width <= 0)
                {
                    owner.Size = new Size(owner.Width, height);

                }
                else if (height <= 0)
                {
                    owner.Size = new Size(width, owner.Height);
                }
            });

            return null;
        }

        /// <summary>
        /// 将宿主窗口尺寸按指定偏移量调整(仅当窗口可调整大小且处于正常状态时,结果非正值时保留原值)。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="arguments">参数数组,两个整数参数:宽度、高度偏移量。</param>
        /// <returns>始终返回 null。</returns>
        private JavaScriptValue? SizeBy(RobotWindow owner, JavaScriptArray arguments)
        {

            if (!owner.Sizable || owner.WindowState != RobotFormWindowState.Normal)
                return null;

            var width = arguments.Count == 2 ? arguments[0].GetInt() : 0;
            var height = arguments.Count == 2 ? arguments[1].GetInt() : 0;



            owner.InvokeOnUIThread(() =>
            {
                width = owner.Width + width;
                height = owner.Height + height;

                if (width > 0 && height > 0)
                {
                    owner.Size = new Size(width, height);
                }
                else if (width <= 0)
                {
                    owner.Size = new Size(owner.Width, height);

                }
                else if (height <= 0)
                {
                    owner.Size = new Size(width, owner.Height);
                }
            });

            return null;
        }

        /// <summary>
        /// 激活宿主窗口,使其成为前台窗口。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="_">参数数组(未使用)。</param>
        /// <returns>始终返回 null。</returns>
        private JavaScriptValue? Active(RobotWindow owner, JavaScriptArray _)
        {

            owner.InvokeOnUIThread(owner.Activate);

            return null;
        }

        /// <summary>
        /// 将宿主窗口居中到父窗口(仅当窗口可调整大小且处于正常状态时)。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="_">参数数组(未使用)。</param>
        /// <returns>始终返回 null。</returns>
        private JavaScriptValue? Center(RobotWindow owner, JavaScriptArray _)
        {


            if (!owner.Sizable || owner.WindowState != RobotFormWindowState.Normal)
                return null;

            owner.InvokeOnUIThread(owner.CenterToParent);

            return null;
        }

        /// <summary>
        /// 获取宿主窗口当前状态(小写字符串,如 "normal"、"maximized"、"minimized")。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="_">参数数组(未使用)。</param>
        /// <returns>窗口状态字符串。</returns>
        private JavaScriptValue GetWindowState(RobotWindow owner, JavaScriptArray _)
        {

            return new JavaScriptValue($"{owner?.WindowState.ToString().ToLower() ?? "normal"}");
        }

        /// <summary>
        /// 获取宿主窗口位置,返回包含 x、y 字段的 JavaScript 对象。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="_">参数数组(未使用)。</param>
        /// <returns>包含 x、y 字段的 JavaScript 对象。</returns>
        private JavaScriptValue GetWindowLocation(RobotWindow owner, JavaScriptArray _)
        {

            var obj = new JavaScriptObject
            {
                { "x", owner?.Location.X ?? 0 },
                { "y", owner?.Location.Y ?? 0 }
            };

            return obj;
        }

        /// <summary>
        /// 获取宿主窗口尺寸,返回包含 width、height 字段的 JavaScript 对象。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="_">参数数组(未使用)。</param>
        /// <returns>包含 width、height 字段的 JavaScript 对象。</returns>
        private JavaScriptValue GetWindowSize(RobotWindow owner, JavaScriptArray _)
        {

            var obj = new JavaScriptObject
            {
                { "width", owner?.Size.Width ?? 0 },
                { "height", owner?.Size.Height ?? 0 }
            };

            return obj;
        }

        /// <summary>
        /// 获取宿主窗口矩形,返回包含 left、top、right、bottom、width、height 字段的 JavaScript 对象。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="_">参数数组(未使用)。</param>
        /// <returns>包含窗口矩形各字段信息的 JavaScript 对象。</returns>
        private JavaScriptValue GetWindowRectangle(RobotWindow owner, JavaScriptArray _)
        {

            var obj = new JavaScriptObject
            {
                { "left", owner?.Location.X ?? 0 },
                { "top", owner?.Location.Y ?? 0 },
                { "right", (owner?.Location.X ?? 0) + (owner?.Size.Width ?? 0) },
                { "bottom", (owner?.Location.Y ?? 0) + (owner?.Size.Height ?? 0) },
                { "width", owner?.Size.Width ?? 0 },
                { "height", owner?.Size.Height ?? 0 }
            };

            return obj;
        }

        /// <summary>
        /// 指示宿主窗口是否为无边框窗口,当前恒返回 true。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="_">参数数组(未使用)。</param>
        /// <returns>恒为 true。</returns>
        private JavaScriptValue IsFramelessWindow(RobotWindow owner, JavaScriptArray _)
        {

            return true;
        }

        /// <summary>
        /// 指示宿主窗口是否嵌入在控件中,当前恒返回 false。
        /// </summary>
        /// <param name="owner">宿主表单。</param>
        /// <param name="_">参数数组(未使用)。</param>
        /// <returns>恒为 false。</returns>
        private JavaScriptValue IsEmbeddedInControl(RobotWindow owner, JavaScriptArray _)
        {
            return false;
        }

        #endregion

    }
}
