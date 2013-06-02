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
    /// Interaction logic for Protocol.xaml
    /// </summary>
    public partial class Protocol : Base
    {
        public Protocol()
        {
            InitializeComponent();
        }

        public override void SaveSettings()
        {
            if (isLoading) return;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_NoDataTimeout = Convert.ToUInt16(txt_Connection_NoDataTimeout.Text);
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_SendPacketSize = Convert.ToUInt32(txt_Connection_SendPacketSize.Text);
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_RecPacketSize = Convert.ToUInt32(txt_Connection_RecPacketSize.Text);
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_StopOnInterrupt = (bool)cb_Connection_stopOninterrupt.IsChecked;

            if ((bool)rb_Connection_EncryptionNone.IsChecked)
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_Encryption = 0;
            else if ((bool)rb_Connection_EncryptionTripleDES.IsChecked)
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_Encryption = 1;
            else if ((bool)rb_Connection_EncryptionSimpleXor.IsChecked)
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_Encryption = 2;

            if ((bool)rb_Connection_CompressionNone.IsChecked)
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_Compression = 0;
            else if ((bool)rb_Connection_CompressiongZip.IsChecked)
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_Compression = 1;
            else if ((bool)rb_Connection_CompressionDeflate.IsChecked)
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_Compression = 2;

            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Save();
        }

        private void txt_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void txt_Connection_SendPacketSize_LostFocus(object sender, RoutedEventArgs e)
        {
            uint u;
            if (!uint.TryParse(txt_Connection_SendPacketSize.Text, out u))
            {
                VDialog.Show("Value is not acceptable.", "Data Validation", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                txt_Connection_SendPacketSize.Text = 1024.ToString();
                new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep((int)(10));
                    this.Dispatcher.Invoke((App.SimpleVoid_Delegate)delegate()
                    {
                        txt_Connection_SendPacketSize.Focus();
                    }, new object[] { });
                }) { IsBackground = true }.Start();
            }
            else
                txt_Connection_SendPacketSize.Text = u.ToString();

            SaveSettings();
        }

        private void txt_Connection_RecPacketSize_LostFocus(object sender, RoutedEventArgs e)
        {
            uint u;
            if (!uint.TryParse(txt_Connection_RecPacketSize.Text, out u))
            {
                VDialog.Show("Value is not acceptable.", "Data Validation", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                txt_Connection_RecPacketSize.Text = 10240.ToString();
                new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep((int)(10));
                    this.Dispatcher.Invoke((App.SimpleVoid_Delegate)delegate()
                    {
                        txt_Connection_RecPacketSize.Focus();
                    }, new object[] { });
                }) { IsBackground = true }.Start();
            }
            else
                txt_Connection_RecPacketSize.Text = u.ToString();

            SaveSettings();
        }
        private void txt_Connection_NoDataTimeout_LostFocus(object sender, EventArgs e)
        {
            short u;
            if (!short.TryParse(txt_Connection_NoDataTimeout.Text, out u))
            {
                VDialog.Show("Value is not acceptable.", "Data Validation", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                txt_Connection_NoDataTimeout.Text = 600.ToString();
                new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep((int)(10));
                    this.Dispatcher.Invoke((App.SimpleVoid_Delegate)delegate()
                    {
                        txt_Connection_NoDataTimeout.Focus();
                    }, new object[] { });
                }) { IsBackground = true }.Start();
            }
            else
                txt_Connection_NoDataTimeout.Text = u.ToString();

            SaveSettings();
        }

        public override void LoadSettings()
        {
            isLoading = true;
            txt_Connection_NoDataTimeout.Text = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_NoDataTimeout.ToString();
            txt_Connection_SendPacketSize.Text = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_SendPacketSize.ToString();
            txt_Connection_RecPacketSize.Text = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_RecPacketSize.ToString();
            cb_Connection_stopOninterrupt.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_StopOnInterrupt;

            rb_Connection_EncryptionNone.IsChecked = false;
            rb_Connection_EncryptionTripleDES.IsChecked = false;
            rb_Connection_EncryptionSimpleXor.IsChecked = false;
            switch (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_Encryption)
            {
                case 0:
                    rb_Connection_EncryptionNone.IsChecked = true;
                    break;
                case 1:
                    rb_Connection_EncryptionTripleDES.IsChecked = true;
                    break;
                case 2:
                    rb_Connection_EncryptionSimpleXor.IsChecked = true;
                    break;
            }


            rb_Connection_CompressionNone.IsChecked = false;
            rb_Connection_CompressiongZip.IsChecked = false;
            rb_Connection_CompressionDeflate.IsChecked = false;
            switch (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_Compression)
            {
                case 0:
                    rb_Connection_CompressionNone.IsChecked = true;
                    break;
                case 1:
                    rb_Connection_CompressiongZip.IsChecked = true;
                    break;
                case 2:
                    rb_Connection_CompressionDeflate.IsChecked = true;
                    break;
            }

            isLoading = false;
        }
    }
}
