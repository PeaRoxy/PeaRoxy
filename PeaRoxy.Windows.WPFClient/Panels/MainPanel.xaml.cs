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

namespace PeaRoxy.Windows.WPFClient
{
    /// <summary>
    /// Interaction logic for MainPanel.xaml
    /// </summary>
    public partial class MainPanel : UserControl
    {
        public new MainWindow Parent;
        public MainPanel()
        {
            InitializeComponent();
        }

        private void txt_serverAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txt_serverAddress.Text == "")
                return;
            try
            {
                txt_serverAddress.Text = txt_serverAddress.Text.Replace("\\", "/");
                if (txt_serverAddress.Text.IndexOf("://") == -1)
                    txt_serverAddress.Text = "http://" + txt_serverAddress.Text;
                Uri uri = new Uri(txt_serverAddress.Text);
                if (!(txt_serverAddress.Text.LastIndexOf(":") == -1 || txt_serverAddress.Text.LastIndexOf(":") == txt_serverAddress.Text.IndexOf(":")))
                    txt_serverPort.Text = uri.Port.ToString();
                txt_serverAddress.Text = uri.Host;
                txt_serverPort_TextChanged(sender, null);
            }
            catch (Exception ex)
            {
                VDialog.Show("Value is not acceptable.\r\n" + ex.Message, "Data Validation", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                txt_serverAddress.Text = "";
                new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep((int)(10));
                    this.Dispatcher.Invoke((App.SimpleVoid_Delegate)delegate()
                    {
                        txt_serverAddress.Focus();
                    }, new object[] { });
                }) { IsBackground = true }.Start();
            }

            SaveSettings();
        }

        private void txt_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void AnimatedExpender_Expanded(object sender, RoutedEventArgs e)
        {
            Expander ex = sender as Expander;
            if (ex.Parent.GetType().Name == typeof(WrapPanel).Name)
            {
                WrapPanel wr = ex.Parent as WrapPanel;
                foreach (var exP in wr.Children)
                {
                    Expander wEx = exP as Expander;
                    if (!wEx.Equals(ex) && wEx.IsExpanded == true)
                        wEx.IsExpanded = false;
                }
                if (wr.Equals(wp_Servers))
                {
                    if (ex_self.IsExpanded)
                    {
                        ex_usernameandpass.IsEnabled = false;
                        if (!ex_open.IsExpanded)
                            ex_open.IsExpanded = true;
                    }
                    else if (ex_server.IsExpanded)
                        ex_usernameandpass.IsEnabled = true;
                    else if (ex_web.IsExpanded)
                        ex_usernameandpass.IsEnabled = true;
                    else if (ex_proxy.IsExpanded)
                        ex_usernameandpass.IsEnabled = true;
                    else if (ex_ns.IsExpanded)
                        ex_usernameandpass.IsEnabled = true;
                }
            }
            if (ex.Content == null)
                return;

            DoubleAnimation da = new DoubleAnimation(25, ((Grid)ex.Content).Height + 25, new Duration(TimeSpan.FromSeconds(0.4)));
            ex.BeginAnimation(Button.HeightProperty, da);

            SaveSettings();
        }



        private void AnimatedExpender_Collapsed(object sender, RoutedEventArgs e)
        {
            Expander ex = sender as Expander;
            if (ex.Parent.GetType().Name == typeof(WrapPanel).Name)
            {
                WrapPanel wr = ex.Parent as WrapPanel;
                bool OnIsEnable = false;
                foreach (var exP in wr.Children)
                {
                    Expander wEx = exP as Expander;
                    if (!wEx.Equals(ex) && wEx.IsExpanded == true)
                        OnIsEnable = true;
                }
                if (!OnIsEnable)
                {
                    ex.IsExpanded = true;
                    return;
                }
            }

            if (ex.Content == null)
                return;

            DoubleAnimation da = new DoubleAnimation(((Grid)ex.Content).Height + 25, 25, new Duration(TimeSpan.FromSeconds(0.2)));
            ex.BeginAnimation(Button.HeightProperty, da);

        }


        private void txt_web_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txt_web.Text == "")
                return;
            try
            {
                txt_web.Text = txt_web.Text.Replace("\\", "/");
                if (txt_web.Text.IndexOf("://") == -1)
                    txt_web.Text = "http://" + txt_web.Text;
                Uri uri = new Uri(txt_web.Text);
                if (uri.Scheme != Uri.UriSchemeHttp)
                    throw new Exception("PeaRoxyWeb: Supporting only HTTP protocol.");
                txt_web.Text = uri.ToString();
            }
            catch (Exception ex)
            {
                VDialog.Show("Value is not acceptable.\r\n" + ex.Message, "Data Validation", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                txt_web.Text = "";
                new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep((int)(10));
                    this.Dispatcher.Invoke((App.SimpleVoid_Delegate)delegate()
                    {
                        txt_web.Focus();
                    }, new object[] { });
                }) { IsBackground = true }.Start();
            }

            SaveSettings();
        }

        private void txt_proxyAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txt_proxyAddress.Text == "")
                return;
            try
            {
                txt_proxyAddress.Text = txt_proxyAddress.Text.Replace("\\", "/");
                if (txt_proxyAddress.Text.IndexOf("://") == -1)
                    txt_proxyAddress.Text = "http://" + txt_proxyAddress.Text;
                Uri uri = new Uri(txt_proxyAddress.Text);
                if (!(txt_proxyAddress.Text.LastIndexOf(":") == -1 || txt_proxyAddress.Text.LastIndexOf(":") == txt_proxyAddress.Text.IndexOf(":")))
                    txt_proxyPort.Text = uri.Port.ToString();
                txt_proxyAddress.Text = uri.Host;
                txt_proxyPort_TextChanged(sender, null);
            }
            catch (Exception ex)
            {
                VDialog.Show("Value is not acceptable.\r\n" + ex.Message, "Data Validation", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                txt_proxyAddress.Text = "";
                new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep((int)(10));
                    this.Dispatcher.Invoke((App.SimpleVoid_Delegate)delegate()
                    {
                        txt_proxyAddress.Focus();
                    }, new object[] { });
                }) { IsBackground = true }.Start();
            }

            SaveSettings();
        }

        private void txt_proxyPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            ushort port;
            if (!ushort.TryParse(txt_proxyPort.Text, out port))
            {
                VDialog.Show("Port Value is not acceptable.", "Data Validation", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                port = 1080;
            }

            txt_proxyPort.Text = port.ToString();
            s_proxyPort.Value = port;
        }

        private void s_proxyPort_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txt_proxyPort.Text = ((ushort)s_proxyPort.Value).ToString();
        }

        private void s_serverPort_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txt_serverPort.Text = ((ushort)s_serverPort.Value).ToString();
        }
        private void txt_serverPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            ushort port;
            if (!ushort.TryParse(txt_serverPort.Text, out port))
            {
                VDialog.Show("Port Value is not acceptable.", "Data Validation", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                port = 1080;
            }

            txt_serverPort.Text = port.ToString();
            s_serverPort.Value = port;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Parent = (MainWindow)Window.GetWindow(this);
            btn_ex_pearoxy.ToolTip = new UserControls.Tooltip("PeaRoxy Server",
                "PeaRoxy Server is part of PeaRoxy Suit and best available proxy solution supported by this software." + Environment.NewLine +
                "" + Environment.NewLine +
                "[Advantages:]" + Environment.NewLine +
                "-- Supports encryption and compression" + Environment.NewLine +
                "-- Data can not be retrieved by firewalls" + Environment.NewLine +
                "-- Undetectable, Traffic transfers like normal HTTP traffic" + Environment.NewLine +
                "" + Environment.NewLine +
                "[Disadvantages:]" + Environment.NewLine +
                "-- Non known"
                );

            btn_ex_web.ToolTip = new UserControls.Tooltip("PeaRoxy Web (PHPear, ASPear)",
                "PeaRoxy Web is part of PeaRoxy Suit as a lightweight version of PeaRoxy Server but with more limitations." + Environment.NewLine +
                "" + Environment.NewLine +
                "[Advantages:]" + Environment.NewLine +
                "-- Supports encryption" + Environment.NewLine +
                "-- Data can not be retrieved by firewalls easily" + Environment.NewLine +
                "-- Undetectable, Traffic transfers like normal HTTP traffic" + Environment.NewLine +
                "" + Environment.NewLine +
                "[Disadvantages:]" + Environment.NewLine +
                "-- No support for non-http connections" + Environment.NewLine +
                "-- SmartPear limitation (No support for HTTPS SmartPear)" + Environment.NewLine +
                "-- Incompatible with TAP Adapter" + Environment.NewLine +
                "-- Low security for HTTPS connections"
                );

            btn_ex_proxy.ToolTip = new UserControls.Tooltip("Proxy Server (HTTPS, SOCKS 5)",
                "Proxy servers are most common, basic and popular way of forwarding traffic through firewalls and filtering systems." + Environment.NewLine +
                "" + Environment.NewLine +
                "[Advantages:]" + Environment.NewLine +
                "-- There are lot of providers out there!" + Environment.NewLine +
                "-- Lot of free (but poor quality) servers" + Environment.NewLine +
                "" + Environment.NewLine +
                "[Disadvantages:]" + Environment.NewLine +
                "-- No encryption support for data of password" + Environment.NewLine +
                "-- Can be blocked or limited by firewalls of filtering systems" + Environment.NewLine +
                "-- Data can be retrieved by hackers and firewalls"
                );

            btn_ex_self.ToolTip = new UserControls.Tooltip("Direct",
                "Send traffic directly via your internet connection."
                );

            ToolTipService.SetInitialShowDelay(btn_ex_pearoxy, 0);
            ToolTipService.SetShowDuration(btn_ex_pearoxy, 60000);

            ToolTipService.SetInitialShowDelay(btn_ex_web, 0);
            ToolTipService.SetShowDuration(btn_ex_web, 60000);

            ToolTipService.SetInitialShowDelay(btn_ex_proxy, 0);
            ToolTipService.SetShowDuration(btn_ex_proxy, 60000);

            ToolTipService.SetInitialShowDelay(btn_ex_self, 0);
            ToolTipService.SetShowDuration(btn_ex_self, 60000);
        }

        public void SaveSettings()
        {
            if (Parent == null || !Parent.inFormLoaded)
                return;

            if (ex_self.IsExpanded)
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Server_Type = 0;
            else if (ex_server.IsExpanded)
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Server_Type = 1;
            else if (ex_web.IsExpanded)
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Server_Type = 2;
            else if (ex_proxy.IsExpanded)
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Server_Type = 3;
            else if (ex_ns.IsExpanded)
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Server_Type = 4;

            if ((bool)rb_proxyType_https.IsChecked)
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ProxyServer_Type = 0;
            else if ((bool)rb_proxyType_socket.IsChecked)
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ProxyServer_Type = 1;

            if (ex_open.IsExpanded)
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Auth_Type = 0;
            else if (ex_usernameandpass.IsExpanded)
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Auth_Type = 2;

            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ProxyServer_Address = txt_proxyAddress.Text;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ProxyServer_Port = (ushort)s_proxyPort.Value;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.PeaRoxySocks_Address = txt_serverAddress.Text;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.PeaRoxySocks_Port = (ushort)s_serverPort.Value;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.PeaRoxyWeb_Address = txt_web.Text;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.PeaRoxySocks_Domain = txt_serverdomain.Text;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.UserAndPassword_User = txt_username.Text;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.UserAndPassword_Pass = txt_password.Password;
            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Save();
        }

        public void ReloadSettings()
        {
            switch (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Server_Type)
            {
                case 0:
                    ex_self.IsExpanded = true;
                    break;
                case 1:
                    ex_server.IsExpanded = true;
                    break;
                case 2:
                    ex_web.IsExpanded = true;
                    break;
                case 3:
                    ex_proxy.IsExpanded = true;
                    break;
                case 4:
                    ex_ns.IsExpanded = true;
                    break;
            }
            switch (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ProxyServer_Type)
            {
                case 0:
                    rb_proxyType_https.IsChecked = true;
                    rb_proxyType_socket.IsChecked = false;
                    break;
                case 1:
                    rb_proxyType_https.IsChecked = false;
                    rb_proxyType_socket.IsChecked = true;
                    break;
            }
            switch (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Auth_Type)
            {
                case 0:
                    ex_open.IsExpanded = true;
                    break;
                case 2:
                    ex_usernameandpass.IsExpanded = true;
                    break;
            }
            txt_proxyAddress.Text = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ProxyServer_Address;
            s_proxyPort.Value = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ProxyServer_Port;
            txt_serverAddress.Text = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.PeaRoxySocks_Address;
            s_serverPort.Value = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.PeaRoxySocks_Port;
            txt_web.Text = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.PeaRoxyWeb_Address;
            txt_serverdomain.Text = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.PeaRoxySocks_Domain;
            txt_username.Text = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.UserAndPassword_User;
            txt_password.Password = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.UserAndPassword_Pass;
        }

        public bool ShowTitle
        {
            set
            {
                if (value)
                {
                    cc3.RotationDirection = Thriple.Controls.RotationDirection.BottomToTop;
                    cc3.BringBackSideIntoView();
                }
                else
                {
                    cc3.RotationDirection = Thriple.Controls.RotationDirection.TopToBottom;
                    cc3.BringFrontSideIntoView();
                }
            }
        }

        public bool isLoading
        {
            set { loadingBox.IsVisible = value; }
        }

        public bool Connecetd
        {
            set
            {
                DoubleAnimation da_ShowImage = new DoubleAnimation((!value).GetHashCode(), value.GetHashCode(), new Duration(TimeSpan.FromSeconds(0.5)));
                DoubleAnimation da_HideImage = new DoubleAnimation(value.GetHashCode(), (!value).GetHashCode(), TimeSpan.FromSeconds(0.5));
                if (img_Connected.Opacity != value.GetHashCode())
                    img_Connected.BeginAnimation(Image.OpacityProperty, da_ShowImage);
                if (img_Disconnected.Opacity != (!value).GetHashCode())
                    img_Disconnected.BeginAnimation(Image.OpacityProperty, da_HideImage);
                if (r_active.Opacity != value.GetHashCode())
                    r_active.BeginAnimation(Image.OpacityProperty, da_ShowImage);
                if (r_inactive.Opacity != (!value).GetHashCode())
                    r_inactive.BeginAnimation(Image.OpacityProperty, da_HideImage);
            }
        }
    }
}
