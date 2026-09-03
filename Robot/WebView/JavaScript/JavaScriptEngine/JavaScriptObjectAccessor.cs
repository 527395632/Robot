// Robot 桌面软件 — JavaScript 对象访问器
// 在 CEF 侧拦截对象属性的读写,转发到远端执行并回填结果

using Robot.Browser;
using System.Linq;
using System.Text.Json;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 对象访问器:在 CEF 侧拦截对象属性的读写,转发到远端执行并回填结果。
    /// </summary>
    internal class JavaScriptObjectAccessor : CefV8Accessor
    {
        /// <summary>
        /// 初始化 <see cref="JavaScriptObjectAccessor"/> 实例。
        /// </summary>
        /// <param name="parentObject">被访问的 JavaScript 对象。</param>
        /// <param name="context">访问发生的 V8 上下文。</param>
        internal JavaScriptObjectAccessor(JavaScriptObject parentObject, CefV8Context context)
        {
            ParentObject = parentObject;
            Context = context;

            Properties = parentObject.PropertySymbols.Where(x => x.ValueType == JavaScriptValueType.Property).ToArray();
        }

        /// <summary>
        /// 被访问的 JavaScript 对象。
        /// </summary>
        public JavaScriptObject ParentObject { get; }

        /// <summary>
        /// 访问发生的 V8 上下文。
        /// </summary>
        public CefV8Context Context { get; }

        /// <summary>
        /// 该对象中所有属性类型的成员。
        /// </summary>
        public JavaScriptValue[] Properties { get; }

        /// <summary>
        /// 读取指定名称的属性:转发到远端执行并回填值或异常。
        /// </summary>
        /// <param name="name">属性名称。</param>
        /// <param name="obj">访问发生的对象。</param>
        /// <param name="returnValue">回填给 V8 的返回值。</param>
        /// <param name="exception">回填给 V8 的异常信息;无异常时为 null。</param>
        /// <returns>始终返回 true,表示已处理该读取。</returns>
        protected override bool Get(string name, CefV8Value obj, out CefV8Value returnValue, out string exception)
        {
            var prop = (JavaScriptProperty?)Properties.SingleOrDefault(x => x.Name == name);

            if (prop != null)
            {
                var browser = Context.GetBrowser();
                var frame = Context.GetFrame();

                var response = MessageBridge.ExecuteRequest(new MessageBridgeRequest
                {
                    Name = JavaScriptEngineBridge.GET_JAVASCRIPT_OBJECT_PROPERTY_MESSAGE,
                    BrowserId = browser.Identifier,
                    FrameId = frame.Identifier,
                    IsRemote = true,
                    Payload = JsonSerializer.Serialize(new AccessJavaScriptObjectPropertyMessage
                    {
                        PropertyUuid = prop.Uuid,
                        ObjectUuid = ParentObject.Uuid,
                        PropertyName = name,
                    })

                });

                if (response.IsSuccess)
                {
                    if (string.IsNullOrEmpty(response.Data))
                    {
                        returnValue = CefV8Value.CreateNull();
                    }
                    else
                    {
                        returnValue = JavaScriptValue.FromJson(response.Data).ToCefV8Value();
                    }

                    exception = null;
                }
                else
                {
                    exception = $"[{nameof(Robot)}]: {response.Exception}";
                    returnValue = CefV8Value.CreateUndefined();
                }

            }
            else
            {
                returnValue = CefV8Value.CreateUndefined();
                exception = $"[{nameof(Robot)}]: Property {name} is not found in Object.";
            }

            return true;
        }

        /// <summary>
        /// 写入指定名称的属性:校验可写性后转发到远端执行并回填异常。
        /// </summary>
        /// <param name="name">属性名称。</param>
        /// <param name="obj">访问发生的对象。</param>
        /// <param name="value">待写入的值。</param>
        /// <param name="exception">回填给 V8 的异常信息;无异常时为 null。</param>
        /// <returns>始终返回 true,表示已处理该写入。</returns>
        protected override bool Set(string name, CefV8Value obj, CefV8Value value, out string exception)
        {
            var prop = (JavaScriptProperty?)Properties.SingleOrDefault(x => x.Name == name);

            if (prop != null)
            {
                if (prop.Writable)
                {
                    var browser = Context.GetBrowser();
                    var frame = Context.GetFrame();

                    var response = MessageBridge.ExecuteRequest(new MessageBridgeRequest
                    {
                        Name = JavaScriptEngineBridge.SET_JAVASCRIPT_OBJECT_PROPERTY_MESSAGE,
                        BrowserId = browser.Identifier,
                        FrameId = frame.Identifier,
                        IsRemote = true,
                        Payload = JsonSerializer.Serialize(new AccessJavaScriptObjectPropertyMessage
                        {
                            PropertyUuid = prop.Uuid,
                            ObjectUuid = ParentObject.Uuid,
                            PropertyName = name,
                            Data = value.ToJavaScriptValue().ToJson()
                        })

                    });

                    if (response.IsSuccess)
                    {
                        exception = null;
                    }
                    else
                    {
                        exception = $"[Robot]: {response.Exception}";
                    }
                }
                else
                {
                    exception = $"[Robot]: Property {name} is not writable.";
                }
            }
            else
            {
                exception = $"[Robot]: Property {name} is not defined in Object.";
            }

            return true;
        }
    }
}
