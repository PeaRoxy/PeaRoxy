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
    /// Interaction logic for SmartPear.xaml
    /// </summary>
    public partial class SmartPear : Base
    {
        public SmartPear()
        {
            InitializeComponent();
        }

        public override void SaveSettings()
        {
            if (isLoading) return;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTP_Enable = (bool)HTTPEnable.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTP_AutoRoute_Enable = (bool)cb_smart_http_autoroute.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTP_AutoRoute_Pattern = txt_smart_http_autoroute.Text;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_AntiDNS_Enable = (bool)cb_smart_antidns.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_AntiDNSPattern = txt_smart_antidns.Text;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTPS_Enable = (bool)HTTPSEnable.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_Timeout_Enable = (bool)cb_smart_timeout.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_Timeout_Value = Convert.ToUInt16(txt_smart_timeout.Text);
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_Direct_Port80Router = (bool)cb_smart_port80ashttp.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_SOCKS_Enable = (bool)SocketEnable.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_Direct_List = string.Empty;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTP_List = string.Empty;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_Direct_AutoRoutePort80AsHTTP = (bool)cb_smart_port80checkhttpautoroutepattern.IsChecked;
            foreach (string s in lb_smart.Items)
            {
                string ps = s.ToLower();
                if (ps.IndexOf("(http)") == 0)
                {
                    ps = ps.Substring(ps.IndexOf(") ") + 2);
                    PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTP_List += ps + Environment.NewLine;
                }
                if (ps.IndexOf("(direct)") == 0)
                {
                    ps = ps.Substring(ps.IndexOf(") ") + 2);
                    PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_Direct_List += ps + Environment.NewLine;
                }
            }

            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Save();
        }

        public void AddToSmartList(string item, bool https)
        {
            if (https)
                lb_smart.Items.Add("(Direct) " + item.ToLower());
            else
                lb_smart.Items.Add("(Http) " + item.ToLower());
            SaveSettings();
        }

        private void txt_smart_timeout_LostFocus(object sender, RoutedEventArgs e)
        {
            short u;
            if (!short.TryParse(txt_smart_timeout.Text, out u))
            {
                VDialog.Show("Value is not acceptable.", "Data Validation", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                txt_smart_timeout.Text = 20.ToString();
                new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep((int)(10));
                    this.Dispatcher.Invoke((App.SimpleVoid_Delegate)delegate()
                    {
                        txt_smart_timeout.Focus();
                    }, new object[] { });
                }) { IsBackground = true }.Start();
            }
            else
                txt_smart_timeout.Text = u.ToString();

            SaveSettings();
        }
        private void cb_smart_http_autoroute_Checked(object sender, RoutedEventArgs e)
        {
            txt_smart_http_autoroute.IsEnabled = (bool)cb_smart_http_autoroute.IsChecked;
            cb_smart_port80checkhttpautoroutepattern.IsChecked = cb_smart_port80checkhttpautoroutepattern.IsEnabled = (bool)cb_smart_port80ashttp.IsChecked && (bool)cb_smart_http_autoroute.IsChecked;
            SaveSettings();
        }
        private void cb_smart_timeout_Checked(object sender, RoutedEventArgs e)
        {
            txt_smart_timeout.IsEnabled = (bool)cb_smart_timeout.IsChecked;
            SaveSettings();
        }

        private void cb_smart_http_enable_Checked(object sender, RoutedEventArgs e)
        {
            txt_smart_http_autoroute.IsEnabled = (bool)HTTPEnable.IsChecked;
            cb_smart_http_autoroute.IsEnabled = (bool)HTTPEnable.IsChecked;
            cb_smart_port80ashttp.IsChecked = cb_smart_port80checkhttpautoroutepattern.IsChecked = cb_smart_port80ashttp.IsEnabled = cb_smart_port80checkhttpautoroutepattern.IsEnabled = (bool)HTTPSEnable.IsChecked && (bool)HTTPEnable.IsChecked;
            SaveSettings();
        }

        private void cb_smart_https_enable_Checked(object sender, RoutedEventArgs e)
        {
            cb_smart_port80ashttp.IsChecked = cb_smart_port80checkhttpautoroutepattern.IsChecked = cb_smart_port80ashttp.IsEnabled = cb_smart_port80checkhttpautoroutepattern.IsEnabled = (bool)HTTPSEnable.IsChecked && (bool)HTTPEnable.IsChecked;
            SocketEnable.IsEnabled = (bool)HTTPSEnable.IsChecked;
            SaveSettings();
        }

        private void cb_smart_port80ashttp_Checked(object sender, RoutedEventArgs e)
        {
            cb_smart_port80checkhttpautoroutepattern.IsChecked = cb_smart_port80checkhttpautoroutepattern.IsEnabled = (bool)cb_smart_port80ashttp.IsChecked && (bool)cb_smart_http_autoroute.IsChecked;
            SaveSettings();
        }

        private void btn_smart_remove_Click(object sender, RoutedEventArgs e)
        {
            string[] pc = new string[lb_smart.SelectedItems.Count];
            lb_smart.SelectedItems.CopyTo(pc, 0);
            foreach (string s in pc)
            {
                lb_smart.Items.Remove(s);
            }
            lb_smart.SelectedItems.Clear();
            SaveSettings();
        }

        private void txt_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void cb_smart_antidns_Checked(object sender, RoutedEventArgs e)
        {
            txt_smart_antidns.IsEnabled = (bool)cb_smart_antidns.IsChecked;
            SaveSettings();
        }

        private void btn_smart_edit_Click(object sender, RoutedEventArgs e)
        {
            Smart_EditItem();
        }

        private void btn_smart_add_Click(object sender, RoutedEventArgs e)
        {
            Smart_AddItem();
        }

        private void lb_smart_MouseDC(object sender, MouseButtonEventArgs e)
        {
            if (EditButton.IsEnabled)
                Smart_EditItem();
        }

        private void btn_smart_edit_cancel_Click(object sender, RoutedEventArgs e)
        {
            HideOptionsDialog();
        }

        private void btn_smart_edit_ok_Click(object sender, RoutedEventArgs e)
        {
            Smart_SaveOrAddItem();
        }

        public override void SetEnable(bool enable)
        {
            SettingsGrid.IsEnabled = grid_optionsdialog.IsEnabled = AddButton.IsEnabled = EditButton.IsEnabled = RemoveButton.IsEnabled = enable;
        }

        public override void LoadSettings()
        {
            isLoading = true;
            HTTPEnable.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTP_Enable;
            cb_smart_http_autoroute.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTP_AutoRoute_Enable;
            txt_smart_http_autoroute.Text = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTP_AutoRoute_Pattern;
            HTTPSEnable.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTPS_Enable;
            cb_smart_timeout.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_Timeout_Enable;
            txt_smart_timeout.Text = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_Timeout_Value.ToString();
            cb_smart_antidns.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_AntiDNS_Enable;
            txt_smart_antidns.Text = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_AntiDNSPattern.ToString();
            cb_smart_port80ashttp.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_Direct_Port80Router;
            SocketEnable.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_SOCKS_Enable;
            cb_smart_port80checkhttpautoroutepattern.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_Direct_AutoRoutePort80AsHTTP;
            FillSmartList();
            isLoading = false;
        }

        public void FillSmartList()
        {
            lb_smart.Items.Clear();
            List<string> HTTP_List_Analysed = PeaRoxy.ClientLibrary.SmartPear.Analyse_HTTP_List(
                new List<string>(PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTP_List.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)));
            List<string> HTTPS_List_Analysed = PeaRoxy.ClientLibrary.SmartPear.Analyse_Direct_List(
                new List<string>(PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_Direct_List.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)));
            foreach (string s in HTTP_List_Analysed)
            {
                lb_smart.Items.Add("(Http) " + s.ToLower());
            }
            foreach (string s in HTTPS_List_Analysed)
            {
                lb_smart.Items.Add("(Direct) " + s.ToLower());
            }
        }

        public void Smart_SaveOrAddItem()
        {
            try
            {
                string rule = txt_smart_edit_rule.Text.ToLower();
                string app = txt_smart_edit_app.Text.ToLower();
                if (app == string.Empty)
                    app = "*";
                if (rule.IndexOf("://") != -1)
                    rule = rule.Substring(rule.IndexOf("://") + 3);
                if (rule.IndexOf("|") != -1)
                    rule = rule.Substring(rule.IndexOf("|") + 1).Trim();
                rule = rule.Replace("\\", "/").Replace("//", "/").Replace("//", "/").Replace("//", "/");
                if (rule == string.Empty)
                    throw new Exception("Rule can not be empty.");
                bool https = false;
                switch (((ComboBoxItem)(cb_smart_edit_type.SelectedItem)).Tag.ToString().ToLower())
                {
                    case "direct":
                        https = true;
                        if (rule.IndexOf("/") != -1)
                            throw new Exception("Direct rules cannot contain slash.");
                        if (rule.IndexOf(":") == -1)
                            throw new Exception("Direct rules must have port number.");
                        break;
                    case "http":

                        break;
                    default:
                        return;
                }
                int cp = -1;
                if (txt_smart_edit_rule.Tag != null)
                {
                    cp = lb_smart.Items.IndexOf(txt_smart_edit_rule.Tag);
                    lb_smart.Items.RemoveAt(cp);
                }
                if (cp == -1)
                    cp = lb_smart.Items.Count;
                if (https)
                    lb_smart.Items.Insert(cp, "(Direct) " + app + " | " + rule);
                else
                    lb_smart.Items.Insert(cp, "(Http) " + app + " | " + rule);
                SaveSettings();
                HideOptionsDialog();
            }
            catch (Exception e)
            {
                VDialog.Show("Can't add or edit rule: " + e.Message, "PeaRoxy Client - Smart List Update", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
            }
        }

        public void Smart_EditItem()
        {
            string rule;
            if (lb_smart.SelectedItems.Count < 1)
                return;
            rule = lb_smart.SelectedItems[0].ToString();
            string ps = rule.ToLower();
            if (ps.IndexOf("(http)") == 0)
            {
                cb_smart_edit_type.SelectedItem = cb_smart_edit_type.Items[0];
            }
            if (ps.IndexOf("(direct)") == 0)
            {
                cb_smart_edit_type.SelectedItem = cb_smart_edit_type.Items[1];
            }
            txt_smart_edit_rule.Tag = rule;
            ps = ps.Substring(ps.IndexOf(") ") + 2);
            txt_smart_edit_app.Text = ps.Substring(0, ps.IndexOf(" | "));
            txt_smart_edit_rule.Text = ps.Substring(ps.IndexOf(" | ") + 3);
            ShowOptionsDialog();
        }
        public void Smart_AddItem()
        {
            txt_smart_edit_rule.Text = string.Empty;
            txt_smart_edit_app.Text = "*";
            txt_smart_edit_rule.Tag = null;
            cb_smart_edit_type.SelectedItem = cb_smart_edit_type.Items[0];
            ShowOptionsDialog();
        }

        private void HideOptionsDialog()
        {
            grid_optionsdialog.IsEnabled = false;
            DoubleAnimation da_ShowS = new DoubleAnimation(0, -250, new Duration(TimeSpan.FromSeconds(0.5)));
            Timeline.SetDesiredFrameRate(da_ShowS, 60); // 60 FPS
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
                    SettingsGrid.IsEnabled =
                    lb_smart.IsEnabled =
                    AddButton.IsEnabled =
                    EditButton.IsEnabled =
                    RemoveButton.IsEnabled = true;
                }, new object[] { });
            }) { IsBackground = true }.Start();
        }

        private void ShowOptionsDialog()
        {
            SettingsGrid.IsEnabled =
            lb_smart.IsEnabled = 
            AddButton.IsEnabled = 
            EditButton.IsEnabled = 
            RemoveButton.IsEnabled = false;
            DoubleAnimation da_ShowS = new DoubleAnimation(-250, 0, new Duration(TimeSpan.FromSeconds(0.5)));
            Timeline.SetDesiredFrameRate(da_ShowS, 60); // 60 FPS
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
    }
}
