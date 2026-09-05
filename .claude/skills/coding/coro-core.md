# Coro 核心库用法

> 属于 `coding` 技能。`Coro` 是整套套件的基础库（`net8.0` / `net10.0` 多目标），提供依赖注入容器、MVVM 基类、命令、日志与通用扩展。涉及容器 / ViewModel / 命令 / 日志 / 特性时必读。
>
> 编码风格仍遵循同目录 `csharp-conventions.md`（一类一文件、全成员 `///` 注释、枚举加 Description、`Nullable=disable` 等）。

## 套件三项目关系

| 项目 | 目标框架 | 角色 |
|------|----------|------|
| `Coro` | `net8.0` / `net10.0` | 核心库：DI、MVVM 基类、命令、日志、特性、扩展。NuGet 包，自动附带 Roslyn 组件 |
| `Coro.Roslyn` | `netstandard2.0` | 源码生成器 + 分析器。通过 `Coro` 的 `analyzers/dotnet/cs/` 路径随包注入 |
| `Coro.WPF` | `net8.0-windows` / `net10.0-windows` | WPF 应用框架：应用入口、路由、窗口、转换器。引用 `Coro` + `Coro.Roslyn`（Analyzer） |

依赖方向：`Coro.WPF` → `Coro`；`Coro` 打包时引用 `Coro.Roslyn`（作为分析器，`ReferenceOutputAssembly=false`）。

## 依赖注入容器（`IServiceContainer` / `ServiceContainer`）

容器同时实现 `IServiceProvider`。构造函数会自动注册自身为 `IServiceContainer` 与 `IServiceProvider` 的单例。

### 注册

```csharp
container.AddSingleton<IMyService, MyService>();   // 接口 → 实现
container.AddSingleton<MyService>(instance);       // 注册实例（强制 Singleton）
container.AddTransient<IMyService>(key: "alt", impl: typeof(MyAltService)); // 带键
container.AddSingleton(_routes);                   // 直接注册实例（如 List<RouteInfo>）
```

- `AddSingleton` / `AddTransient`：单例 / 瞬时。生命周期只有这两种（`ServiceLifetime` 中 Scope 已注释掉，不可用）。
- 可带 `string key` 区分同名服务的不同实现；无键时 key 为 `string.Empty`。
- **接口类型必须给实现类型或实例**，否则 `OnRegister` 抛 `ArgumentNullException`。
- 注册实例（传 `instance`）时自动按 `Singleton` 处理，且实现类型取实例的实际类型。
- 重复注册同一（key, serviceType）会先移除旧的再注册（旧的若 `IDisposable` 会被 Dispose）。

### 获取

```csharp
var svc = container.GetService<IMyService>();          // 无则返回 default（null）
var req = container.GetRequiredService<IMyService>();  // 无则抛 NullReferenceException
var keyed = container.GetService<IMyService>(key: "alt");
```

### 生命周期行为

- `Singleton`：首次获取时惰性创建并缓存（带双重 `lock` 检查）。
- `Transient`：每次获取都新建一个实例。
- 获取的构造通过 `CreateObject` 完成（见下）。

### 实例化与构造注入（`CreateObject`）

`CreateObject(type, argument, failThrow)` 负责反射实例化：

- 优先选择带 `[Inject]` 特性的构造函数；没有则取第一个构造函数；都没有则抛 `EntryPointNotFoundException`。
- 每个构造参数按以下顺序取值：
  1. 若传入了 `argument` 且其属性名与参数名匹配 → 取该属性值；
  2. 否则看参数的 `[Inject]` 特性：先按名称从 `argument` 属性取，再从容器 `GetService(name, paramType)` 取；
  3. 值类型取不到则 `Activator.CreateInstance` 给默认值；
  4. 仍为 `null` 且 `[Inject].IsRequired == true` → 抛 `ArgumentNullException`。
- `failThrow=true`：构造失败直接抛异常（路由取 VM / View 时用）；`failThrow=false`：失败时通过 `ILogger` 记录并返回 `null`。

### 移除与释放

- `Remove(type)` / `Remove<T>()` / `Remove(key, type)` / `Remove<T>(key)`：移除并 Dispose 其实例。
- `ServiceContainer.Dispose()`：遍历所有描述符，Dispose 除自身外的可释放实例。

## MVVM 基类

### `ObservableObject`（`INotifyPropertyChanged`）

- 事件 `OnPropertyChanged`：`EventHandler<string>`，属性变更后触发（区别于框架的 `PropertyChanged`）。
- `RaisePropertyChanged([CallerMemberName])`：同时触发 `PropertyChanged` 与 `OnPropertyChanged`。

### `ViewModelBasic : ObservableObject`

所有 ViewModel 的基类。

- `Container`：`IServiceContainer`，在 `OnCreating` 中由框架注入（只读属性，`private set`）。
- 生命周期虚方法（按需重写）：

  | 方法 | 触发时机 |
  |------|----------|
  | `OnCreating(container, view)` | VM 创建时最先调用，`Container` 在此赋值 |
  | `OnCreated()` | View 加载完成 |
  | `OnShow()` / `OnHide()` | 视图显示 / 隐藏 |
  | `OnDestroy()` | 视图销毁 |

- `UIThread(Action)` / `UIThread<T>(Func<T>)`：抽象方法，子类实现线程调度（WPF 版用 `Dispatcher`，见 `coro-wpf.md`）。
- 重写了 `RaisePropertyChanged`：默认走 `UIThread` 通知，保证属性变更在 UI 线程派发。

## 命令（`Command` / 特性）

命令是把 ViewModel 的某个方法包装成 `ICommand` 的机制。`Command` 类是 `sealed`，由源码生成器生成对应的 command/state 成员（见 `coro-roslyn.md`）。

### 同步命令 `[Command]`

```csharp
[Command]                 // 默认 AutoManageState = true
public void Save() { /* ... */ }

[Command(false)]          // 关闭自动状态管理
public void Load() { /* ... */ }
```

- 执行：在调用线程同步调用目标方法。
- `AutoManageState=true` 时：执行前把对应 state 属性置 `false`（命令不可执行），结束后（含异常）恢复 `true`。
- 参数解析：方法参数若带 `[Inject]` 则从容器注入；否则若调用参数是 `IList` 则按位取；否则整体作为参数传入；支持枚举字符串解析与 `Convert.ChangeType` 转换。
- 参数绑定失败：记录 `ILogger.Error`，`AutoManageState` 时恢复状态并继续抛出。

### 异步命令 `[AsyncCommand]`

```csharp
[AsyncCommand]            // 默认 AutoManageState = true
public void Export() { /* 耗时操作 */ }
```

- 执行：在**后台线程**（`Thread{IsBackground=true}`）中调用目标方法，异常经 `ILogger.Fatal` 记录。
- 其余状态 / 参数行为与同步命令一致。

### 约束（由分析器强制，见 `coro-roslyn.md`）

- 命令方法必须是**同步、无返回值**（不能 `async`、不能返回 `Task`）——耗时逻辑用 `[AsyncCommand]` 而不是 `async` 方法。
- 命令方法所在类必须继承 `ViewModelBasic`。
- 生成的成员名（`XxxState` / `XxxCommand` / `_xxxState` / `_xxxCommand`）不得与类内已有字段 / 属性 / 方法重名。
- 方法名以 `On` 开头时会先去掉 `On` 前缀再命名（`OnSave` → `SaveState` / `SaveCommand`）。

## 通知属性 `[ObservableProperty]`

标记一个**私有字段**，源码生成器为其生成一个同名（PascalCase）的公开属性，`set` 时值变更才触发 `RaisePropertyChanged`（见 `coro-roslyn.md`）。

```csharp
public partial class MainViewModel : ViewModel
{
    [ObservableProperty]
    private int _count;
    // 生成：public int Count { get => _count; set { if (!Equals(_count, value)) { _count = value; RaisePropertyChanged(); } } }
}
```

约束：
- 字段必须 `private`；
- 所在类必须继承 `ObservableObject`；
- 定义字段不可直接访问（只能通过生成的属性）；
- 所在类必须 `partial`。

## 依赖注入特性 `[Inject]`

可作用于 **构造函数 / 属性 / 字段 / 参数**。

- 作用于**字段 / 属性**：源码生成器收集这些成员，生成一个 `[Inject]` 构造函数，把这些成员作为参数并赋值（见 `coro-roslyn.md`）。
- 作用于**构造参数**：容器实例化时按特性 `Name` / `IsRequired` 从容器取值。
- 属性：`Name`（依赖名，对应容器 key，空串表示默认）、`IsRequired`（取不到时是否抛异常）。

```csharp
public partial class MainViewModel : ViewModel
{
    [Inject]                 // 默认 Name="", IsRequired=false
    private ILogger _logger;

    [Inject("alt", true)]    // 指定 key，且必须
    private IMyService _svc;
}
```

约束：
- 字段声明一次只能一个变量（`[Inject] A _a, B _b;` 不允许）；
- 多个注入成员名（转 camelCase 后）不能冲突；
- 所在类必须 `partial`。

## 日志（`ILogger` / `LoggerBasic` / `FileLogger` / `ProxyLogger`）

- `LogLevel`：`Debug=1 / Info=2 / Warning=4 / Error=8 / Fatal=16`（位掩码，可与 `Level` 组合）。
- `ILogger`：`Debug / Info / Warning / Error / Fatal`（均有 `(format, args)` 与 `Error/Fatal(Exception)` 重载），`Append` / `AppendLine`，以及 `Level` 属性。
- `LoggerBasic`：抽象基类，默认 `Level = (LogLevel)31`（全开）。`Append` 按 `Level & level == level` 过滤，格式 `【level -> HH:mm:ss.fff】\t内容`，最终调抽象 `OnWrite(time, level, message)`。
- `FileLogger`：写到 `LogDirectory`（默认 `<BaseDirectory>/Logs`）下的 `yyyy-MM-dd/HH.log`，追加模式。
- `ProxyLogger`：构造传入 `Action<string>`，`OnWrite` 时回调（便于接到控制台 / 其他输出）。

用法：
```csharp
container.AddSingleton<ILogger, FileLogger>();   // 或 new ProxyLogger(s => Console.WriteLine(s))
var logger = container.GetService<ILogger>();
logger.Info("启动, 参数 {0}", arg);
logger.Error(ex);
```

## 异常（`MessageException` + `MessageMode`）

用于携带「展示级别」的业务异常，供上层统一弹窗提示。

```csharp
public enum MessageMode { Info, Success, Warning, Error }
public class MessageException : Exception
{
    public MessageException(MessageMode mode, string message) : base(message) { Mode = mode; }
    public MessageMode Mode { get; }
}
```

用法：`throw new MessageException(MessageMode.Error, "保存失败");`，由 UI 层捕获并据 `Mode` 选择提示样式。

## 通用扩展（C# 14 `extension` 原生扩展方法）

项目使用 C# 14 的 `extension` 修饰符声明扩展方法（而非传统 `static` 扩展类写法）。

- `CollectionExtensions`：`IList<T>.AddRange(IEnumerable<T>)`。
- `EnumExtensions`：`Enum.GetDescription()`（读成员 `[Description]`）、`Enum.GetItems()`（全部成员描述文案列表）。

```csharp
extension(Enum source)
{
    public string GetDescription() { /* 读 [Description] */ }
    public IList<string> GetItems() { /* 全部成员文案 */ }
}
```

## 快速对照：写一个 ViewModel 时用什么

| 需求 | 用什么 |
|------|--------|
| 属性要驱动 UI | 私有字段 + `[ObservableProperty]` |
| 按钮 / 事件绑定 | 方法 + `[Command]`（同步）或 `[AsyncCommand]`（耗时） |
| 拿依赖（日志 / 服务） | 字段 / 属性 + `[Inject]` |
| 记录日志 | 注入或 `Container.GetService<ILogger>()` |
| 抛业务异常 | `MessageException(MessageMode, msg)` |
| 类标记 | `partial`，继承 `ViewModel`（WPF）或 `ViewModelBasic` |
