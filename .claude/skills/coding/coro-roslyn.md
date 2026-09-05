# Coro.Roslyn：源码生成器 & 分析器

> 属于 `coding` 技能。`Coro.Roslyn`（`netstandard2.0`，`IsAnalyzerProject`）是随 `Coro` 包自动注入的 Roslyn 组件，包含 **3 个增量源码生成器**（`IIncrementalGenerator`）与 **4 个诊断分析器**（`DiagnosticAnalyzer`）。
>
> 涉及 `[Inject]` / `[ObservableProperty]` / `[Command]` / `[AsyncCommand]` 的生成代码，或遇到 `CORO10xx` 错误码时必读。

## 核心原则：目标类必须 `partial`

三个生成器都往 `partial` 分部类里追加成员，输出文件名为 `{Namespace}.{ClassName}.g.cs`。**使用了任何 Coro 特性（`Inject` / `ObservableProperty` / `Command` / `AsyncCommand`）的类必须声明为 `partial`**，否则触发 `CORO1001`（编译错误）。

```csharp
public partial class MainViewModel : ViewModel
{
    [Inject]
    private ILogger _logger;

    [ObservableProperty]
    private int _count;

    [Command]
    public void Save() { }
}
```

## 源码生成器（写什么 → 生成什么）

### 1. `[ObservableProperty]` → 公开属性

触发：带 `ObservableProperty` 前缀特性的**字段**。

对每个字段 `private int _count;`，在同名 `partial` 类中生成一个 PascalCase 公开属性（字段名 `ToPascalCase`，`_count` → `Count`）：

```csharp
public int Count
{
    get => _count;
    set
    {
        if (!object.Equals(_count, value))
        {
            _count = value;
            RaisePropertyChanged();
        }
    }
}
```

- 值相等不重复通知；字段上的其他特性（除 `Inject`/`ObservableProperty` 本身）会被保留到属性上。
- **不要直接读写定义字段 `_count`**（触发 `CORO1004`）。

### 2. `[Command]` / `[AsyncCommand]` → state + command 成员

触发：带 `Command` 或 `AsyncCommand` 前缀特性的**方法**。

对方法 `OnSave()`：先按规则取基准名——方法名以 `On` 开头且第 3 个字符是大写时去掉 `On`（`OnSave` → `Save`；`Save` → `Save`），再据此生成 4 个成员：

| 成员 | 命名 | 示例（方法 `OnSave`） |
|------|------|------|
| state 字段 | `_{camel}State` | `_saveState`（`bool`，初值 `true`） |
| state 属性 | `{Pascal}State` | `SaveState`（`public bool`，`set` 时值变更才 `RaisePropertyChanged`） |
| command 字段 | `_{camel}Command` | `_saveCommand`（`ICommand`） |
| command 属性 | `{Pascal}Command` | `SaveCommand`（`public ICommand`） |

command 属性为表达式体，惰性创建：

```csharp
public System.Windows.Input.ICommand SaveCommand
    => _saveCommand ??= new Coro.Command(this, nameof(OnSave), nameof(SaveState));
```

绑定：`Command` 构造参数为 `(this, nameof(方法), nameof(state属性))`。同步 / 异步执行、`AutoManageState` 语义见 `coro-core.md`。

- 命名冲突（4 个名字任一撞现有字段/属性/方法）触发 `CORO1008`。
- 方法须同步无返回值、所在类须继承 `ViewModelBasic`（`CORO1007` / `CORO1009`）。

### 3. `[Inject]` → 注入构造函数

触发：带 `Inject` 前缀特性的**字段或属性**。

收集类内所有被标记成员，生成一个 `[Inject]` 构造函数，把这些成员作为参数（参数名取成员名 `ToCamelCase`，并保留原特性），函数体逐一赋值：

```csharp
[Inject]
public MainViewModel(ILogger logger, IMyService svc)
{
    _logger = logger;
    _svc = svc;
}
```

- 成员上的 `/// <summary>` 注释会被搬到对应 `<param>`。
- 字段声明一次只允许一个变量（`CORO1002`）；多个注入成员转 camelCase 后不能重名（`CORO1003`）。

## 分析器 & 错误码（全部 `DiagnosticSeverity.Error`，分类 `Design`）

所有分析器均启用对生成代码的分析（`Analyze | ReportDiagnostics`），即生成代码也会被检查。

| 错误码 | 标题 | 触发条件 |
|--------|------|----------|
| `CORO1001` | 未启用分布类 | 含 Coro 特性的类不是 `partial` |
| `CORO1002` | 依赖注入不支持字段多参 | `[Inject]` 字段声明多个变量 |
| `CORO1003` | 依赖注入[字段/属性]名冲突 | 两个注入成员 camelCase 后同名 |
| `CORO1004` | 通知属性字段不可用 | 直接引用 `[ObservableProperty]` 的定义字段 |
| `CORO1005` | 通知属性字段必须是私有 | `[ObservableProperty]` 字段非 `private` |
| `CORO1006` | 通知属性类继承错误 | 含通知属性的类未继承 `ObservableObject` |
| `CORO1007` | 命令方法须同步无返回值 | `[Command]/[AsyncCommand]` 方法为 `async` 或返回 `Task` |
| `CORO1008` | 命令方法成员名称冲突 | 命令生成的成员名与现有成员重名 |
| `CORO1009` | 命令方法类继承错误 | 含命令方法的类未继承 `ViewModelBasic` |

排查思路：看到 `CORO10xx` 先按上表定位是哪类特性、哪个成员、缺 `partial` 还是命名冲突，再回到对应生成器规则修正。

## 命名辅助（内部工具）

- `StringExtensions.ToCamelCase / ToPascalCase`：把任意命名（下划线 / 连字符 / 空格分隔）归一化为小驼峰 / 帕斯卡。
- `SyntaxTokenExtensions.ToCamelCaseToken / ToPascalCaseToken`：在保留原有 Trivia 的前提下改写 token 标识符（`ToCamelCaseToken1` 额外加 `_` 前缀）。

## 使用清单（写 VM 时对照）

1. 类声明 `partial`，继承 `ViewModel`（WPF）或 `ViewModelBasic`。
2. 私有字段 + `[ObservableProperty]`；不要直接读写该字段。
3. 方法 + `[Command]`（同步）/ `[AsyncCommand]`（耗时），方法保持同步无返回值。
4. 字段 / 属性 + `[Inject]` 声明依赖；保证注入成员名 camelCase 后不冲突。
5. 编译后若报 `CORO10xx`，对照「错误码」表逐条修正。
6. 生成代码在 `obj/GenFiles`（`EmitCompilerGeneratedFiles` 已开启），可查看实际产物。
