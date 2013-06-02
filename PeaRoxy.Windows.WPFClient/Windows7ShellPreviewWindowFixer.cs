using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PeaRoxy.Windows.WPFClient
{
    class Windows7ShellPreviewWindowFixer
    {
        [DllImport("user32.dll")]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("user32.dll")]
        static extern UInt32 GetWindowLong(IntPtr hWnd, Int32 nIndex);
        [DllImport("user32.dll")]
        static extern UInt32 SetWindowLong(IntPtr hWnd, Int32 nIndex, UInt32 dwNewLong);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
        const Int32 GWL_STYLE = (-16);
        const Int32 GWL_EXSTYLE = (-20);
        const UInt32 WS_CAPTION = 0xC00000;
        const UInt32 WS_SYSMENU = 0x80000;
        const UInt32 WS_THICKFRAME = 0x40000;
        const UInt32 WS_MINIMIZEBOX = 0x20000;
        const UInt32 WS_MAXIMIZEBOX = 0x10000;
        const UInt32 WS_EX_DLGMODALFRAME = 0x0001;
        const UInt32 WS_VISIBLE = 0x10000000;
        const UInt32 WS_DISABLED = 0x8000000;
        const UInt32 WS_TABSTOP = 0x10000;
        const uint SWP_FRAMECHANGED = 0x0020;
        public static void Fix(string windowTitle, Process process)
        {

            IntPtr previewChartWindow = IntPtr.Zero;
            do
            {
                previewChartWindow = FindWindowEx(IntPtr.Zero, previewChartWindow, null, windowTitle);
                if (previewChartWindow != IntPtr.Zero)
                {
                    uint pId;
                    GetWindowThreadProcessId(previewChartWindow, out pId);
                    if (pId > 0 && pId == process.Id)
                        if (process.MainWindowHandle != previewChartWindow)
                        {
                            UInt32 windowLong = GetWindowLong(previewChartWindow, GWL_STYLE);
                            windowLong = windowLong & (~WS_CAPTION);
                            windowLong = windowLong & (~WS_SYSMENU);
                            windowLong = windowLong & (~WS_THICKFRAME);
                            windowLong = windowLong & (~WS_MINIMIZEBOX);
                            windowLong = windowLong & (~WS_MAXIMIZEBOX);
                            windowLong = windowLong & (~WS_VISIBLE);
                            windowLong = windowLong & (~WS_TABSTOP);
                            windowLong = windowLong & (WS_DISABLED);
                            SetWindowLong(previewChartWindow, GWL_STYLE, windowLong);

                            windowLong = GetWindowLong(previewChartWindow, GWL_EXSTYLE);
                            windowLong = windowLong | WS_EX_DLGMODALFRAME;
                            SetWindowLong(previewChartWindow, GWL_EXSTYLE, windowLong);

                            SetWindowPos(previewChartWindow, IntPtr.Zero, -500, -500, 0, 0, SWP_FRAMECHANGED);
                        }
                }
            } while (previewChartWindow != IntPtr.Zero);
        }
    }
}
