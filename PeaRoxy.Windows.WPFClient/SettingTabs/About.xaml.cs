﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="About.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for About.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.SettingTabs
{
    #region

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Forms;

    using EasyHook;

    using LukeSw.Windows.Forms;

    using PeaRoxy.ClientLibrary;
    using PeaRoxy.CommonLibrary;
    using PeaRoxy.CoreProtocol;
    using PeaRoxy.Windows.Network.TAP;
    using PeaRoxy.Windows.WPFClient.Properties;

    using MessageBox = System.Windows.MessageBox;

    #endregion

    /// <summary>
    ///     Interaction logic for About.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public partial class About
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="About"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1515:SingleLineCommentMustBePrecededByBlankLine", Justification = "Reviewed. Suppression is OK here.")]
        public About()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The set enable.
        /// </summary>
        /// <param name="enable">
        /// The enable.
        /// </param>
        public override void SetEnable(bool enable)
        {
            this.ResetSettingsButton.IsEnabled = enable;
            this.FirstTimeWizardButton.IsEnabled = enable;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The btn_resetsettings_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnResetsettingsClick(object sender, RoutedEventArgs e)
        {
            if (
                VDialog.Show(
                    "This operation will reset all settings to default and close this application, are you sure?!",
                    "Reset Settings",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            Settings.Default.Reset();
            Settings.Default.FirstRun = false;
            Settings.Default.Save();
            VDialog.Show(
                "Done. We will close this application and then you can reopen it with default settings.", 
                "Reset Settings", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information);
            App.End();
        }

        #endregion

        private void FirstTimeWizardButton_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.Welcome_Shown = false;
            Settings.Default.Save();
            VDialog.Show(
                "Done. We will close this application and then you can reopen it and follow the wizard.",
                "First Time Wizard",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            App.End();
        }

        private void BaseLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.MainVersion.Content = this.MainVersion.Content.ToString()
                    .Replace("%version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                this.Libraries.Content =
                        "VDialog.dll (Win7 Dialogs) \r\n" +
                        "EasyHook.dll \r\n" +
                        "Tun2Socks \r\n" +
                        "Dns2Socks \r\n" +
                        "TAP Adapter \r\n" +
                        "Images";
                this.Versions.Content =
                    string.Format(
                        "v{0} © Łukasz Świątkowski\r\n" +
                        "v{1} © EasyHook Team\r\n" +
                        "© Ambroz Bizjak\r\n" +
                        "SourceForge.net/projects/dns2socks\r\n" +
                        "© OpenVPN.net\r\n" +
                        "© Icons8.com",
                        Assembly.GetAssembly(typeof(VDialog)).GetName().Version.ToString(3),
                        Assembly.GetAssembly(typeof(RemoteHooking)).GetName().Version.ToString(3));
            }
            catch { }
        }
    }
}