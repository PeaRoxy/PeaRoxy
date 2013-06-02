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
    /// Interaction logic for LocalDNS.xaml
    /// </summary>
    public partial class LocalDNS : Base
    {
        public LocalDNS()
        {
            InitializeComponent();
        }

        private void cb_dns_enable_Checked(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void txt_dns_ipaddress_LostFocus(object sender, RoutedEventArgs e)
        {
            System.Net.IPAddress ip = null;
            if (txt_dns_ipaddress.Text.Split('.').Length != 4 || !System.Net.IPAddress.TryParse(txt_dns_ipaddress.Text, out ip))
            {
                txt_dns_ipaddress.Text = "8.8.8.8";
                VDialog.Show("IP address is not acceptable.", "Data Validation", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep((int)(10));
                    this.Dispatcher.Invoke((App.SimpleVoid_Delegate)delegate()
                    {
                        txt_dns_ipaddress.Focus();
                    }, new object[] { });
                }) { IsBackground = true }.Start();
            }
            else
                txt_dns_ipaddress.Text = ip.ToString();

            SaveSettings();
        }

        public override void SaveSettings()
        {
            if (isLoading) return;
            lbl_dns_ipaddress.IsEnabled = (bool)cb_dns_enable.IsChecked;
            txt_dns_ipaddress.IsEnabled = (bool)cb_dns_enable.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.DNS_Enable = (bool)cb_dns_enable.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.DNS_IPAddress = txt_dns_ipaddress.Text;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Save();
        }

        public override void LoadSettings()
        {
            isLoading = true;
            txt_dns_ipaddress.Text = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.DNS_IPAddress;
            cb_dns_enable.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.DNS_Enable;
            isLoading = false;
        }
    }
}
