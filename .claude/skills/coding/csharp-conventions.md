# C# 通用代码规范

> 属于 `coding` 技能。本项目 C# 编码必须逐条遵循以下规则（均为强制项）。

## 强制规则

1. **可空引用类型关闭**：项目 `.csproj` 中必须设置 `<Nullable>disable</Nullable>`，禁止 `enable`。
   ```xml
   <PropertyGroup>
     <Nullable>disable</Nullable>
   </PropertyGroup>
   ```
2. **一类一文件**：每个类单独保存为一个 `.cs` 文件，文件名与主类型同名（`XXX` → `XXX.cs`）, 如果是分布类则 (`XXX` → `XXX.分布类功能.cs`)。一个文件不混放多个顶层类型。
3. **全成员注释**：类中的所有信息都必须加注释——类本身、每个属性、每个字段、每个方法、方法参数、返回值、可能抛出的异常，缺一不可。
4. **XML 文档注释**：必须使用 VS 能识别的 `///` XML 文档注释，禁止用普通 `//` 代替成员注释。
5. **XML 注释格式**（详见下方 <注释规范> 章节）：
   - **`<summary>` 换行**：`/// <summary>` 标签独占一行，说明文字写在下一行，`/// </summary>` 再独占一行。不得把 `<summary>文字</summary>` 挤在同一行。
   - **`<param>` / `<returns>` / `<exception>` 要尽可能的单行, 除非内容为多行**：开始标签、说明文字、结束标签全部写在同一行（`/// <param name="x">文字。</param>`）
   - **方法内注释**：方法体内的说明性注释，单行用 `//`，多行用 `/* ... */`，不得用 `///`。
6. **枚举必加 Description**：每个枚举的每个成员都必须用 `[Description("...")]` 修饰。
7. **特性换行**：特性（`[...]`）必须写在被修饰成员的**独立行**上，禁止与成员声明挤在同一行；多个特性时每个特性各自独占一行，全部置于成员声明之前。
8. **Properties**: 如果项目存在 <AssemblyInfo.cs> 文件, 则必须放入此文件夹

## 特性写法（独占行，不与成员同行）

```csharp
// 正确：特性各占一行，成员声明另起一行
[Command]
public void Save() { }

[Inject]
private ILogger _logger;

// 错误：特性与成员写在同一行
[Command] public void Save() { }
[Inject] private ILogger _logger;
```

## 注释规范

### 标签格式总览

| 标签 | 格式 | 示例 |
|------|------|------|
| `<summary>` | **三行换行式** | `/// <summary>` / `/// 说明文字。` / `/// </summary>` |
| `<param>` | **尽可能单行式, 除非内容为多行** | `/// <param name="no">订单编号。</param>` |
| `<returns>` | **尽可能单行式, 除非内容为多行** | `/// <returns>新建的订单实例。</returns>` |
| `<exception>` | **尽可能单行式, 除非内容为多行** | `/// <exception cref="ArgumentException">订单编号为空时抛出。</exception>` |

原则一句话：**只有 `<summary>` 换行，`<param>` / `<returns>` / `<exception>` 等之类的如果内容不换行, 必须单行显示**

### 成员注释

类、属性、字段、方法、构造方法、事件、枚举的 `<summary>` 一律三行式：

```csharp
/// <summary>
/// 订单金额。
/// </summary>
public decimal Amount { get; set; }
```

### 方法注释（完整示例）

标签顺序固定：`<summary>` → `<param>`（一个参数一行，可多个）→ `<returns>` → `<exception>`（可多个，仅当方法可能抛出异常时写）。

```csharp
/// <summary>
/// 新建订单。
/// </summary>
/// <param name="no">订单编号。</param>
/// <param name="amount">订单金额。</param>
/// <returns>新建的订单实例。</returns>
/// <exception cref="ArgumentException">订单编号为空时抛出。</exception>
public static Order Create(string no, decimal amount)
{
    // 单行注释：校验入参
    if (string.IsNullOrWhiteSpace(no))
    {
        throw new ArgumentException("订单编号不能为空", nameof(no));
    }

    /* 多行注释：
     * 校验通过后创建订单实例并返回。 */
    return new Order { OrderNo = no, Amount = amount };
}
```

### 方法内注释

- 单行说明用 `//`，写在语句上方独立成行。
- 多行说明用 `/* ... */`。
- 方法体内一律不得使用 `///`。

### 错误对照

- 错误：`<summary>` 与说明文字挤在同一行（不得写成一行）：
  ```csharp
  /// <summary>订单金额。</summary>
  ```
- 错误：`<param>` / `<returns>` / `<exception>` 写成换行式：
  ```csharp
  /// <param name="no">
  /// 订单编号。
  /// </param>
  ```
- 错误：方法体内用 `///` 写说明注释：
  ```csharp
  public void Save()
  {
      /// 保存文件。   ← 错误，方法内不得用 ///
      SaveFile();
  }
  ```

## 完整示例：类（一类一文件 + 全注释 + Description）

```csharp
using System.ComponentModel;

/// <summary>
/// 订单实体。
/// </summary>
public class Order
{
    /// <summary>
    /// 订单编号。
    /// </summary>
    public string OrderNo { get; set; }

    /// <summary>
    /// 订单金额。
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 订单状态。
    /// </summary>
    public OrderStatus Status { get; set; }
}
```

## 完整示例：枚举（每个成员加 Description）

```csharp
using System.ComponentModel;

/// <summary>
/// 订单状态。
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// 草稿。
    /// </summary>
    [Description("草稿")]
    Draft = 0,

    /// <summary>
    /// 已提交。
    /// </summary>
    [Description("已提交")]
    Submitted = 1,

    /// <summary>
    /// 已完成。
    /// </summary>
    [Description("已完成")]
    Completed = 2
}
```

- 每个枚举值都要有 `[Description("显示文案")]`（用于下拉、显示名等）。
- 枚举类型本身与每个值都要有换行式 `/// <summary>` 注释。
