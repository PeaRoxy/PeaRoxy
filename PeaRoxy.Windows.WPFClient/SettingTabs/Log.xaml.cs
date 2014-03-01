// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Log.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for Log.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.SettingTabs
{
    #region

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    using PeaRoxy.ClientLibrary;
    using PeaRoxy.Windows.WPFClient.Properties;

    #endregion

    /// <summary>
    ///     Interaction logic for Log.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public partial class Log
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Log"/> class.
        /// </summary>
        public Log()
        {
            this.InitializeComponent();
            ProxyController.NewLog += this.NewLog;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load settings.
        /// </summary>
        public override void LoadSettings()
        {
            this.IsLoading = true;
            this.CbLogErrorreportingHttp.IsChecked = Settings.Default.ErrorRenderer_EnableHTTP;
            this.CbLogErrorreporting80.IsChecked = Settings.Default.ErrorRenderer_Enable80;
            this.CbLogErrorreporting443.IsChecked = Settings.Default.ErrorRenderer_Enable443;
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

            Settings.Default.ErrorRenderer_EnableHTTP = this.CbLogErrorreportingHttp.IsChecked ?? false;
            Settings.Default.ErrorRenderer_Enable80 = this.CbLogErrorreporting80.IsChecked ?? false;
            Settings.Default.ErrorRenderer_Enable443 = this.CbLogErrorreporting443.IsChecked ?? false;
            Settings.Default.Save();
        }

        /// <summary>
        /// The set enable.
        /// </summary>
        /// <param name="enable">
        /// The enable.
        /// </param>
        public override void SetEnable(bool enable)
        {
            this.SettingsGrid.IsEnabled = enable;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The new log.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void NewLog(string message, EventArgs e)
        {
            message = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + message;
            this.Logs.Items.Add(message);
            try
            {
                this.Logs.ScrollIntoView(message);
                //if (VisualTreeHelper.GetChildrenCount(this.Logs) <= 0
                //    || VisualTreeHelper.GetChildrenCount(VisualTreeHelper.GetChild(this.Logs, 0)) <= 0) return;

                //ScrollViewer scroll =
                //    VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(this.Logs, 0), 0) as ScrollViewer;
                //if (scroll != null)
                //{
                //    scroll.ScrollToEnd();
                //}
            }
            catch { }
        }

        /// <summary>
        /// The logs selection changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void LbLogSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Logs.SelectionMode = SelectionMode.Multiple;
            this.Logs.SelectedItems.Clear();
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