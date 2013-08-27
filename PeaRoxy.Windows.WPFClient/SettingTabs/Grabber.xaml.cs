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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PeaRoxy.Windows.WPFClient.SettingTabs
{
    /// <summary>
    /// Interaction logic for Grabber.xaml
    /// </summary>
    public partial class Grabber : Base
    {
        public Grabber()
        {
            InitializeComponent();
        }

        private void txt_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void btn_hook_remove_Click(object sender, RoutedEventArgs e)
        {
            string[] pc = new string[lb_hook_processes.SelectedItems.Count];
            lb_hook_processes.SelectedItems.CopyTo(pc, 0);
            foreach (string s in pc)
            {
                lb_hook_processes.Items.Remove(s);
            }
            lb_hook_processes.SelectedItems.Clear();
            SaveSettings();
        }

        private void btn_hook_add_Click(object sender, RoutedEventArgs e)
        {

            Hook_AddItem();
        }

        private void txt_tap_ipaddress_LostFocus(object sender, RoutedEventArgs e)
        {
            System.Net.IPAddress ip = null;
            if (txt_tap_ipaddress.Text.Split('.').Length != 4 || !System.Net.IPAddress.TryParse(txt_tap_ipaddress.Text, out ip) || ip.GetAddressBytes()[3] != 0)
            {
                txt_tap_ipaddress.Text = "10.0.0.0";
                VDialog.Show("IP address is not acceptable.", "Data Validation", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep((int)(10));
                    this.Dispatcher.Invoke((App.SimpleVoid_Delegate)delegate()
                    {
                        txt_tap_ipaddress.Focus();
                    }, new object[] { });
                }) { IsBackground = true }.Start();
            }
            else
                txt_tap_ipaddress.Text = ip.ToString();

            SaveSettings();
        }

        private void cb_grabber_active_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveSettings();
        }

        public override void SetEnable(bool enable)
        {
            SettingsGrid.IsEnabled = enable;
        }

        public override void SaveSettings()
        {
            if (isLoading) return;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Grabber = ActiveGrabber.SelectedIndex;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.TAP_IPRange = txt_tap_ipaddress.Text;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Hook_IntoRuningProcesses = (bool)cb_hook_running.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Hook_Processes = "";
            foreach (string p in lb_hook_processes.Items)
            {
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Hook_Processes += p + Environment.NewLine;
            }
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Save();
        }

        public override void LoadSettings()
        {
            isLoading = true;
            cb_hook_running.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Hook_IntoRuningProcesses;

            List<string> Hook_Processes = new List<string>(
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Hook_Processes.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            lb_hook_processes.Items.Clear();
            foreach (string p in Hook_Processes)
            {
                lb_hook_processes.Items.Add(p);
            }

            txt_tap_ipaddress.Text = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.TAP_IPRange;
            ActiveGrabber.SelectedIndex = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Grabber;
            isLoading = false;
        }

        public void Hook_SaveItem()
        {
            try
            {
                string app = txt_hook_edit_app.Text.ToLower();
                if (app.Trim() != string.Empty)
                    lb_hook_processes.Items.Add(((ComboBoxItem)(cb_hook_edit_type.SelectedItem)).Tag.ToString() + app);
                SaveSettings();
                HideOptionsDialog();
            }
            catch (Exception e)
            {
                VDialog.Show("Can't add process name: " + e.Message, "PeaRoxy Client - Hook Processes Update", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
            }
        }


        public void Hook_AddItem()
        {
            txt_hook_edit_app.Text = ".exe";
            cb_hook_edit_type.SelectedIndex = 0;
            ShowOptionsDialog();
        }

        private void HideOptionsDialog()
        {
            grid_optionsdialog.IsEnabled = false;
            DoubleAnimation da_ShowS = new DoubleAnimation(0, -250, new Duration(TimeSpan.FromSeconds(0.5)));
            da_ShowS.EasingFunction = new ElasticEase();
            ((ElasticEase)da_ShowS.EasingFunction).EasingMode = EasingMode.EaseIn;
            ((ElasticEase)da_ShowS.EasingFunction).Oscillations = 1;
            TranslateTransform tt2 = new TranslateTransform();
            grid_optionsdialog.RenderTransform = tt2;
            tt2.BeginAnimation(TranslateTransform.YProperty, da_ShowS);
            new System.Threading.Thread(delegate()
            {
                System.Threading.Thread.Sleep(500);
                this.Dispatcher.Invoke((App.SimpleVoid_Delegate)delegate()
                {
                    gb_hook.IsEnabled =
                    gb_tap.IsEnabled =
                    ActiveGrabber.IsEditable =
                    lbl_grabber_active.IsEnabled = true;
                }, new object[] { });
            }) { IsBackground = true }.Start();
        }

        private void ShowOptionsDialog()
        {
            gb_hook.IsEnabled =
            gb_tap.IsEnabled =
            ActiveGrabber.IsEditable =
            lbl_grabber_active.IsEnabled = false;
            DoubleAnimation da_ShowS = new DoubleAnimation(-250, 0, new Duration(TimeSpan.FromSeconds(0.5)));
            da_ShowS.EasingFunction = new ElasticEase();
            ((ElasticEase)da_ShowS.EasingFunction).EasingMode = EasingMode.EaseOut;
            ((ElasticEase)da_ShowS.EasingFunction).Oscillations = 50;
            TranslateTransform tt2 = new TranslateTransform();
            grid_optionsdialog.RenderTransform = tt2;
            tt2.BeginAnimation(TranslateTransform.YProperty, da_ShowS);
            new System.Threading.Thread(delegate()
            {
                System.Threading.Thread.Sleep(1000);
                this.Dispatcher.Invoke((App.SimpleVoid_Delegate)delegate()
                {
                    grid_optionsdialog.IsEnabled = true;
                }, new object[] { });
            }) { IsBackground = true }.Start();
        }

        private void btn_hook_edit_cancel_Click(object sender, RoutedEventArgs e)
        {
            HideOptionsDialog();
        }

        private void btn_hook_edit_ok_Click(object sender, RoutedEventArgs e)
        {
            Hook_SaveItem();
        }
    }
}
