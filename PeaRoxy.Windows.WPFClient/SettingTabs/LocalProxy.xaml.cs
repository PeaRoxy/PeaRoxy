// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalProxy.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for LocalProxy.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.SettingTabs
{
    #region

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Net;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;

    using LukeSw.Windows.Forms;

    using PeaRoxy.Windows.WPFClient.Properties;

    #endregion

    /// <summary>
    ///     Interaction logic for LocalProxy.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public partial class LocalProxy
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalProxy"/> class.
        /// </summary>
        public LocalProxy()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load settings.
        /// </summary>
        public override void LoadSettings()
        {
            this.IsLoading = true;
            this.TxtLocalProxyServerPort.Text = Settings.Default.Proxy_Port.ToString(CultureInfo.InvariantCulture);
            if (Settings.Default.Proxy_Address == "*")
            {
                this.CbLocalProxyServerAddressAny.IsChecked = true;
            }
            else
            {
                this.TxtLocalProxyServerAddress.Text = Settings.Default.Proxy_Address;
            }

            this.CbAutoConfigScriptEnable.IsChecked = Settings.Default.AutoConfig_Enable;
            this.CbAutoConfigScriptKeepRunning.IsChecked = Settings.Default.AutoConfig_KeepRuning;
            this.TxtAutoConfigScriptAddress.Text = Settings.Default.AutoConfig_Address;

            this.RbAutoConfigScriptJsMime.IsChecked = false;
            this.RbAutoConfigScriptNsMime.IsChecked = false;
            switch (Settings.Default.AutoConfig_Mime)
            {
                case 1:
                    this.RbAutoConfigScriptNsMime.IsChecked = true;
                    break;
                case 2:
                    this.RbAutoConfigScriptJsMime.IsChecked = true;
                    break;
            }

            this.IsLoading = false;
        }

        /// <summary>
        /// The save settings.
        /// </summary>
        public override void SaveSettings()
        {
            if (this.IsLoading)
            {
                return;
            }

            Settings.Default.Proxy_Port = ushort.Parse(TxtLocalProxyServerPort.Text);

            if (this.CbLocalProxyServerAddressAny.IsChecked ?? false)
            {
                Settings.Default.Proxy_Address = "*";
            }
            else
            {
                Settings.Default.Proxy_Address = this.TxtLocalProxyServerAddress.Text;
            }

            Settings.Default.AutoConfig_Enable = this.CbAutoConfigScriptEnable.IsChecked ?? false;
            Settings.Default.AutoConfig_KeepRuning = this.CbAutoConfigScriptKeepRunning.IsChecked ?? false;
            Settings.Default.AutoConfig_Address = this.TxtAutoConfigScriptAddress.Text;

            if (this.RbAutoConfigScriptNsMime.IsChecked ?? false)
            {
                Settings.Default.AutoConfig_Mime = 1;
            }
            else if (this.RbAutoConfigScriptJsMime.IsChecked ?? false)
            {
                Settings.Default.AutoConfig_Mime = 2;
            }

            Settings.Default.Save();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The cb_auto config script enable_ checked changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CbAutoConfigScriptEnableCheckedChanged(object sender, RoutedEventArgs e)
        {
            this.CbAutoConfigScriptKeepRunning.IsEnabled = this.CbAutoConfigScriptEnable.IsChecked ?? false;
            this.TxtAutoConfigScriptAddress.IsEnabled = this.CbAutoConfigScriptEnable.IsChecked ?? false;
            this.LblAutoConfigScriptPreAddress.IsEnabled = this.CbAutoConfigScriptEnable.IsChecked ?? false;
            this.RbAutoConfigScriptNsMime.IsEnabled = this.CbAutoConfigScriptEnable.IsChecked ?? false;
            this.RbAutoConfigScriptJsMime.IsEnabled = this.CbAutoConfigScriptEnable.IsChecked ?? false;
            this.SaveSettings();
        }

        /// <summary>
        /// The cb_local proxy server address any_ checked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CbLocalProxyServerAddressAnyChecked(object sender, RoutedEventArgs e)
        {
            this.TxtLocalProxyServerAddress.IsEnabled = !(this.CbLocalProxyServerAddressAny.IsChecked ?? false);
            this.LblAutoConfigScriptPreAddressRefresh(null, null);
            this.SaveSettings();
        }

        /// <summary>
        /// The lbl_auto config script pre address_ refresh.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void LblAutoConfigScriptPreAddressRefresh(object sender, EventArgs e)
        {
            if (this.TxtLocalProxyServerPort == null || this.TxtLocalProxyServerAddress == null || this.LblAutoConfigScriptPreAddress == null)
            {
                return;
            }

            this.LblAutoConfigScriptPreAddress.Content = "http://"
                                                          + (this.CbLocalProxyServerAddressAny.IsChecked ?? false ? IPAddress.Loopback.ToString() : this.TxtLocalProxyServerAddress.Text) + ":"
                                                          + this.TxtLocalProxyServerPort.Text + "/";
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
        /// The txt_local proxy server address_ lost focus.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void TxtLocalProxyServerAddressLostFocus(object sender, RoutedEventArgs e)
        {
            IPAddress ip;
            if (this.TxtLocalProxyServerAddress.Text.Split('.').Length != 4
                || !IPAddress.TryParse(this.TxtLocalProxyServerAddress.Text, out ip))
            {
                this.TxtLocalProxyServerAddress.Text = "127.0.0.1";
                VDialog.Show(
                    "IP address is not acceptable.", 
                    "Data Validation", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Exclamation);
                new Thread(
                    delegate()
                        {
                            Thread.Sleep(10);
                            this.Dispatcher.Invoke(
                                (App.SimpleVoidDelegate)(() => this.TxtLocalProxyServerAddress.Focus()), 
                                new object[] { });
                        }) {
                              IsBackground = true 
                           }.Start();
            }
            else
            {
                this.TxtLocalProxyServerAddress.Text = ip.ToString();
            }

            this.SaveSettings();
        }

        /// <summary>
        /// The txt_local proxy server port_ text changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void TxtLocalProxyServerPortTextChanged(object sender, TextChangedEventArgs e)
        {
            ushort port;
            if (!ushort.TryParse(this.TxtLocalProxyServerPort.Text, out port))
            {
                VDialog.Show(
                    "Port Value is not acceptable.", 
                    "Data Validation", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Exclamation);
                port = 1080;
            }

            this.LblAutoConfigScriptPreAddressRefresh(null, null);
            this.TxtLocalProxyServerPort.Text = port.ToString(CultureInfo.InvariantCulture);
            this.SaveSettings();
        }

        #endregion
    }
}