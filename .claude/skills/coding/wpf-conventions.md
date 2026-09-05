# WPF 代码规范

> 属于 `coding` 技能。本项目 WPF 编码必须逐条遵循以下规则（均为强制项）。与 `csharp-conventions.md` 冲突时，本文件优先。

## 依赖属性（DependencyProperty）编写规范

1. **顺序**：包装属性（CLR 属性）在上，`DependencyProperty` 静态字段在下，**两者之间无空行**。

2. **注释**：只有包装属性加 XML 注释（格式 `获取或设置 + 说明`）；**DP 静态字段不加注释**。

3. **单行优先**：`DependencyProperty.Register` 调用**尽量单行完成**；仅当注册含**匿名函数且必须多行代码**时才允许换行（每参数一行）。

4. **匿名回调内联**：`PropertyMetadata` 的变更回调**尽可能在注册处内联为 lambda 实现**，不定义独立方法；仅当回调被**多处引用**时才提取为独立方法。

### 正确示例（普通 DP）

```csharp
/// <summary>
/// 获取或设置服务器地址。
/// </summary>
public string Endpoint
{
    get => (string)GetValue(EndpointProperty);
    set => SetValue(EndpointProperty, value);
}
public static readonly DependencyProperty EndpointProperty = DependencyProperty.Register(nameof(Endpoint), typeof(string), typeof(ChatSettingsControl), new PropertyMetadata(string.Empty));
```

### 正确示例（含匿名回调的附加属性）

```csharp
/// <summary>
/// 绑定 FlowDocument 到 RichTextBox.Document 的附加属性。
/// </summary>
public static readonly DependencyProperty RenderedDocumentProperty =
    DependencyProperty.RegisterAttached(
        "RenderedDocument",
        typeof(FlowDocument),
        typeof(ChatControl),
        new PropertyMetadata(null, (d, e) =>
        {
            if (d is RichTextBox box)
            {
                box.Document = e.NewValue as FlowDocument;
            }
        }));
```

### 错误示例

```csharp
// 错误：DP 字段在上、属性在下，且字段加了注释、多行参数
/// <summary>
/// 服务器地址。
/// </summary>
public static readonly DependencyProperty EndpointProperty =
    DependencyProperty.Register(
        nameof(Endpoint),
        typeof(string),
        typeof(ChatSettingsControl),
        new PropertyMetadata(string.Empty));

/// <summary>
/// 获取或设置服务器地址。
/// </summary>
public string Endpoint { get; set; }
```

## 命名约束

- 控件的自定义 DP 名称**不得与 `FrameworkElement` 既有成员冲突**（如 `Name`），使用语义化名称（如 `AgentName`）。
