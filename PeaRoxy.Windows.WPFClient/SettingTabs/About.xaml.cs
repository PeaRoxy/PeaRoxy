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
                                    "Platform.dll \r\n" +
                                    "Windows.dll \r\n" +
                                    "Windows.Network.dll \r\n" +
                                    "CommonLibrary.dll \r\n" +
                                    "Thriple.dll (3D Controls) \r\n" +
                                    "VDialog.dll (Win7 Dialogs) \r\n" +
                                    "EasyHook.dll";
            lbl_versions.Content = "v" + System.Reflection.Assembly.GetAssembly(typeof(PeaRoxy.ClientLibrary.Proxy_Controller)).GetName().Version.ToString(3) + "\r\n" +
                                   "v" + System.Reflection.Assembly.GetAssembly(typeof(PeaRoxy.CoreProtocol.PeaRoxyProtocol)).GetName().Version.ToString(3) + "\r\n" +
                                   "v" + System.Reflection.Assembly.GetAssembly(typeof(PeaRoxy.Platform.ClassRegistry)).GetName().Version.ToString(3) + "\r\n" +
                                   "v" + System.Reflection.Assembly.GetAssembly(typeof(PeaRoxy.Windows.WindowsModule)).GetName().Version.ToString(3) + "\r\n" +
                                   "v" + System.Reflection.Assembly.GetAssembly(typeof(PeaRoxy.Windows.Network.TapTunnel)).GetName().Version.ToString(3) + "\r\n" +
                                   "v" + System.Reflection.Assembly.GetAssembly(typeof(PeaRoxy.CommonLibrary.Common)).GetName().Version.ToString(3) + "\r\n" +
                                   "v" + System.Reflection.Assembly.GetAssembly(typeof(Thriple.Panels.Panel3D)).GetName().Version.ToString(3) + " © Josh Smith\r\n" +
                                   "v" + System.Reflection.Assembly.GetAssembly(typeof(VDialog)).GetName().Version.ToString(3) + " © Łukasz Świątkowski\r\n" +
                                   "v" + System.Reflection.Assembly.GetAssembly(typeof(EasyHook.RemoteHooking)).GetName().Version.ToString(3) + " © EasyHook Team"; ;
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
