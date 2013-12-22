// --------------------------------------------------------------------------------------------------------------------
// <copyright file="General.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for General.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.SettingTabs
{
    #region

    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;

    using Microsoft.Win32;

    using PeaRoxy.ClientLibrary;
    using PeaRoxy.CommonLibrary;
    using PeaRoxy.Windows.WPFClient.Properties;

    #endregion

    /// <summary>
    ///     Interaction logic for General.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public partial class General
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="General"/> class.
        /// </summary>
        public General()
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
            this.CbStartServerAtStartup.IsChecked = Settings.Default.Startup_StartServer;
            this.CbOpenProgramAtStartup.IsChecked = Settings.Default.Startup_ShowWindow;

            this.CbRunAtStartup.IsChecked = false;
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
                if (key != null && key.GetValue("PeaRoxy Client") != null)
                {
                    this.CbRunAtStartup.IsChecked = true;
                }
            }
            catch (Exception)
            {
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

            Settings.Default.Startup_StartServer = this.CbStartServerAtStartup.IsChecked ?? false;
            Settings.Default.Startup_ShowWindow = this.CbOpenProgramAtStartup.IsChecked ?? false;
            Settings.Default.Save();
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (key == null)
                {
                    return;
                }

                if (this.CbRunAtStartup.IsChecked ?? false)
                {
                    key.SetValue(
                        "PeaRoxy Client",
                        "\"" + Process.GetCurrentProcess().MainModule.FileName + "\" /autoRun");
                }
                else
                {
                    key.DeleteValue("PeaRoxy Client");
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// The set enable.
        /// </summary>
        /// <param name="enable">
        /// The enable.
        /// </param>
        public override void SetEnable(bool enable)
        {
            this.CheckBoxes.IsEnabled = enable;
        }

        /// <summary>
        /// The update stats.
        /// </summary>
        /// <param name="listener">
        /// The listener.
        /// </param>
        /// <param name="currentDownSpeed">
        /// The current down speed.
        /// </param>
        /// <param name="currentUpSpeed">
        /// The current up speed.
        /// </param>
        public void UpdateStats(ProxyController listener, long currentDownSpeed, long currentUpSpeed)
        {
            this.LblStatAcceptingthreads.Content = listener.AcceptingClock + " - " + listener.AcceptingConnections;
            this.LblStatActiveconnections.Content = listener.RoutingClock + " - " + listener.RoutingConnections;
            this.LblStatDownloaded.Content = Common.FormatFileSizeAsString(listener.ReceivedBytes);
            this.LblStatUploaded.Content = Common.FormatFileSizeAsString(listener.SentBytes);
            this.LblStatDownloadrate.Content = Common.FormatFileSizeAsString(currentDownSpeed);
            this.LblStatUploadrate.Content = Common.FormatFileSizeAsString(currentUpSpeed);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The cb_run at startup_ checked changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CbRunAtStartupCheckedChanged(object sender, EventArgs e)
        {
            this.CbOpenProgramAtStartup.IsEnabled = this.CbRunAtStartup.IsChecked ?? false;
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