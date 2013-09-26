using LukeSw.Windows.Forms;
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
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Base
    {
        public About()
        {
            InitializeComponent();
            lbl_MainVersion.Content = lbl_MainVersion.Content.ToString().Replace("%version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            lbl_libraries.Content = "ClientLibrary.dll \r\n" +
                                    "CoreProtocol.dll \r\n" +
                                    "Windows.dll \r\n" +
                                    "Windows.Network.TAP.dll \r\n" +
                                    //"Windows.Network.Hook.exe \r\n" +
                                    "CommonLibrary.dll \r\n" +
                                    "Thriple.dll (3D Controls) \r\n" +
                                    "VDialog.dll (Win7 Dialogs) \r\n" +
                                    "EasyHook.dll \r\n" +
                                    "Tun2Socks \r\n" +
                                    "TAP Adapter \r\n" +
                                    "Images";
            lbl_versions.Content = "v" + System.Reflection.Assembly.GetAssembly(typeof(ClientLibrary.Proxy_Controller)).GetName().Version.ToString(3) + "\r\n" +
                                   "v" + System.Reflection.Assembly.GetAssembly(typeof(CoreProtocol.PeaRoxyProtocol)).GetName().Version.ToString(3) + "\r\n" +
                                   "v" + System.Reflection.Assembly.GetAssembly(typeof(Windows.WindowsModule)).GetName().Version.ToString(3) + "\r\n" +
                                   "v" + System.Reflection.Assembly.GetAssembly(typeof(Windows.Network.TAP.TapTunnel)).GetName().Version.ToString(3) + "\r\n" +
                                   //"v" + System.Reflection.Assembly.GetAssembly(typeof(Windows.Network.Hook.TapTunnel)).GetName().Version.ToString(3) + "\r\n" +
                                   "v" + System.Reflection.Assembly.GetAssembly(typeof(CommonLibrary.Common)).GetName().Version.ToString(3) + "\r\n" +
                                   "v" + System.Reflection.Assembly.GetAssembly(typeof(Thriple.Panels.Panel3D)).GetName().Version.ToString(3) + " © Josh Smith\r\n" +
                                   "v" + System.Reflection.Assembly.GetAssembly(typeof(VDialog)).GetName().Version.ToString(3) + " © Łukasz Świątkowski\r\n" +
                                   "v" + System.Reflection.Assembly.GetAssembly(typeof(EasyHook.RemoteHooking)).GetName().Version.ToString(3) + " © EasyHook Team\r\n" +
                                   "© Ambroz Bizjak\r\n" +
                                   "© OpenVPN.net\r\n" +
                                   "© Icons8.com";
        }

        private void btn_resetsettings_Click(object sender, RoutedEventArgs e)
        {
            if (VDialog.Show("This operation will reset all settings to default and close this application, are you sure?!", "Reset Settings", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Reset();
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.FirstRun = false;
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Save();
                VDialog.Show("Done. We will close this application now and then you can re-open and use it with default settings.", "Reset Settings", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                App.End();
            }
        }

        public override void SetEnable(bool enable)
        {
            ResetSettingsButton.IsEnabled = enable;
        }
    }
}
