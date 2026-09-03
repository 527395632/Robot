// Robot 桌面软件 — 远端对象映射创建任务
// 在远端线程执行,将接收到的对象集合挂载到全局 external 对象上

using System;
using System.Diagnostics;
using System.Windows.Forms;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// 远端对象映射创建任务:在远端线程执行,将接收到的对象集合挂载到全局 external 对象上。
    /// </summary>
    internal class JavaScriptObjectMapCreationTaskOnRemote : CefTask
    {
        /// <summary>
        /// 对象映射桥。
        /// </summary>
        public JavaScriptObjectMappingBridge Bridge { get; }

        /// <summary>
        /// 待挂载的对象集合。
        /// </summary>
        public JavaScriptObject Objects { get; }

        /// <summary>
        /// 目标帧。
        /// </summary>
        public required CefFrame Frame { get; init; }

        /// <summary>
        /// 初始化 <see cref="JavaScriptObjectMapCreationTaskOnRemote"/> 实例。
        /// </summary>
        /// <param name="bridge">对象映射桥。</param>
        /// <param name="data">对象集合 JSON;解析失败时使用空对象。</param>
        public JavaScriptObjectMapCreationTaskOnRemote(JavaScriptObjectMappingBridge bridge, string data)
        {
            Bridge = bridge;

            try
            {
                Objects = JavaScriptValue.FromJson(data)!.ToObject();
            }
            catch
            {
                Objects = new JavaScriptObject();
            }
        }

        /// <summary>
        /// 执行任务:在 V8 上下文中创建 external 对象并挂载所有属性。
        /// </summary>
        protected override void Execute()
        {
            if (Frame == null) return;

            CefV8Context context;

            try
            {
                context = Frame.V8Context;
                if (context == null) return;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }

            context.Enter();
            try
            {
                using var global = context.GetGlobal();

                if (global.HasValue("external"))
                {
                    global.DeleteValue("external");
                }

                CefV8Value externalObject = CefV8Value.CreateObject();

                global.SetValue("external", externalObject, CefV8PropertyAttribute.DontDelete | CefV8PropertyAttribute.DontEnum);

                foreach (var key in Objects.PropertyNames)
                {
                    var source = Objects.GetValue(key);

                    externalObject.SetValue(key, source.ToCefV8Value(), CefV8PropertyAttribute.DontDelete | CefV8PropertyAttribute.DontEnum);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                context.Exit();
            }
        }
    }
}
