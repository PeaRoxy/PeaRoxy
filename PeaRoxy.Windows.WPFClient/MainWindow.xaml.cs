using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using LukeSw.Windows.Forms;
using Microsoft.Research.DynamicDataDisplay.Common;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.WindowsAPICodePack.ApplicationServices;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace PeaRoxy.Windows.WPFClient
{
    /// <summary>
    /// Interaction UI logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private delegate void SimpleVoid_Delegate();
        public bool inFormLoaded = false;
        private bool pageChanging = false;
        private bool IsFormDfferent = true; // Is in active condition
        private RingArray<chartPoint> downPoints;
        private RingArray<chartPoint> upPoints;
        public bool isHidden = false;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            downPoints = new RingArray<chartPoint>(240);
            upPoints = new RingArray<chartPoint>(240);
            EnumerableDataSource<chartPoint> downDataSource = new EnumerableDataSource<chartPoint>(downPoints);
            downDataSource.SetYMapping(y => y.Data);

            downDataSource.SetXMapping(x => MainPage.hztsa_chartThumbnile.ConvertToDouble(x.Time));
            EnumerableDataSource<chartPoint> upDataSource = new EnumerableDataSource<chartPoint>(upPoints);
            upDataSource.SetYMapping(y => y.Data);
            upDataSource.SetXMapping(x => MainPage.hztsa_chartThumbnile.ConvertToDouble(x.Time));
            ((SettingTabs.General)Options.General.SettingsPage).Chart.AddLineGraph(downDataSource, Color.FromRgb(100, 170, 255), 1);
            ((SettingTabs.General)Options.General.SettingsPage).Chart.AddLineGraph(upDataSource, Color.FromRgb(170, 50, 0), 1);
            ((SettingTabs.General)Options.General.SettingsPage).Chart.AxisGrid.Opacity = 1;
            ((SettingTabs.General)Options.General.SettingsPage).Chart.LegendVisible = false;
            MainPage.ThumbnileChart.AddLineGraph(downDataSource, Color.FromRgb(100, 170, 255), 1);
            MainPage.ThumbnileChart.AddLineGraph(upDataSource, Color.FromRgb(170, 50, 0), 1);
            MainPage.ThumbnileChart.AxisGrid.Opacity = 1;
            MainPage.ThumbnileChart.LegendVisible = false;
            ReloadSettings();
            this.inFormLoaded = true;
            ShowForm(true);
            if (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Startup_StartServer)
            {
                MainPage.IsEnabled = false;
            }

            if (!App.isExecutedByUser)
            {
                if (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Startup_ShowWindow == false)
                {
                    this.IndfferentForm();
                    if (TaskbarManager.IsPlatformSupported)
                        this.WindowState = System.Windows.WindowState.Minimized;
                    else
                        this.Visibility = System.Windows.Visibility.Hidden;
                    this.isHidden = true;
                    new System.Threading.Thread(delegate()
                    {
                        System.Threading.Thread.Sleep(10000);
                        this.Dispatcher.Invoke((SimpleVoid_Delegate)delegate()
                        {
                            this.HideForm();
                        }, new object[] { });
                    }) { IsBackground = true }.Start();
                }
                new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep(10000);
                    this.Dispatcher.Invoke((SimpleVoid_Delegate)delegate()
                    {
                        App.Notify.Visible = false;
                        App.Notify.Visible = true;
                    }, new object[] { });
                }) { IsBackground = true }.Start();
            }
            App.Notify.Visible = true;
        }

        public void ReloadSettings()
        {
            MainPage.ReloadSettings();
            Options.ReloadSettings();
            App.Notify.Visible = false;
            App.Notify.Visible = true;
        }
        private void IndfferentForm()
        {
            if (!IsFormDfferent)
                return;
            this.IsFormDfferent = false;
            App.Notify.DoubleClick -= Notify_DoubleClick;
            App.Notify.Click -= Notify_Click;
            MainPage.Loading = true;
            Controls.IsEnabled = false;
            Header.IsEnabled = false;
            Toolbar.IsEnabled = false;
        }

        private void DfferentForm()
        {
            if (IsFormDfferent)
                return;
            this.IsFormDfferent = true;
            this.IsEnabled = true;
            App.Notify.DoubleClick += Notify_DoubleClick;
            App.Notify.Click += Notify_Click;
            MainPage.Loading = false;
            Controls.IsEnabled = true;
            Header.IsEnabled = true;
            Toolbar.IsEnabled = true;
        }
        public void ShowForm(bool appstart = false)
        {
            this.Activate();
            if (this.Status == CurrentStatus.Connected)
                Controls.CurrentStatus = UserControls.StartStopButton.Status.ShowStop;
            else
                Controls.CurrentStatus = UserControls.StartStopButton.Status.ShowStart;
            MainPage.ShowTitle = true;

            if (appstart && PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Startup_StartServer)
            {
                new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep((int)(10000));
                    this.Dispatcher.Invoke((SimpleVoid_Delegate)delegate()
                    {
                        this.StartServer(true);
                    }, new object[] { });
                }) { IsBackground = true }.Start();
            }
            if (TaskbarManager.IsPlatformSupported)
                this.WindowState = System.Windows.WindowState.Normal;
            else
                this.Visibility = System.Windows.Visibility.Visible;
            this.Focusable = true;
            this.isHidden = false;
            DoubleAnimation da_ShowWindow = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.5)));
            this.BeginAnimation(MainWindow.OpacityProperty, da_ShowWindow);

            IndfferentForm();
            new System.Threading.Thread(delegate()
            {
                if ((appstart && !App.isExecutedByUser) && !PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Startup_ShowWindow)
                    System.Threading.Thread.Sleep(12000);
                else
                    System.Threading.Thread.Sleep(1000);
                this.Dispatcher.Invoke((SimpleVoid_Delegate)delegate()
                {
                    if (!appstart || !PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Startup_StartServer)
                        DfferentForm();
                    RefreshStatus();
                }, new object[] { });
            }) { IsBackground = true }.Start();
            this.Focus();
        }
        private void HideForm()
        {
            this.Activate();
            MainPage.ShowTitle = false;
            RefreshStatus();
            Controls.CurrentStatus = UserControls.StartStopButton.Status.Hide;
            DoubleAnimation da_HideWindow = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.6)));
            this.BeginAnimation(MainWindow.OpacityProperty, da_HideWindow);


            new System.Threading.Thread(delegate()
            {
                System.Threading.Thread.Sleep(1000);
                this.Dispatcher.Invoke((SimpleVoid_Delegate)delegate()
                {
                    if (TaskbarManager.IsPlatformSupported)
                        this.WindowState = System.Windows.WindowState.Minimized;
                    else
                        this.Visibility = System.Windows.Visibility.Hidden;

                    this.Focusable = false;
                    this.isHidden = true;
                }, new object[] { });
            }) { IsBackground = true }.Start();
        }
        private void RefreshStatus()
        {
            bool needDff = IsFormDfferent;
            IndfferentForm();
            bool isConnected = false;
            DoubleAnimation da_ShowConnectedImage = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.5)));
            DoubleAnimation da_HideConnectedImage = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
            DoubleAnimation da_ShowDisconnectedImage = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.5)));
            DoubleAnimation da_HideDisconnectedImage = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.5)));
            switch (this.Status)
            {
                case CurrentStatus.Disconnected:
                    Options.SetState(true);
                    MainPage.Connecetd = false;
                    Controls.CurrentStatus = UserControls.StartStopButton.Status.ShowStart;
                    break;
                case CurrentStatus.Sleep:
                    Options.SetState(true, false);
                    MainPage.Connecetd = false;
                    Controls.CurrentStatus = UserControls.StartStopButton.Status.ShowStart;
                    break;
                case CurrentStatus.Connected:
                    Options.SetState(false);
                    MainPage.Connecetd = true;
                    isConnected = true;
                    Controls.CurrentStatus = UserControls.StartStopButton.Status.ShowStop;
                    break;
                default:
                    break;
            }
            if (needDff)
                new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep((int)(1200));
                    this.Dispatcher.Invoke((SimpleVoid_Delegate)delegate()
                    {
                        DfferentForm();
                        MainPage.IsEnabled = !isConnected;
                        Toolbar.GrabberGrid.IsEnabled = !isConnected;
                        QuickButtonsRefresh();
                    }, new object[] { });
                }) { IsBackground = true }.Start();
        }

        private void btn_connect_Click(object sender, RoutedEventArgs e)
        {
            this.StartServer();
        }
        private void btn_dissconnect_Click(object sender, RoutedEventArgs e)
        {
            if (!this.StopServer())
                DfferentForm();
            RefreshStatus();
        }

        private void Notify_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MouseEventArgs p = e as System.Windows.Forms.MouseEventArgs;
            if (p != null && p.Button == System.Windows.Forms.MouseButtons.Left)
                if (!this.isHidden) //this.WindowState != System.Windows.WindowState.Minimized)//this.Visibility == System.Windows.Visibility.Visible)
                    this.Activate();
                else
                    ShowForm();
        }

        private void Notify_DoubleClick(object sender, EventArgs e)
        {
            if (!this.isHidden) //this.WindowState != System.Windows.WindowState.Minimized)//this.Visibility == System.Windows.Visibility.Visible)
                HideForm();
            else
                ShowForm();
        }

        void Toolbar_BackClick(object sender, RoutedEventArgs e)
        {
            double time = 1;

            if (this.pageChanging)
                return;
            this.pageChanging = true;

            MainPage.IsEnabled = false;
            Options.IsEnabled = false;
            Controls.IsEnabled = false;
            Toolbar.GrabberGrid.IsEnabled = false;
            Toolbar.SmartPearGrid.IsEnabled = false;
            Toolbar.ReConfigGrid.IsEnabled = false;
            Toolbar.NavigatorState = UserControls.ToolbarButtons.State.Option;

            DoubleAnimation da_alignLogoImageButtons = new DoubleAnimation(MainGrid.RenderSize.Width - 55, 0, new Duration(TimeSpan.FromSeconds(time * 0.8)));
            TranslateTransform tt_alignLogoImageButtons = new TranslateTransform();
            da_alignLogoImageButtons.EasingFunction = new PowerEase();
            ((PowerEase)da_alignLogoImageButtons.EasingFunction).EasingMode = EasingMode.EaseIn;
            img_logo.RenderTransform = tt_alignLogoImageButtons;
            tt_alignLogoImageButtons.BeginAnimation(TranslateTransform.XProperty, da_alignLogoImageButtons);

            DoubleAnimation da_HideMainGrid = new DoubleAnimation(-(MainGrid.RenderSize.Width - 50), 0, new Duration(TimeSpan.FromSeconds(time)));
            TranslateTransform tt_HideMainGrid = MainGrid.RenderTransform as TranslateTransform;
            TranslateTransform tt_ShowOptionsGrid = Options.RenderTransform as TranslateTransform;
            da_HideMainGrid.EasingFunction = new PowerEase();
            ((PowerEase)da_HideMainGrid.EasingFunction).EasingMode = EasingMode.EaseIn;
            tt_HideMainGrid.BeginAnimation(TranslateTransform.XProperty, da_HideMainGrid);
            tt_ShowOptionsGrid.BeginAnimation(TranslateTransform.XProperty, da_HideMainGrid);

            new System.Threading.Thread(delegate()
            {
                System.Threading.Thread.Sleep((int)(time * 1000));
                this.Dispatcher.Invoke((SimpleVoid_Delegate)delegate()
                {
                    this.pageChanging = false;
                    RefreshStatus();
                }, new object[] { });
            }) { IsBackground = true }.Start();
        }

        void Toolbar_OptionsClick(object sender, RoutedEventArgs e)
        {
            double time = 1;

            if (this.pageChanging)
                return;
            this.pageChanging = true;

            MainPage.IsEnabled = false;
            Options.IsEnabled = false;
            Controls.IsEnabled = false;
            Toolbar.GrabberGrid.IsEnabled = false;
            Toolbar.SmartPearGrid.IsEnabled = false;
            Toolbar.ReConfigGrid.IsEnabled = false;
            Toolbar.NavigatorState = UserControls.ToolbarButtons.State.Back;

            DoubleAnimation da_alignLogoImageButtons = new DoubleAnimation(0, MainGrid.RenderSize.Width - 55, new Duration(TimeSpan.FromSeconds(time * 1.33)));
            TranslateTransform tt_alignLogoImageButtons = new TranslateTransform();
            da_alignLogoImageButtons.EasingFunction = new PowerEase();
            ((PowerEase)da_alignLogoImageButtons.EasingFunction).EasingMode = EasingMode.EaseInOut;
            img_logo.RenderTransform = tt_alignLogoImageButtons;
            tt_alignLogoImageButtons.BeginAnimation(TranslateTransform.XProperty, da_alignLogoImageButtons);

            DoubleAnimation da_HideMainGrid = new DoubleAnimation(0, -(MainGrid.RenderSize.Width - 50), new Duration(TimeSpan.FromSeconds(time)));
            TranslateTransform tt_HideMainGrid = MainGrid.RenderTransform as TranslateTransform;
            TranslateTransform tt_ShowOptionsGrid = Options.RenderTransform as TranslateTransform;
            da_HideMainGrid.EasingFunction = new PowerEase();
            ((PowerEase)da_HideMainGrid.EasingFunction).EasingMode = EasingMode.EaseOut;
            tt_HideMainGrid.BeginAnimation(TranslateTransform.XProperty, da_HideMainGrid);
            tt_ShowOptionsGrid.BeginAnimation(TranslateTransform.XProperty, da_HideMainGrid);

            new System.Threading.Thread(delegate()
            {
                System.Threading.Thread.Sleep((int)(time * 1000));
                this.Dispatcher.Invoke((App.SimpleVoid_Delegate)delegate()
                {
                    Options.IsEnabled = true;
                    this.pageChanging = false;
                }, new object[] { });
            }) { IsBackground = true }.Start();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            StopServer();
        }

        private void img_logo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsFormDfferent && !pageChanging && Toolbar.NavigatorState == UserControls.ToolbarButtons.State.Option && e.ClickCount == 2)
            {
                Toolbar_OptionsClick(null, null);
                new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep(1000);
                    this.Dispatcher.Invoke((SimpleVoid_Delegate)delegate()
                    {
                        Options.About.isSelected = true;
                    }, new object[] { });
                }) { IsBackground = true }.Start();
            }
            else
                DragMove();
        }

        public void Dispose()
        { }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            if (!this.isHidden && this.IsFormDfferent)
                App.Notify.GetType().GetMethod("OnDoubleClick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(App.Notify, new object[] { null });
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (TaskbarManager.IsPlatformSupported)
                if (this.isHidden && this.IsFormDfferent && this.WindowState == System.Windows.WindowState.Normal)
                    ShowForm();
                else
                    WindowState = System.Windows.WindowState.Minimized;
        }

        private void Drag_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Controls_StartClick(object sender, RoutedEventArgs e)
        {
            this.StartServer();
        }

        private void Controls_StopClick(object sender, RoutedEventArgs e)
        {
            if (!this.StopServer())
                DfferentForm();
            RefreshStatus();
        }

        private void Exit_MinimizeClick(object sender, RoutedEventArgs e)
        {
            HideForm();
        }

        private void Toolbar_ReConfigClick(object sender, RoutedEventArgs e)
        {
            reConfig();
        }
        private void Toolbar_SmartPearSelectedChanged(object sender, RoutedEventArgs e)
        {
            MenuItem mi = ((UserControls.ToolbarButtons.MenuItemEventArgs)e).SenderMenuItem;
            if (!mi.IsChecked)
                mi.IsChecked = true;

            //if (!mi_smartpear_disable.IsChecked || !mi_smartpear_enable.IsChecked)
            //return;
            
            Toolbar.SmartPearEnableMenuItem.IsChecked = false;
            Toolbar.SmartPearDisableMenuItem.IsChecked = false;
            mi.IsChecked = true;
            if (Toolbar.SmartPearDisableMenuItem.IsChecked)
            {
                ((SettingTabs.SmartPear)Options.SmartPear.SettingsPage).HTTPEnable.IsChecked = false;
                ((SettingTabs.SmartPear)Options.SmartPear.SettingsPage).HTTPSEnable.IsChecked = false;
                if (Listener != null)
                {
                    Listener.SmartPear.Forwarder_HTTP_Enable = Listener.ActiveServer.GetType().Equals(typeof(ClientLibrary.Server_Types.Server_NoServer));
                    Listener.SmartPear.Forwarder_HTTPS_Enable = false;
                    Listener.SmartPear.Detector_DNSGrabber_Enable = false;
                }
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Save();
                ReloadSettings();
            }
            else
            {
                if (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Grabber == 1)
                    Toolbar_SmartPearSelectedChanged(Toolbar.SmartPearDisableMenuItem, e);
                else
                {
                    if (Listener != null)
                        if ((Listener.Status == ClientLibrary.Proxy_Controller.ControllerStatus.Both || Listener.Status == ClientLibrary.Proxy_Controller.ControllerStatus.OnlyProxy) &&
                            Listener.ActiveServer.GetType().Equals(typeof(ClientLibrary.Server_Types.Server_NoServer)))
                        {
                            Toolbar.SmartPearEnableMenuItem.IsChecked = true;
                            Toolbar.SmartPearDisableMenuItem.IsChecked = false;
                            return;
                        }
                        else if ((Listener.Status == ClientLibrary.Proxy_Controller.ControllerStatus.Both || Listener.Status == ClientLibrary.Proxy_Controller.ControllerStatus.OnlyProxy) &&
                            Listener.ActiveServer.GetType().Equals(typeof(ClientLibrary.Server_Types.Server_PeaRoxyWeb)))
                        {
                            Listener.SmartPear.Forwarder_HTTP_Enable = true;
                            Listener.SmartPear.Forwarder_HTTPS_Enable = false;
                            Listener.SmartPear.Detector_DNSGrabber_Enable = true;
                        }
                        else
                        {
                            Listener.SmartPear.Forwarder_HTTP_Enable = true;
                            Listener.SmartPear.Forwarder_HTTPS_Enable = true;
                            Listener.SmartPear.Detector_DNSGrabber_Enable = true;
                        }
                    ((SettingTabs.SmartPear)Options.SmartPear.SettingsPage).HTTPEnable.IsChecked = true;
                    ((SettingTabs.SmartPear)Options.SmartPear.SettingsPage).HTTPSEnable.IsChecked = true;
                    Options.SaveSettings();
                }
            }
            QuickButtonsRefresh();
        }

        private void QuickButtonsRefresh()
        {

            Toolbar.SmartPearIsEnable = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Grabber != 1;
            Toolbar.ReConfigIsEnable = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Grabber == 0 || Listener == null ||
                (Listener.Status == ClientLibrary.Proxy_Controller.ControllerStatus.Stopped || Listener.Status == ClientLibrary.Proxy_Controller.ControllerStatus.OnlyAutoConfig);
            Toolbar.SmartPearEnableMenuItem.IsEnabled = true;
            if (!Toolbar.SmartPearIsEnable)
                Toolbar.SmartPearColor = UserControls.ToolbarButtons.Color.White;
            else if (Listener != null &&
                    (Listener.Status == ClientLibrary.Proxy_Controller.ControllerStatus.Both || Listener.Status == ClientLibrary.Proxy_Controller.ControllerStatus.OnlyProxy) &&
                    Listener.ActiveServer.GetType().Equals(typeof(ClientLibrary.Server_Types.Server_NoServer)))
            {
                Toolbar.SmartPearColor = UserControls.ToolbarButtons.Color.Red;
                Toolbar.SmartPearEnableMenuItem.IsEnabled = false;
                Toolbar.SmartPearEnableMenuItem.IsChecked = false;
                Toolbar.SmartPearDisableMenuItem.IsChecked = true;
            }
            else if (Listener != null &&
                    (Listener.Status == ClientLibrary.Proxy_Controller.ControllerStatus.Both || Listener.Status == ClientLibrary.Proxy_Controller.ControllerStatus.OnlyProxy) &&
                    (Listener.SmartPear.Forwarder_HTTP_Enable ^ Listener.SmartPear.Forwarder_HTTPS_Enable))
            {
                Toolbar.SmartPearColor = UserControls.ToolbarButtons.Color.Yellow;
                Toolbar.SmartPearEnableMenuItem.IsChecked = true;
                Toolbar.SmartPearDisableMenuItem.IsChecked = false;
            }
            else if (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTP_Enable &&
                    PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTPS_Enable)
            {
                Toolbar.SmartPearColor = UserControls.ToolbarButtons.Color.Blue;
                Toolbar.SmartPearEnableMenuItem.IsChecked = true;
                Toolbar.SmartPearDisableMenuItem.IsChecked = false;
            }
            else if (!PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTP_Enable &&
                    !PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTPS_Enable)
            {
                Toolbar.SmartPearColor = UserControls.ToolbarButtons.Color.Red;
                Toolbar.SmartPearEnableMenuItem.IsChecked = false;
                Toolbar.SmartPearDisableMenuItem.IsChecked = true;
            }
            else
            {
                Toolbar.SmartPearColor = UserControls.ToolbarButtons.Color.Yellow;
                Toolbar.SmartPearEnableMenuItem.IsChecked = false;
                Toolbar.SmartPearDisableMenuItem.IsChecked = false;
            }
            Toolbar.GrabberNoneMenuItem.IsChecked = false;
            Toolbar.GrabberTAPMenuItem.IsChecked = false;
            Toolbar.GrabberHookMenuItem.IsChecked = false;
            switch (((SettingTabs.Grabber)Options.Grabber.SettingsPage).ActiveGrabber.SelectedIndex)
            {
                case 1:
                    Toolbar.GrabberTAPMenuItem.IsChecked = true;
                    break;
                case 2:
                    Toolbar.GrabberHookMenuItem.IsChecked = true;
                    break;
                default:
                    Toolbar.GrabberNoneMenuItem.IsChecked = true;
                    break;
            }
        }

        private void Toolbar_GrabberSelectedChanged(object sender, RoutedEventArgs e)
        {
            MenuItem mi = ((UserControls.ToolbarButtons.MenuItemEventArgs)e).SenderMenuItem;
            if (!mi.IsChecked)
                mi.IsChecked = true;
            Toolbar.GrabberNoneMenuItem.IsChecked = false;
            Toolbar.GrabberHookMenuItem.IsChecked = false;
            Toolbar.GrabberTAPMenuItem.IsChecked = false;
            mi.IsChecked = true;
            if (Toolbar.GrabberTAPMenuItem.IsChecked)
                ((SettingTabs.Grabber)Options.Grabber.SettingsPage).ActiveGrabber.SelectedIndex = 1;
            else if (Toolbar.GrabberHookMenuItem.IsChecked)
                ((SettingTabs.Grabber)Options.Grabber.SettingsPage).ActiveGrabber.SelectedIndex = 2;
            else
                ((SettingTabs.Grabber)Options.Grabber.SettingsPage).ActiveGrabber.SelectedIndex = 0;
            Options.SaveSettings();
            QuickButtonsRefresh();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            DropShadow.BlurRadius = 12;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            DropShadow.BlurRadius = 7;
        }
    }
}
