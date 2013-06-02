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
    /// Interaction logic for LocalProxy.xaml
    /// </summary>
    public partial class LocalProxy : Base
    {
        public LocalProxy()
        {
            InitializeComponent();
        }

        public override void SaveSettings()
        {
            if (isLoading) return;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_Port = (ushort)s_localProxyServerPort.Value;

            if ((bool)cb_localProxyServerAddressAny.IsChecked)
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_Address = "*";
            else
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_Address = txt_localProxyServerAddress.Text;

            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_SOCKS = (bool)cb_localProxyServerSOCKS.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_HTTP = (bool)cb_localProxyServerHTTP.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_HTTPS = (bool)cb_localProxyServerHTTPS.IsChecked;

            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.AutoConfig_Enable = (bool)cb_autoConfigScriptEnable.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.AutoConfig_KeepRuning = (bool)cb_autoConfigScriptKeepRunning.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.AutoConfig_Address = txt_autoConfigScriptAddress.Text;


            if ((bool)rb_autoConfigScriptNSMime.IsChecked)
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.AutoConfig_Mime = 1;
            else if ((bool)rb_autoConfigScriptJSMime.IsChecked)
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.AutoConfig_Mime = 2;

            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Save();

        }

        private void s_localProxyServerPort_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txt_localProxyServerPort.Text = ((ushort)s_localProxyServerPort.Value).ToString();
        }

        private void txt_localProxyServerPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            ushort port;
            if (!ushort.TryParse(txt_localProxyServerPort.Text, out port))
            {
                VDialog.Show("Port Value is not acceptable.", "Data Validation", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                port = 1080;
            }
            lbl_autoConfigScriptPreAddress_Refresh(null, null);
            txt_localProxyServerPort.Text = port.ToString();
            s_localProxyServerPort.Value = port;
        }

        private void txt_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }
       
        private void cb_localProxyServerAddressAny_Checked(object sender, RoutedEventArgs e)
        {
            txt_localProxyServerAddress.IsEnabled = !(bool)cb_localProxyServerAddressAny.IsChecked;
            lbl_autoConfigScriptPreAddress_Refresh(null, null);
            SaveSettings();
        }
        private void txt_localProxyServerAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            System.Net.IPAddress ip = null;
            if (txt_localProxyServerAddress.Text.Split('.').Length != 4 || !System.Net.IPAddress.TryParse(txt_localProxyServerAddress.Text, out ip))
            {
                txt_localProxyServerAddress.Text = "127.0.0.1";
                VDialog.Show("IP address is not acceptable.", "Data Validation", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep((int)(10));
                    this.Dispatcher.Invoke((App.SimpleVoid_Delegate)delegate()
                    {
                        txt_localProxyServerAddress.Focus();
                    }, new object[] { });
                }) { IsBackground = true }.Start();
            }
            else
                txt_localProxyServerAddress.Text = ip.ToString();

            SaveSettings();
        }
        private void cb_autoConfigScriptEnable_CheckedChanged(object sender, RoutedEventArgs e)
        {
            cb_autoConfigScriptKeepRunning.IsEnabled = (bool)cb_autoConfigScriptEnable.IsChecked;
            txt_autoConfigScriptAddress.IsEnabled = (bool)cb_autoConfigScriptEnable.IsChecked;
            lbl_autoConfigScriptPreAddress.IsEnabled = (bool)cb_autoConfigScriptEnable.IsChecked;
            rb_autoConfigScriptNSMime.IsEnabled = (bool)cb_autoConfigScriptEnable.IsChecked;
            rb_autoConfigScriptJSMime.IsEnabled = (bool)cb_autoConfigScriptEnable.IsChecked;
            cb_localProxyServerHTTP.IsEnabled = !(bool)cb_autoConfigScriptEnable.IsChecked;
            cb_localProxyServerHTTP.IsChecked = (bool)cb_autoConfigScriptEnable.IsChecked || (bool)cb_localProxyServerHTTP.IsChecked;
            SaveSettings();
        }

        private void lbl_autoConfigScriptPreAddress_Refresh(object sender, EventArgs e)
        {
            if (txt_localProxyServerPort == null || txt_localProxyServerAddress == null || lbl_autoConfigScriptPreAddress == null)
                return;
            lbl_autoConfigScriptPreAddress.Content = "http://" + (((bool)cb_localProxyServerAddressAny.IsChecked) ? Environment.MachineName : txt_localProxyServerAddress.Text) + ":" + txt_localProxyServerPort.Text + "/";
        }

        public override void LoadSettings()
        {
            isLoading = true;
            s_localProxyServerPort.Value = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_Port;
            if (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_Address == "*")
                cb_localProxyServerAddressAny.IsChecked = true;
            else
                txt_localProxyServerAddress.Text = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_Address;

            cb_localProxyServerSOCKS.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_SOCKS;
            cb_localProxyServerHTTP.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_HTTP;
            cb_localProxyServerHTTPS.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_HTTPS;

            cb_autoConfigScriptEnable.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.AutoConfig_Enable;
            cb_autoConfigScriptKeepRunning.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.AutoConfig_KeepRuning;
            txt_autoConfigScriptAddress.Text = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.AutoConfig_Address;

            rb_autoConfigScriptJSMime.IsChecked = false;
            rb_autoConfigScriptNSMime.IsChecked = false;
            switch (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.AutoConfig_Mime)
            {
                case 1:
                    rb_autoConfigScriptNSMime.IsChecked = true;
                    break;
                case 2:
                    rb_autoConfigScriptJSMime.IsChecked = true;
                    break;
            }
            isLoading = false;
        }
    }
}
