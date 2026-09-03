
using System;
using System.Drawing;
using System.Windows.Forms;

using Vanara.PInvoke;
using static Vanara.PInvoke.Gdi32;
using static Vanara.PInvoke.SHCore;
using static Vanara.PInvoke.User32;

namespace Robot.Forms;

/// <summary>
/// 系统 DPI 管理器: 提供设备 DPI、逻辑/设备单位缩放因子、Per-Monitor-V2 感知检测,
/// 以及按窗口/屏幕/坐标点查询 DPI 与缩放因子的能力。
/// </summary>
internal static class SystemDpiManager
{
    /// <summary>
    /// 逻辑 DPI (96)。
    /// </summary>
    public const float LOGICAL_DPI = 96.0f;

    /// <summary>
    /// 设备 DPI。
    /// </summary>
    private static float deviceDpi = LOGICAL_DPI;

    /// <summary>
    /// 逻辑到设备单位缩放因子。
    /// </summary>
    private static float logicalToDeviceUnitsScalingFactor = 0.0f;

    /// <summary>
    /// 是否已初始化设备 DPI。
    /// </summary>
    private static bool isInitialized;

    /// <summary>
    /// 是否已初始化 DPI 管理器。
    /// </summary>
    private static bool isDpiManagerInitialized;

    /// <summary>
    /// 是否需要查询 Per-Monitor-V2 感知。
    /// </summary>
    private static bool doesNeedQueryForPerMonitorV2Awareness;

    /// <summary>
    /// 是否满足缩放要求 (设备 DPI 非 96 或需查询 Per-Monitor-V2)。
    /// </summary>
    private static bool isScalingRequirementMet = false;

    /// <summary>
    /// 逻辑到设备单位缩放因子 (设备 DPI / 96), 首次访问时初始化。
    /// </summary>
    public static double LogicalToDeviceUnitsScalingFactor
    {
        get
        {
            if (logicalToDeviceUnitsScalingFactor == 0.0)
            {
                Initialize();
                logicalToDeviceUnitsScalingFactor = deviceDpi / LOGICAL_DPI;
            }
            return logicalToDeviceUnitsScalingFactor;
        }
    }

    /// <summary>
    /// 是否处于 Per-Monitor-V2 DPI 感知模式。
    /// </summary>
    public static bool IsPerMonitorV2Awareness
    {
        get
        {
            InitializeDpiManager();
            if (doesNeedQueryForPerMonitorV2Awareness)
            {
                var dpiAwareness = DpiMethods.TryGetThreadDpiAwarenessContext();
                var DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new DPI_AWARENESS_CONTEXT((nint)(-4));
                return DpiMethods.TryFindDpiAwarenessContextsEqual(dpiAwareness, DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            }
            else
            {
                return false;
            }
        }
    }


    /// <summary>
    /// 是否需要缩放 (设备 DPI 非 96)。
    /// </summary>
    public static bool IsScalingRequired
    {
        get
        {
            Initialize();
            return deviceDpi != LOGICAL_DPI;
        }
    }


    /// <summary>
    /// 是否满足缩放要求 (设备 DPI 非 96 或需查询 Per-Monitor-V2)。
    /// </summary>
    internal static bool IsScalingRequirementMet
    {
        get
        {
            InitializeDpiManager();
            return isScalingRequirementMet;
        }
    }

    /// <summary>
    /// 初始化设备 DPI (通过 GetDC/GetDeviceCaps 获取屏幕 DPI)。
    /// </summary>
    private static void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        var hDC = GetDC(HWND.NULL);

        if (hDC != (nint)0)
        {
            deviceDpi = GetDeviceCaps(hDC, DeviceCap.LOGPIXELSX);

            ReleaseDC(HWND.NULL, hDC);
        }

        isInitialized = true;

    }

    /// <summary>
    /// 将逻辑单位转换为设备单位 (可选指定设备像素基准)。
    /// </summary>
    public static int LogicalToDeviceUnits(int value, int devicePixels = 0)
    {
        if (devicePixels == 0)
        {
            return (int)Math.Round(LogicalToDeviceUnitsScalingFactor * value);
        }
        double scalingFactor = devicePixels / LOGICAL_DPI;
        return (int)Math.Round(scalingFactor * value);
    }

    /// <summary>
    /// 设备 DPI (首次访问时初始化)。
    /// </summary>
    public static int DeviceDpi
    {
        get
        {
            Initialize();
            return (int)deviceDpi;
        }
    }

    /// <summary>
    /// 初始化 DPI 管理器: 初始化设备 DPI, 并检测进程 DPI 感知模式 (Windows 10/1603+)。
    /// </summary>
    public static void InitializeDpiManager()
    {
        if (isDpiManagerInitialized)
        {
            return;
        }

        Initialize();

        // 存在该 API 时说明处于 Windows 10/1603 或更高版本。
        if (Win32APIAvailableHelper.IsAvailable("Shcore.dll", nameof(GetProcessDpiAwareness)))
        {

            // 确实处于 Windows 10/1603 或更高版本, 但进程仍可能是 DpiUnaware 或 SystemAware, 因此需要进一步确认...
            PROCESS_DPI_AWARENESS processDpiAwareness;
            var currentProcessId = Kernel32.GetCurrentProcessId();

            var PROCESS_QUERY_INFORMATION = new ACCESS_MASK(0x0400);

            var hProcess = Kernel32.OpenProcess(PROCESS_QUERY_INFORMATION, false, currentProcessId);

            var result = GetProcessDpiAwareness(hProcess, out processDpiAwareness);

            // 仅当进程不是 DpiUnaware/SystemAware 时, 才有必要在需要时查询 PerMonitorV2 感知。
            if (!(processDpiAwareness == PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE ||
                  processDpiAwareness == PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE))
            {
                doesNeedQueryForPerMonitorV2Awareness = true;
            }
        }

        if (IsScalingRequired || doesNeedQueryForPerMonitorV2Awareness)
        {
            isScalingRequirementMet = true;
        }

        isDpiManagerInitialized = true;
    }

    //[DllImport("Shcore.dll", ExactSpelling = true)]
    //[PInvokeData("shellscalingapi.h", MSDNShortId = "NF:shellscalingapi.GetDpiForMonitor")]
    //private static extern HRESULT GetDpiForMonitor([In] HMONITOR hmonitor, [In] MONITOR_DPI_TYPE dpiType, [Out] out uint dpiX, [Out] out uint dpiY);

    /// <summary>
    /// 获取窗口所在屏幕的 DPI (Per-Monitor-V2 时按窗口所在显示器, 否则返回设备 DPI)。
    /// </summary>
    public static int GetDpiForWindow(HWND handle)
    {


        if (IsPerMonitorV2Awareness)
        {
            var hMonitor = MonitorFromWindow(handle, MonitorFlags.MONITOR_DEFAULTTONEAREST);

            GetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_DEFAULT, out var dpiX, out var dpiY);

            return (int)dpiY;
        }

        return DeviceDpi;
    }

    /// <summary>
    /// 获取窗口所在屏幕的缩放因子 (DPI / 96)。
    /// </summary>
    public static float GetScaleFactorForWindow(HWND handle)
    {
        var dpi = GetDpiForWindow(handle);

        return dpi / LOGICAL_DPI;
    }

    /// <summary>
    /// 获取指定坐标点所在屏幕的 DPI。
    /// </summary>
    public static int GetScreenDpiFromPoint(Point point)
    {
        if (IsPerMonitorV2Awareness)
        {
            var hMonitor = MonitorFromPoint(point, MonitorFlags.MONITOR_DEFAULTTONEAREST);

            GetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out var dpiX, out _);

            return (int)dpiX;

        }

        return DeviceDpi;
    }

    /// <summary>
    /// 获取指定屏幕 (取其中心点) 的 DPI。
    /// </summary>
    public static int GetScreenDpi(Screen currentScreen)
    {
        if (IsPerMonitorV2Awareness)
        {
            var hMonitor = MonitorFromPoint(new Point(currentScreen.Bounds.X + currentScreen.Bounds.Width / 2, currentScreen.Bounds.Y + currentScreen.Bounds.Height / 2), MonitorFlags.MONITOR_DEFAULTTONEAREST);

            GetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out var dpiX, out _);
            return (int)dpiX;

        }

        return DeviceDpi;
    }

    /// <summary>
    /// 按缩放因子计算缩放后的尺寸 (四舍五入)。
    /// </summary>
    public static Size CalcScaledSize(Size value, SizeF scaleFactor)
    {
        return new Size(
            (int)Math.Round(value.Width * scaleFactor.Width, MidpointRounding.AwayFromZero),
            (int)Math.Round(value.Height * scaleFactor.Height, MidpointRounding.AwayFromZero));
    }


    /// <summary>
    /// DPI 感知上下文相关的 Win32 API 封装: 在 API 可用时调用, 否则返回 null/false。
    /// </summary>
    static class DpiMethods
    {
        /// <summary>
        /// GetThreadDpiAwarenessContext API 是否可用。
        /// </summary>
        public static bool GetThreadDpiAwarenessContextIsAvailable()
        {
            return Win32APIAvailableHelper.IsAvailable("User32.dll", nameof(GetThreadDpiAwarenessContext));
        }

        /// <summary>
        /// SetThreadDpiAwarenessContext API 是否可用。
        /// </summary>
        public static bool SetThreadDpiAwarenessContextIsAvailable()
        {
            return Win32APIAvailableHelper.IsAvailable("User32.dll", nameof(SetThreadDpiAwarenessContext));
        }

        /// <summary>
        /// AreDpiAwarenessContextsEqual API 是否可用。
        /// </summary>
        public static bool AreDpiAwarenessContextsEqualIsAvailable()
        {
            return Win32APIAvailableHelper.IsAvailable("User32.dll", nameof(AreDpiAwarenessContextsEqual));
        }

        /// <summary>
        /// GetWindowDpiAwarenessContext API 是否可用。
        /// </summary>
        public static bool GetWindowDpiAwarenessContextIsAvailable()
        {
            return Win32APIAvailableHelper.IsAvailable("User32.dll", nameof(GetWindowDpiAwarenessContext));
        }

        /// <summary>
        /// 尝试比较两个 DPI 感知上下文值, 相等时返回 true, 不相等或底层操作系统不支持该 API 时返回 false。
        /// </summary>
        /// <returns>true/false</returns>
        public static bool TryFindDpiAwarenessContextsEqual(DPI_AWARENESS_CONTEXT? dpiContextA, DPI_AWARENESS_CONTEXT? dpiContextB)
        {
            if (dpiContextA == null)
            {
                return dpiContextB == null; // 两者都为 null 时返回 true, 否则返回 false
            }
            else if (dpiContextB == null)
            {
                return false; // 已知 A 非 null, 故返回 false
            }

            if (AreDpiAwarenessContextsEqualIsAvailable())
            {
                return AreDpiAwarenessContextsEqual((DPI_AWARENESS_CONTEXT)dpiContextA, (DPI_AWARENESS_CONTEXT)dpiContextB);
            }

            // 不支持该 API 的旧版操作系统。
            return false;
        }

        /// <summary>
        /// 尝试获取线程 DPI 感知上下文。
        /// </summary>
        /// <returns>若当前操作系统版本支持该 API 则返回线程 DPI 感知上下文, 否则返回 IntPtr.Zero。</returns>
        public static DPI_AWARENESS_CONTEXT? TryGetThreadDpiAwarenessContext()
        {
            if (GetThreadDpiAwarenessContextIsAvailable())
            {
                return GetThreadDpiAwarenessContext();
            }
            // 不支持该 API 的旧版操作系统。
            return null;
        }

        /// <summary>
        /// 尝试设置线程 DPI 感知上下文。
        /// </summary>
        /// <returns>若当前操作系统版本支持该 API 则返回旧的线程 DPI 感知上下文, 否则返回 IntPtr.Zero。</returns>
        public static DPI_AWARENESS_CONTEXT? TrySetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT? dpiContext)
        {
            if (SetThreadDpiAwarenessContextIsAvailable())
            {
                if (dpiContext == null)
                {
                    throw new ArgumentNullException();
                }
                return SetThreadDpiAwarenessContext((DPI_AWARENESS_CONTEXT)dpiContext);
            }
            // 不支持该 API 的旧版操作系统。
            return null;
        }

        /// <summary>
        /// 尝试获取窗口 DPI 感知上下文。
        /// </summary>
        /// <returns>若当前操作系统版本支持该 API 则返回窗口 DPI 感知上下文, 否则返回 DpiAwarenessContext.DPI_AWARENESS_CONTEXT_UNSPECIFIED。</returns>
        public static DPI_AWARENESS_CONTEXT? TryGetWindowDpiAwarenessContext(HWND hWnd)
        {
            if (GetWindowDpiAwarenessContextIsAvailable())
            {
                return GetWindowDpiAwarenessContext(hWnd);
            }
            // 不支持该 API 的旧版操作系统。
            return null;
        }
    }
}
