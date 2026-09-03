// Robot 桌面软件 — Win32 API 可用性检测助手
// 检测指定系统库中某个导出函数是否存在, 结果按库名+函数名缓存

using System;
using System.Collections.Concurrent;

using static Vanara.PInvoke.Kernel32;

namespace Robot
{

    /// <summary>
    /// Win32 API 可用性检测助手:检测指定系统库中某个导出函数是否存在, 结果按“库名+函数名”缓存。
    /// </summary>
    internal class Win32APIAvailableHelper
    {
        /// <summary>
        /// kernel32 库名称。
        /// </summary>
        const string KERNEL32 = "kernel32.dll";

        /// <summary>
        /// API 可用性缓存(键为“库名+函数名”, 值为是否可用)。
        /// </summary>
        private static readonly ConcurrentDictionary<string, bool> availableApis = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// 从系统路径加载库, 若不可用则返回 null。
        /// </summary>
        /// <param name="libraryName">库名称。</param>
        /// <returns>加载成功时返回库句柄; 不可用时返回 null。</returns>
        private static SafeHINSTANCE? LoadLibraryFromSystemPathIfAvailable(string libraryName)
        {
            SafeHINSTANCE? module = null;

            /* KB2533623 引入了 LOAD_LIBRARY_SEARCH_SYSTEM32 标志, 同时也引入了
             * AddDllDirectory 函数。这里通过检测 AddDllDirectory 是否存在,
             * 间接判断系统是否支持 LOAD_LIBRARY_SEARCH_SYSTEM32 标志。 */

            var kernel32 = GetModuleHandle(KERNEL32);
            if (!kernel32.IsNull)
            {
                if (GetProcAddress(kernel32, "AddDllDirectory") != IntPtr.Zero)
                {
                    module = LoadLibraryEx(libraryName, IntPtr.Zero, LoadLibraryExFlags.LOAD_LIBRARY_SEARCH_SYSTEM32);
                }
                else
                {
                    // 当前系统不支持 LOAD_LIBRARY_SEARCH_SYSTEM32, 回退到普通的 LoadLibrary
                    module = LoadLibrary(libraryName);
                }
            }
            return module;
        }

        /// <summary>
        /// 判断指定库中某个导出函数是否可用。
        /// </summary>
        /// <param name="libName">库名称。</param>
        /// <param name="procName">导出函数名称。</param>
        /// <returns>函数可用时返回 true; 否则返回 false。</returns>
        public static bool IsAvailable(string libName, string procName)
        {
            var isAvailable = false;

            if (!string.IsNullOrEmpty(libName) && !string.IsNullOrEmpty(procName))
            {
                var key = $"{libName.ToLower()}+{procName}";

                if (availableApis.TryGetValue(key, out isAvailable))
                {
                    return isAvailable;
                }

                // 从系统路径加载库
                var hmod = LoadLibraryFromSystemPathIfAvailable(libName);

                if (hmod != null)
                {
                    var pfnProc = GetProcAddress(hmod, procName);
                    if (pfnProc != IntPtr.Zero)
                    {
                        isAvailable = true;
                    }
                }

                FreeLibrary(hmod);
                availableApis[key] = isAvailable;
            }

            return isAvailable;
        }
    }
}
