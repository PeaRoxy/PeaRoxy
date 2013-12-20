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
    /// Interaction logic for Log.xaml
    /// </summary>
    public partial class Log : Base
    {
        public Log()
        {
            InitializeComponent();
            PeaRoxy.ClientLibrary.ProxyController.NewLog += NewLog;
        }

        private void txt_TextBox_LostFocus(object sender, RoutedEventArgs e)
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
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ErrorRenderer_EnableHTTP = (bool)cb_log_errorreporting_http.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ErrorRenderer_Enable80 = (bool)cb_log_errorreporting_80.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ErrorRenderer_Enable443 = (bool)cb_log_errorreporting_443.IsChecked;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Save();
        }

        public override void LoadSettings()
        {
            isLoading = true;
            cb_log_errorreporting_http.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ErrorRenderer_EnableHTTP;
            cb_log_errorreporting_80.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ErrorRenderer_Enable80;
            cb_log_errorreporting_443.IsChecked = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ErrorRenderer_Enable443;
            isLoading = false;
        }

        private void lb_Log_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lb_Log.SelectionMode = SelectionMode.Multiple;
            lb_Log.SelectedItems.Clear();
        }

        private void NewLog(string message, EventArgs e)
        {
            lb_Log.Items.Add(message);
            try
            {
                if (System.Windows.Media.VisualTreeHelper.GetChildrenCount(lb_Log) > 0 &&
                    System.Windows.Media.VisualTreeHelper.GetChildrenCount(System.Windows.Media.VisualTreeHelper.GetChild(lb_Log, 0)) > 0)
                {
                    ScrollViewer scroll = System.Windows.Media.VisualTreeHelper.GetChild(System.Windows.Media.VisualTreeHelper.GetChild(lb_Log, 0), 0) as ScrollViewer;
                    scroll.ScrollToEnd();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
