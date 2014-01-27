// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Protocol.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for Protocol.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.SettingTabs
{
    #region

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;

    using LukeSw.Windows.Forms;

    using PeaRoxy.Windows.WPFClient.Properties;

    #endregion

    /// <summary>
    ///     Interaction logic for Protocol.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public partial class Protocol
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Protocol"/> class.
        /// </summary>
        public Protocol()
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
            this.TxtConnectionNoDataTimeout.Text = Settings.Default.Connection_NoDataTimeout.ToString(CultureInfo.InvariantCulture);
            this.TxtConnectionSendPacketSize.Text = Settings.Default.Connection_SendPacketSize.ToString(CultureInfo.InvariantCulture);
            this.TxtConnectionRecPacketSize.Text = Settings.Default.Connection_RecPacketSize.ToString(CultureInfo.InvariantCulture);
            this.CbConnectionStopOninterrupt.IsChecked = Settings.Default.Connection_StopOnInterrupt;

            this.RbConnectionEncryptionNone.IsChecked = false;
            this.RbConnectionEncryptionTripleDes.IsChecked = false;
            this.RbConnectionEncryptionSimpleXor.IsChecked = false;
            switch (Settings.Default.Connection_Encryption)
            {
                case 0:
                    this.RbConnectionEncryptionNone.IsChecked = true;
                    break;
                case 1:
                    this.RbConnectionEncryptionTripleDes.IsChecked = true;
                    break;
                case 2:
                    this.RbConnectionEncryptionSimpleXor.IsChecked = true;
                    break;
            }

            this.RbConnectionCompressionNone.IsChecked = false;
            this.RbConnectionCompressiongZip.IsChecked = false;
            this.RbConnectionCompressionDeflate.IsChecked = false;
            switch (Settings.Default.Connection_Compression)
            {
                case 0:
                    this.RbConnectionCompressionNone.IsChecked = true;
                    break;
                case 1:
                    this.RbConnectionCompressiongZip.IsChecked = true;
                    break;
                case 2:
                    this.RbConnectionCompressionDeflate.IsChecked = true;
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

            Settings.Default.Connection_NoDataTimeout = Convert.ToUInt16(this.TxtConnectionNoDataTimeout.Text);
            Settings.Default.Connection_SendPacketSize = Convert.ToUInt32(this.TxtConnectionSendPacketSize.Text);
            Settings.Default.Connection_RecPacketSize = Convert.ToUInt32(this.TxtConnectionRecPacketSize.Text);
            Settings.Default.Connection_StopOnInterrupt = this.CbConnectionStopOninterrupt.IsChecked ?? false;

            if (this.RbConnectionEncryptionNone.IsChecked ?? false)
            {
                Settings.Default.Connection_Encryption = 0;
            }
            else if (this.RbConnectionEncryptionTripleDes.IsChecked ?? false)
            {
                Settings.Default.Connection_Encryption = 1;
            }
            else if (this.RbConnectionEncryptionSimpleXor.IsChecked ?? false)
            {
                Settings.Default.Connection_Encryption = 2;
            }

            if (this.RbConnectionCompressionNone.IsChecked ?? false)
            {
                Settings.Default.Connection_Compression = 0;
            }
            else if (this.RbConnectionCompressiongZip.IsChecked ?? false)
            {
                Settings.Default.Connection_Compression = 1;
            }
            else if (this.RbConnectionCompressionDeflate.IsChecked ?? false)
            {
                Settings.Default.Connection_Compression = 2;
            }

            Settings.Default.Save();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The txt_ connection_ no data timeout_ lost focus.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void TxtConnectionNoDataTimeoutLostFocus(object sender, EventArgs e)
        {
            short u;
            if (!short.TryParse(this.TxtConnectionNoDataTimeout.Text, out u))
            {
                VDialog.Show(
                    "Value is not acceptable.", 
                    "Data Validation", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Exclamation);
                this.TxtConnectionNoDataTimeout.Text = 600.ToString(CultureInfo.InvariantCulture);
                new Thread(
                    delegate()
                        {
                            Thread.Sleep(10);
                            this.Dispatcher.Invoke(
                                (App.SimpleVoidDelegate)(() => this.TxtConnectionNoDataTimeout.Focus()), 
                                new object[] { });
                        }) {
                              IsBackground = true 
                           }.Start();
            }
            else
            {
                this.TxtConnectionNoDataTimeout.Text = u.ToString(CultureInfo.InvariantCulture);
            }

            this.SaveSettings();
        }

        /// <summary>
        /// The txt_ connection_ rec packet size_ lost focus.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void TxtConnectionRecPacketSizeLostFocus(object sender, RoutedEventArgs e)
        {
            uint u;
            if (!uint.TryParse(this.TxtConnectionRecPacketSize.Text, out u))
            {
                VDialog.Show(
                    "Value is not acceptable.", 
                    "Data Validation", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Exclamation);
                this.TxtConnectionRecPacketSize.Text = 10240.ToString(CultureInfo.InvariantCulture);
                new Thread(
                    delegate()
                        {
                            Thread.Sleep(10);
                            this.Dispatcher.Invoke(
                                (App.SimpleVoidDelegate)(() => this.TxtConnectionRecPacketSize.Focus()), 
                                new object[] { });
                        }) {
                              IsBackground = true 
                           }.Start();
            }
            else
            {
                this.TxtConnectionRecPacketSize.Text = u.ToString(CultureInfo.InvariantCulture);
            }

            this.SaveSettings();
        }

        /// <summary>
        /// The txt_ connection_ send packet size_ lost focus.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void TxtConnectionSendPacketSizeLostFocus(object sender, RoutedEventArgs e)
        {
            uint u;
            if (!uint.TryParse(this.TxtConnectionSendPacketSize.Text, out u))
            {
                VDialog.Show(
                    "Value is not acceptable.", 
                    "Data Validation", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Exclamation);
                this.TxtConnectionSendPacketSize.Text = 1024.ToString(CultureInfo.InvariantCulture);
                new Thread(
                    delegate()
                        {
                            Thread.Sleep(10);
                            this.Dispatcher.Invoke(
                                (App.SimpleVoidDelegate)(() => this.TxtConnectionSendPacketSize.Focus()), 
                                new object[] { });
                        }) {
                              IsBackground = true 
                           }.Start();
            }
            else
            {
                this.TxtConnectionSendPacketSize.Text = u.ToString(CultureInfo.InvariantCulture);
            }

            this.SaveSettings();
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

        #endregion
    }
}