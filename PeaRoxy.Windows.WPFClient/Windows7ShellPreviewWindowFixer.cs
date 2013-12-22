// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Windows7ShellPreviewWindowFixer.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The windows 7 shell preview window fixer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient
{
    #region

    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    #endregion

    /// <summary>
    /// The windows 7 shell preview window fixer.
    /// </summary>
    internal static class Windows7ShellPreviewWindowFixer
    {
        #region Constants

        /// <summary>
        /// The GWL_EXSTYLE
        /// </summary>
        private const int GwlExstyle = -20;

        /// <summary>
        /// The GWL_STYLE
        /// </summary>
        private const int GwlStyle = -16;

        /// <summary>
        /// The SWP_FRAMECHANGED
        /// </summary>
        private const uint SwpFramechanged = 0x0020;

        /// <summary>
        /// The WS_CAPTION
        /// </summary>
        private const uint WsCaption = 0xC00000;

        /// <summary>
        /// The WS_DISABLED
        /// </summary>
        private const uint WsDisabled = 0x8000000;

        /// <summary>
        /// The WS_EX_DLGMODALFRAME
        /// </summary>
        private const uint WsExDlgmodalframe = 0x0001;

        /// <summary>
        /// The WS_MAXIMIZEBOX
        /// </summary>
        private const uint WsMaximizebox = 0x10000;

        /// <summary>
        /// The WS_MINIMIZEBOX
        /// </summary>
        private const uint WsMinimizebox = 0x20000;

        /// <summary>
        /// The WS_SYSMENU
        /// </summary>
        private const uint WsSysmenu = 0x80000;

        /// <summary>
        /// The WS_TABSTOP
        /// </summary>
        private const uint WsTabstop = 0x10000;

        /// <summary>
        /// The WS_THICKFRAME
        /// </summary>
        private const uint WsThickframe = 0x40000;

        /// <summary>
        /// The WS_VISIBLE
        /// </summary>
        private const uint WsVisible = 0x10000000;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The fix.
        /// </summary>
        /// <param name="windowTitle">
        /// The window title.
        /// </param>
        /// <param name="process">
        /// The process.
        /// </param>
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
                    {
                        if (process.MainWindowHandle != previewChartWindow)
                        {
                            uint windowLong = GetWindowLong(previewChartWindow, GwlStyle);
                            windowLong = windowLong & (~WsCaption);
                            windowLong = windowLong & (~WsSysmenu);
                            windowLong = windowLong & (~WsThickframe);
                            windowLong = windowLong & (~WsMinimizebox);
                            windowLong = windowLong & (~WsMaximizebox);
                            windowLong = windowLong & (~WsVisible);
                            windowLong = windowLong & (~WsTabstop);
                            windowLong = windowLong & WsDisabled;
                            SetWindowLong(previewChartWindow, GwlStyle, windowLong);

                            windowLong = GetWindowLong(previewChartWindow, GwlExstyle);
                            windowLong = windowLong | WsExDlgmodalframe;
                            SetWindowLong(previewChartWindow, GwlExstyle, windowLong);

                            SetWindowPos(previewChartWindow, IntPtr.Zero, -500, -500, 0, 0, SwpFramechanged);
                        }
                    }
                }
            }
            while (previewChartWindow != IntPtr.Zero);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The find window ex.
        /// </summary>
        /// <param name="hwndParent">
        /// The parent window.
        /// </param>
        /// <param name="hwndChildAfter">
        /// The next child.
        /// </param>
        /// <param name="lpszClass">
        /// The class name.
        /// </param>
        /// <param name="lpszWindow">
        /// The window name.
        /// </param>
        /// <returns>
        /// The <see cref="IntPtr"/>.
        /// </returns>
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(
            IntPtr hwndParent, 
            IntPtr hwndChildAfter, 
            string lpszClass, 
            string lpszWindow);

        /// <summary>
        /// The get window long.
        /// </summary>
        /// <param name="windowHandler">
        /// The window.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <returns>
        /// The <see cref="uint"/>.
        /// </returns>
        [DllImport("user32.dll")]
        private static extern uint GetWindowLong(IntPtr windowHandler, int index);

        /// <summary>
        /// The get window thread process id.
        /// </summary>
        /// <param name="windowHandler">
        /// The window.
        /// </param>
        /// <param name="processId">
        /// The process id.
        /// </param>
        /// <returns>
        /// The <see cref="uint"/>.
        /// </returns>
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr windowHandler, out uint processId);

        /// <summary>
        /// The set window long.
        /// </summary>
        /// <param name="windowHandler">
        /// The window.
        /// </param>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <param name="newLong">
        /// The new long.
        /// </param>
        /// <returns>
        /// The <see cref="uint"/>.
        /// </returns>
        [DllImport("user32.dll")]
        private static extern uint SetWindowLong(IntPtr windowHandler, int index, uint newLong);

        /// <summary>
        /// The set window position.
        /// </summary>
        /// <param name="windowHandler">
        /// The window
        /// </param>
        /// <param name="windowHandlerInsertAfter">
        /// The window to insert after.
        /// </param>
        /// <param name="x">
        /// The X.
        /// </param>
        /// <param name="y">
        /// The Y.
        /// </param>
        /// <param name="cx">
        /// The CX.
        /// </param>
        /// <param name="cy">
        /// The CY.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(
            IntPtr windowHandler,
            IntPtr windowHandlerInsertAfter, 
            int x, 
            int y, 
            int cx, 
            int cy, 
            uint flags);

        #endregion
    }
}