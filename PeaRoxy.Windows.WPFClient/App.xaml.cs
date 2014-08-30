// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for App.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient
{
    #region

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Media.Animation;

    using LukeSw.Windows.Forms;

    using Microsoft.WindowsAPICodePack.ApplicationServices;
    using Microsoft.WindowsAPICodePack.Taskbar;

    #endregion

    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "Reviewed. Suppression is OK here.")]
    public partial class App
    {
        #region Static Fields

        /// <summary>
        ///     The defualt app.
        /// </summary>
        private static App defualtApp;

        private static JumpList jumpList;

        #endregion

        #region Delegates

        /// <summary>
        ///     The simple void_ delegate.
        /// </summary>
        public delegate void SimpleVoidDelegate();

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets a value indicating whether is executed by user.
        /// </summary>
        public static bool IsExecutedByUser { get; private set; }

        /// <summary>
        ///     Gets the notify.
        /// </summary>
        public static NotifyIcon Notify { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The end.
        /// </summary>
        public static void End()
        {
            if (jumpList != null && TaskbarManager.IsPlatformSupported)
            {
                jumpList.ClearAllUserTasks();
                jumpList.Refresh();
            }
            ApplicationRestartRecoveryManager.UnregisterApplicationRestart();
            if (Current != null && Current.MainWindow != null)
            {
                MainWindow cWindow = Current.MainWindow as MainWindow;
                if (cWindow != null)
                {
                    cWindow.StopServer(true);
                }
                Current.Shutdown();
            }
            Environment.Exit(0);
        }

        /// <summary>
        ///     The main.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                defualtApp = new App();
                WpfSingleInstance.Make();
                if (CommandLine.Default.Quit)
                {
                    End();
                }

                WpfSingleInstance.SecondInstanceCallback += SecondInstanceExecuted;
                IsExecutedByUser = !CommandLine.Default.AutoRun;
                Timeline.DesiredFrameRateProperty.OverrideMetadata(
                    typeof(Timeline),
                    new FrameworkPropertyMetadata { DefaultValue = 60 });
                RunApp();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        ///     The second instance executed.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        public static void SecondInstanceExecuted(string args)
        {
            try
            {
                if (CommandLine.GetByArguments(args).Quit)
                {
                    End();
                }

                if (((MainWindow)Current.MainWindow).IsHidden)
                {
                    ((MainWindow)Current.MainWindow).ShowForm();
                }

                Current.MainWindow.Activate();
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region Methods

        internal static void InitJumpList()
        {
            jumpList = JumpList.CreateJumpList();
            jumpList.KnownCategoryToDisplay = JumpListKnownCategoryType.Neither;
            jumpList.ClearAllUserTasks();
            string ourFileName = Assembly.GetEntryAssembly().Location;
            JumpListLink quitLink = new JumpListLink(ourFileName, "Quit PeaRoxy")
                                        {
                                            Arguments = "--quit",
                                        };
            jumpList.AddUserTasks(new JumpListTask[] { quitLink });
            jumpList.Refresh();
        }

        /// <summary>
        ///     The run app.
        /// </summary>
        private static void RunApp()
        {
            try
            {
                // Application Services
                ApplicationRestartRecoveryManager.RegisterForApplicationRestart(
                    new RestartSettings(string.Empty, RestartRestrictions.None));
            }
            catch (Exception)
            {
            }
            try
            {
                Notify = new NotifyIcon { Visible = false };
                defualtApp.InitializeComponent();
                defualtApp.DispatcherUnhandledException += (s, e) =>
                {
                    VDialog.Show(
                        e.Exception.Message + "\r\n\r\n" + e.Exception.StackTrace,
                        "FATAL ERROR",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    e.Handled = false;
                    End();
                };
                defualtApp.Run();
            }
            catch (Exception)
            {
            }

            End();
        }

        #endregion
    }
}