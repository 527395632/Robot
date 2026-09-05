# Coro.WPF：应用框架用法

> 属于 `coding` 技能。`Coro.WPF`（`net8.0-windows` / `net10.0-windows`，`UseWPF`）在 `Coro` 之上提供 WPF 应用框架：应用入口、视图模型基类、约定式路由、自定义窗口、常用转换器。引用 `Coro` + `Coro.Roslyn`（作为 `Analyzer`）。
>
> 涉及 WPF 应用入口 / 路由 / 窗口 / 转换器 / 新建页面时必读。XAML 与 UI 编码风格另见同目录 `wpf-conventions.md`。

## 项目定位

- `CoroApplication`：应用入口（继承 `System.Windows.Application`）。
- `ViewModel`：WPF 视图模型基类（继承 `Coro.ViewModelBasic`，实现 `IRouteContext`）。
- 路由：`RouteAttribute` / `IRouteRegister` / `RouteRegister` / `RouteInfo` / `IRouteContext` / `RouteRegisterExtensions.UseMvvmRouter`。
- 窗口：`IWindow` / `UIWindow` / `WindowMode` / `UIWindowState` + Win32 亚克力。
- 主题 / 资源：`Themes/Generic.xaml`（WPF 自动加载）、`Themes/Theme.主题.xaml`（颜色主题，`App.xaml` 显式合并）+ Token / 样式字典（详见「主题与资源系统」）。
- 转换器：`Coro.WPF.Converters` 下一组 `IValueConverter`。
- `AssemblyInfo.cs` 声明了 `ThemeInfo(SourceAssembly)` 与 `XmlnsDefinition`，把 `Coro` / `Coro.WPF` / `Coro.WPF.Component` 映射到标准 presentation 命名空间，XAML 中可直接使用这些类型。

## 应用入口（`CoroApplication`）

应用入口**必须**用 `App.xaml`，且 **XAML 根元素写成 `CoroApplication`**（不是 `<Application>`）——这样 XAML 编译器生成的分部类 `App` 基类就是 `CoroApplication`，code-behind 才能 override 框架虚方法（若用 `<Application>` 根，生成基类是 `System.Windows.Application`，与 `CoroApplication` 冲突，报 CS0263）：

```xml
<CoroApplication x:Class="Coro.Design.App"
                 xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <CoroApplication.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="/Coro.WPF;component/Themes/Generic.xaml"/>
                <ResourceDictionary Source="/Coro.WPF;component/Themes/Theme.主题.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </CoroApplication.Resources>
</CoroApplication>
```

`App.xaml.cs` 声明 `public partial class App`（**不写基类**，基类由 XAML 根提供），在其中 override 下面的虚方法。csproj 需 `OutputType=WinExe`、**不设 `StartupObject`**（`App.xaml` 作为 ApplicationDefinition 生成入口）。

- `Container`：`public IServiceContainer`（`ServiceContainer` 实例），全应用共享。
- 启动流程（`OnStartup` 内，按序）：
  1. 挂三个全局异常处理器（`DispatcherUnhandledException` / `AppDomain.UnhandledException` / `TaskScheduler.UnobservedTaskException`）→ 统一 `ILogger.Fatal`。
  2. 注册默认服务：`ILogger → FileLogger`（Singleton）、`IRouteRegister → RouteRegister`（Singleton）、`IWindow → UIWindow`（Transient）。
  3. 调 `RouteRegister(...)` 与 `ConfigureServices(Container)`。
  4. 按 `DefaultRoute`（默认 `"AppMain"`）取视图，包进 `IWindow`，`SetWindow`，`Show`。
- 退出：`OnExit` 调 `Container.Dispose()`。

**可重写点（按需 override）：**

| 成员 | 默认 | 用途 |
|------|------|------|
| `DefaultRoute` | `"AppMain"` | 主窗口内容对应的路由 |
| `RouteRegister(IRouteRegister)` | `UseMvvmRouter(typeof(CoroApplication).Assembly)`（= Coro.WPF 程序集） | **必须 override**，改扫本应用程序集 `Assembly.GetExecutingAssembly()` |
| `ConfigureServices(IServiceContainer)` | 空 | 注册你自己的服务（在此 `AddSingleton` / `AddTransient`） |
| `SetWindow(IWindow)` | TitleBar + 居中 + 可缩放 + 正常 | 自定义主窗口外观 |

`App.xaml.cs`（`public partial class App`，基类来自 XAML 根）中按需 override：

```csharp
public partial class App
{
    protected override string DefaultRoute => "AppMain";

    protected override void RouteRegister(IRouteRegister routeRegister)
    {
        routeRegister.UseMvvmRouter(Assembly.GetExecutingAssembly());
    }

    protected override void ConfigureServices(IServiceContainer services)
    {
        services.AddSingleton<IMyService, MyService>();
    }
}
```

## 视图模型（`ViewModel : ViewModelBasic, IRouteContext`）

- `View`：`FrameworkElement`（`private set`），`OnCreating` 时由框架赋值为绑定的视图。
- `Logger`：`ILogger`（`Container?.GetService<ILogger>()`）。
- 生命周期（框架自动接线，重写虚方法即可，无需手动订阅事件）：

  | 虚方法 | 触发 |
  |--------|------|
  | `OnCreating(container, view)` | VM 创建、View 已关联（`Container` / `View` 已就绪） |
  | `OnCreated()` | View 首次 `Loaded` |
  | `OnShow()` / `OnHide()` | View `IsVisibleChanged` 变 `true` / `false` |
  | `OnDestroy()` | 若 View 是 `Window` 则其 `Closed`；否则 `Unloaded` |

- `OnRouterArgument(object argument)`：路由跳转携带参数时回调（`ViewModel` 默认空实现，按需重写接收参数）。
- `UIThread`：`ViewModel` 用 `View.Dispatcher.Invoke` 实现，`RaisePropertyChanged` 自动在 UI 线程派发。

## 路由（约定式 MVVM Router）

- `RouteAttribute(url)`：标在 ViewModel 上声明其路由地址（特性独占一行，见 `csharp-conventions.md`）。
- 约定：`UseMvvmRouter(assembly)` 扫描程序集内所有**具体** `ViewModel` 子类，按命名规则配对 View：
  - VM 名去掉 `ViewModel` 或 `VM` 后缀得基准名（`SettingsViewModel` / `SettingsVM` → `Settings`）。
  - 在同程序集、命名空间 `...ViewModels` → `...Views` 下找 `{基准名}View`。
  - 读 VM 上的 `[Route]` 地址，非空则 `Map(url, view, viewModel)`。
- `IRouteRegister` 三个 `Map` 重载：`Map(url, viewType, vmType)`、`Map<TView,TViewModel>(url)`、`Map<TView,TViewModel>()`（按 VM 的 `[Route]` 取地址）。
- `RouteRegister`：构造注入 `IServiceContainer`，把 `List<RouteInfo>` 以单例注册；`RouteInfo(url, viewType, viewModelType)`。

**取视图（`ServiceContainerExtensions`，`IServiceContainer` 扩展）：**

```csharp
var view = container.GetView<SettingsView>("Settings");              // 按路由地址
var view2 = container.GetView<SettingsView>("Settings", arg);       // 带参数 → 回调 OnRouterArgument
var view3 = container.GetView<SettingsView>();                      // 按视图类型
```

- `GetView<T>(url, argument)`：查路由 → `CreateObject` 建 VM（失败抛异常）→ `OnRouterArgument(argument)` → 建 View、设 `DataContext`、调 `OnCreating`。
- 另有 `Inject(object)`：对已有实例按 `[Inject]` 手动注入字段 / 属性。

## 转换器（`Coro.WPF.Converters`）

| 转换器 | 作用 |
|--------|------|
| `BooleanReverseConverter` | `bool` → 取反 |
| `BoolToVisibilityConverter` | `null`→Collapsed；`bool`→自身；`string`→非空 Visible；`IList`→非空 Visible；其他→非 null Visible。参数 `"R"` 取反 |
| `ColorToBrushConverter` | `Color` → 冻结的 `SolidColorBrush` |
| `CornerRadiusToDoubleConverter` | `CornerRadius` → `TopLeft` 值 |
| `DivideConverter` | `double total` → `total / Parts - Subtract`（`Parts` 默认 5、`Subtract` 默认 0.5，参数可覆盖 `Parts`） |
| `EnumConverter` | 枚举 → `[Description]` 文案；反向：文案 → 枚举值 |
| `EnumItemsConverter` | 枚举 → 全部成员描述文案列表（下拉项） |
| `ExpandToAngleConverter` | `bool` → `90`/`0` 角度（折叠 / 展开箭头旋转） |

## 新建一个功能页面（端到端步骤）

以「Settings」页为例，只需两个文件，路由自动注册：

1. 视图模型：命名空间 `...ViewModels`，类 `SettingsViewModel`，`partial`、继承 `ViewModel`。
   ```csharp
   [Route("Settings")]
   public partial class SettingsViewModel : ViewModel
   {
       [ObservableProperty]
       private bool _autoSave;

       [Command]
       public void Save()
       {
           Logger?.Info("autoSave = {0}", AutoSave.ToString());
       }
   }
   ```
2. 视图：命名空间 `...Views`，类 `SettingsView`（XAML），名字 = 基准名 + `View`。
3. 完成——`UseMvvmRouter` 启动时自动注册 `Settings → (SettingsView, SettingsViewModel)`，无需手写 `Map`。
4. 跳转：`container.GetView<SettingsView>("Settings")`（或带参 `(..., arg)`，参数经 `OnRouterArgument` 传入）。

要点核对（配合 `coro-core.md` / `coro-roslyn.md`）：
- 类 `partial`；特性各占一行。
- 绑定属性用私有字段 + `[ObservableProperty]`，不直接读写字段。
- 动作方法用 `[Command]`（同步）/ `[AsyncCommand]`（耗时），方法保持同步无返回值。
- 依赖用 `[Inject]` 字段 / 属性，名称 camelCase 后不冲突。
- 主窗口内容用 `DefaultRoute`（默认 `"AppMain"`）对应的那个路由。
