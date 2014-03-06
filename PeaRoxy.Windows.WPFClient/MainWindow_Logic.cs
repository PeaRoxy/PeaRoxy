// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow_Logic.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The main window.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient
{
    #region

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Interop;
    using System.Windows.Media.Imaging;
    using System.Windows.Resources;
    using System.Windows.Threading;

    using LukeSw.Windows.Forms;

    using Microsoft.WindowsAPICodePack.Taskbar;

    using PeaRoxy.ClientLibrary;
    using PeaRoxy.ClientLibrary.ServerModules;
    using PeaRoxy.CommonLibrary;
    using PeaRoxy.Updater;
    using PeaRoxy.Windows.Network.Hook;
    using PeaRoxy.Windows.Network.TAP;
    using PeaRoxy.Windows.WPFClient.Properties;
    using PeaRoxy.Windows.WPFClient.SettingTabs;

    using Application = System.Windows.Forms.Application;
    using Common = PeaRoxy.Windows.Common;
    using IWin32Window = System.Windows.Forms.IWin32Window;
    using SmartPear = PeaRoxy.Windows.WPFClient.SettingTabs.SmartPear;

    #endregion

    /// <summary>
    ///     The main window.
    /// </summary>
    public partial class MainWindow : ISynchronizeInvoke, IWin32Window
    {
        #region Fields

        /// <summary>
        ///     The connected icon.
        /// </summary>
        private readonly Icon connectedIcon;

        private readonly BackgroundWorker updaterWorker;

        /// <summary>
        ///     The download speed.
        /// </summary>
        private long downloadSpeed;

        private AppVersion latestVersion;

        /// <summary>
        ///     The listener.
        /// </summary>
        private ProxyController listener;

        private bool skipUpdate;

        /// <summary>
        ///     The upload speed.
        /// </summary>
        private long uploadSpeed;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MainWindow" /> class.
        /// </summary>
        public MainWindow()
        {
            this.IsFormLoaded = false;
            this.InitializeComponent();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (Settings.Default.FirstRun)
            {
                int currentSettingsVersion = Settings.Default.Settings_Version;
                Settings.Default.Upgrade();
                Settings.Default.FirstRun = false;
                Settings.Default.Save();
                if (currentSettingsVersion != Settings.Default.Settings_Version)
                {
                    Settings.Default.Reset();
                    Settings.Default.FirstRun = false;
                    Settings.Default.Save();
                }
            }
            if (!Settings.Default.Welcome_Shown)
            {
                WelcomeWindow welcome = new WelcomeWindow();
                welcome.ShowDialog();
            }

            WindowsModule platform = new WindowsModule();
            platform.RegisterPlatform();

            this.updaterWorker = new BackgroundWorker();
            this.updaterWorker.DoWork += (sender, args) =>
                {
                    this.latestVersion = null;
                    try
                    {
                        if (!this.skipUpdate)
                        {
                            Updater updaterObject = new Updater(
                                "PeaRoxyClient",
                                Assembly.GetExecutingAssembly().GetName().Version,
                                this.listener != null
                                && this.listener.Status.HasFlag(ProxyController.ControllerStatus.Proxy)
                                    ? new WebProxy(this.listener.Ip + ":" + this.listener.Port, true)
                                    : null);
                            if (updaterObject.IsNewVersionAvailable())
                            {
                                this.latestVersion = updaterObject.GetLatestVersion();
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                };
            this.updaterWorker.RunWorkerCompleted += (sender, args) =>
                {
                    try
                    {
                        if (this.latestVersion != null)
                        {
                            VDialog updaterDialog = new VDialog
                                                        {
                                                            FooterText =
                                                                "Read release info for v"
                                                                + this.latestVersion.Version.ToString(),
                                                            FooterIcon = VDialogIcon.Information,
                                                            Content =
                                                                "New version of this application is available for download, do you want us to download it for you?",
                                                            WindowTitle = "AutoUpdater",
                                                            MainIcon = VDialogIcon.Question,
                                                            Buttons =
                                                                new[]
                                                                    {
                                                                        new VDialogButton(VDialogResult.No, "No"),
                                                                        new VDialogButton(
                                                                            VDialogResult.Yes,
                                                                            "Yes",
                                                                            true)
                                                                    }
                                                        };

                            updaterDialog.FooterLinks.Add(
                                new LinkLabel.Link(0, updaterDialog.FooterText.Length, this.latestVersion.PageLink));
                            updaterDialog.LinkClicked += (o, eventArgs) =>
                                {
                                    string target = eventArgs.Link.LinkData as string;
                                    if (target != null)
                                    {
                                        Process.Start(target);
                                    }
                                };
                            this.skipUpdate = updaterDialog.Show() != VDialogResult.Yes;
                            if (!this.skipUpdate)
                            {
                                Downloader downForm = new Downloader(
                                    this.latestVersion,
                                    this.listener != null
                                    && this.listener.Status.HasFlag(ProxyController.ControllerStatus.Proxy)
                                        ? new WebProxy(this.listener.Ip + ":" + this.listener.Port, true)
                                        : null);
                                downForm.ShowDialog();
                            }
                        }
                    }
                    catch
                    {
                    }
                };

            this.updaterWorker.RunWorkerAsync();

            StreamResourceInfo streamResourceInfo;
            try
            {
                App.Notify.ContextMenu =
                    new ContextMenu(
                        new[]
                            {
                                new MenuItem(
                                    "Show / Hide",
                                    delegate
                                        {
                                            App.Notify.GetType()
                                                .GetMethod(
                                                    "OnDoubleClick",
                                                    BindingFlags.Instance | BindingFlags.NonPublic)
                                                .Invoke(App.Notify, new object[] { null });
                                        }) { DefaultItem = true },
                                new MenuItem("-", (EventHandler)null), new MenuItem("Exit", delegate { App.End(); })
                            });
                streamResourceInfo =
                    System.Windows.Application.GetResourceStream(
                        new Uri(
                            "pack://application:,,,/PeaRoxy.Windows.WPFClient;component/Images/Icons/Pear_Red_Notify.ico"));
                if (streamResourceInfo != null)
                {
                    Stream st = streamResourceInfo.Stream;
                    App.Notify.Icon = new Icon(st);
                    st.Close();
                    st.Dispose();
                }

                App.Notify.Text = @"PeaRoxy Client - Stopped";
            }
            catch (Exception)
            {
            }

            streamResourceInfo =
                System.Windows.Application.GetResourceStream(
                    new Uri("pack://application:,,,/PeaRoxy.Windows.WPFClient;component/Images/Icons/Connected.ico"));
            if (streamResourceInfo != null)
            {
                this.connectedIcon = new Icon(streamResourceInfo.Stream);
            }

            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.5) };
            timer.Tick += this.UpdateStats;
            timer.Start();

            this.Status = CurrentStatus.Disconnected;
        }

        #endregion

        #region Enums

        /// <summary>
        ///     The current status.
        /// </summary>
        public enum CurrentStatus
        {
            /// <summary>
            ///     The disconnected.
            /// </summary>
            Disconnected,

            /// <summary>
            ///     The connected.
            /// </summary>
            Connected,

            /// <summary>
            ///     The sleep.
            /// </summary>
            Sleep,
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the handle.
        /// </summary>
        public IntPtr Handle
        {
            get
            {
                WindowInteropHelper interopHelper = new WindowInteropHelper(this);
                return interopHelper.Handle;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether invoke required.
        /// </summary>
        public bool InvokeRequired
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        ///     Gets or sets the status.
        /// </summary>
        public CurrentStatus Status { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The begin invoke.
        /// </summary>
        /// <param name="method">
        ///     The method.
        /// </param>
        /// <param name="args">
        ///     The args.
        /// </param>
        /// <returns>
        ///     The <see cref="IAsyncResult" />.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Not supported
        /// </exception>
        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     The end invoke.
        /// </summary>
        /// <param name="result">
        ///     The result.
        /// </param>
        /// <returns>
        ///     The <see cref="object" />.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Not supported
        /// </exception>
        public object EndInvoke(IAsyncResult result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     The fail disconnected.
        /// </summary>
        /// <param name="e">
        ///     The e.
        /// </param>
        public void FailDisconnected(EventArgs e)
        {
            this.listener.AutoDisconnect = false;
            this.Dispatcher.Invoke(
                (SimpleVoidDelegate)delegate
                    {
                        VDialog.Show(
                            this,
                            "Connection to the server interrupted.",
                            "PeaRoxy Client",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Stop);
                        App.Notify.ShowBalloonTip(
                            1000,
                            "PeaRoxy Client",
                            "Connection to the server interrupted.",
                            ToolTipIcon.Error);
                        this.ControlsStopClick(null, null);
                    },
                new object[] { });
        }

        /// <summary>
        ///     The invoke.
        /// </summary>
        /// <param name="method">
        ///     The method.
        /// </param>
        /// <param name="args">
        ///     The args.
        /// </param>
        /// <returns>
        ///     The <see cref="object" />.
        /// </returns>
        public object Invoke(Delegate method, object[] args)
        {
            return this.Dispatcher.Invoke(method, args);
        }

        /// <summary>
        ///     The re config.
        /// </summary>
        /// <param name="silent">
        ///     The silent.
        /// </param>
        /// <param name="forceState">
        ///     Indicate when method must force the state to active or deactive. null for refresh/auto
        /// </param>
        public void ReConfig(bool silent, bool? forceState = null)
        {
            bool isListenerActive = this.listener != null
                                    && this.listener.Status.HasFlag(ProxyController.ControllerStatus.Proxy);
            if (!forceState.HasValue || !forceState.Value)
            {
                if (!isListenerActive)
                {
                    ProxyModule.DisableProxy();
                    ProxyModule.DisableProxyAutoConfig();
                }

                if (isListenerActive && TapTunnelModule.IsRunning())
                {
                    TapTunnelModule.StopTunnel();
                }
                TapTunnelModule.CleanAllTunnelProcesses();

                if (isListenerActive && HookModule.IsHookProcessRunning())
                {
                    HookModule.StopHookProcess();
                }
                HookModule.CleanAllHookProcesses();
            }

            if ((forceState.HasValue && !forceState.Value) || !isListenerActive)
            {
                return;
            }

            switch ((Grabber.GrabberType)Settings.Default.Grabber)
            {
                case Grabber.GrabberType.Proxy:
                    string autoConfigUrl = string.Empty;
                    if (Settings.Default.AutoConfig_Enable)
                    {
                        autoConfigUrl = string.Format(
                            "http://{0}:{1}/{2}",
                            (Settings.Default.Proxy_Address == "*")
                                ? IPAddress.Loopback.ToString()
                                : Settings.Default.Proxy_Address,
                            Settings.Default.Proxy_Port,
                            Settings.Default.AutoConfig_Address);
                    }

                    bool res =
                        ProxyModule.SetActiveProxy(
                            new IPEndPoint(
                                (Settings.Default.Proxy_Address == "*"
                                     ? IPAddress.Any
                                     : IPAddress.Parse(Settings.Default.Proxy_Address)),
                                this.listener.Port));
                    if (!string.IsNullOrWhiteSpace(autoConfigUrl))
                    {
                        res = res || ProxyModule.SetActiveAutoConfig(autoConfigUrl);
                    }
                    bool firefoxRes = ProxyModule.ForceFirefoxToUseSystemSettings();

                    if (!res)
                    {
                        if (silent)
                        {
                            App.Notify.ShowBalloonTip(
                                5000,
                                "Proxy Grabber",
                                "We failed to register PeaRoxy as active proxy for this system. You may need to do it manually or restart/re-logon your PC and let us try again.",
                                ToolTipIcon.Warning);
                        }
                        else
                        {
                            if (VDialog.Show(
                                this,
                                "We failed to register PeaRoxy as active proxy for this system. You may need to do it manually or restart/re-logon your PC and let us try again.\r\nDo want us to logoff your user account?! Please save your work in other applications before making a decision.",
                                "Proxy Grabber",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                            {
                                Common.LogOffUser();
                            }
                        }
                    }
                    else
                    {
                        if (firefoxRes)
                        {
                            App.Notify.ShowBalloonTip(
                                5000,
                                "Proxy Grabber",
                                "PeaRoxy successfully registered as active proxy of this system. You may need to restart your browser to be able to use it along with PeaRoxy.",
                                ToolTipIcon.Info);
                        }
                        else
                        {
                            App.Notify.ShowBalloonTip(
                                5000,
                                "Proxy Grabber",
                                "PeaRoxy successfully registered as active proxy of this system. Firefox probably need some manual configurations, also you may need to restart your browser to be able to use it along with PeaRoxy.",
                                ToolTipIcon.Warning);
                        }
                    }

                    break;
                case Grabber.GrabberType.Tap:
                    if (this.listener != null && this.listener.Status.HasFlag(ProxyController.ControllerStatus.Proxy))
                    {
                        TapTunnelModule.AdapterAddressRange = IPAddress.Parse(Settings.Default.TAP_IPRange);
                        TapTunnelModule.DnsResolvingAddress = IPAddress.Parse(Settings.Default.DNS_IPAddress);
                        TapTunnelModule.DnsResolvingAddress2 = IPAddress.Parse(Settings.Default.Proxy_Address);
                        TapTunnelModule.SocksProxyEndPoint =
                            new IPEndPoint(IPAddress.Parse(Settings.Default.Proxy_Address), this.listener.Port);
                        TapTunnelModule.TunnelName = "PeaRoxy Tunnel";

                        if (TapTunnelModule.StartTunnel())
                        {
                            App.Notify.ShowBalloonTip(
                                5000,
                                "TAP Grabber",
                                "TAP Adapter activated successfully. You are ready to go.",
                                ToolTipIcon.Info);
                        }
                        else
                        {
                            TapTunnelModule.StopTunnel();
                            if (silent)
                            {
                                App.Notify.ShowBalloonTip(
                                    5000,
                                    "TAP Grabber",
                                    "Failed to start TAP Adapter. Tap grabber disabled.",
                                    ToolTipIcon.Warning);
                            }
                            else
                            {
                                VDialog.Show(
                                    this,
                                    "Failed to start TAP Adapter. Tap grabber disabled.",
                                    "TAP Grabber",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                            }
                        }
                    }
                    break;
                case Grabber.GrabberType.Hook:
                    HookModule.StartHookProcess(
                        Settings.Default.Hook_Processes.Split(
                            new[] { Environment.NewLine },
                            StringSplitOptions.RemoveEmptyEntries),
                        new IPEndPoint(
                            (Settings.Default.Proxy_Address == "*"
                                 ? IPAddress.Any
                                 : IPAddress.Parse(Settings.Default.Proxy_Address)),
                            this.listener.Port),
                        Settings.Default.Smart_AntiDNS_Enable ? Settings.Default.Smart_AntiDNSPattern : string.Empty);

                    App.Notify.ShowBalloonTip(
                        5000,
                        "Hook Grabber",
                        "Hook process started successfully and actively monitor running applications from now on. You are ready to go.",
                        ToolTipIcon.Info);
                    break;
            }
        }

        /// <summary>
        ///     The start server.
        /// </summary>
        /// <param name="silent">
        ///     The silent.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Invalid username
        /// </exception>
        /// <exception cref="Exception">
        ///     Failed to start server
        /// </exception>
        public bool StartServer(bool silent = false)
        {
            try
            {
                this.MainPage.IsEnabled = false;
                if (Settings.Default.Auth_Type == 2)
                {
                    if (string.IsNullOrEmpty(Settings.Default.UserAndPassword_User))
                    {
                        // ReSharper disable once NotResolvedInText
                        throw new ArgumentException(@"Invalid value.", "UserName");
                    }
                }

                if (this.listener != null)
                {
                    this.StopServer(true);
                }
                else
                {
                    this.listener = new ProxyController(null, null, 0);
                    this.listener.SmartPear.ForwarderListUpdated += this.SmartListUpdated;
                    this.listener.FailDisconnected += this.FailDisconnected;
                }

                this.listener.Ip = (Settings.Default.Proxy_Address == "*")
                                       ? IPAddress.Any
                                       : IPAddress.Parse(Settings.Default.Proxy_Address);
                this.listener.Port = Settings.Default.Proxy_Port;
                this.listener.IsAutoConfigEnable = Settings.Default.AutoConfig_Enable;
                this.listener.AutoConfigMime = (ProxyController.AutoConfigMimeType)Settings.Default.AutoConfig_Mime;
                this.listener.AutoConfigPath = Settings.Default.AutoConfig_Address;
                this.listener.IsHttpSupported = Settings.Default.Proxy_HTTP;
                this.listener.IsHttpsSupported = Settings.Default.Proxy_HTTPS;
                this.listener.IsSocksSupported = Settings.Default.Proxy_SOCKS;
                this.listener.SendPacketSize = (int)Settings.Default.Connection_SendPacketSize;
                this.listener.ReceivePacketSize = (int)Settings.Default.Connection_RecPacketSize;
                this.listener.AutoDisconnect = Settings.Default.Connection_StopOnInterrupt;

                this.listener.ErrorRenderer.Enable = Settings.Default.ErrorRenderer_EnableHTTP;
                this.listener.ErrorRenderer.OnPort80Direct = Settings.Default.ErrorRenderer_Enable80;
                this.listener.ErrorRenderer.OnPort443Direct = Settings.Default.ErrorRenderer_Enable443;

                this.listener.DnsResolver.DnsResolverSupported =
                    this.listener.DnsResolver.DnsResolverUdpSupported = Settings.Default.DNS_Enable;
                this.listener.DnsResolver.DnsResolverServerIp = IPAddress.Parse(Settings.Default.DNS_IPAddress);

                this.SmartPearApplySettings();

                ServerType ser;
                string serverAddress = string.Empty;
                int authType = Settings.Default.Auth_Type;
                switch (Settings.Default.Server_Type)
                {
                    case 0:
                        ser = new NoServer();
                        break;
                    case 1:
                        serverAddress = Settings.Default.PeaRoxySocks_Address;
                        ser = new PeaRoxy(
                            Settings.Default.PeaRoxySocks_Address,
                            Settings.Default.PeaRoxySocks_Port,
                            Settings.Default.PeaRoxySocks_Domain,
                            (authType == 2) ? Settings.Default.UserAndPassword_User : string.Empty,
                            (authType == 2) ? Settings.Default.UserAndPassword_Pass : string.Empty,
                            (Common.EncryptionType)Settings.Default.Connection_Encryption,
                            (Common.CompressionType)Settings.Default.Connection_Compression);
                        break;
                    case 2:
                        serverAddress = new Uri(Settings.Default.PeaRoxyWeb_Address).DnsSafeHost;
                        ser = new PeaRoxyWeb(
                            Settings.Default.PeaRoxyWeb_Address,
                            (authType == 2) ? Settings.Default.UserAndPassword_User : string.Empty,
                            (authType == 2) ? Settings.Default.UserAndPassword_Pass : string.Empty,
                            (Common.EncryptionType)Settings.Default.Connection_Encryption);
                        this.listener.SmartPear.ForwarderHttpsEnable = false;
                        break;
                    case 3:
                        serverAddress = Settings.Default.ProxyServer_Address;
                        switch (Settings.Default.ProxyServer_Type)
                        {
                            case 0:
                                ser = new Https(
                                    Settings.Default.ProxyServer_Address,
                                    Settings.Default.ProxyServer_Port,
                                    (authType == 2) ? Settings.Default.UserAndPassword_User : string.Empty,
                                    (authType == 2) ? Settings.Default.UserAndPassword_Pass : string.Empty);
                                break;
                            case 1:
                                ser = new Socks5(
                                    Settings.Default.ProxyServer_Address,
                                    Settings.Default.ProxyServer_Port,
                                    (authType == 2) ? Settings.Default.UserAndPassword_User : string.Empty,
                                    (authType == 2) ? Settings.Default.UserAndPassword_Pass : string.Empty);
                                break;
                            default:
                                throw new Exception("No known server type.");
                        }

                        break;
                    default:
                        throw new Exception("No known server type.");
                }

                ser.NoDataTimeout = Settings.Default.Connection_NoDataTimeout;
                this.listener.ActiveServer = ser;
                this.IndfferentForm();

                // Grabber Pre-Listen Settings
                switch ((Grabber.GrabberType)Settings.Default.Grabber)
                {
                    case Grabber.GrabberType.Tap:
                        if (Settings.Default.Server_Type == 0)
                        {
                            throw new Exception("You can't select No Server when using TAP Adapter grabber.");
                        }

                        if (Settings.Default.Server_Type == 2)
                        {
                            throw new Exception("You can't select PeaRoxyWeb Server when using TAP Adapter grabber.");
                        }

                        this.listener.DnsResolver.DnsResolverSupported =
                            this.listener.DnsResolver.DnsResolverUdpSupported = true;
                        if (this.listener.SmartPear.ForwarderHttpEnable || this.listener.SmartPear.ForwarderHttpsEnable
                            || this.listener.SmartPear.ForwarderSocksEnable)
                        {
                            if (!silent)
                            {
                                VDialog.Show(
                                    this,
                                    "We have disabled SmartPear functionality because of using TAP Adapter grabber.",
                                    "SmartPear",
                                    MessageBoxButtons.OK);
                            }

                            this.listener.SmartPear.ForwarderHttpEnable = false;
                            this.listener.SmartPear.ForwarderHttpsEnable = false;
                            this.listener.SmartPear.ForwarderSocksEnable = false;
                        }

                        break;
                    case Grabber.GrabberType.Hook:
                        if (Settings.Default.Server_Type == 2)
                        {
                            throw new Exception("You can't select PeaRoxyWeb Server when using Hook grabber.");
                        }
                        this.listener.IsHttpsSupported = true;
                        break;
                }

                if (!Settings.Default.DNS_Enable && !(ser is NoServer || ser is PeaRoxyWeb))
                {
                    try
                    {
                        Dns.GetHostAddresses("google.com");
                    }
                    catch (Exception)
                    {
                        if (silent
                            || VDialog.Show(
                                "It seems that your network can't resolve host names. PeaRoxy, PeaRoxyWeb, HTTPS and SOCKS all support requests with host name instead of IP too;\nbut in case that you want to have complete compatibility for all applications, it is recomended to enable DNS Resolver and edit your network adaptor's settings to use 'localhost' as resolving server.\n\nNow, do want us to enable DNS Resolver for you temporary?",
                                "DNS Resolver",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                        {
                            this.listener.DnsResolver.DnsResolverSupported =
                                this.listener.DnsResolver.DnsResolverUdpSupported = true;
                        }
                    }
                }

                this.listener.TestServerAsyc(
                    (suc, mes) => this.Dispatcher.Invoke(
                        (SimpleVoidDelegate)delegate
                            {
                                try
                                {
                                    if (suc)
                                    {
                                        if (this.listener.SmartPear.ForwarderHttpEnable
                                            || this.listener.SmartPear.ForwarderHttpsEnable
                                            || this.listener.SmartPear.ForwarderSocksEnable)
                                        {
                                            try
                                            {
                                                this.listener.TestServer(new NoServer());
                                            }
                                            catch (Exception ex)
                                            {
                                                if (silent
                                                    || VDialog.Show(
                                                        this,
                                                        string.Format(
                                                            "It seems you have SmartPear enabled but no direct internet connection, Do you want us to disable SmartPear temporary?! \r\n\r\n{0}",
                                                            ex.Message),
                                                        "Smart Pear",
                                                        MessageBoxButtons.YesNo,
                                                        MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                                                {
                                                    this.listener.SmartPear.ForwarderHttpEnable = false;
                                                    this.listener.SmartPear.ForwarderHttpsEnable = false;
                                                    this.listener.SmartPear.ForwarderSocksEnable = false;
                                                }
                                            }
                                        }

                                        this.listener.Start();

                                        // Grabber Pro-Listen Settings
                                        if ((Grabber.GrabberType)Settings.Default.Grabber == Grabber.GrabberType.Tap)
                                        {
                                            List<IPAddress> hostIps = new List<IPAddress>();
                                            if (CommonLibrary.Common.IsIpAddress(serverAddress))
                                            {
                                                hostIps.Add(IPAddress.Parse(serverAddress));
                                            }
                                            else
                                            {
                                                hostIps.AddRange(Dns.GetHostEntry(serverAddress).AddressList);
                                            }
                                            TapTunnelModule.ExceptionIPs = hostIps.ToArray();
                                        }

                                        this.ReConfig(silent);

                                        App.Notify.ShowBalloonTip(
                                            2000,
                                            "PeaRoxy Client",
                                            "Connected to the server\r\n" + ser.ToString(),
                                            ToolTipIcon.Info);
                                        App.Notify.Text = @"PeaRoxy Client - Working";
                                        StreamResourceInfo streamResourceInfo =
                                            System.Windows.Application.GetResourceStream(
                                                new Uri(
                                                    "pack://application:,,,/PeaRoxy.Windows.WPFClient;component/Images/Icons/Pear_Notify.ico"));
                                        if (streamResourceInfo != null)
                                        {
                                            Stream st = streamResourceInfo.Stream;
                                            App.Notify.Icon = new Icon(st);
                                            st.Close();
                                            st.Dispose();
                                        }

                                        this.Status = CurrentStatus.Connected;
                                        this.RefreshStatus(true);
                                        if (!this.updaterWorker.IsBusy)
                                        {
                                            this.updaterWorker.RunWorkerAsync();
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception(mes);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    VDialog.Show(
                                        this,
                                        "Error: " + ex.Message + "\r\n" + ex.StackTrace,
                                        "Start Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                                    if (this.listener != null)
                                    {
                                        this.listener.Stop();
                                    }

                                    this.MainPage.IsEnabled = true;
                                    this.RefreshStatus(true);
                                }
                            },
                        new object[] { }));

                return true;
            }
            catch (Exception ex)
            {
                VDialog.Show(
                    this,
                    "Error: " + ex.Message + "\r\n" + ex.StackTrace,
                    "Start Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                if (this.listener != null)
                {
                    this.listener.Stop();
                }

                this.MainPage.IsEnabled = true;
                this.RefreshStatus(true);
            }

            return false;
        }

        /// <summary>
        ///     The stop server.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool StopServer(bool silent)
        {
            try
            {
                if (this.listener != null)
                {
                    if (this.listener.Status != ProxyController.ControllerStatus.None)
                    {
                        if (this.listener.IsAutoConfigEnable && !Settings.Default.AutoConfig_KeepRuning)
                        {
                            this.listener.IsAutoConfigEnable = false;
                        }

                        this.listener.Stop();
                    }

                    this.Status = this.listener.Status.HasFlag(ProxyController.ControllerStatus.AutoConfig)
                                      ? CurrentStatus.Sleep
                                      : CurrentStatus.Disconnected;

                    this.ReConfig(silent);
                }

                this.Options.SaveSettings();
                this.MainPage.SaveSettings();
                ((SmartPear)this.Options.SmartPear.SettingsPage).FillSmartList();
                StreamResourceInfo streamResourceInfo =
                    System.Windows.Application.GetResourceStream(
                        new Uri(
                            "pack://application:,,,/PeaRoxy.Windows.WPFClient;component/Images/Icons/Pear_Red_Notify.ico"));
                if (streamResourceInfo != null)
                {
                    Stream st = streamResourceInfo.Stream;
                    App.Notify.Icon = new Icon(st);
                    st.Close();
                    st.Dispose();
                }

                App.Notify.Text = @"PeaRoxy Client - Stopped";
                return true;
            }
            catch (Exception ex)
            {
                VDialog.Show(
                    this,
                    "Error: " + ex.Message + "\r\n" + ex.StackTrace,
                    "Can't stop server",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            return false;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The smart_ list updated.
        /// </summary>
        /// <param name="item">
        ///     The item.
        /// </param>
        /// <param name="https">
        ///     The https.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void SmartListUpdated(string item, bool https, EventArgs e)
        {
            this.Dispatcher.Invoke(
                (SimpleVoidDelegate)delegate
                    {
                        ((SmartPear)this.Options.SmartPear.SettingsPage).AddToSmartList(item, https);
                        App.Notify.ShowBalloonTip(
                            10,
                            https ? "PeaRoxy Client - Https/Socks" : "PeaRoxy Client - Http",
                            "Added to SmartPear list: \r\n" + item,
                            ToolTipIcon.Info);
                    },
                new object[] { });
        }

        private void SmartPearApplySettings()
        {
            if (this.listener != null && this.listener.Status.HasFlag(ProxyController.ControllerStatus.Proxy))
            {
                this.listener.SmartPear.DetectorDirectPort80AsHttp = Settings.Default.Smart_Direct_AutoRoutePort80AsHTTP;
                this.listener.SmartPear.ForwarderHttpEnable = Settings.Default.Smart_HTTP_Enable;
                this.listener.SmartPear.DetectorHttpEnable = Settings.Default.Smart_HTTP_AutoRoute_Enable;
                this.listener.SmartPear.DetectorHttpPattern = Settings.Default.Smart_HTTP_AutoRoute_Pattern;
                this.listener.SmartPear.DetectorDnsGrabberPattern = Settings.Default.Smart_AntiDNSPattern;
                this.listener.SmartPear.DetectorDnsGrabberEnable = Settings.Default.Smart_AntiDNS_Enable;
                this.listener.SmartPear.ForwarderHttpsEnable = Settings.Default.Smart_HTTPS_Enable;
                this.listener.SmartPear.DetectorTimeoutEnable = Settings.Default.Smart_Timeout_Enable;
                this.listener.SmartPear.DetectorTimeout = Settings.Default.Smart_Timeout_Value;
                this.listener.SmartPear.ForwarderDirectPort80AsHttp = Settings.Default.Smart_Direct_Port80Router;
                this.listener.SmartPear.ForwarderSocksEnable = Settings.Default.Smart_SOCKS_Enable;
                this.listener.SmartPear.ForwarderHttpList.Clear();
                this.listener.SmartPear.ForwarderDirectList.Clear();
                foreach (string s in
                    Settings.Default.Smart_HTTP_List.Split(
                        new[] { Environment.NewLine },
                        StringSplitOptions.RemoveEmptyEntries))
                {
                    this.listener.SmartPear.ForwarderHttpList.Add(s.ToLower());
                }

                foreach (string s in
                    Settings.Default.Smart_Direct_List.Split(
                        new[] { Environment.NewLine },
                        StringSplitOptions.RemoveEmptyEntries))
                {
                    this.listener.SmartPear.ForwarderDirectList.Add(s.ToLower());
                }
            }
        }

        /// <summary>
        ///     The tabbed thumbnail bitmap requested.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void TabbedThumbnailBitmapRequested(object sender, TabbedThumbnailBitmapRequestedEventArgs e)
        {
            try
            {
                TabbedThumbnail tt = TaskbarManager.Instance.TabbedThumbnail.GetThumbnailPreview(this.Handle);
                BitmapSource bs = this.MainPage.ThumbnileChart.CreateScreenshot();
                tt.SetImage(bs);
                ThreadPool.QueueUserWorkItem(
                    c =>
                        {
                            Thread.Sleep(1000);
                            this.Dispatcher.Invoke(
                                (SimpleVoidDelegate)delegate
                                    {
                                        try
                                        {
                                            tt.InvalidatePreview();
                                        }
                                        catch (Exception)
                                        {
                                        }
                                    },
                                new object[] { });
                        });
                e.Handled = true;
                return;
            }
            catch (Exception)
            {
            }

            e.Handled = false;
        }

        private void Toolbar_OnSmartPearUpdateClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string xmlAddress = Updater.GetSmartPearProfileAddress(Settings.Default.SelectedProfile);
                Downloader downloader = new Downloader(
                    new Uri(xmlAddress),
                    false,
                    this.listener != null && this.listener.Status.HasFlag(ProxyController.ControllerStatus.Proxy)
                        ? new WebProxy(this.listener.Ip + ":" + this.listener.Port, true)
                        : null);
                if (downloader.ShowDialog() == System.Windows.Forms.DialogResult.OK && File.Exists(downloader.Filename))
                {
                    WelcomeWindow.ImportSmartProfile(SmartProfile.FromXml(File.ReadAllText(downloader.Filename)));
                    this.SmartPearApplySettings();
                    this.Options.SmartPear.SettingsPage.LoadSettings();
                    this.QuickButtonsRefresh();
                }
            }
            catch (Exception)
            {
                App.Notify.ShowBalloonTip(
                    2000,
                    "Smart Pear Update",
                    "We failed to download or apply SmartPear settings.",
                    ToolTipIcon.Error);
            }
        }

        /// <summary>
        ///     The update stats.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void UpdateStats(object sender, EventArgs e)
        {
            if (this.listener == null)
            {
                return;
            }

            this.downloadSpeed = (this.listener.AverageReceivingSpeed + this.downloadSpeed) / 2;
            this.uploadSpeed = (this.listener.AverageSendingSpeed + this.uploadSpeed) / 2;
            this.downloadPoints.Add(new ChartPoint(this.downloadSpeed / 1024d));
            this.uploadPoints.Add(new ChartPoint(this.uploadSpeed / 1024d));
            if (this.listener != null && this.listener.Status.HasFlag(ProxyController.ControllerStatus.Proxy))
            {
                App.Notify.Text = string.Format(
                    "PeaRoxy Client\r\nCurrent Transfer Rate: {0}/s",
                    CommonLibrary.Common.FormatFileSizeAsString(this.downloadSpeed + this.uploadSpeed));
            }

            try
            {
                if (TaskbarManager.IsPlatformSupported)
                {
                    if (this.listener.Status.HasFlag(ProxyController.ControllerStatus.Proxy))
                    {
                        TaskbarManager.Instance.SetOverlayIcon(this.connectedIcon, "Connected");
                    }
                    else
                    {
                        TaskbarManager.Instance.SetOverlayIcon(null, "Stopped");
                    }
                }
            }
            catch (Exception)
            {
            }

            if (this.IsVisible && this.Options.IsEnabled)
            {
                ((General)this.Options.General.SettingsPage).UpdateStats(
                    this.listener,
                    this.downloadSpeed,
                    this.uploadSpeed);
                ((ActiveConnections)this.Options.Connections.SettingsPage).UpdateConnections(this.listener);
            }
        }

        /// <summary>
        ///     The window_ content rendered.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void WindowContentRendered(object sender, EventArgs e)
        {
            try
            {
                if (!TaskbarManager.IsPlatformSupported)
                {
                    return;
                }

                // Thumbnail
                TabbedThumbnail tabbedThumbnail = new TabbedThumbnail(this.Handle, this.Handle);
                TaskbarManager.Instance.TabbedThumbnail.AddThumbnailPreview(tabbedThumbnail);
                tabbedThumbnail.DisplayFrameAroundBitmap = false;
                tabbedThumbnail.ClippingRectangle = new Rectangle(
                    0,
                    0,
                    (int)this.MainPage.ThumbnileChart.ActualWidth,
                    (int)this.MainPage.ThumbnileChart.ActualHeight);
                tabbedThumbnail.Title = this.Title;
                StreamResourceInfo streamResourceInfo =
                    System.Windows.Application.GetResourceStream(
                        new Uri(
                            "pack://application:,,,/PeaRoxy.Windows.WPFClient;component/Images/Icons/Pear_Notify.ico"));
                if (streamResourceInfo != null)
                {
                    Stream st = streamResourceInfo.Stream;
                    tabbedThumbnail.SetWindowIcon(new Icon(st));
                }

                tabbedThumbnail.PeekOffset = new Vector(5000, 5000);
                tabbedThumbnail.TabbedThumbnailBitmapRequested += this.TabbedThumbnailBitmapRequested;
                Windows7ShellPreviewWindowFixer.Fix(this.Title, Process.GetCurrentProcess());

                App.InitJumpList();
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}