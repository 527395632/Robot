// Robot 桌面软件 — JavaScript 对象
// 以键值对形式承载 JavaScript 对象的属性,支持定义属性、添加函数与字典式访问

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 对象:以键值对形式承载 JavaScript 对象的属性,支持定义属性、添加函数与字典式访问。
    /// </summary>
    public class JavaScriptObject : JavaScriptValue, IDictionary<string, JavaScriptValue>
    {
        /// <summary>
        /// 属性键值对存储。
        /// </summary>
        internal Dictionary<string, JavaScriptValue> Contents { get; } = new Dictionary<string, JavaScriptValue>();

        /// <summary>
        /// 按名称访问属性。
        /// </summary>
        /// <param name="key">属性名称。</param>
        public JavaScriptValue this[string key]
        {
            get => Contents[key];
            set
            {
                var item = Contents[key] = value;
                item.Parent = this;
            }
        }

        /// <summary>
        /// 定义一个带访问器/设置器的属性。
        /// </summary>
        /// <param name="name">属性名称。</param>
        /// <param name="getter">属性读取委托。</param>
        /// <param name="setter">属性写入委托;为 null 时只读。</param>
        /// <returns>当前对象实例(支持链式调用)。</returns>
        public JavaScriptObject DefineProperty(string name, Func<JavaScriptValue> getter, Action<JavaScriptValue>? setter = null)
        {
            var prop = new JavaScriptProperty()
            {
                Getter = getter,
                Setter = setter,
                Parent = this
            };
            Contents.Add(name, prop);
            return this;
        }

        /// <summary>
        /// 添加一个普通属性值。
        /// </summary>
        /// <param name="key">属性名称。</param>
        /// <param name="value">属性值。</param>
        /// <returns>当前对象实例(支持链式调用)。</returns>
        public JavaScriptObject Add(string key, JavaScriptValue value)
        {
            value.Parent = this;
            Contents.Add(key, value);
            return this;
        }

        /// <summary>
        /// 添加一个异步函数属性。
        /// </summary>
        /// <param name="key">属性名称。</param>
        /// <param name="promiseDelegate">异步函数委托。</param>
        /// <returns>当前对象实例(支持链式调用)。</returns>
        public JavaScriptObject Add(string key, Action<JavaScriptArray, JavaScriptPromise> promiseDelegate)
        {
            return Add(key, new JavaScriptAsynchronousFunction(promiseDelegate) { Parent = this });
        }

        /// <summary>
        /// 添加一个同步函数属性。
        /// </summary>
        /// <param name="key">属性名称。</param>
        /// <param name="functionDelegate">同步函数委托。</param>
        /// <returns>当前对象实例(支持链式调用)。</returns>
        public JavaScriptObject Add(string key, Func<JavaScriptArray, JavaScriptValue?> functionDelegate)
        {
            return Add(key, new JavaScriptSynchronousFunction(functionDelegate) { Parent = this });
        }

        /// <summary>
        /// 判断是否包含指定名称的属性。
        /// </summary>
        /// <param name="key">属性名称。</param>
        /// <returns>包含时返回 true。</returns>
        public bool ContainsKey(string key)
        {
            return Contents.ContainsKey(key);
        }

        /// <summary>
        /// 移除指定名称的属性。
        /// </summary>
        /// <param name="key">属性名称。</param>
        /// <returns>移除成功时返回 true。</returns>
        public bool Remove(string key)
        {
            Contents[key].Parent = null;

            return Contents.Remove(key);
        }

        /// <summary>
        /// 获取指定名称的属性值。
        /// </summary>
        /// <param name="key">属性名称。</param>
        /// <returns>该属性对应的值。</returns>
        public JavaScriptValue GetValue(string key)
        {
            return Contents[key];
        }

        /// <summary>
        /// 尝试获取指定名称的属性值。
        /// </summary>
        /// <param name="key">属性名称。</param>
        /// <param name="value">获取到的属性值;不存在时为 null。</param>
        /// <returns>获取成功时返回 true。</returns>
        public bool TryGetValue(string key, out JavaScriptValue value)
        {
            return Contents.TryGetValue(key, out value!);
        }

        /// <summary>
        /// 清空所有属性。
        /// </summary>
        public void Clear()
        {
            Contents.Values.ToList().ForEach(x => x.Parent = null);

            Contents.Clear();
        }

        /// <summary>
        /// 获取指定属性值在本对象中的名称。
        /// </summary>
        /// <param name="item">待查找的属性值。</param>
        /// <returns>该属性值对应的名称。</returns>
        /// <exception cref="IndexOutOfRangeException">该值不属于本对象时抛出。</exception>
        public string NameOf(JavaScriptValue item)
        {
            var idx = Contents.Values.ToList().IndexOf(item);

            if (idx >= 0)
            {
                return Contents.Keys.ToList()[idx];
            }

            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// 所有属性名称。
        /// </summary>
        public IEnumerable<string> PropertyNames => Keys;

        /// <summary>
        /// 所有属性值。
        /// </summary>
        public IEnumerable<JavaScriptValue> PropertySymbols => Values;

        /// <summary>
        /// 属性数量。
        /// </summary>
        public int Length => Contents.Count;

        /// <summary>
        /// 所有属性名称集合。
        /// </summary>
        public ICollection<string> Keys => Contents.Keys;

        /// <summary>
        /// 所有属性值集合。
        /// </summary>
        public ICollection<JavaScriptValue> Values => Contents.Values;

        /// <summary>
        /// 属性数量。
        /// </summary>
        public int Count => Contents.Count;

        /// <summary>
        /// 是否只读(始终为 false)。
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// 初始化 <see cref="JavaScriptObject"/> 实例。
        /// </summary>
        public JavaScriptObject()
        : base(JavaScriptValueType.Object) { }

        /// <summary>
        /// 显式实现 IDictionary 的添加方法(不设置父引用)。
        /// </summary>
        /// <param name="key">属性名称。</param>
        /// <param name="value">属性值。</param>
        void IDictionary<string, JavaScriptValue>.Add(string key, JavaScriptValue value)
        {
            Contents.Add(key, value);
        }

        /// <summary>
        /// 添加一个键值对。
        /// </summary>
        /// <param name="item">待添加的键值对。</param>
        public void Add(KeyValuePair<string, JavaScriptValue> item)
        {
            Contents.Add(item.Key, item.Value);
        }

        /// <summary>
        /// 判断是否包含指定键值对。
        /// </summary>
        /// <param name="item">待查找的键值对。</param>
        /// <returns>键与值均匹配时返回 true。</returns>
        public bool Contains(KeyValuePair<string, JavaScriptValue> item)
        {
            return Contents.ContainsKey(item.Key) && Contents.ContainsValue(item.Value);
        }

        /// <summary>
        /// 将键值对复制到数组(未实现)。
        /// </summary>
        /// <param name="array">目标数组。</param>
        /// <param name="arrayIndex">起始索引。</param>
        /// <exception cref="NotImplementedException">始终抛出。</exception>
        public void CopyTo(KeyValuePair<string, JavaScriptValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 移除指定键值对。
        /// </summary>
        /// <param name="item">待移除的键值对。</param>
        /// <returns>移除成功时返回 true。</returns>
        public bool Remove(KeyValuePair<string, JavaScriptValue> item)
        {
            return Contents.Remove(item.Key);
        }

        /// <summary>
        /// 获取键值对枚举器。
        /// </summary>
        /// <returns>键值对枚举器。</returns>
        public IEnumerator<KeyValuePair<string, JavaScriptValue>> GetEnumerator()
        {
            return Contents.GetEnumerator();
        }

        /// <summary>
        /// 获取非泛型枚举器。
        /// </summary>
        /// <returns>枚举器。</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Contents.GetEnumerator();
        }

        /// <summary>
        /// 生成该对象各属性的值定义。
        /// </summary>
        /// <returns>承载该对象元数据的值定义。</returns>
        internal override JavaScriptValueDefinition ToDefinition()
        {
            return new JavaScriptValueDefinition
            {
                Name = Name,
                Uuid = Uuid,
                ValueType = ValueType,
                ValueDefinition = Contents.ToDictionary(k => k.Key, v => v.Value.ToDefinition())
            };
        }

        /// <summary>
        /// 将该对象及其所有属性关联到指定帧。
        /// </summary>
        /// <param name="frame">目标帧;为 null 时解除关联。</param>
        protected internal override void AssociateToFrame(CefFrame? frame)
        {
            base.AssociateToFrame(frame);

            foreach (var item in Contents.Values)
            {
                item.AssociateToFrame(frame);
            }
        }

        /// <summary>
        /// 从 JSON 反序列化出 JavaScript 对象。
        /// </summary>
        /// <param name="json">JSON 字符串。</param>
        /// <returns>反序列化得到的对象;JSON 为 null 时返回 null。</returns>
        internal static new JavaScriptObject? FromJson(string json)
        {
            return FromDefinition(JsonSerializer.Deserialize<Dictionary<string, JavaScriptValueDefinition>>(json));
        }

        /// <summary>
        /// 从值定义字典构建 JavaScript 对象。
        /// </summary>
        /// <param name="definition">属性值定义字典。</param>
        /// <returns>构建得到的对象;入参为 null 时返回 null。</returns>
        internal static JavaScriptObject? FromDefinition(Dictionary<string, JavaScriptValueDefinition>? definition)
        {
            if (definition == null)
            {
                return null;
            }

            var value = new JavaScriptObject();

            foreach (var kv in definition)
            {
                value.Add(kv.Key, FromDefinition(kv.Value));
            }

            return value;
        }
    }
}
