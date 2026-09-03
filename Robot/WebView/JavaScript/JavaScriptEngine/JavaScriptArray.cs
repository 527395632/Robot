// Robot 桌面软件 — JavaScript 数组值
// 表示 JavaScript 数组的 JavaScriptValue 子类,并实现 IList<JavaScriptValue>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 数组值:以列表形式承载多个 JavaScript 值,并实现 <see cref="IList{JavaScriptValue}"/>。
    /// </summary>
    public class JavaScriptArray : JavaScriptValue, IList<JavaScriptValue>
    {
        /// <summary>
        /// 数组内容列表。
        /// </summary>
        internal List<JavaScriptValue> Contents { get; } = new List<JavaScriptValue>();

        /// <summary>
        /// 是否只读(始终为 false)。
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// 元素数量。
        /// </summary>
        public int Count => Contents.Count;

        /// <summary>
        /// 初始化 <see cref="JavaScriptArray"/> 实例。
        /// </summary>
        public JavaScriptArray()
        : base(JavaScriptValueType.Array)
        {

        }

        /// <summary>
        /// 按索引访问元素。
        /// </summary>
        /// <param name="index">元素索引。</param>
        public JavaScriptValue this[int index]
        {
            get => Contents[index];
            set => Contents[index] = value;
        }


        /// <summary>
        /// 追加元素并返回当前数组(支持链式调用)。
        /// </summary>
        /// <param name="item">要追加的元素。</param>
        /// <returns>当前数组实例。</returns>
        public JavaScriptArray Add(JavaScriptValue item)
        {
            item.Parent = this;
            Contents.Add(item);

            return this;
        }

        /// <summary>
        /// 清空数组并解除所有元素的父引用。
        /// </summary>
        public void Clear()
        {
            Contents.ForEach(x => x.Parent = null);

            Contents.Clear();
        }

        /// <summary>
        /// 判断数组是否包含指定元素。
        /// </summary>
        /// <param name="item">要查找的元素。</param>
        /// <returns>包含返回 true,否则 false。</returns>
        public bool Contains(JavaScriptValue item)
        {
            return Contents.Contains(item);
        }

        /// <summary>
        /// 获取指定元素首次出现的索引。
        /// </summary>
        /// <param name="item">要查找的元素。</param>
        /// <returns>元素索引;未找到返回 -1。</returns>
        public int IndexOf(JavaScriptValue item)
        {
            return Contents.IndexOf(item);
        }

        /// <summary>
        /// 在指定索引处插入元素。
        /// </summary>
        /// <param name="index">插入位置索引。</param>
        /// <param name="item">要插入的元素。</param>
        public void Insert(int index, JavaScriptValue item)
        {
            item.Parent = this;
            Contents.Insert(index, item);
        }

        /// <summary>
        /// 移除指定元素并解除其父引用。
        /// </summary>
        /// <param name="item">要移除的元素。</param>
        /// <returns>移除成功返回 true,否则 false。</returns>
        public bool Remove(JavaScriptValue item)
        {
            item.Parent = null;

            return Contents.Remove(item);
        }

        /// <summary>
        /// 移除指定索引处的元素并解除其父引用。
        /// </summary>
        /// <param name="index">要移除的元素索引。</param>
        public void RemoveAt(int index)
        {

            Contents[index].Parent = null;

            Contents.RemoveAt(index);
        }

        /// <summary>
        /// 获取指定索引处的元素。
        /// </summary>
        /// <param name="index">元素索引。</param>
        /// <returns>指定索引处的元素。</returns>
        public JavaScriptValue GetValue(int index)
        {
            return Contents[index];
        }

        /// <summary>
        /// 显式实现 <see cref="ICollection{JavaScriptValue}.Add"/>:仅追加元素,不设置父引用。
        /// </summary>
        /// <param name="item">要追加的元素。</param>
        void ICollection<JavaScriptValue>.Add(JavaScriptValue item)
        {
            Contents.Add(item);
        }

        /// <summary>
        /// 将数组内容复制到目标数组。
        /// </summary>
        /// <param name="array">目标数组。</param>
        /// <param name="arrayIndex">目标数组起始索引。</param>
        public void CopyTo(JavaScriptValue[] array, int arrayIndex)
        {
            Contents.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 获取数组元素的枚举器。
        /// </summary>
        /// <returns>数组元素的枚举器。</returns>
        public IEnumerator<JavaScriptValue> GetEnumerator()
        {
            return Contents.GetEnumerator();
        }

        /// <summary>
        /// 显式实现 <see cref="IEnumerable.GetEnumerator"/>:返回非泛型枚举器。
        /// </summary>
        /// <returns>非泛型枚举器。</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Contents.GetEnumerator();
        }

        /// <summary>
        /// 转换为 JavaScript 值定义(递归转换各元素)。
        /// </summary>
        /// <returns>对应的 JavaScript 值定义。</returns>
        internal override JavaScriptValueDefinition ToDefinition()
        {
            return new JavaScriptValueDefinition
            {
                Name = Name,
                Uuid = Uuid,
                ValueType = ValueType,
                ValueDefinition = Contents.Select(x => x.ToDefinition()).ToList()
            };
        }

        /// <summary>
        /// 关联到目标帧(递归关联各元素)。
        /// </summary>
        /// <param name="frame">目标帧。</param>
        protected internal override void AssociateToFrame(CefFrame? frame)
        {
            base.AssociateToFrame(frame);

            foreach (var item in Contents)
            {
                item.AssociateToFrame(frame);
            }
        }

        /// <summary>
        /// 从 JSON 反序列化 JavaScript 数组。
        /// </summary>
        /// <param name="json">JSON 字符串。</param>
        /// <returns>反序列化得到的 JavaScript 数组;失败返回 null。</returns>
        internal static new JavaScriptArray? FromJson(string json)
        {
            return FromDefinition(JsonSerializer.Deserialize<List<JavaScriptValueDefinition>>(json));
        }

        /// <summary>
        /// 从 JavaScript 值定义列表构建 JavaScript 数组。
        /// </summary>
        /// <param name="definition">JavaScript 值定义列表。</param>
        /// <returns>构建得到的 JavaScript 数组;入参为 null 时返回 null。</returns>
        internal static JavaScriptArray? FromDefinition(List<JavaScriptValueDefinition>? definition)
        {
            if (definition == null)
            {
                return null;
            }

            var value = new JavaScriptArray();


            foreach (var item in definition)
            {
                value.Add(FromDefinition(item));
            }


            return value;
        }

    }


    /// <summary>
    /// JavaScript 数组扩展方法。
    /// </summary>
    public static class JavaScriptArrayExtension
    {
        /// <summary>
        /// 将 JavaScript 值转换为 JavaScript 数组。
        /// </summary>
        /// <param name="jsValue">要转换的 JavaScript 值。</param>
        /// <returns>转换得到的 JavaScript 数组。</returns>
        /// <exception cref="InvalidOperationException">入参不是 JavaScript 数组时抛出。</exception>
        public static JavaScriptArray ToArray(this JavaScriptValue jsValue)
        {
            if (jsValue != null && jsValue.ValueType == JavaScriptValueType.Array)
            {
                return (JavaScriptArray)jsValue;
            }

            throw new InvalidOperationException($"This is not a {nameof(JavaScriptArray)}.");
        }
    }
}
