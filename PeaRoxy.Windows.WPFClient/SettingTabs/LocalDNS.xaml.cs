// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalDNS.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for LocalDNS.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.SettingTabs
{
    #region

    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;

    using LukeSw.Windows.Forms;

    using PeaRoxy.Windows.WPFClient.Properties;

    #endregion

    /// <summary>
    ///     Interaction logic for LocalDNS.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public partial class LocalDns
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDns"/> class.
        /// </summary>
        public LocalDns()
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
            this.TxtDnsIpaddress.Text = Settings.Default.DNS_IPAddress;
            this.CbDnsEnable.IsChecked = Settings.Default.DNS_Enable;
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

            this.LblDnsIpaddress.IsEnabled = this.CbDnsEnable.IsChecked ?? false;
            this.TxtDnsIpaddress.IsEnabled = this.CbDnsEnable.IsChecked ?? false;
            Settings.Default.DNS_Enable = this.CbDnsEnable.IsChecked ?? false;
            Settings.Default.DNS_IPAddress = this.TxtDnsIpaddress.Text;
            Settings.Default.Save();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The cb_dns_enable_ checked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CbDnsEnableChecked(object sender, RoutedEventArgs e)
        {
            this.SaveSettings();
        }

        /// <summary>
        /// The txt_dns_ipaddress_ lost focus.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void TxtDnsIpaddressLostFocus(object sender, RoutedEventArgs e)
        {
            IPAddress ip;
            if (this.TxtDnsIpaddress.Text.Split('.').Length != 4
                || !IPAddress.TryParse(this.TxtDnsIpaddress.Text, out ip))
            {
                this.TxtDnsIpaddress.Text = "8.8.8.8";
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
                                (App.SimpleVoidDelegate)(() => this.TxtDnsIpaddress.Focus()), 
                                new object[] { });
                        }) {
                              IsBackground = true 
                           }.Start();
            }
            else
            {
                this.TxtDnsIpaddress.Text = ip.ToString();
            }

            this.SaveSettings();
        }

        #endregion
    }
}