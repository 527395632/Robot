using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Xilium.CefGlue;
using Vanara.PInvoke;

using static Vanara.PInvoke.User32;
using Robot.Forms;

namespace Robot.App.Forms
{

    /// <summary>
    /// 离屏 IME 处理器: 通过 Imm32 原生 API 管理输入法上下文, 将组合/候选窗口定位到离屏渲染的 WebView 光标处。
    /// </summary>
    internal class OffscreenImeHandler
    {
        /// <summary>
        /// IME 原生 API 封装: 声明 Imm32.dll 导出函数、相关常量与结构体。
        /// </summary>
        internal static class ImeNative
        {
            /// <summary>
            /// 获取组合结果字符串。
            /// </summary>
            internal const uint GCS_RESULTSTR = 0x0800;

            /// <summary>
            /// 获取组合字符串。
            /// </summary>
            internal const uint GCS_COMPSTR = 0x0008;

            /// <summary>
            /// 获取组合属性(已转换/未转换标记)。
            /// </summary>
            internal const uint GCS_COMPATTR = 0x0010;

            /// <summary>
            /// 获取组合光标位置。
            /// </summary>
            internal const uint GCS_CURSORPOS = 0x0080;

            /// <summary>
            /// 获取组合子句边界。
            /// </summary>
            internal const uint GCS_COMPCLAUSE = 0x0020;

            /// <summary>
            /// 组合字符串操作: 完成转换。
            /// </summary>
            internal const uint CPS_COMPLETE = 0x0001;

            /// <summary>
            /// 组合字符串操作: 取消转换。
            /// </summary>
            internal const uint CPS_CANCEL = 0x0004;

            /// <summary>
            /// 组合字符串标志: 不移动光标。
            /// </summary>
            internal const uint CS_NOMOVECARET = 0x4000;

            /// <summary>
            /// IME 通知动作: 组合字符串操作。
            /// </summary>
            internal const uint NI_COMPOSITIONSTR = 0x0015;

            /// <summary>
            /// 组合属性: 输入中(未转换)。
            /// </summary>
            internal const uint ATTR_INPUT = 0x00;

            /// <summary>
            /// 组合属性: 已转换目标。
            /// </summary>
            internal const uint ATTR_TARGET_CONVERTED = 0x01;

            /// <summary>
            /// 组合属性: 未转换目标。
            /// </summary>
            internal const uint ATTR_TARGET_NOTCONVERTED = 0x03;

            /// <summary>
            /// IME 上下文标志: 显示 UI 组合窗口。
            /// </summary>
            internal const uint ISC_SHOWUICOMPOSITIONWINDOW = 0x80000000;

            /// <summary>
            /// 候选窗口样式: 默认。
            /// </summary>
            internal const uint CFS_DEFAULT = 0x0000;

            /// <summary>
            /// 候选窗口样式: 指定矩形区域。
            /// </summary>
            internal const uint CFS_RECT = 0x0001;

            /// <summary>
            /// 候选窗口样式: 指定点位置。
            /// </summary>
            internal const uint CFS_POINT = 0x0002;

            /// <summary>
            /// 候选窗口样式: 强制指定位置。
            /// </summary>
            internal const uint CFS_FORCE_POSITION = 0x0020;

            /// <summary>
            /// 候选窗口样式: 指定候选窗口位置。
            /// </summary>
            internal const uint CFS_CANDIDATEPOS = 0x0040;

            /// <summary>
            /// 候选窗口样式: 排除区域。
            /// </summary>
            internal const uint CFS_EXCLUDE = 0x0080;

            /// <summary>
            /// 语言 ID: 日语。
            /// </summary>
            internal const uint LANG_JAPANESE = 0x11;

            /// <summary>
            /// 语言 ID: 中文。
            /// </summary>
            internal const uint LANG_CHINESE = 0x04;

            /// <summary>
            /// 语言 ID: 韩语。
            /// </summary>
            internal const uint LANG_KOREAN = 0x12;

            /// <summary>
            /// 关联上下文标志: 子窗口。
            /// </summary>
            internal const uint IACE_CHILDREN = 0x0001;

            /// <summary>
            /// 关联上下文标志: 使用默认 IME 上下文。
            /// </summary>
            internal const uint IACE_DEFAULT = 0x0010;

            /// <summary>
            /// 关联上下文标志: 忽略无上下文。
            /// </summary>
            internal const uint IACE_IGNORENOCONTEXT = 0x0020;

            /// <summary>
            /// 二维点坐标。
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                /// <summary>
                /// X 坐标。
                /// </summary>
                public int X;

                /// <summary>
                /// Y 坐标。
                /// </summary>
                public int Y;

                /// <summary>
                /// 初始化点坐标。
                /// </summary>
                /// <param name="x">X 坐标。</param>
                /// <param name="y">Y 坐标。</param>
                public POINT(int x, int y)
                {
                    X = x;
                    Y = y;
                }
            }

            /// <summary>
            /// 矩形区域。
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                /// <summary>
                /// 左边界。
                /// </summary>
                public int Left, Top, Right, Bottom;

                /// <summary>
                /// 初始化矩形区域。
                /// </summary>
                /// <param name="left">左边界。</param>
                /// <param name="top">上边界。</param>
                /// <param name="right">右边界。</param>
                /// <param name="bottom">下边界。</param>
                public RECT(int left, int top, int right, int bottom)
                {
                    Left = left;
                    Top = top;
                    Right = right;
                    Bottom = bottom;
                }
            }

            /// <summary>
            /// 组合窗口定位信息。
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct COMPOSITIONFORM
            {
                /// <summary>
                /// 定位样式。
                /// </summary>
                public int dwStyle;

                /// <summary>
                /// 当前光标位置。
                /// </summary>
                public POINT ptCurrentPos;

                /// <summary>
                /// 组合区域。
                /// </summary>
                public RECT rcArea;
            }

            /// <summary>
            /// 候选窗口定位信息。
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct CANDIDATEFORM
            {
                /// <summary>
                /// 候选窗口索引。
                /// </summary>
                public int dwIndex;

                /// <summary>
                /// 定位样式。
                /// </summary>
                public int dwStyle;

                /// <summary>
                /// 当前光标位置。
                /// </summary>
                public POINT ptCurrentPos;

                /// <summary>
                /// 候选区域。
                /// </summary>
                public RECT rcArea;
            }

            /// <summary>
            /// 创建新的 IME 上下文。
            /// </summary>
            /// <returns>新建的 IME 上下文句柄。</returns>
            [DllImport("Imm32.dll")]
            internal static extern IntPtr ImmCreateContext();

            /// <summary>
            /// 将 IME 上下文关联到窗口。
            /// </summary>
            /// <param name="hWnd">窗口句柄。</param>
            /// <param name="hIMC">IME 上下文句柄。</param>
            /// <returns>关联前的 IME 上下文句柄。</returns>
            [DllImport("Imm32.dll")]
            internal static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

            /// <summary>
            /// 带标志地将 IME 上下文关联到窗口。
            /// </summary>
            /// <param name="hWnd">窗口句柄。</param>
            /// <param name="hIMC">IME 上下文句柄。</param>
            /// <param name="flag">关联标志。</param>
            /// <returns>关联前的 IME 上下文句柄。</returns>
            [DllImport("Imm32.dll")]
            internal static extern IntPtr ImmAssociateContextEx(IntPtr hWnd, IntPtr hIMC, uint flag);

            /// <summary>
            /// 设置 IME 上下文的开启状态。
            /// </summary>
            /// <param name="himc">IME 上下文句柄。</param>
            /// <param name="b">是否开启。</param>
            /// <returns>是否设置成功。</returns>
            [DllImport("Imm32.dll")]
            public static extern bool ImmSetOpenStatus(IntPtr himc, bool b);

            /// <summary>
            /// 销毁 IME 上下文。
            /// </summary>
            /// <param name="hIMC">IME 上下文句柄。</param>
            /// <returns>是否销毁成功。</returns>
            [DllImport("Imm32.dll")]
            internal static extern bool ImmDestroyContext(IntPtr hIMC);

            /// <summary>
            /// 获取窗口的 IME 上下文。
            /// </summary>
            /// <param name="hWnd">窗口句柄。</param>
            /// <returns>IME 上下文句柄。</returns>
            [DllImport("Imm32.dll")]
            internal static extern IntPtr ImmGetContext(IntPtr hWnd);

            /// <summary>
            /// 释放窗口的 IME 上下文。
            /// </summary>
            /// <param name="hWnd">窗口句柄。</param>
            /// <param name="hIMC">IME 上下文句柄。</param>
            /// <returns>是否释放成功。</returns>
            [DllImport("Imm32.dll")]
            internal static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

            /// <summary>
            /// 向 IME 发送通知。
            /// </summary>
            /// <param name="hIMC">IME 上下文句柄。</param>
            /// <param name="action">通知动作。</param>
            /// <param name="index">动作索引。</param>
            /// <param name="value">动作值。</param>
            /// <returns>是否通知成功。</returns>
            [DllImport("Imm32.dll")]
            internal static extern bool ImmNotifyIME(IntPtr hIMC, uint action, uint index, uint value);

            /// <summary>
            /// 获取组合字符串。
            /// </summary>
            /// <param name="hIMC">IME 上下文句柄。</param>
            /// <param name="dwIndex">字符串类型。</param>
            /// <param name="lpBuf">接收缓冲区。</param>
            /// <param name="dwBufLen">缓冲区长度。</param>
            /// <returns>字符串长度。</returns>
            [DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
            internal static extern uint ImmGetCompositionString(IntPtr hIMC, uint dwIndex, byte[] lpBuf, uint dwBufLen);

            /// <summary>
            /// 设置组合窗口位置。
            /// </summary>
            /// <param name="hIMC">IME 上下文句柄。</param>
            /// <param name="lpCompForm">组合窗口定位信息。</param>
            /// <returns>结果码。</returns>
            [DllImport("Imm32.dll")]
            internal static extern int ImmSetCompositionWindow(IntPtr hIMC, ref COMPOSITIONFORM lpCompForm);

            /// <summary>
            /// 设置候选窗口位置。
            /// </summary>
            /// <param name="hIMC">IME 上下文句柄。</param>
            /// <param name="lpCandidateForm">候选窗口定位信息。</param>
            /// <returns>结果码。</returns>
            [DllImport("Imm32.dll")]
            public static extern int ImmSetCandidateWindow(IntPtr hIMC, [MarshalAs(UnmanagedType.Struct)] ref CANDIDATEFORM lpCandidateForm);
        }

        /// <summary>
        /// 组合属性: 已转换目标。
        /// </summary>
        const int ATTR_TARGET_CONVERTED = 0x01;

        /// <summary>
        /// 组合属性: 未转换目标。
        /// </summary>
        const int ATTR_TARGET_NOTCONVERTED = 0x03;

        /// <summary>
        /// 组合下划线颜色。
        /// </summary>
        const uint COLOR_UNDERLINE = 0xFF000000;

        /// <summary>
        /// 组合下划线背景色(透明)。
        /// </summary>
        const uint COLOR_BKCOLOR = 0x00000000;

        /// <summary>
        /// 当前输入语言的主语言 ID。
        /// </summary>
        private int languageCodeId;

        /// <summary>
        /// 是否已为本 IME 上下文创建系统光标。
        /// </summary>
        private bool systemCaret;

        /// <summary>
        /// 当前光标在组合文本中的索引, -1 表示未设置。
        /// </summary>
        private int cursorIndex = -1;

        /// <summary>
        /// 是否正处于组合输入状态。
        /// </summary>
        private bool isComposing = false;

        /// <summary>
        /// 当前组合文本的范围。
        /// </summary>
        CefRange compositionRange = new CefRange();

        /// <summary>
        /// 组合文本各字符的边界矩形集合。
        /// </summary>
        List<CefRectangle> compositionBounds = new List<CefRectangle>();


        /// <summary>
        /// IME 关联的窗口句柄。
        /// </summary>
        internal HWND hWnd { get; set; }

        /// <summary>
        /// IME 关联窗口的原生句柄。
        /// </summary>
        internal IntPtr Handle => hWnd.DangerousGetHandle();

        /// <summary>
        /// 宿主窗口。
        /// </summary>
        internal RobotWindow Owner { get; }

        /// <summary>
        /// 初始化离屏 IME 处理器。
        /// </summary>
        /// <param name="owner">宿主窗口。</param>
        public OffscreenImeHandler(RobotWindow owner)
        {
            Owner = owner;
        }

        /// <summary>
        /// 提取语言 ID 的主语言部分。
        /// </summary>
        /// <param name="lgid">完整语言 ID。</param>
        /// <returns>主语言 ID。</returns>
        private int PrimaryLangId(int lgid)
        {
            return lgid & 0x3ff;
        }

        /// <summary>
        /// 创建 IME 窗口: 对中日文输入法创建临时系统光标, 以便其按光标位置定位候选窗口。
        /// </summary>
        internal void CreateImeWindow()
        {
            // Chinese/Japanese IMEs somehow ignore function calls to
            // ::ImmSetCandidateWindow(), and use the position of the current system
            // caret instead -::GetCaretPos().
            // Therefore, we create a temporary system caret for Chinese IMEs and use
            // it during this input context.
            // Since some third-party Japanese IME also uses ::GetCaretPos() to determine
            // their window position, we also create a caret for Japanese IMEs.


            languageCodeId = PrimaryLangId(InputLanguage.CurrentInputLanguage.Culture.KeyboardLayoutId);

            if (languageCodeId == ImeNative.LANG_JAPANESE || languageCodeId == ImeNative.LANG_CHINESE)
            {
                if (!systemCaret)
                {
                    if (CreateCaret(hWnd, HBITMAP.NULL, 1, 3))
                    {
                        systemCaret = true;

                        //System.Diagnostics.Debug.WriteLine("System caret created.");
                    }
                }
            }
        }

        /// <summary>
        /// 销毁 IME 窗口: 若已创建系统光标则销毁之。
        /// </summary>
        internal void DestroyImeWindow()
        {
            // Destroy the system caret if we have created for this IME input context.
            if (systemCaret)
            {
                DestroyCaret();
                systemCaret = false;

                //System.Diagnostics.Debug.WriteLine("System caret destroyed.");

            }
        }

        /// <summary>
        /// 判断组合属性是否为已转换/未转换目标(即选中状态)。
        /// </summary>
        /// <param name="attribute">组合属性字节。</param>
        /// <returns>是否为选中属性。</returns>
        internal bool IsSelectionAttribute(byte attribute)
        {
            return (attribute == ATTR_TARGET_CONVERTED || attribute == ATTR_TARGET_NOTCONVERTED);
        }

        /// <summary>
        /// 获取组合文本中选中(已转换/未转换)部分的起止索引。
        /// </summary>
        /// <param name="imc">IME 上下文句柄。</param>
        /// <param name="targetStart">输出: 选中起始索引。</param>
        /// <param name="targetEnd">输出: 选中结束索引。</param>
        internal void GetCompositionSelectionRange(IntPtr imc, out int targetStart, out int targetEnd)
        {
            var attributeSize = ImeNative.ImmGetCompositionString(imc, ImeNative.GCS_COMPATTR, null, 0);

            if (attributeSize > 0)
            {

                var buff = new byte[attributeSize];

                ImeNative.ImmGetCompositionString(imc, ImeNative.GCS_COMPATTR, buff, attributeSize);

                int start, end;
                for (start = 0; start < attributeSize; ++start)
                {
                    if (IsSelectionAttribute(buff[start]))
                        break;
                }

                for (end = start; end < attributeSize; ++end)
                {
                    if (!IsSelectionAttribute(buff[end]))
                        break;
                }

                targetStart = start;
                targetEnd = end;

            }
            else
            {
                targetStart = 0;
                targetEnd = 0;

            }
        }

        /// <summary>
        /// 按组合子句边界生成下划线集合, 标记落在选中区间内的子句为粗线。
        /// </summary>
        /// <param name="imc">IME 上下文句柄。</param>
        /// <param name="start">选中起始索引。</param>
        /// <param name="end">选中结束索引。</param>
        /// <returns>下划线集合。</returns>
        internal IEnumerable<CefCompositionUnderline> GetCompositionUnderlines(IntPtr imc, int start, int end)
        {
            var clauseSize = ImeNative.ImmGetCompositionString(imc, ImeNative.GCS_COMPCLAUSE, null, 0);

            var clauseLength = (int)clauseSize / sizeof(int);

            var result = new List<CefCompositionUnderline>();

            if (clauseLength > 0)
            {
                var clauseData = new byte[(int)clauseSize];

                ImeNative.ImmGetCompositionString(imc, ImeNative.GCS_COMPCLAUSE, clauseData, clauseSize);

                for (var i = 0; i < clauseLength - 1; i++)
                {
                    var from = BitConverter.ToInt32(clauseData, i * sizeof(int));
                    var to = BitConverter.ToInt32(clauseData, (i + 1) * sizeof(int));

                    var range = new CefRange(from, to);

                    var think = range.From >= start && range.To <= end;

                    var underline = new CefCompositionUnderline
                    {
                        Range = range,
                        Color = new CefColor(COLOR_UNDERLINE),
                        BackgroundColor = new CefColor(COLOR_BKCOLOR),
                        Thick = think,
                    };

                    result.Add(underline);
                }
            }

            return result;

        }


        /// <summary>
        /// 移动 IME 窗口: 按光标位置定位候选窗口与组合窗口, 并按语言调整光标与排除区域。
        /// </summary>
        internal void MoveImeWindow()
        {
            const int kCaretMargin = 1;


            if (GetFocus() != hWnd)
            {
                return;
            }

            if (compositionBounds.Count == 0)
            {
                return;
            }

            CefRectangle rc;

            var location = cursorIndex;

            if (location == -1)
            {
                location = compositionRange.From;
            }

            if (location >= compositionRange.From)
            {
                location -= compositionRange.From;
            }

            if (location < compositionBounds.Count)
            {
                rc = compositionBounds[location];
            }
            else
            {
                return;
            }





            var x = rc.X + rc.Width;
            var y = rc.Y + rc.Height;

            //System.Diagnostics.Debug.WriteLine($"[MoveImeWindow] -> caret:{systemCaret} {x}:{y}");


            if (systemCaret)
            {
                if (languageCodeId == ImeNative.LANG_JAPANESE)
                {
                    SetCaretPos(rc.X, rc.Y + rc.Height);
                }
                else
                {
                    SetCaretPos(rc.X, rc.Y);
                }
            }

            var imc = ImeNative.ImmGetContext(Handle);


            var candidatePosition = new ImeNative.CANDIDATEFORM
            {
                dwIndex = 0,
                dwStyle = (int)ImeNative.CFS_CANDIDATEPOS,
                ptCurrentPos = new ImeNative.POINT(x, y),
                rcArea = new ImeNative.RECT(0, 0, 0, 0)
            };


            ImeNative.ImmSetCandidateWindow(imc, ref candidatePosition);


            if (languageCodeId == ImeNative.LANG_CHINESE)
            {
                // Chinese IMEs ignore function calls to ::ImmSetCandidateWindow()
                // when a user disables TSF (Text Service Framework) and CUAS (Cicero
                // Unaware Application Support).
                // On the other hand, when a user enables TSF and CUAS, Chinese IMEs
                // ignore the position of the current system caret and use the
                // parameters given to ::ImmSetCandidateWindow() with its 'dwStyle'
                // parameter CFS_CANDIDATEPOS.
                // Therefore, we do not only call ::ImmSetCandidateWindow() but also
                // set the positions of the temporary system caret if it exists.

                var candidatePotision = new ImeNative.COMPOSITIONFORM
                {
                    dwStyle = (int)ImeNative.CFS_CANDIDATEPOS,
                    ptCurrentPos = new ImeNative.POINT(rc.X, rc.Y),
                    rcArea = new ImeNative.RECT(0, 0, 0, 0)
                };

                ImeNative.ImmSetCompositionWindow(imc, ref candidatePotision);
            }



            if (languageCodeId == ImeNative.LANG_KOREAN)
            {
                // Korean IMEs require the lower-left corner of the caret to move their
                // candidate windows.
                rc.Y += kCaretMargin;
            }

            // Japanese IMEs and Korean IMEs also use the rectangle given to
            // ::ImmSetCandidateWindow() with its 'dwStyle' parameter CFS_EXCLUDE
            // Therefore, we also set this parameter here.

            var excludeRectangle = new ImeNative.CANDIDATEFORM
            {
                dwIndex = 0,
                dwStyle = (int)ImeNative.CFS_EXCLUDE,
                ptCurrentPos = new ImeNative.POINT(rc.X, rc.Y),
                rcArea = new ImeNative.RECT(rc.X, rc.Y, rc.X + rc.Width, rc.Y + rc.Height)
            };

            ImeNative.ImmSetCandidateWindow(imc, ref excludeRectangle);

            ImeNative.ImmReleaseContext(Handle, imc);
        }

        /// <summary>
        /// 清理组合状态: 通知 IME 完成转换并重置组合状态。
        /// </summary>
        internal void CleanupComposition()
        {
            if (isComposing)
            {
                var imc = ImeNative.ImmGetContext(Handle);
                if (imc != IntPtr.Zero)
                {
                    ImeNative.ImmNotifyIME(imc, ImeNative.NI_COMPOSITIONSTR, ImeNative.CPS_COMPLETE, 0);
                    ImeNative.ImmReleaseContext(Handle, imc);
                }

                ResetComposition();
            }
        }

        /// <summary>
        /// 重置组合状态: 清除组合标记与光标索引。
        /// </summary>
        internal void ResetComposition()
        {
            // Reset the composition status.
            isComposing = false;
            cursorIndex = -1;
        }

        /// <summary>
        /// 获取组合信息: 按消息参数解析选中范围、光标位置与下划线集合。
        /// </summary>
        /// <param name="imc">IME 上下文句柄。</param>
        /// <param name="lparam">WM_IME_COMPOSITION 消息参数。</param>
        /// <param name="compositionText">组合文本。</param>
        /// <param name="compositionStart">输出: 组合起始索引。</param>
        /// <returns>下划线集合。</returns>
        internal List<CefCompositionUnderline> GetCompositionInfo(IntPtr imc, uint lparam, string compositionText, out int compositionStart)
        {
            var underlines = new List<CefCompositionUnderline>();
            var length = compositionText.Length;

            var targetStart = length;
            var targetEnd = length;

            if ((lparam & ImeNative.GCS_COMPATTR) == ImeNative.GCS_COMPATTR)
            {
                GetCompositionSelectionRange(imc, out targetStart, out targetEnd);
            }

            if (((lparam & ImeNative.CS_NOMOVECARET) != ImeNative.CS_NOMOVECARET) && ((lparam & ImeNative.GCS_CURSORPOS) == ImeNative.GCS_CURSORPOS))
            {
                var cursor = (int)ImeNative.ImmGetCompositionString(imc, ImeNative.GCS_CURSORPOS, null, 0);
                compositionStart = cursor;
            }
            else
            {
                compositionStart = 0;
            }


            if ((lparam & ImeNative.GCS_COMPCLAUSE) == ImeNative.GCS_COMPCLAUSE)
            {
                underlines = GetCompositionUnderlines(imc, targetStart, targetEnd).ToList();
            }

            if (underlines.Count == 0)
            {
                var underline = new CefCompositionUnderline();
                underline.Color = new CefColor(COLOR_UNDERLINE);
                underline.BackgroundColor = new CefColor(COLOR_BKCOLOR);

                if (targetStart > 0)
                {
                    underline.Range = new CefRange(targetStart, targetEnd);
                    underline.Thick = true;
                    underlines.Add(underline);
                }

                if (targetEnd < length)
                {
                    underline.Range = new CefRange(targetEnd, length);
                    underline.Thick = false;
                    underlines.Add(underline);
                }
            }

            return underlines;

        }

        /// <summary>
        /// 按类型获取组合字符串(Unicode 解码)。
        /// </summary>
        /// <param name="imc">IME 上下文句柄。</param>
        /// <param name="lparam">WM_IME_COMPOSITION 消息参数。</param>
        /// <param name="type">字符串类型。</param>
        /// <param name="result">输出: 解码后的字符串。</param>
        /// <returns>是否获取成功。</returns>
        internal bool GetString(IntPtr imc, uint lparam, uint type, out string result)
        {
            if (((int)lparam & type) != type)
            {
                result = null;
                return false;
            }

            var strlen = ImeNative.ImmGetCompositionString(imc, type, null, 0);

            if (strlen <= 0)
            {
                result = null;
                return false;
            }

            var buff = new byte[strlen];

            ImeNative.ImmGetCompositionString(imc, type, buff, strlen);

            result = Encoding.Unicode.GetString(buff);

            return true;
        }

        /// <summary>
        /// 获取组合结果字符串。
        /// </summary>
        /// <param name="lparam">WM_IME_COMPOSITION 消息参数。</param>
        /// <param name="result">输出: 结果字符串。</param>
        /// <returns>是否获取成功。</returns>
        internal bool GetResult(uint lparam, out string? result)
        {
            var ret = false;

            var imc = ImeNative.ImmGetContext(Handle);

            if (imc != IntPtr.Zero)
            {
                ret = GetString(imc, lparam, ImeNative.GCS_RESULTSTR, out result);

                ImeNative.ImmReleaseContext(Handle, imc);
            }
            else
            {
                result = null;
            }

            return ret;
        }

        /// <summary>
        /// 获取组合文本及其下划线集合, 成功时标记为组合中。
        /// </summary>
        /// <param name="lparam">WM_IME_COMPOSITION 消息参数。</param>
        /// <param name="compositionText">输出: 组合文本。</param>
        /// <param name="compostionStart">输出: 组合起始索引。</param>
        /// <param name="underlines">输出: 下划线集合。</param>
        /// <returns>是否获取成功。</returns>
        internal bool GetComposition(uint lparam, out string compositionText, out int compostionStart, ref List<CefCompositionUnderline> underlines)
        {
            var imc = ImeNative.ImmGetContext(Handle);

            var ret = GetString(imc, lparam, ImeNative.GCS_COMPSTR, out compositionText);

            if (ret)
            {
                underlines = GetCompositionInfo(imc, lparam, compositionText, out compostionStart);

                isComposing = true;
            }
            else
            {
                compostionStart = 0;
            }

            ImeNative.ImmReleaseContext(Handle, imc);

            return ret;

        }

        /// <summary>
        /// 更新光标位置并移动 IME 窗口。
        /// </summary>
        /// <param name="index">光标在组合文本中的索引。</param>
        internal void UpdateCaretPosition(int index)
        {
            // Save the caret position.
            cursorIndex = index;
            // Move the IME window.
            MoveImeWindow();
        }




        #region IME Control
        /// <summary>
        /// 禁用 IME: 清理组合状态并解除 IME 上下文关联。
        /// </summary>
        public void DisableIME()
        {
            CleanupComposition();
            ImeNative.ImmAssociateContextEx(Handle, IntPtr.Zero, 0);
        }

        /// <summary>
        /// 取消 IME: 通知 IME 取消转换并重置组合状态。
        /// </summary>
        public void CancelIME()
        {
            if (isComposing)
            {
                var imc = ImeNative.ImmGetContext(Handle);
                if (imc != IntPtr.Zero)
                {
                    ImeNative.ImmNotifyIME(imc, ImeNative.NI_COMPOSITIONSTR, ImeNative.CPS_CANCEL, 0);
                    ImeNative.ImmReleaseContext(Handle, imc);
                }
                ResetComposition();
            }
        }

        /// <summary>
        /// 启用 IME: 加载默认 IME 上下文。
        /// </summary>
        public void EnableIME()
        {
            // Load the default IME context.
            ImeNative.ImmAssociateContextEx(Handle, IntPtr.Zero, ImeNative.IACE_DEFAULT);
        }

        #endregion

        /// <summary>
        /// 更新组合范围与边界矩形(按窗口缩放因子缩放), 并移动 IME 窗口。
        /// </summary>
        /// <param name="selectRange">选中的组合范围。</param>
        /// <param name="bounds">组合文本各字符的边界矩形。</param>
        public void ChangeCompositionRange(CefRange selectRange, IEnumerable<CefRectangle> bounds)
        {
            var scaleFactor = SystemDpiManager.GetScaleFactorForWindow(hWnd);

            compositionRange = selectRange;

            var rects = new List<CefRectangle>();

            foreach (var rect in bounds)
            {
                var scaledBounds = new CefRectangle((int)(rect.X * scaleFactor), (int)(rect.Y * scaleFactor), (int)(rect.Width * scaleFactor), (int)(rect.Height * scaleFactor));
                rects.Add(scaledBounds);

            }

            compositionBounds = rects;

            MoveImeWindow();

        }

        /// <summary>
        /// 处理 IME 设置上下文消息: 绑定窗口句柄并清除显示 UI 组合窗口标志后转发默认窗口过程。
        /// </summary>
        /// <param name="m">WM_IME_SETCONTEXT 消息。</param>
        public void OnIMESetContext(ref Message m)
        {
            hWnd = Owner.WindowHandle;

            var retval = (ulong)m.LParam;

            retval &= ~ImeNative.ISC_SHOWUICOMPOSITIONWINDOW;

            var lParam = (IntPtr)retval;

            DefWindowProc(hWnd, (uint)m.Msg, m.WParam, lParam);

        }

        /// <summary>
        /// 处理 IME 开始组合: 重置组合状态并创建 IME 窗口。
        /// </summary>
        public void OnImeStartComposition()
        {


            ResetComposition();

            CreateImeWindow();
        }

        /// <summary>
        /// 处理 IME 组合消息: 提交结果文本或更新组合文本与下划线, 并更新光标位置。
        /// </summary>
        /// <param name="message">IME 消息类型。</param>
        /// <param name="wParam">消息 W 参数。</param>
        /// <param name="lParam">消息 L 参数。</param>
        public void OnImeComposition(WindowMessage message, IntPtr wParam, IntPtr lParam)
        {
            var browserHost = Owner.WebView?.BrowserHost;

            if (browserHost == null) return;

            if (GetResult((uint)lParam, out var textStr))
            {
                browserHost.ImeCommitText(textStr, new CefRange(int.MaxValue, int.MaxValue), 0);

                browserHost.ImeSetComposition(textStr, 0, new CefCompositionUnderline(), new CefRange(int.MaxValue, int.MaxValue), new CefRange(0, 0));

                browserHost.ImeFinishComposingText(false);

                ResetComposition();

            }
            else
            {
                var underlines = new List<CefCompositionUnderline>();

                if (GetComposition((uint)lParam, out textStr, out var compostionStart, ref underlines))
                {

                    browserHost.ImeSetComposition(textStr, underlines.Count, underlines[0], new CefRange(int.MaxValue, int.MaxValue), new CefRange(compostionStart, compostionStart + textStr.Length));

                    UpdateCaretPosition(compostionStart - 1);
                }
                else
                {
                    browserHost.ImeSetComposition(string.Empty, 1, new CefCompositionUnderline { }, new CefRange(0, 1), new CefRange(0, 1));

                    OnImeCancelComposition();
                }
            }





        }

        /// <summary>
        /// 处理 IME 取消组合: 清空组合文本、提交空文本并销毁 IME 窗口。
        /// </summary>
        public void OnImeCancelComposition()
        {
            var browserHost = Owner.WebView?.BrowserHost;

            if (browserHost == null) return;

            browserHost?.ImeSetComposition(string.Empty, 0, new CefCompositionUnderline(), new CefRange(int.MaxValue, int.MaxValue), new CefRange(0, 0));

            browserHost?.ImeCommitText(string.Empty, new CefRange(int.MaxValue, int.MaxValue), 0);



            if (languageCodeId != ImeNative.LANG_KOREAN)
            {
                browserHost?.ImeFinishComposingText(false);
            }




            browserHost?.ImeCancelComposition();
            ResetComposition();
            DestroyImeWindow();
        }

    }
}
