using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using LukeSw.Windows.Forms;
using System.Text;
using System.Windows.Shell;
using Microsoft.WindowsAPICodePack.ApplicationServices;

namespace PeaRoxy.Windows.WPFClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public delegate void SimpleVoid_Delegate();
        static App DefualtApp;
        public static System.Windows.Forms.NotifyIcon Notify { get; private set; }
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                App.Args = string.Join(" ", args).ToLower().Trim();
                DefualtApp = new App();
                SingleInstance.WpfSingleInstance.Make();
                if (App.Args.Contains("/quit"))
                {
                    Application.Current.Shutdown();
                    Environment.Exit(0);
                }
                SingleInstance.WpfSingleInstance.SecondInstanceCallback +=
                    new SingleInstance.WpfSingleInstance.SecondInstanceDelegate(SecondInstanceExecuted);
                App.isExecutedByUser = true;
                if (App.Args.Contains("/autorun"))
                    App.isExecutedByUser = false;
                App.RunApp();
            }
            catch (Exception) { }
        }

        public static void SecondInstanceExecuted(string args)
        {
            try
            {
                App.Args = args.ToLower().Trim();
                if (App.Args.Contains("/quit"))
                {
                    ((PeaRoxy.Windows.WPFClient.MainWindow)Application.Current.MainWindow).StopServer();
                    Application.Current.Shutdown();
                    Environment.Exit(0);
                }
                if (((PeaRoxy.Windows.WPFClient.MainWindow)Application.Current.MainWindow).isHidden)
                    ((PeaRoxy.Windows.WPFClient.MainWindow)Application.Current.MainWindow).ShowForm();
                ((PeaRoxy.Windows.WPFClient.MainWindow)Application.Current.MainWindow).Activate();
            }
            catch (Exception) { }
        }

        private static void RunApp()
        {
            try
            {
                // Application Services
                ApplicationRestartRecoveryManager.RegisterForApplicationRestart(
                    new RestartSettings(string.Empty, RestartRestrictions.None));
                Notify = new System.Windows.Forms.NotifyIcon();
                Notify.Visible = false;
                DefualtApp.InitializeComponent();
                DefualtApp.DispatcherUnhandledException += (s, e) =>
                {
                    VDialog.Show("FATAL Error: " + e.Exception.Message + "\r\n" + e.Exception.StackTrace, "Start Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    e.Handled = false;
                    App.End();
                };
                DefualtApp.Run();
            }
            catch (Exception) { }
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        public static void End()
        {
            ApplicationRestartRecoveryManager.UnregisterApplicationRestart();
            if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported)
            {
                Microsoft.WindowsAPICodePack.Taskbar.JumpList.CreateJumpList().ClearAllUserTasks();
                Microsoft.WindowsAPICodePack.Taskbar.JumpList.CreateJumpList().Refresh();
            }
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        public static string Args { get; private set; }
        public static bool isExecutedByUser { get; private set; }
    }
}
