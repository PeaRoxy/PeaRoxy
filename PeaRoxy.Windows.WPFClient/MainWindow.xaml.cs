// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction UI logic for MainWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient
{
    #region

    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    using Microsoft.Research.DynamicDataDisplay;
    using Microsoft.Research.DynamicDataDisplay.Common;
    using Microsoft.Research.DynamicDataDisplay.DataSources;
    using Microsoft.WindowsAPICodePack.Taskbar;

    using PeaRoxy.ClientLibrary;
    using PeaRoxy.ClientLibrary.ServerModules;
    using PeaRoxy.Windows.WPFClient.Properties;
    using PeaRoxy.Windows.WPFClient.SettingTabs;
    using PeaRoxy.Windows.WPFClient.UserControls;

    using MenuItem = System.Windows.Controls.MenuItem;
    using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
    using SmartPear = PeaRoxy.Windows.WPFClient.SettingTabs.SmartPear;

    #endregion

    /// <summary>
    ///     Interaction UI logic for MainWindow.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
        Justification = "Reviewed. Suppression is OK here.")]
    public partial class MainWindow : IDisposable
    {
        #region Fields

        /// <summary>
        ///     The down points.
        /// </summary>
        private RingArray<ChartPoint> downloadPoints;

        /// <summary>
        ///     The is form dfferent.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        private bool isFormDfferent = true; // Is in active condition

        /// <summary>
        ///     The page changing.
        /// </summary>
        private bool pageChanging;

        /// <summary>
        ///     The up points.
        /// </summary>
        private RingArray<ChartPoint> uploadPoints;

        #endregion

        #region Delegates

        /// <summary>
        ///     The simple void_ delegate.
        /// </summary>
        private delegate void SimpleVoidDelegate();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether is hidden.
        /// </summary>
        public bool IsHidden { get; private set; }

        /// <summary>
        /// Gets a value indicating whether form was loaded.
        /// </summary>
        public bool IsFormLoaded { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The dispose.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        ///     The reload settings.
        /// </summary>
        public void ReloadSettings()
        {
            this.MainPage.ReloadSettings();
            this.Options.ReloadSettings();
            App.Notify.Visible = false;
            App.Notify.Visible = true;
        }

        /// <summary>
        /// The show form.
        /// </summary>
        /// <param name="appstart">
        /// The app start.
        /// </param>
        public void ShowForm(bool appstart = false)
        {
            this.Activate();
            this.Controls.CurrentStatus = this.Status == CurrentStatus.Connected
                                              ? StartStopButton.Status.ShowStop
                                              : StartStopButton.Status.ShowStart;

            if (appstart && Settings.Default.Startup_StartServer)
            {
                new Thread(
                    delegate()
                        {
                            Thread.Sleep(10000);
                            this.Dispatcher.Invoke((SimpleVoidDelegate)(() => this.StartServer(true)), new object[] { });
                        }) {
                              IsBackground = true 
                           }.Start();
            }

            if (TaskbarManager.IsPlatformSupported)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.Visibility = Visibility.Visible;
            }

            this.Focusable = true;
            this.IsHidden = false;
            DoubleAnimation showWindowAnimation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.5)));
            this.BeginAnimation(OpacityProperty, showWindowAnimation);

            this.IndfferentForm();
            new Thread(
                delegate()
                    {
                        if ((appstart && !App.IsExecutedByUser) && !Settings.Default.Startup_ShowWindow)
                        {
                            Thread.Sleep(12000);
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }

                        this.Dispatcher.Invoke(
                            (SimpleVoidDelegate)
                            (() => this.RefreshStatus(!appstart || !Settings.Default.Startup_StartServer)), 
                            new object[] { });
                    }) {
                          IsBackground = true 
                       }.Start();
            this.Focus();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The controls_ start click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ControlsStartClick(object sender, RoutedEventArgs e)
        {
            this.StartServer();
        }

        /// <summary>
        /// The controls_ stop click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ControlsStopClick(object sender, RoutedEventArgs e)
        {
            this.RefreshStatus(this.StopServer(false));
        }

        /// <summary>
        ///     The dfferent form.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        private void DfferentForm()
        {
            if (this.isFormDfferent)
            {
                return;
            }

            this.isFormDfferent = true;
            this.IsEnabled = true;
            App.Notify.DoubleClick += this.NotifyDoubleClick;
            App.Notify.Click += this.NotifyClick;
            this.MainPage.IsLoading = false;
            this.Controls.IsEnabled = true;
            this.Header.IsEnabled = true;
            this.Toolbar.IsEnabled = true;
        }

        /// <summary>
        /// The drag_ mouse left button down.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void DragMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// The exit_ minimize click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ExitMinimizeClick(object sender, RoutedEventArgs e)
        {
            this.HideForm();
        }

        /// <summary>
        ///     The hide form.
        /// </summary>
        private void HideForm()
        {
            this.Activate();
            this.RefreshStatus();
            this.Controls.CurrentStatus = StartStopButton.Status.Hide;
            DoubleAnimation hideWindowAnimation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.6)));
            this.BeginAnimation(OpacityProperty, hideWindowAnimation);

            new Thread(
                delegate()
                    {
                        Thread.Sleep(1000);
                        this.Dispatcher.Invoke(
                            (SimpleVoidDelegate)delegate
                                {
                                    if (TaskbarManager.IsPlatformSupported)
                                    {
                                        this.WindowState = WindowState.Minimized;
                                    }
                                    else
                                    {
                                        this.Visibility = Visibility.Hidden;
                                    }

                                    this.Focusable = false;
                                    this.IsHidden = true;
                                }, 
                            new object[] { });
                    }) {
                          IsBackground = true 
                       }.Start();
        }

        /// <summary>
        /// The image logo mouse left button down.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ImgLogoMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.isFormDfferent && !this.pageChanging && this.Toolbar.NavigatorState == ToolbarButtons.State.Option
                && e.ClickCount == 2)
            {
                this.ToolbarOptionsClick(null, null);
                new Thread(
                    delegate()
                        {
                            Thread.Sleep(1000);
                            this.Dispatcher.Invoke(
                                (SimpleVoidDelegate)delegate { this.Options.About.IsSelected = true; }, 
                                new object[] { });
                        }) {
                              IsBackground = true 
                           }.Start();
            }
            else
            {
                this.DragMove();
            }
        }

        /// <summary>
        ///     The indfferent form.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        private void IndfferentForm()
        {
            if (!this.isFormDfferent)
            {
                return;
            }

            this.isFormDfferent = false;
            App.Notify.DoubleClick -= this.NotifyDoubleClick;
            App.Notify.Click -= this.NotifyClick;
            this.MainPage.IsLoading = true;
            this.Controls.IsEnabled = false;
            this.Header.IsEnabled = false;
            this.Toolbar.IsEnabled = false;
        }

        /// <summary>
        /// The notify_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void NotifyClick(object sender, EventArgs e)
        {
            MouseEventArgs p = e as MouseEventArgs;
            if (p != null && p.Button == MouseButtons.Left)
            {
                if (!this.IsHidden)
                {
                    this.Activate();
                }
                else
                {
                    this.ShowForm();
                }
            }
        }

        /// <summary>
        /// The notify_ double click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void NotifyDoubleClick(object sender, EventArgs e)
        {
            if (!this.IsHidden)
            {
                this.HideForm();
            }
            else
            {
                this.ShowForm();
            }
        }

        /// <summary>
        ///     The quick buttons refresh.
        /// </summary>
        private void QuickButtonsRefresh()
        {
            this.Toolbar.SmartPearIsEnable = Settings.Default.Grabber != 1;
            this.Toolbar.ReConfigIsEnable = true;
            this.Toolbar.SmartPearEnableMenuItem.IsEnabled = true;
            if (!this.Toolbar.SmartPearIsEnable)
            {
                this.Toolbar.SmartPearColor = ToolbarButtons.Color.White;
            }
            else if (this.listener != null
                     && this.listener.Status.HasFlag(ProxyController.ControllerStatus.Proxy)
                     && this.listener.ActiveServer is NoServer)
            {
                this.Toolbar.SmartPearColor = ToolbarButtons.Color.Red;
                this.Toolbar.SmartPearEnableMenuItem.IsEnabled = false;
                this.Toolbar.SmartPearEnableMenuItem.IsChecked = false;
                this.Toolbar.SmartPearDisableMenuItem.IsChecked = true;
            }
            else if (this.listener != null
                     && this.listener.Status.HasFlag(ProxyController.ControllerStatus.Proxy)
                     && (this.listener.SmartPear.ForwarderHttpEnable ^ this.listener.SmartPear.ForwarderHttpsEnable))
            {
                this.Toolbar.SmartPearColor = ToolbarButtons.Color.Yellow;
                this.Toolbar.SmartPearEnableMenuItem.IsChecked = true;
                this.Toolbar.SmartPearDisableMenuItem.IsChecked = false;
            }
            else if (Settings.Default.Smart_HTTP_Enable && Settings.Default.Smart_HTTPS_Enable)
            {
                this.Toolbar.SmartPearColor = ToolbarButtons.Color.Blue;
                this.Toolbar.SmartPearEnableMenuItem.IsChecked = true;
                this.Toolbar.SmartPearDisableMenuItem.IsChecked = false;
            }
            else if (!Settings.Default.Smart_HTTP_Enable && !Settings.Default.Smart_HTTPS_Enable)
            {
                this.Toolbar.SmartPearColor = ToolbarButtons.Color.Red;
                this.Toolbar.SmartPearEnableMenuItem.IsChecked = false;
                this.Toolbar.SmartPearDisableMenuItem.IsChecked = true;
            }
            else
            {
                this.Toolbar.SmartPearColor = ToolbarButtons.Color.Yellow;
                this.Toolbar.SmartPearEnableMenuItem.IsChecked = false;
                this.Toolbar.SmartPearDisableMenuItem.IsChecked = false;
            }

            this.Toolbar.GrabberNoneMenuItem.IsChecked = false;
            this.Toolbar.GrabberTapMenuItem.IsChecked = false;
            this.Toolbar.GrabberHookMenuItem.IsChecked = false;
            this.Toolbar.GrabberProxyMenuItem.IsChecked = false;
            switch (((Grabber)this.Options.Grabber.SettingsPage).SelectedGrabber)
            {
                case Grabber.GrabberType.Tap:
                    this.Toolbar.GrabberTapMenuItem.IsChecked = true;
                    break;
                case Grabber.GrabberType.Hook:
                    this.Toolbar.GrabberHookMenuItem.IsChecked = true;
                    break;
                case Grabber.GrabberType.Proxy:
                    this.Toolbar.GrabberProxyMenuItem.IsChecked = true;
                    break;
                default:
                    this.Toolbar.GrabberNoneMenuItem.IsChecked = true;
                    break;
            }
        }

        /// <summary>
        /// The refresh status.
        /// </summary>
        /// <param name="doDfferent">
        /// The do dfferent.
        /// </param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        private void RefreshStatus(bool? doDfferent = null)
        {
            if (!doDfferent.HasValue)
            {
                doDfferent = this.isFormDfferent;
            }

            this.IndfferentForm();
            bool isConnected = false;
            switch (this.Status)
            {
                case CurrentStatus.Disconnected:
                    this.Options.SetState(true);
                    this.MainPage.Connecetd = false;
                    this.Controls.CurrentStatus = StartStopButton.Status.ShowStart;
                    break;
                case CurrentStatus.Sleep:
                    this.Options.SetState(true, false);
                    this.MainPage.Connecetd = false;
                    this.Controls.CurrentStatus = StartStopButton.Status.ShowStart;
                    break;
                case CurrentStatus.Connected:
                    this.Options.SetState(false);
                    this.MainPage.Connecetd = true;
                    isConnected = true;
                    this.Controls.CurrentStatus = StartStopButton.Status.ShowStop;
                    break;
            }

            if (doDfferent.Value)
            {
                new Thread(
                    delegate()
                        {
                            Thread.Sleep(1200);
                            this.Dispatcher.Invoke(
                                (SimpleVoidDelegate)delegate
                                    {
                                        this.DfferentForm();
                                        this.MainPage.IsEnabled = !isConnected;
                                        this.Toolbar.GrabberGrid.IsEnabled = !isConnected;
                                        this.QuickButtonsRefresh();
                                    }, 
                                new object[] { });
                        }) {
                              IsBackground = true 
                           }.Start();
            }
        }

        /// <summary>
        /// The toolbar_ back click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ToolbarBackClick(object sender, RoutedEventArgs e)
        {
            const double Time = 1;

            if (this.pageChanging)
            {
                return;
            }

            this.pageChanging = true;

            this.MainPage.IsEnabled = false;
            this.Options.IsEnabled = false;
            this.Controls.IsEnabled = false;
            this.Toolbar.GrabberIsEnable = false;
            this.Toolbar.SmartPearIsEnable = false;
            this.Toolbar.ReConfigIsEnable = false;
            this.Toolbar.NavigatorState = ToolbarButtons.State.Option;

            DoubleAnimation alignLogoImageButtonsAnimation = new DoubleAnimation(
                this.MainGrid.RenderSize.Width - 55, 
                0, 
                new Duration(TimeSpan.FromSeconds(Time * 0.8)));
            TranslateTransform alignLogoImageButtonsTransform = new TranslateTransform();
            alignLogoImageButtonsAnimation.EasingFunction = new PowerEase();
            ((PowerEase)alignLogoImageButtonsAnimation.EasingFunction).EasingMode = EasingMode.EaseIn;
            this.ImgLogo.RenderTransform = alignLogoImageButtonsTransform;
            alignLogoImageButtonsTransform.BeginAnimation(TranslateTransform.XProperty, alignLogoImageButtonsAnimation);

            DoubleAnimation hideMainGridAnimation = new DoubleAnimation(
                -(this.MainGrid.RenderSize.Width - 50), 
                0, 
                new Duration(TimeSpan.FromSeconds(Time)));
            TranslateTransform hideMainGridTransform = this.MainGrid.RenderTransform as TranslateTransform;
            TranslateTransform showOptionsGridTransform = this.Options.RenderTransform as TranslateTransform;
            hideMainGridAnimation.EasingFunction = new PowerEase();
            ((PowerEase)hideMainGridAnimation.EasingFunction).EasingMode = EasingMode.EaseIn;
            if (hideMainGridTransform != null)
            {
                hideMainGridTransform.BeginAnimation(TranslateTransform.XProperty, hideMainGridAnimation);
            }
            if (showOptionsGridTransform != null)
            {
                showOptionsGridTransform.BeginAnimation(TranslateTransform.XProperty, hideMainGridAnimation);
            }

            new Thread(
                delegate()
                    {
                        Thread.Sleep((int)(Time * 1000));
                        this.Dispatcher.Invoke(
                            (SimpleVoidDelegate)delegate
                                {
                                    this.pageChanging = false;
                                    this.RefreshStatus();
                                }, 
                            new object[] { });
                    }) {
                          IsBackground = true 
                       }.Start();
        }

        /// <summary>
        /// The toolbar_ grabber selected changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ToolbarGrabberSelectedChanged(object sender, RoutedEventArgs e)
        {
            MenuItem mi = ((ToolbarButtons.MenuItemEventArgs)e).SenderMenuItem;
            if (!mi.IsChecked)
            {
                mi.IsChecked = true;
            }

            this.Toolbar.GrabberNoneMenuItem.IsChecked = false;
            this.Toolbar.GrabberHookMenuItem.IsChecked = false;
            this.Toolbar.GrabberTapMenuItem.IsChecked = false;
            this.Toolbar.GrabberProxyMenuItem.IsChecked = false;
            mi.IsChecked = true;
            if (this.Toolbar.GrabberTapMenuItem.IsChecked)
            {
                ((Grabber)this.Options.Grabber.SettingsPage).SelectedGrabber = Grabber.GrabberType.Tap;
            }
            else if (this.Toolbar.GrabberHookMenuItem.IsChecked)
            {
                ((Grabber)this.Options.Grabber.SettingsPage).SelectedGrabber = Grabber.GrabberType.Hook;
            }
            else if (this.Toolbar.GrabberProxyMenuItem.IsChecked)
            {
                ((Grabber)this.Options.Grabber.SettingsPage).SelectedGrabber = Grabber.GrabberType.Proxy;
            }
            else
            {
                ((Grabber)this.Options.Grabber.SettingsPage).SelectedGrabber = Grabber.GrabberType.None;
            }

            this.Options.SaveSettings();
            this.QuickButtonsRefresh();
        }

        /// <summary>
        /// The toolbar_ options click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ToolbarOptionsClick(object sender, RoutedEventArgs e)
        {
            const double Time = 1;

            if (this.pageChanging)
            {
                return;
            }

            this.pageChanging = true;

            this.MainPage.IsEnabled = false;
            this.Options.IsEnabled = false;
            this.Controls.IsEnabled = false;
            this.Toolbar.GrabberIsEnable = false;
            this.Toolbar.SmartPearIsEnable = false;
            this.Toolbar.ReConfigIsEnable = false;
            this.Toolbar.NavigatorState = ToolbarButtons.State.Back;

            DoubleAnimation alignLogoImageButtonsAnimation = new DoubleAnimation(
                0, 
                this.MainGrid.RenderSize.Width - 55, 
                new Duration(TimeSpan.FromSeconds(Time * 1.33)));
            TranslateTransform alignLogoImageButtonsTransform = new TranslateTransform();
            alignLogoImageButtonsAnimation.EasingFunction = new PowerEase();
            ((PowerEase)alignLogoImageButtonsAnimation.EasingFunction).EasingMode = EasingMode.EaseInOut;
            this.ImgLogo.RenderTransform = alignLogoImageButtonsTransform;
            alignLogoImageButtonsTransform.BeginAnimation(TranslateTransform.XProperty, alignLogoImageButtonsAnimation);

            DoubleAnimation hideMainGridAnimation = new DoubleAnimation(
                0, 
                -(this.MainGrid.RenderSize.Width - 50), 
                new Duration(TimeSpan.FromSeconds(Time)));
            TranslateTransform hideMainGridTransform = this.MainGrid.RenderTransform as TranslateTransform;
            TranslateTransform showOptionsGridTransform = this.Options.RenderTransform as TranslateTransform;
            hideMainGridAnimation.EasingFunction = new PowerEase();
            ((PowerEase)hideMainGridAnimation.EasingFunction).EasingMode = EasingMode.EaseOut;
            if (hideMainGridTransform != null)
            {
                hideMainGridTransform.BeginAnimation(TranslateTransform.XProperty, hideMainGridAnimation);
            }
            if (showOptionsGridTransform != null)
            {
                showOptionsGridTransform.BeginAnimation(TranslateTransform.XProperty, hideMainGridAnimation);
            }

            new Thread(
                delegate()
                    {
                        Thread.Sleep((int)(Time * 1000));
                        this.Dispatcher.Invoke(
                            (App.SimpleVoidDelegate)delegate
                                {
                                    this.Options.IsEnabled = true;
                                    this.pageChanging = false;
                                }, 
                            new object[] { });
                    }) {
                          IsBackground = true 
                       }.Start();
        }

        /// <summary>
        /// The toolbar_ re config click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ToolbarReConfigClick(object sender, RoutedEventArgs e)
        {
            this.ReConfig(false);
        }

        /// <summary>
        /// The toolbar_ smart pear selected changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ToolbarSmartPearSelectedChanged(object sender, RoutedEventArgs e)
        {
            MenuItem mi = ((ToolbarButtons.MenuItemEventArgs)e).SenderMenuItem;
            if (!mi.IsChecked)
            {
                mi.IsChecked = true;
            }

            this.Toolbar.SmartPearEnableMenuItem.IsChecked = false;
            this.Toolbar.SmartPearDisableMenuItem.IsChecked = false;
            mi.IsChecked = true;
            if (this.Toolbar.SmartPearDisableMenuItem.IsChecked)
            {
                ((SmartPear)this.Options.SmartPear.SettingsPage).HttpEnable.IsChecked = false;
                ((SmartPear)this.Options.SmartPear.SettingsPage).HttpsEnable.IsChecked = false;
                if (this.listener != null)
                {
                    this.listener.SmartPear.ForwarderHttpEnable = this.listener.ActiveServer is NoServer;
                    this.listener.SmartPear.ForwarderHttpsEnable = false;
                    this.listener.SmartPear.DetectorDnsGrabberEnable = false;
                }

                Settings.Default.Save();
                this.ReloadSettings();
            }
            else
            {
                if (Settings.Default.Grabber == 1)
                {
                    this.ToolbarSmartPearSelectedChanged(this.Toolbar.SmartPearDisableMenuItem, e);
                }
                else
                {
                    if (this.listener != null)
                    {
                        if (this.listener.Status.HasFlag(ProxyController.ControllerStatus.Proxy)
                            && this.listener.ActiveServer is NoServer)
                        {
                            this.Toolbar.SmartPearEnableMenuItem.IsChecked = true;
                            this.Toolbar.SmartPearDisableMenuItem.IsChecked = false;
                            return;
                        }

                        if (this.listener.Status.HasFlag(ProxyController.ControllerStatus.Proxy)
                            && this.listener.ActiveServer is PeaRoxyWeb)
                        {
                            this.listener.SmartPear.ForwarderHttpEnable = true;
                            this.listener.SmartPear.ForwarderHttpsEnable = false;
                            this.listener.SmartPear.DetectorDnsGrabberEnable = true;
                        }
                        else
                        {
                            this.listener.SmartPear.ForwarderHttpEnable = true;
                            this.listener.SmartPear.ForwarderHttpsEnable = true;
                            this.listener.SmartPear.DetectorDnsGrabberEnable = true;
                        }
                    }

                    ((SmartPear)this.Options.SmartPear.SettingsPage).HttpEnable.IsChecked = true;
                    ((SmartPear)this.Options.SmartPear.SettingsPage).HttpsEnable.IsChecked = true;
                    this.Options.SaveSettings();
                }
            }

            this.QuickButtonsRefresh();
        }

        /// <summary>
        /// The window_ activated.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void WindowActivated(object sender, EventArgs e)
        {
            this.DropShadow.BlurRadius = 12;
        }

        /// <summary>
        /// The window_ closing.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            if (!this.IsHidden && this.isFormDfferent)
            {
                App.Notify.GetType()
                    .GetMethod("OnDoubleClick", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(App.Notify, new object[] { null });
            }
        }

        /// <summary>
        /// The window_ deactivated.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void WindowDeactivated(object sender, EventArgs e)
        {
            this.DropShadow.BlurRadius = 7;
        }

        /// <summary>
        /// The window_ loaded.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.downloadPoints = new RingArray<ChartPoint>(240);
            this.uploadPoints = new RingArray<ChartPoint>(240);
            EnumerableDataSource<ChartPoint> downloadDataSource =
                new EnumerableDataSource<ChartPoint>(this.downloadPoints);
            downloadDataSource.SetYMapping(y => y.Data);

            downloadDataSource.SetXMapping(x => this.MainPage.HztsaChartThumbnile.ConvertToDouble(x.Time));
            EnumerableDataSource<ChartPoint> uploadDataSource = new EnumerableDataSource<ChartPoint>(this.uploadPoints);
            uploadDataSource.SetYMapping(y => y.Data);
            uploadDataSource.SetXMapping(x => this.MainPage.HztsaChartThumbnile.ConvertToDouble(x.Time));
            ((General)this.Options.General.SettingsPage).Chart.AddLineGraph(
                downloadDataSource, 
                Color.FromRgb(100, 170, 255), 
                1);
            ((General)this.Options.General.SettingsPage).Chart.AddLineGraph(
                uploadDataSource, 
                Color.FromRgb(170, 50, 0), 
                1);
            ((General)this.Options.General.SettingsPage).Chart.AxisGrid.Opacity = 1;
            ((General)this.Options.General.SettingsPage).Chart.LegendVisible = false;
            this.MainPage.ThumbnileChart.AddLineGraph(downloadDataSource, Color.FromRgb(100, 170, 255), 1);
            this.MainPage.ThumbnileChart.AddLineGraph(uploadDataSource, Color.FromRgb(170, 50, 0), 1);
            this.MainPage.ThumbnileChart.AxisGrid.Opacity = 1;
            this.MainPage.ThumbnileChart.LegendVisible = false;
            this.ReloadSettings();
            this.IsFormLoaded = true;
            this.ShowForm(true);
            if (Settings.Default.Startup_StartServer)
            {
                this.MainPage.IsEnabled = false;
            }

            if (!App.IsExecutedByUser)
            {
                if (Settings.Default.Startup_ShowWindow == false)
                {
                    this.IndfferentForm();
                    if (TaskbarManager.IsPlatformSupported)
                    {
                        this.WindowState = WindowState.Minimized;
                    }
                    else
                    {
                        this.Visibility = Visibility.Hidden;
                    }

                    this.IsHidden = true;
                    new Thread(
                        delegate()
                            {
                                Thread.Sleep(10000);
                                this.Dispatcher.Invoke((SimpleVoidDelegate)this.HideForm, new object[] { });
                            }) {
                                  IsBackground = true 
                               }.Start();
                }

                new Thread(
                    delegate()
                        {
                            Thread.Sleep(10000);
                            this.Dispatcher.Invoke(
                                (SimpleVoidDelegate)delegate
                                    {
                                        App.Notify.Visible = false;
                                        App.Notify.Visible = true;
                                    }, 
                                new object[] { });
                        }) {
                              IsBackground = true 
                           }.Start();
            }

            App.Notify.Visible = true;
        }

        /// <summary>
        /// The window_ state changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void WindowStateChanged(object sender, EventArgs e)
        {
            if (TaskbarManager.IsPlatformSupported)
            {
                if (this.IsHidden && this.isFormDfferent && this.WindowState == WindowState.Normal)
                {
                    this.ShowForm();
                }
                else
                {
                    this.WindowState = WindowState.Minimized;
                }
            }
        }

        /// <summary>
        /// The window unloaded.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void WindowUnloaded(object sender, RoutedEventArgs e)
        {
            this.StopServer(true);
        }

        #endregion
    }
}