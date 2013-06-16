using PeaRoxy.ClientLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PeaRoxy.Windows.WPFClient.SettingTabs
{
    /// <summary>
    /// Interaction logic for General.xaml
    /// </summary>
    public partial class General : Base
    {

        public General()
        {
            InitializeComponent();
        }

        public void UpdateStats(Proxy_Controller Listener, long currentDownSpeed, long currentUpSpeed)
        {
            lbl_stat_acceptingthreads.Content = Listener.AcceptingCycle.ToString() + " - " + Listener.WaitingAcceptionConnections;
            lbl_stat_activeconnections.Content = Listener.RoutingCycle.ToString() + " - " + Listener.RoutingConnections;
            lbl_stat_downloaded.Content = CommonLibrary.Common.FormatFileSizeAsString(Listener.BytesReceived);
            lbl_stat_uploaded.Content = CommonLibrary.Common.FormatFileSizeAsString(Listener.BytesSent);
            lbl_stat_downloadrate.Content = CommonLibrary.Common.FormatFileSizeAsString(currentDownSpeed);
            lbl_stat_uploadrate.Content = CommonLibrary.Common.FormatFileSizeAsString(currentUpSpeed);
        }

        private void cb_runAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            cb_openProgramAtStartup.IsEnabled = (bool)cb_runAtStartup.IsChecked;
            SaveSettings();
        }

        private void txt_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        public override void SetEnable(bool enable)
        {
            CheckBoxes.IsEnabled = enable;
        }

        public override void SaveSettings()
        {
            if (isLoading) return;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Startup_StartServer = (bool)cb_StartServerAtStartup.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Startup_ShowWindow = (bool)cb_openProgramAtStartup.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Save();
            try
            {
                if ((bool)cb_runAtStartup.IsChecked)
                    Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true).SetValue("PeaRoxy Client", "\"" + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName + "\" /autoRun");
                else
                    Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true).DeleteValue("PeaRoxy Client");
            }
            catch (Exception) { }
        }

        public override void LoadSettings()
        {
            isLoading = true;
            cb_StartServerAtStartup.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Startup_StartServer;
            cb_openProgramAtStartup.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Startup_ShowWindow;

            cb_runAtStartup.IsChecked = false;
            try
            {
                if (Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run").GetValue("PeaRoxy Client") != null)
                    cb_runAtStartup.IsChecked = true;
            }
            catch (Exception) { }
            isLoading = false;
        }
    }
}
