// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainPanel.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for MainPanel.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.Panels
{
    #region

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Media.Animation;

    using LukeSw.Windows.Forms;

    using PeaRoxy.Windows.WPFClient.Properties;
    using PeaRoxy.Windows.WPFClient.UserControls;

    using Thriple.Controls;

    #endregion

    /// <summary>
    ///     Interaction logic for MainPanel.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public partial class MainPanel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPanel"/> class.
        /// </summary>
        public MainPanel()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Sets a value indicating whether connecetd.
        /// </summary>
        public bool Connecetd
        {
            set
            {
                DoubleAnimation showImageAnimation = new DoubleAnimation(
                    (!value).GetHashCode(), 
                    value.GetHashCode(), 
                    new Duration(TimeSpan.FromSeconds(0.5)));
                DoubleAnimation hideImageAnimation = new DoubleAnimation(
                    value.GetHashCode(), 
                    (!value).GetHashCode(), 
                    TimeSpan.FromSeconds(0.5));
                if (Math.Abs(this.ImgConnected.Opacity - value.GetHashCode()) > 0.01)
                {
                    this.ImgConnected.BeginAnimation(OpacityProperty, showImageAnimation);
                }

                if (Math.Abs(this.ImgDisconnected.Opacity - (!value).GetHashCode()) > 0.01)
                {
                    this.ImgDisconnected.BeginAnimation(OpacityProperty, hideImageAnimation);
                }

                if (Math.Abs(this.RActive.Opacity - value.GetHashCode()) > 0.01)
                {
                    this.RActive.BeginAnimation(OpacityProperty, showImageAnimation);
                }

                if (Math.Abs(this.RInactive.Opacity - (!value).GetHashCode()) > 0.01)
                {
                    this.RInactive.BeginAnimation(OpacityProperty, hideImageAnimation);
                }
            }
        }

        /// <summary>
        /// Sets a value indicating whether show title.
        /// </summary>
        public bool ShowTitle
        {
            set
            {
                if (value)
                {
                    this.TitleControl3D.RotationDirection = RotationDirection.BottomToTop;
                    this.TitleControl3D.BringBackSideIntoView();
                }
                else
                {
                    this.TitleControl3D.RotationDirection = RotationDirection.TopToBottom;
                    this.TitleControl3D.BringFrontSideIntoView();
                }
            }
        }

        /// <summary>
        /// Sets a value indicating whether is loading.
        /// </summary>
        public bool IsLoading
        {
            set
            {
                this.LoadingBox.IsVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        private new MainWindow Parent { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The reload settings.
        /// </summary>
        public void ReloadSettings()
        {
            switch (Settings.Default.Server_Type)
            {
                case 0:
                    this.ExSelf.IsExpanded = true;
                    break;
                case 1:
                    this.ExServer.IsExpanded = true;
                    break;
                case 2:
                    this.ExWeb.IsExpanded = true;
                    break;
                case 3:
                    this.ExProxy.IsExpanded = true;
                    break;
            }

            switch (Settings.Default.ProxyServer_Type)
            {
                case 0:
                    this.RbProxyTypeHttps.IsChecked = true;
                    this.RbProxyTypeSocket.IsChecked = false;
                    break;
                case 1:
                    this.RbProxyTypeHttps.IsChecked = false;
                    this.RbProxyTypeSocket.IsChecked = true;
                    break;
            }

            switch (Settings.Default.Auth_Type)
            {
                case 0:
                    this.ExOpen.IsExpanded = true;
                    break;
                case 2:
                    this.ExUsernameAndPass.IsExpanded = true;
                    break;
            }

            this.TxtProxyAddress.Text = Settings.Default.ProxyServer_Address;
            this.TxtServerAddress.Text = Settings.Default.PeaRoxySocks_Address;
            this.TxtWeb.Text = Settings.Default.PeaRoxyWeb_Address;
            this.TxtServerdomain.Text = Settings.Default.PeaRoxySocks_Domain;
            this.TxtUsername.Text = Settings.Default.UserAndPassword_User;
            this.TxtPassword.Password = Settings.Default.UserAndPassword_Pass;
        }

        /// <summary>
        /// The save settings.
        /// </summary>
        public void SaveSettings()
        {
            if (this.Parent == null || !this.Parent.IsFormLoaded)
            {
                return;
            }

            if (this.ExSelf.IsExpanded)
            {
                Settings.Default.Server_Type = 0;
            }
            else if (this.ExServer.IsExpanded)
            {
                Settings.Default.Server_Type = 1;
            }
            else if (this.ExWeb.IsExpanded)
            {
                Settings.Default.Server_Type = 2;
            }
            else if (this.ExProxy.IsExpanded)
            {
                Settings.Default.Server_Type = 3;
            }

            if (this.RbProxyTypeHttps.IsChecked ?? false)
            {
                Settings.Default.ProxyServer_Type = 0;
            }
            else if (this.RbProxyTypeSocket.IsChecked ?? false)
            {
                Settings.Default.ProxyServer_Type = 1;
            }

            if (this.ExOpen.IsExpanded)
            {
                Settings.Default.Auth_Type = 0;
            }
            else if (this.ExUsernameAndPass.IsExpanded)
            {
                Settings.Default.Auth_Type = 2;
            }

            Settings.Default.ProxyServer_Address = this.TxtProxyAddress.Text;
            Settings.Default.ProxyServer_Port = Convert.ToUInt16(this.TxtProxyPort.Text);
            Settings.Default.PeaRoxySocks_Address = this.TxtServerAddress.Text;
            Settings.Default.PeaRoxySocks_Port = Convert.ToUInt16(this.TxtServerPort.Text);
            Settings.Default.PeaRoxyWeb_Address = this.TxtWeb.Text;
            Settings.Default.PeaRoxySocks_Domain = this.TxtServerdomain.Text;
            Settings.Default.UserAndPassword_User = this.TxtUsername.Text;
            Settings.Default.UserAndPassword_Pass = this.TxtPassword.Password;
            Settings.Default.Save();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The animated expender_ collapsed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void AnimatedExpenderCollapsed(object sender, RoutedEventArgs e)
        {
            Expander ex = sender as Expander;
            if (ex != null && ex.Parent.GetType().Name == typeof(WrapPanel).Name)
            {
                WrapPanel wr = ex.Parent as WrapPanel;
                bool onIsEnable = false;
                if (wr != null)
                {
                    foreach (var exP in wr.Children)
                    {
                        Expander wEx = exP as Expander;
                        if (wEx != null && (!wEx.Equals(ex) && wEx.IsExpanded))
                        {
                            onIsEnable = true;
                        }
                    }
                }

                if (!onIsEnable)
                {
                    ex.IsExpanded = true;
                    return;
                }
            }

            if (ex == null || ex.Content == null)
            {
                return;
            }

            DoubleAnimation animation = new DoubleAnimation(
                ((Grid)ex.Content).Height + 25, 
                25, 
                new Duration(TimeSpan.FromSeconds(0.2)));
            ex.BeginAnimation(HeightProperty, animation);
        }

        /// <summary>
        /// The animated expender_ expanded.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void AnimatedExpenderExpanded(object sender, RoutedEventArgs e)
        {
            Expander ex = sender as Expander;
            if (ex != null && ex.Parent.GetType().Name == typeof(WrapPanel).Name)
            {
                WrapPanel wr = ex.Parent as WrapPanel;
                if (wr != null)
                {
                    foreach (var exP in wr.Children)
                    {
                        Expander wEx = exP as Expander;
                        if (wEx != null && (!wEx.Equals(ex) && wEx.IsExpanded))
                        {
                            wEx.IsExpanded = false;
                        }
                    }

                    if (wr.Equals(this.WpServers))
                    {
                        if (this.ExSelf.IsExpanded)
                        {
                            this.ExUsernameAndPass.IsEnabled = false;
                            if (!this.ExOpen.IsExpanded)
                            {
                                this.ExOpen.IsExpanded = true;
                            }
                        }
                        else if (this.ExServer.IsExpanded)
                        {
                            this.ExUsernameAndPass.IsEnabled = true;
                        }
                        else if (this.ExWeb.IsExpanded)
                        {
                            this.ExUsernameAndPass.IsEnabled = true;
                        }
                        else if (this.ExProxy.IsExpanded)
                        {
                            this.ExUsernameAndPass.IsEnabled = true;
                        }
                    }
                }
            }

            if (ex == null || ex.Content == null)
            {
                return;
            }

            DoubleAnimation animation = new DoubleAnimation(
                25, 
                ((Grid)ex.Content).Height + 25, 
                new Duration(TimeSpan.FromSeconds(0.4)));
            ex.BeginAnimation(HeightProperty, animation);

            this.SaveSettings();
        }

        /// <summary>
        /// The user control_ loaded.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "Reviewed. Suppression is OK here.")]
        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            this.Parent = (MainWindow)Window.GetWindow(this);
            this.btn_ex_pearoxy.ToolTip = new Tooltip(
                "PeaRoxy Server", 
                "PeaRoxy Server is part of PeaRoxy Suit and best available proxy solution supported by this software."
                + Environment.NewLine + string.Empty + Environment.NewLine + "[Advantages:]" + Environment.NewLine
                + "-- Supports encryption and compression" + Environment.NewLine
                + "-- Data can not be retrieved by firewalls" + Environment.NewLine
                + "-- Undetectable, Traffic transfers like normal HTTP traffic" + Environment.NewLine + string.Empty
                + Environment.NewLine + "[Disadvantages:]" + Environment.NewLine + "-- Non known");

            this.BtnExWeb.ToolTip = new Tooltip(
                "PeaRoxy Web (PHPear, ASPear)", 
                "PeaRoxy Web is part of PeaRoxy Suit as a lightweight version of PeaRoxy Server but with more limitations."
                + Environment.NewLine + string.Empty + Environment.NewLine + "[Advantages:]" + Environment.NewLine
                + "-- Supports encryption" + Environment.NewLine + "-- Data can not be retrieved by firewalls easily"
                + Environment.NewLine + "-- Undetectable, Traffic transfers like normal HTTP traffic"
                + Environment.NewLine + string.Empty + Environment.NewLine + "[Disadvantages:]" + Environment.NewLine
                + "-- No support for non-http connections" + Environment.NewLine
                + "-- SmartPear limitation (No support for HTTPS SmartPear)" + Environment.NewLine
                + "-- Incompatible with TAP Adapter" + Environment.NewLine + "-- Low security for HTTPS connections");

            this.BtnExProxy.ToolTip = new Tooltip(
                "Proxy Server (HTTPS, SOCKS 5)", 
                "Proxy servers are most common, basic and popular way of forwarding traffic through firewalls and filtering systems."
                + Environment.NewLine + string.Empty + Environment.NewLine + "[Advantages:]" + Environment.NewLine
                + "-- There are lot of providers out there!" + Environment.NewLine
                + "-- Lot of free (but poor quality) servers" + Environment.NewLine + string.Empty + Environment.NewLine
                + "[Disadvantages:]" + Environment.NewLine + "-- No encryption support for data of password"
                + Environment.NewLine + "-- Can be blocked or limited by firewalls of filtering systems"
                + Environment.NewLine + "-- Data can be retrieved by hackers and firewalls");

            this.BtnExSelf.ToolTip = new Tooltip("Direct", "Send traffic directly via your internet connection.");

            ToolTipService.SetInitialShowDelay(this.btn_ex_pearoxy, 0);
            ToolTipService.SetShowDuration(this.btn_ex_pearoxy, 60000);

            ToolTipService.SetInitialShowDelay(this.BtnExWeb, 0);
            ToolTipService.SetShowDuration(this.BtnExWeb, 60000);

            ToolTipService.SetInitialShowDelay(this.BtnExProxy, 0);
            ToolTipService.SetShowDuration(this.BtnExProxy, 60000);

            ToolTipService.SetInitialShowDelay(this.BtnExSelf, 0);
            ToolTipService.SetShowDuration(this.BtnExSelf, 60000);
        }

        /// <summary>
        /// The txt_ text box_ lost focus.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void TxtTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            this.SaveSettings();
        }

        /// <summary>
        /// The txt_proxy address_ lost focus.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void TxtProxyAddressLostFocus(object sender, RoutedEventArgs e)
        {
            if (this.TxtProxyAddress.Text == string.Empty)
            {
                return;
            }

            try
            {
                this.TxtProxyAddress.Text = this.TxtProxyAddress.Text.Replace("\\", "/");
                if (this.TxtProxyAddress.Text.IndexOf("://", StringComparison.Ordinal) == -1)
                {
                    this.TxtProxyAddress.Text = "http://" + this.TxtProxyAddress.Text;
                }

                Uri uri = new Uri(this.TxtProxyAddress.Text);
                if (
                    !(this.TxtProxyAddress.Text.LastIndexOf(":", StringComparison.Ordinal) == -1
                      || this.TxtProxyAddress.Text.LastIndexOf(":", StringComparison.Ordinal) == this.TxtProxyAddress.Text.IndexOf(":", StringComparison.Ordinal)))
                {
                    this.TxtProxyPort.Text = uri.Port.ToString(CultureInfo.InvariantCulture);
                }

                this.TxtProxyAddress.Text = uri.Host;
                this.TxtProxyPortTextChanged(sender, null);
            }
            catch (Exception ex)
            {
                VDialog.Show(
                    "Value is not acceptable.\r\n" + ex.Message, 
                    "Data Validation", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Exclamation);
                this.TxtProxyAddress.Text = string.Empty;
                new Thread(
                    delegate()
                        {
                            Thread.Sleep(10);
                            this.Dispatcher.Invoke(
                                (App.SimpleVoidDelegate)(() => this.TxtProxyAddress.Focus()), 
                                new object[] { });
                        }) {
                              IsBackground = true 
                           }.Start();
            }

            this.SaveSettings();
        }

        /// <summary>
        /// The txt_proxy port_ text changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void TxtProxyPortTextChanged(object sender, TextChangedEventArgs e)
        {
            ushort port;
            if (!ushort.TryParse(this.TxtProxyPort.Text, out port))
            {
                VDialog.Show(
                    "Port Value is not acceptable.", 
                    "Data Validation", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Exclamation);
                port = 1080;
            }

            this.TxtProxyPort.Text = port.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// The txt_server address_ lost focus.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void TxtServerAddressLostFocus(object sender, RoutedEventArgs e)
        {
            if (this.TxtServerAddress.Text == string.Empty)
            {
                return;
            }

            try
            {
                this.TxtServerAddress.Text = this.TxtServerAddress.Text.Replace("\\", "/");
                if (this.TxtServerAddress.Text.IndexOf("://", StringComparison.Ordinal) == -1)
                {
                    this.TxtServerAddress.Text = "http://" + this.TxtServerAddress.Text;
                }

                Uri uri = new Uri(this.TxtServerAddress.Text);
                if (
                    !(this.TxtServerAddress.Text.LastIndexOf(":", StringComparison.Ordinal) == -1
                      || this.TxtServerAddress.Text.LastIndexOf(":", StringComparison.Ordinal) == this.TxtServerAddress.Text.IndexOf(":", StringComparison.Ordinal)))
                {
                    this.TxtServerPort.Text = uri.Port.ToString(CultureInfo.InvariantCulture);
                }

                this.TxtServerAddress.Text = uri.Host;
                this.TxtServerPortTextChanged(sender, null);
            }
            catch (Exception ex)
            {
                VDialog.Show(
                    "Value is not acceptable.\r\n" + ex.Message, 
                    "Data Validation", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Exclamation);
                this.TxtServerAddress.Text = string.Empty;
                new Thread(
                    delegate()
                        {
                            Thread.Sleep(10);
                            this.Dispatcher.Invoke(
                                (App.SimpleVoidDelegate)(() => this.TxtServerAddress.Focus()), 
                                new object[] { });
                        }) {
                              IsBackground = true 
                           }.Start();
            }

            this.SaveSettings();
        }

        /// <summary>
        /// The txt_server port_ text changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void TxtServerPortTextChanged(object sender, TextChangedEventArgs e)
        {
            ushort port;
            if (!ushort.TryParse(this.TxtServerPort.Text, out port))
            {
                VDialog.Show(
                    "Port Value is not acceptable.", 
                    "Data Validation", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Exclamation);
                port = 1080;
            }

            this.TxtServerPort.Text = port.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// The txt_web_ lost focus.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <exception cref="Exception">
        /// No https support
        /// </exception>
        private void TxtWebLostFocus(object sender, RoutedEventArgs e)
        {
            if (this.TxtWeb.Text == string.Empty)
            {
                return;
            }

            try
            {
                this.TxtWeb.Text = this.TxtWeb.Text.Replace("\\", "/");
                if (this.TxtWeb.Text.IndexOf("://", StringComparison.Ordinal) == -1)
                {
                    this.TxtWeb.Text = "http://" + this.TxtWeb.Text;
                }

                Uri uri = new Uri(this.TxtWeb.Text);
                if (uri.Scheme != Uri.UriSchemeHttp)
                {
                    throw new Exception("PeaRoxyWeb: Supporting only HTTP protocol.");
                }

                this.TxtWeb.Text = uri.ToString();
            }
            catch (Exception ex)
            {
                VDialog.Show(
                    "Value is not acceptable.\r\n" + ex.Message, 
                    "Data Validation", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Exclamation);
                this.TxtWeb.Text = string.Empty;
                new Thread(
                    delegate()
                        {
                            Thread.Sleep(10);
                            this.Dispatcher.Invoke(
                                (App.SimpleVoidDelegate)(() => this.TxtWeb.Focus()), 
                                new object[] { });
                        }) {
                              IsBackground = true 
                           }.Start();
            }

            this.SaveSettings();
        }

        #endregion
    }
}