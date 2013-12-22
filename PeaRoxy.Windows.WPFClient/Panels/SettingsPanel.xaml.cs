// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsPanel.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for Settings.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.Panels
{
    #region

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;

    using PeaRoxy.Windows.WPFClient.UserControls;

    #endregion

    /// <summary>
    ///     Interaction logic for Settings.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public partial class SettingsPanel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPanel"/> class.
        /// </summary>
        public SettingsPanel()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The reload settings.
        /// </summary>
        public void ReloadSettings()
        {
            foreach (UIElement tab in this.WpOptionButtons.Children)
            {
                if (tab is SettingsButton && ((SettingsButton)tab).SettingsPage != null)
                {
                    ((SettingsButton)tab).SettingsPage.LoadSettings();
                }
            }
        }

        /// <summary>
        /// The save settings.
        /// </summary>
        public void SaveSettings()
        {
            foreach (UIElement tab in this.WpOptionButtons.Children)
            {
                if (tab is SettingsButton && ((SettingsButton)tab).SettingsPage != null)
                {
                    ((SettingsButton)tab).SettingsPage.SaveSettings();
                }
            }
        }

        /// <summary>
        /// The set state.
        /// </summary>
        /// <param name="isOptionsEnable">
        /// The is options enable.
        /// </param>
        /// <param name="isListeningOptionsEnable">
        /// The is listening options enable.
        /// </param>
        public void SetState(bool isOptionsEnable, bool? isListeningOptionsEnable = null)
        {
            if (isListeningOptionsEnable == null)
            {
                isListeningOptionsEnable = isOptionsEnable;
            }

            foreach (UIElement tab in this.WpOptionButtons.Children)
            {
                if (tab is SettingsButton && ((SettingsButton)tab).SettingsPage != null)
                {
                    ((SettingsButton)tab).SettingsPage.SetEnable(isOptionsEnable);
                }
            }

            this.LocalListener.SettingsPage.SetEnable(isListeningOptionsEnable.Value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The set active page.
        /// </summary>
        /// <param name="page">
        /// The page.
        /// </param>
        private void SetActivePage(SettingsButton page)
        {
            WrapPanel wrapPanel = page.Parent as WrapPanel;

            if (wrapPanel != null)
            {
                if (!page.IsSelected || page.SettingsPage == null)
                {
                    return;
                }

                foreach (UIElement tab in wrapPanel.Children)
                {
                    if (tab is SettingsButton && !tab.Equals(page))
                    {
                        (tab as SettingsButton).IsSelected = false;
                    }
                }
            }

            UIElement lastPage = this.CcOptions.Content as UIElement;
            DoubleAnimation hideLastPageAnimation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.10)));
            if (lastPage != null)
            {
                lastPage.BeginAnimation(OpacityProperty, hideLastPageAnimation);
            }

            new Thread(
                delegate()
                    {
                        Thread.Sleep(200);
                        this.Dispatcher.Invoke(
                            (App.SimpleVoidDelegate)delegate
                                {
                                    DoubleAnimation showPageAnimation = new DoubleAnimation(
                                        0,
                                        1,
                                        new Duration(TimeSpan.FromSeconds(0.10)));
                                    page.SettingsPage.BeginAnimation(OpacityProperty, showPageAnimation);
                                    this.CcOptions.Content = page.SettingsPage;
                                },
                            new object[] { });
                    }) { IsBackground = true }.Start();
        }

        /// <summary>
        /// The settings button_ selected changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void SettingsButtonSelectedChanged(object sender, RoutedEventArgs e)
        {
            SettingsButton s = sender as SettingsButton;
            this.SetActivePage(s);
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
        private void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            this.ReloadSettings();
            this.General.IsSelected = true;
        }

        #endregion
    }
}