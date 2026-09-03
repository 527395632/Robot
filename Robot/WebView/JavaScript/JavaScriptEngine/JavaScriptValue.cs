// Robot 桌面软件 — JavaScript 值(基类)
// 承载任意 JavaScript 值,支持隐式类型转换、跨进程序列化与帧关联

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 值(基类):承载任意 JavaScript 值,支持隐式类型转换、跨进程序列化与帧关联。
    /// </summary>
    public class JavaScriptValue : IDisposable
    {
        /// <summary>
        /// 全局 JavaScript 值集合(用于按标识查找与统一释放)。
        /// </summary>
        private static readonly HashSet<JavaScriptValue> JAVASCRIPT_VALUE_COLLECTION = new();

        /// <summary>
        /// 全局 JavaScript 值数量。
        /// </summary>
        public static int JAVASCRIPT_VALUE_COLLECTION_COUNT => JAVASCRIPT_VALUE_COLLECTION.Count;

        /// <summary>
        /// 释放所有 JavaScript 值。
        /// </summary>
        internal static void Release()
        {
            for (var i = 0; i < JAVASCRIPT_VALUE_COLLECTION.Count; i++)
            {
                JAVASCRIPT_VALUE_COLLECTION.ElementAt(i)?.Dispose();
            }
        }

        /// <summary>
        /// 释放与指定浏览器关联的所有 JavaScript 值。
        /// </summary>
        /// <param name="browser">目标浏览器。</param>
        internal static void Release(CefBrowser browser)
        {
            var items = JAVASCRIPT_VALUE_COLLECTION.Where(x => x.GetAssociatedFrame()?.Browser.Identifier == browser.Identifier).ToArray();

            foreach (JavaScriptValue item in items)
            {
                item.Dispose();
            }
        }

        /// <summary>
        /// 释放与指定帧关联的所有 JavaScript 值。
        /// </summary>
        /// <param name="frame">目标帧。</param>
        internal static void Release(CefFrame frame)
        {
            var items = JAVASCRIPT_VALUE_COLLECTION.Where(x => x.GetAssociatedFrame()?.Identifier == frame.Identifier).ToArray();

            foreach (JavaScriptValue item in items)
            {
                item.Dispose();
            }
        }

        /// <summary>
        /// 隐式转换为字符串。
        /// </summary>
        /// <param name="value">JavaScript 值。</param>
        public static implicit operator string?(JavaScriptValue value) => value.GetString();

        /// <summary>
        /// 隐式转换为布尔值。
        /// </summary>
        /// <param name="value">JavaScript 值。</param>
        public static implicit operator bool(JavaScriptValue value) => value.GetBoolean();

        /// <summary>
        /// 隐式转换为双精度浮点数。
        /// </summary>
        /// <param name="value">JavaScript 值。</param>
        public static implicit operator double(JavaScriptValue value) => value.GetDouble();

        /// <summary>
        /// 隐式转换为单精度浮点数。
        /// </summary>
        /// <param name="value">JavaScript 值。</param>
        public static implicit operator float(JavaScriptValue value) => value.GetFloat();

        /// <summary>
        /// 隐式转换为 32 位整数。
        /// </summary>
        /// <param name="value">JavaScript 值。</param>
        public static implicit operator int(JavaScriptValue value) => value.GetInt();

        /// <summary>
        /// 隐式转换为 64 位整数。
        /// </summary>
        /// <param name="value">JavaScript 值。</param>
        public static implicit operator long(JavaScriptValue value) => value.GetBigInt();

        /// <summary>
        /// 隐式转换为日期时间。
        /// </summary>
        /// <param name="value">JavaScript 值。</param>
        public static implicit operator DateTime(JavaScriptValue value) => value.GetDateTime();

        /// <summary>
        /// 隐式转换为十进制数。
        /// </summary>
        /// <param name="value">JavaScript 值。</param>
        public static implicit operator decimal(JavaScriptValue value) => value.GetDecimal();

        /// <summary>
        /// 从字符串隐式转换为 JavaScript 值。
        /// </summary>
        /// <param name="value">字符串。</param>
        public static implicit operator JavaScriptValue(string value) => new JavaScriptValue(value);

        /// <summary>
        /// 从布尔值隐式转换为 JavaScript 值。
        /// </summary>
        /// <param name="value">布尔值。</param>
        public static implicit operator JavaScriptValue(bool value) => new JavaScriptValue(value);

        /// <summary>
        /// 从双精度浮点数隐式转换为 JavaScript 值。
        /// </summary>
        /// <param name="value">双精度浮点数。</param>
        public static implicit operator JavaScriptValue(double value) => new JavaScriptValue(value);

        /// <summary>
        /// 从单精度浮点数隐式转换为 JavaScript 值。
        /// </summary>
        /// <param name="value">单精度浮点数。</param>
        public static implicit operator JavaScriptValue(float value) => new JavaScriptValue(value);

        /// <summary>
        /// 从 32 位整数隐式转换为 JavaScript 值。
        /// </summary>
        /// <param name="value">32 位整数。</param>
        public static implicit operator JavaScriptValue(int value) => new JavaScriptValue(value);

        /// <summary>
        /// 从无符号 32 位整数隐式转换为 JavaScript 值。
        /// </summary>
        /// <param name="value">无符号 32 位整数。</param>
        public static implicit operator JavaScriptValue(uint value) => new JavaScriptValue(value);

        /// <summary>
        /// 从 64 位整数隐式转换为 JavaScript 值。
        /// </summary>
        /// <param name="value">64 位整数。</param>
        public static implicit operator JavaScriptValue(long value) => new JavaScriptValue(value);

        /// <summary>
        /// 从无符号 64 位整数隐式转换为 JavaScript 值。
        /// </summary>
        /// <param name="value">无符号 64 位整数。</param>
        public static implicit operator JavaScriptValue(ulong value) => new JavaScriptValue(value);

        /// <summary>
        /// 从 16 位整数隐式转换为 JavaScript 值。
        /// </summary>
        /// <param name="value">16 位整数。</param>
        public static implicit operator JavaScriptValue(short value) => new JavaScriptValue(value);

        /// <summary>
        /// 从无符号 16 位整数隐式转换为 JavaScript 值。
        /// </summary>
        /// <param name="value">无符号 16 位整数。</param>
        public static implicit operator JavaScriptValue(ushort value) => new JavaScriptValue(value);

        /// <summary>
        /// 从十进制数隐式转换为 JavaScript 值。
        /// </summary>
        /// <param name="value">十进制数。</param>
        public static implicit operator JavaScriptValue(decimal value) => new JavaScriptValue(value);

        /// <summary>
        /// 从日期时间隐式转换为 JavaScript 值。
        /// </summary>
        /// <param name="value">日期时间。</param>
        public static implicit operator JavaScriptValue(DateTime value) => new JavaScriptValue(value);

        /// <summary>
        /// 原始值。
        /// </summary>
        internal readonly object? RawValue = null;

        /// <summary>
        /// 值唯一标识。
        /// </summary>
        internal protected Guid Uuid { get; internal set; } = Guid.NewGuid();

        /// <summary>
        /// 值关联的帧。
        /// </summary>
        internal protected CefFrame? Frame { get; internal set; }

        /// <summary>
        /// 值类型。
        /// </summary>
        public JavaScriptValueType ValueType { get; internal set; } = JavaScriptValueType.Undefined;

        /// <summary>
        /// 父级值(数组或对象)。
        /// </summary>
        internal protected JavaScriptValue? Parent { get; internal set; } = null;

        /// <summary>
        /// 是否已冻结。
        /// </summary>
        internal protected bool IsFreeze { get; private set; } = false;

        /// <summary>
        /// 将该值关联到指定帧。
        /// </summary>
        /// <param name="frame">目标帧;为 null 时解除关联。</param>
        internal protected virtual void AssociateToFrame(CefFrame? frame)
        {
            Frame = frame;
        }

        /// <summary>
        /// 获取关联的帧(自身无帧时回溯父级)。
        /// </summary>
        /// <returns>关联的帧;无关联时为 null。</returns>
        internal protected CefFrame? GetAssociatedFrame()
        {
            var frame = Frame ?? Parent?.GetAssociatedFrame();

            Frame = frame;

            return frame;
        }

        /// <summary>
        /// 初始化未定义的 <see cref="JavaScriptValue"/> 实例。
        /// </summary>
        public JavaScriptValue()
            : this(JavaScriptValueType.Undefined) { }

        /// <summary>
        /// 以布尔值初始化 <see cref="JavaScriptValue"/> 实例。
        /// </summary>
        /// <param name="value">布尔值。</param>
        public JavaScriptValue(bool value)
            : this(JavaScriptValueType.Bool, value) { }

        /// <summary>
        /// 以 32 位整数初始化 <see cref="JavaScriptValue"/> 实例。
        /// </summary>
        /// <param name="value">32 位整数。</param>
        public JavaScriptValue(int value)
            : this(JavaScriptValueType.Number, value) { }

        /// <summary>
        /// 以无符号 32 位整数初始化 <see cref="JavaScriptValue"/> 实例。
        /// </summary>
        /// <param name="value">无符号 32 位整数。</param>
        public JavaScriptValue(uint value)
            : this(JavaScriptValueType.Number, value) { }

        /// <summary>
        /// 以 64 位整数初始化 <see cref="JavaScriptValue"/> 实例。
        /// </summary>
        /// <param name="value">64 位整数。</param>
        public JavaScriptValue(long value)
            : this(JavaScriptValueType.Number, value) { }

        /// <summary>
        /// 以无符号 64 位整数初始化 <see cref="JavaScriptValue"/> 实例。
        /// </summary>
        /// <param name="value">无符号 64 位整数。</param>
        public JavaScriptValue(ulong value)
            : this(JavaScriptValueType.Number, value) { }

        /// <summary>
        /// 以 16 位整数初始化 <see cref="JavaScriptValue"/> 实例。
        /// </summary>
        /// <param name="value">16 位整数。</param>
        public JavaScriptValue(short value)
            : this(JavaScriptValueType.Number, value) { }

        /// <summary>
        /// 以无符号 16 位整数初始化 <see cref="JavaScriptValue"/> 实例。
        /// </summary>
        /// <param name="value">无符号 16 位整数。</param>
        public JavaScriptValue(ushort value)
            : this(JavaScriptValueType.Number, value) { }

        /// <summary>
        /// 以双精度浮点数初始化 <see cref="JavaScriptValue"/> 实例。
        /// </summary>
        /// <param name="value">双精度浮点数。</param>
        public JavaScriptValue(double value)
            : this(JavaScriptValueType.Number, value) { }

        /// <summary>
        /// 以单精度浮点数初始化 <see cref="JavaScriptValue"/> 实例。
        /// </summary>
        /// <param name="value">单精度浮点数。</param>
        public JavaScriptValue(float value)
            : this(JavaScriptValueType.Number, value) { }

        /// <summary>
        /// 以十进制数初始化 <see cref="JavaScriptValue"/> 实例。
        /// </summary>
        /// <param name="value">十进制数。</param>
        public JavaScriptValue(decimal value)
            : this(JavaScriptValueType.Number, value) { }

        /// <summary>
        /// 以字符串初始化 <see cref="JavaScriptValue"/> 实例。
        /// </summary>
        /// <param name="value">字符串。</param>
        public JavaScriptValue(string value)
            : this(JavaScriptValueType.String, value) { }

        /// <summary>
        /// 以日期时间初始化 <see cref="JavaScriptValue"/> 实例。
        /// </summary>
        /// <param name="value">日期时间。</param>
        public JavaScriptValue(DateTime value)
            : this(JavaScriptValueType.Date, value) { }

        /// <summary>
        /// 以指定类型与原始值初始化 <see cref="JavaScriptValue"/> 实例。
        /// </summary>
        /// <param name="valueType">值类型。</param>
        /// <param name="value">原始值;为 null 时使用该类型的默认值。</param>
        internal protected JavaScriptValue(JavaScriptValueType valueType, object? value = null)
        {
            switch (valueType)
            {
                case JavaScriptValueType.Undefined:
                    RawValue = null;
                    ValueType = JavaScriptValueType.Undefined;
                    break;
                case JavaScriptValueType.Null:
                    RawValue = null;
                    ValueType = JavaScriptValueType.Null;
                    break;
                case JavaScriptValueType.Bool:
                    ValueType = JavaScriptValueType.Bool;
                    RawValue = value ?? default(bool);
                    break;
                case JavaScriptValueType.Number:
                    ValueType = JavaScriptValueType.Number;
                    RawValue = value ?? 0;
                    break;
                case JavaScriptValueType.String:
                    ValueType = JavaScriptValueType.String;
                    RawValue = value ?? default(string);
                    break;
                case JavaScriptValueType.Date:
                    ValueType = JavaScriptValueType.Date;
                    RawValue = value ?? DateTime.Now;
                    break;
                case JavaScriptValueType.Object:
                    ValueType = JavaScriptValueType.Object;
                    break;
                case JavaScriptValueType.Array:
                    ValueType = JavaScriptValueType.Array;
                    break;
                case JavaScriptValueType.Function:
                    ValueType = JavaScriptValueType.Function;
                    JAVASCRIPT_VALUE_COLLECTION.Add(this);
                    break;
                case JavaScriptValueType.Property:
                    ValueType = JavaScriptValueType.Property;
                    JAVASCRIPT_VALUE_COLLECTION.Add(this);
                    break;
            }
        }

        /// <summary>
        /// 获取布尔值表示。
        /// </summary>
        /// <returns>布尔值;非布尔/数值类型时返回 false。</returns>
        public bool GetBoolean()
        {
            if (ValueType == JavaScriptValueType.Number)
            {
                var value = (int?)RawValue ?? 0;

                return value != 0;
            }

            if (ValueType == JavaScriptValueType.Bool)
            {
                var value = (bool?)RawValue ?? false;

                return value;
            }

            return false;
        }

        /// <summary>
        /// 获取双精度浮点数表示。
        /// </summary>
        /// <returns>双精度浮点数;非数值/布尔类型时返回默认值。</returns>
        public double GetDouble()
        {
            if (ValueType == JavaScriptValueType.Number)
            {
                if(RawValue is double || RawValue is float || RawValue is decimal)
                {
                    return (double)Convert.ChangeType(RawValue, TypeCode.Double);
                }

                return (double)(Convert.ChangeType(RawValue, TypeCode.Int32));
            }

            if (ValueType == JavaScriptValueType.Bool)
            {
                return GetBoolean() ? 1 : 0;
            }

            return default;
        }

        /// <summary>
        /// 获取单精度浮点数表示。
        /// </summary>
        /// <returns>单精度浮点数。</returns>
        public float GetFloat()
        {
            var value = GetDouble();

            return Convert.ToSingle(value);
        }

        /// <summary>
        /// 获取十进制数表示。
        /// </summary>
        /// <returns>十进制数。</returns>
        public decimal GetDecimal()
        {
            var value = GetDouble();

            return Convert.ToDecimal(value);
        }

        /// <summary>
        /// 获取 32 位整数表示。
        /// </summary>
        /// <returns>32 位整数。</returns>
        public int GetInt()
        {
            var value = GetDouble();

            return Convert.ToInt32(value);
        }

        /// <summary>
        /// 获取 64 位整数表示。
        /// </summary>
        /// <returns>64 位整数。</returns>
        public long GetBigInt()
        {
            var value = GetDouble();

            return Convert.ToInt64(value);
        }

        /// <summary>
        /// 获取日期时间表示。
        /// </summary>
        /// <returns>本地时间;无法解析时返回默认值。</returns>
        public DateTime GetDateTime()
        {
            if (ValueType == JavaScriptValueType.String)
            {
                var value = (string?)RawValue;

                if (value == null) return DateTime.Now;

                if (DateTime.TryParse(value, out var retval))
                {
                    retval = retval.ToLocalTime();
                    return retval;
                }
            }

            if (ValueType == JavaScriptValueType.Date)
            {
                return ((DateTime?)RawValue)?.ToLocalTime() ?? DateTime.Now;
            }

            return default;
        }

        /// <summary>
        /// 获取字符串表示。
        /// </summary>
        /// <returns>字符串表示;未定义/空类型时返回 null。</returns>
        public string? GetString()
        {
            switch (ValueType)
            {
                case JavaScriptValueType.Bool:
                case JavaScriptValueType.Number:
                    return $"{RawValue}";

                case JavaScriptValueType.Date:
                    return $"{GetDateTime()}";

                case JavaScriptValueType.Json:
                case JavaScriptValueType.String:
                    return $"{RawValue}";

                case JavaScriptValueType.Object:
                    return $"[object]";

                case JavaScriptValueType.Function:
                    return $"[function]";

                case JavaScriptValueType.Array:
                    return $"[array]";

                case JavaScriptValueType.Property:
                    return $"[property]";

                default:
                    return null;
            }
        }

        /// <summary>
        /// 释放资源(供子类重写)。
        /// </summary>
        /// <param name="isDisposing">是否由显式 Dispose 触发。</param>
        protected virtual void Dispose(bool isDisposing)
        {
        }

        /// <summary>
        /// 按唯一标识查找 JavaScript 值。
        /// </summary>
        /// <param name="uuid">值唯一标识。</param>
        /// <returns>匹配的值;不存在时为 null。</returns>
        public static JavaScriptValue? GetJavaScriptValue(Guid uuid)
        {
            return JAVASCRIPT_VALUE_COLLECTION.SingleOrDefault(x => x.Uuid == uuid);
        }

        /// <summary>
        /// 释放该值并从全局集合中移除。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            JAVASCRIPT_VALUE_COLLECTION.Remove(this);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 值在父级(数组或对象)中的名称。
        /// </summary>
        internal protected string Name
        {
            get
            {
                if (Parent is JavaScriptArray)
                {
                    var p = (JavaScriptArray)Parent;

                    var index = p.IndexOf(this);

                    return $"{index}";
                }
                else if (Parent is JavaScriptObject)
                {
                    var p = (JavaScriptObject)Parent;

                    var name = p.NameOf(this);

                    return name;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// 生成该值的值定义。
        /// </summary>
        /// <returns>承载该值元数据的值定义。</returns>
        internal virtual JavaScriptValueDefinition ToDefinition()
        {
            return new JavaScriptValueDefinition
            {
                Name = Name,
                Uuid = Uuid,
                ValueType = ValueType,
                ValueDefinition = RawValue
            };
        }

        /// <summary>
        /// 将该值序列化为 JSON。
        /// </summary>
        /// <returns>JSON 字符串。</returns>
        internal virtual string ToJson()
        {
            return JsonSerializer.Serialize(ToDefinition());
        }

        /// <summary>
        /// 从 JSON 反序列化出 JavaScript 值。
        /// </summary>
        /// <param name="json">JSON 字符串。</param>
        /// <returns>反序列化得到的值。</returns>
        internal static JavaScriptValue FromJson(string json)
        {
            return FromDefinition(JsonSerializer.Deserialize<JavaScriptValueDefinition>(json));
        }

        /// <summary>
        /// 从值定义构建 JavaScript 值。
        /// </summary>
        /// <param name="definition">值定义。</param>
        /// <returns>构建得到的值;定义或原始值为 null 时返回未定义值。</returns>
        internal static JavaScriptValue FromDefinition(JavaScriptValueDefinition? definition)
        {
            if (definition?.ValueDefinition == null)
            {
                return new JavaScriptValue();
            }

            var type = definition.ValueType;
            var def = (JsonElement)(definition.ValueDefinition ?? string.Empty);
            var uuid = definition.Uuid;
            JavaScriptValue? value = null;

            if (type == JavaScriptValueType.Property)
            {
                value = JavaScriptProperty.FromJson(def.GetRawText());
            }

            if (type == JavaScriptValueType.Function)
            {
                var funcDef = JavaScriptFunctionInvokerDefinition.FromJson(def.GetRawText());

                if (!funcDef.IsRenderer)
                {
                    if (funcDef.IsAsynchronous)
                    {
                        value = new JavaScriptFunctionInvoker
                        {
                            IsAsynchronous = true,
                            IsRenderer = false,
                        };
                    }
                    else
                    {
                        value = new JavaScriptFunctionInvoker
                        {
                            IsAsynchronous = false,
                            IsRenderer = false
                        };
                    }
                }
                else
                {
                    value = new JavaScriptFunctionInvoker
                    {
                        IsAsynchronous = false,
                        IsRenderer = true
                    };
                }
            }

            if (type == JavaScriptValueType.Json)
            {
                var json = def.GetString();

                if (json == null) return new JavaScriptValue(JavaScriptValueType.Null);
                value = new JavaScriptJsonValue(json);
            }

            if (type == JavaScriptValueType.Null)
            {
                value = new JavaScriptValue(JavaScriptValueType.Null);
            }

            if (type == JavaScriptValueType.Bool)
            {
                value = new JavaScriptValue(def.GetBoolean());
            }

            if (type == JavaScriptValueType.Number)
            {
                value = new JavaScriptValue(def.GetDouble());
            }

            if (type == JavaScriptValueType.String)
            {
                value = new JavaScriptValue(def.GetString() ?? string.Empty);
            }

            if (type == JavaScriptValueType.Date)
            {
                value = new JavaScriptValue(def.GetDateTime());
            }

            if (type == JavaScriptValueType.Object)
            {
                value = JavaScriptObject.FromJson(def.GetRawText());
            }

            if (type == JavaScriptValueType.Array)
            {
                value = JavaScriptArray.FromJson(def.GetRawText());
            }

            if (value == null)
            {
                value = new JavaScriptValue();
            }

            value.Uuid = uuid;

            return value;
        }
    }
}
