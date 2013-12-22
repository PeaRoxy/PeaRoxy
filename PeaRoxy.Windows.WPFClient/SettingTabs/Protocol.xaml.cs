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
            this.txt_Connection_NoDataTimeout.Text = Settings.Default.Connection_NoDataTimeout.ToString(CultureInfo.InvariantCulture);
            this.txt_Connection_SendPacketSize.Text = Settings.Default.Connection_SendPacketSize.ToString(CultureInfo.InvariantCulture);
            this.txt_Connection_RecPacketSize.Text = Settings.Default.Connection_RecPacketSize.ToString(CultureInfo.InvariantCulture);
            this.cb_Connection_stopOninterrupt.IsChecked = Settings.Default.Connection_StopOnInterrupt;

            this.rb_Connection_EncryptionNone.IsChecked = false;
            this.rb_Connection_EncryptionTripleDES.IsChecked = false;
            this.rb_Connection_EncryptionSimpleXor.IsChecked = false;
            switch (Settings.Default.Connection_Encryption)
            {
                case 0:
                    this.rb_Connection_EncryptionNone.IsChecked = true;
                    break;
                case 1:
                    this.rb_Connection_EncryptionTripleDES.IsChecked = true;
                    break;
                case 2:
                    this.rb_Connection_EncryptionSimpleXor.IsChecked = true;
                    break;
            }

            this.rb_Connection_CompressionNone.IsChecked = false;
            this.rb_Connection_CompressiongZip.IsChecked = false;
            this.rb_Connection_CompressionDeflate.IsChecked = false;
            switch (Settings.Default.Connection_Compression)
            {
                case 0:
                    this.rb_Connection_CompressionNone.IsChecked = true;
                    break;
                case 1:
                    this.rb_Connection_CompressiongZip.IsChecked = true;
                    break;
                case 2:
                    this.rb_Connection_CompressionDeflate.IsChecked = true;
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

            Settings.Default.Connection_NoDataTimeout = Convert.ToUInt16(this.txt_Connection_NoDataTimeout.Text);
            Settings.Default.Connection_SendPacketSize = Convert.ToUInt32(this.txt_Connection_SendPacketSize.Text);
            Settings.Default.Connection_RecPacketSize = Convert.ToUInt32(this.txt_Connection_RecPacketSize.Text);
            Settings.Default.Connection_StopOnInterrupt = this.cb_Connection_stopOninterrupt.IsChecked ?? false;

            if (this.rb_Connection_EncryptionNone.IsChecked ?? false)
            {
                Settings.Default.Connection_Encryption = 0;
            }
            else if (this.rb_Connection_EncryptionTripleDES.IsChecked ?? false)
            {
                Settings.Default.Connection_Encryption = 1;
            }
            else if (this.rb_Connection_EncryptionSimpleXor.IsChecked ?? false)
            {
                Settings.Default.Connection_Encryption = 2;
            }

            if (this.rb_Connection_CompressionNone.IsChecked ?? false)
            {
                Settings.Default.Connection_Compression = 0;
            }
            else if (this.rb_Connection_CompressiongZip.IsChecked ?? false)
            {
                Settings.Default.Connection_Compression = 1;
            }
            else if (this.rb_Connection_CompressionDeflate.IsChecked ?? false)
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
            if (!short.TryParse(this.txt_Connection_NoDataTimeout.Text, out u))
            {
                VDialog.Show(
                    "Value is not acceptable.", 
                    "Data Validation", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Exclamation);
                this.txt_Connection_NoDataTimeout.Text = 600.ToString(CultureInfo.InvariantCulture);
                new Thread(
                    delegate()
                        {
                            Thread.Sleep(10);
                            this.Dispatcher.Invoke(
                                (App.SimpleVoidDelegate)(() => this.txt_Connection_NoDataTimeout.Focus()), 
                                new object[] { });
                        }) {
                              IsBackground = true 
                           }.Start();
            }
            else
            {
                this.txt_Connection_NoDataTimeout.Text = u.ToString(CultureInfo.InvariantCulture);
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
            if (!uint.TryParse(this.txt_Connection_RecPacketSize.Text, out u))
            {
                VDialog.Show(
                    "Value is not acceptable.", 
                    "Data Validation", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Exclamation);
                this.txt_Connection_RecPacketSize.Text = 10240.ToString(CultureInfo.InvariantCulture);
                new Thread(
                    delegate()
                        {
                            Thread.Sleep(10);
                            this.Dispatcher.Invoke(
                                (App.SimpleVoidDelegate)(() => this.txt_Connection_RecPacketSize.Focus()), 
                                new object[] { });
                        }) {
                              IsBackground = true 
                           }.Start();
            }
            else
            {
                this.txt_Connection_RecPacketSize.Text = u.ToString(CultureInfo.InvariantCulture);
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
            if (!uint.TryParse(this.txt_Connection_SendPacketSize.Text, out u))
            {
                VDialog.Show(
                    "Value is not acceptable.", 
                    "Data Validation", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Exclamation);
                this.txt_Connection_SendPacketSize.Text = 1024.ToString(CultureInfo.InvariantCulture);
                new Thread(
                    delegate()
                        {
                            Thread.Sleep(10);
                            this.Dispatcher.Invoke(
                                (App.SimpleVoidDelegate)(() => this.txt_Connection_SendPacketSize.Focus()), 
                                new object[] { });
                        }) {
                              IsBackground = true 
                           }.Start();
            }
            else
            {
                this.txt_Connection_SendPacketSize.Text = u.ToString(CultureInfo.InvariantCulture);
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