// --------------------------------------------------------------------------------------------------------------------
// <copyright file="frm_Main.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The main form.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ZARA
{
    #region

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    using CircularProgressBar;

    using LukeSw.Windows.Forms;

    using PeaRoxy.ClientLibrary;
    using PeaRoxy.ClientLibrary.ServerModules;
    using PeaRoxy.Updater;
    using PeaRoxy.Windows;
    using PeaRoxy.Windows.Network.Hook;
    using PeaRoxy.Windows.Network.TAP;

    using WinFormAnimation;

    using ZARA.Properties;

    using Common = PeaRoxy.CommonLibrary.Common;
    using Timer = WinFormAnimation.Timer;
    using WinCommon = PeaRoxy.Windows.Common;

    #endregion

    /// <summary>
    ///     The main form.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter",
        Justification = "Reviewed. Suppression is OK here.")]

    // ReSharper disable once InconsistentNaming
    public sealed partial class frm_Main : Form
    {
        #region Constants

        /// <summary>
        ///     The HT_CAPTION.
        /// </summary>
        public const int HtCaption = 0x2;

        /// <summary>
        ///     The WM_NCLBUTTONDOWN.
        /// </summary>
        public const int WmNclbuttondown = 0xA1;

        #endregion

        #region Fields

        /// <summary>
        ///     The animation.
        /// </summary>
        private readonly Animator2D ani = new Animator2D(Timer.FpsLimiter.Fps60);

        /// <summary>
        ///     The download points.
        /// </summary>
        private readonly Queue<ChartPoint> downloadPoints;

        /// <summary>
        ///     The upload points.
        /// </summary>
        private readonly Queue<ChartPoint> uploadPoints;

        /// <summary>
        ///     The down speed.
        /// </summary>
        private long downloadSpeed;

        private AppVersion latestVersion;

        /// <summary>
        ///     The listener.
        /// </summary>
        private ProxyController listener;

        private bool skipUpdate;

        private BackgroundWorker updaterWorker;

        /// <summary>
        ///     The up speed.
        /// </summary>
        private long uploadSpeed;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="frm_Main" /> class.
        /// </summary>
        public frm_Main()
        {
            this.InitializeComponent();
            this.downloadPoints = new Queue<ChartPoint>(240);
            this.uploadPoints = new Queue<ChartPoint>(240);
            this.MaximumSize = this.MinimumSize = this.Size = new Size(496, 316);
            this.pnl_main.Location = new Point(-200, 0);
        }

        #endregion

        #region Delegates

        /// <summary>
        ///     The simple void delegate.
        /// </summary>
        private delegate void SimpleVoidDelegate();

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
            // ReSharper disable once UnusedMember.Global
            Sleep,
        }

        public enum GrabberType
        {
            None = 0,

            Tap = 1,

            Hook = 2,

            Proxy = 3,
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the status.
        /// </summary>
        public CurrentStatus Status { get; set; }

        #endregion

        #region Public Methods and Operators

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

            switch ((GrabberType)Settings.Default.Grabber)
            {
                case GrabberType.Proxy:
                    bool res = ProxyModule.SetActiveProxy(new IPEndPoint(IPAddress.Loopback, this.listener.Port));
                    bool firefoxRes = ProxyModule.ForceFirefoxToUseSystemSettings();

                    if (!res)
                    {
                        if (silent)
                        {
                            Program.Notify.ShowBalloonTip(
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
                                MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                WinCommon.LogOffUser();
                            }
                        }
                    }
                    else
                    {
                        if (firefoxRes)
                        {
                            Program.Notify.ShowBalloonTip(
                                5000,
                                "Proxy Grabber",
                                "PeaRoxy successfully registered as active proxy of this system. You may need to restart your browser to be able to use it along with PeaRoxy.",
                                ToolTipIcon.Info);
                        }
                        else
                        {
                            Program.Notify.ShowBalloonTip(
                                5000,
                                "Proxy Grabber",
                                "PeaRoxy successfully registered as active proxy of this system. Firefox probably need some manual configurations, also you may need to restart your browser to be able to use it along with PeaRoxy.",
                                ToolTipIcon.Warning);
                        }
                    }

                    break;
                case GrabberType.Tap:
                    if (this.listener != null && this.listener.Status.HasFlag(ProxyController.ControllerStatus.Proxy))
                    {
                        TapTunnelModule.AdapterAddressRange = IPAddress.Parse(Settings.Default.TAP_IPRange);
                        TapTunnelModule.DnsResolvingAddress = IPAddress.Loopback;
                        TapTunnelModule.SocksProxyEndPoint = new IPEndPoint(IPAddress.Loopback, this.listener.Port);
                        TapTunnelModule.AutoDnsResolving = true;
                        TapTunnelModule.AutoDnsResolvingAddress = IPAddress.Parse(Settings.Default.DNS_IPAddress);
                        TapTunnelModule.TunnelName = "ZARA Tunnel";

                        if (TapTunnelModule.StartTunnel())
                        {
                            Program.Notify.ShowBalloonTip(
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
                                Program.Notify.ShowBalloonTip(
                                    5000,
                                    "TAP Grabber",
                                    "Failed to start TAP Adapter. Tap grabber disabled. Please go to control panel to see if network adapter is disable.",
                                    ToolTipIcon.Warning);
                            }
                            else
                            {
                                VDialog.Show(
                                    this,
                                    "Failed to start TAP Adapter. Tap grabber disabled. Please go to control panel to see if network adapter is disable.",
                                    "TAP Grabber",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                            }
                        }
                    }
                    break;
                case GrabberType.Hook:
                    HookModule.StartHookProcess(
                        Settings.Default.Hook_Processes.Split(
                            new[] { Environment.NewLine },
                            StringSplitOptions.RemoveEmptyEntries),
                        new IPEndPoint(IPAddress.Loopback, this.listener.Port),
                        string.Empty);

                    Program.Notify.ShowBalloonTip(
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
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Username is null or empty
        /// </exception>
        /// <exception cref="Exception">
        ///     Failed to start server
        /// </exception>
        public bool StartServer()
        {
            try
            {
                if (string.IsNullOrEmpty(Settings.Default.UserAndPassword_User))
                {
                    // ReSharper disable once NotResolvedInText
                    throw new ArgumentException(@"Invalid value.", "UserName");
                }

                if (this.listener != null)
                {
                    this.StopServer();
                }
                else
                {
                    this.listener = new ProxyController(null, null, 0);
                    this.listener.AutoDisconnectedDueToFailureNotify += this.FailDisconnected;
                }

                this.Enabled = false;

                this.listener.Ip = IPAddress.Loopback;
                this.listener.Port = 0;
                this.listener.IsAutoConfigEnable = false;
                this.listener.IsHttpSupported = false;
                this.listener.IsHttpsSupported = false;
                this.listener.IsSocksSupported = true;
                this.listener.SendPacketSize = Settings.Default.Connection_SendPacketSize;
                this.listener.ReceivePacketSize = Settings.Default.Connection_RecPacketSize;
                this.listener.AutoDisconnect = Settings.Default.Connection_StopOnInterrupt;

                this.listener.ErrorRenderer.EnableOnHttp = false;
                this.listener.ErrorRenderer.EnableOnPort80 = false;
                this.listener.ErrorRenderer.EnableOnPort443 = false;

                this.listener.DnsResolver.DnsResolverSupported =
                    this.listener.DnsResolver.DnsResolverUdpSupported = false;
                this.listener.DnsResolver.DnsResolverServerIp = IPAddress.Parse(Settings.Default.DNS_IPAddress);

                this.listener.SmartPear.DetectorTreatPort80AsHttp = false;
                this.listener.SmartPear.ForwarderHttpEnable = false;
                this.listener.SmartPear.DetectorHttpCheckEnable = false;
                this.listener.SmartPear.DetectorHttpPattern = string.Empty;
                this.listener.SmartPear.DetectorDnsPoisoningPattern = string.Empty;
                this.listener.SmartPear.DetectorDnsPoisoningEnable = false;
                this.listener.SmartPear.ForwarderHttpsEnable = false;
                this.listener.SmartPear.DetectorTimeoutEnable = false;
                this.listener.SmartPear.DetectorTimeout = 0;
                this.listener.SmartPear.ForwarderTreatPort80AsHttp = false;
                this.listener.SmartPear.ForwarderSocksEnable = false;
                this.listener.SmartPear.ForwarderHttpList.Clear();
                this.listener.SmartPear.ForwarderDirectList.Clear();

                string serverAddress = Settings.Default.ServerAddress;

                ServerType ser = new PeaRoxy(
                    Settings.Default.ServerAddress,
                    Settings.Default.ServerPort,
                    string.Empty,
                    Settings.Default.UserAndPassword_User,
                    Settings.Default.UserAndPassword_Pass,
                    Common.EncryptionTypes.SimpleXor);

                ser.NoDataTimeout = Settings.Default.Connection_NoDataTimeout;
                this.listener.ActiveServer = ser;

                this.listener.TestServerAsyc(
                    (suc, mes) => this.Invoke(
                        (SimpleVoidDelegate)delegate
                            {
                                try
                                {
                                    if (suc)
                                    {
                                        this.listener.Start();

                                        // Grabber Pro-Listen Settings
                                        if ((GrabberType)Settings.Default.Grabber == GrabberType.Tap)
                                        {
                                            List<IPAddress> hostIps = new List<IPAddress>();
                                            if (Common.IsIpAddress(serverAddress))
                                            {
                                                hostIps.Add(IPAddress.Parse(serverAddress));
                                            }
                                            else
                                            {
                                                hostIps.AddRange(Dns.GetHostEntry(serverAddress).AddressList);
                                            }
                                            TapTunnelModule.ExceptionIPs = hostIps.ToArray();
                                        }

                                        this.ReConfig(false);

                                        Program.Notify.ShowBalloonTip(
                                            2000,
                                            "Z A Я A",
                                            "Connected to the server\r\n" + ser.ToString(),
                                            ToolTipIcon.Info);
                                        Program.Notify.Text = @"Z A Я A - Working";
                                        this.Status = CurrentStatus.Connected;
                                        this.RefreshStatus();
                                    }
                                    else
                                    {
                                        throw new Exception(mes);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    this.ShowDialog(
                                        "Error: " + ex.Message + ex.StackTrace,
                                        "Start Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                                    if (this.listener != null)
                                    {
                                        this.listener.Stop();
                                    }

                                    this.RefreshStatus();
                                    if (!this.updaterWorker.IsBusy)
                                    {
                                        this.updaterWorker.RunWorkerAsync();
                                    }
                                }
                            },
                        new object[] { }));

                return true;
            }
            catch (Exception ex)
            {
                this.ShowDialog("Error: " + ex.Message, "Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (this.listener != null)
                {
                    this.listener.Stop();
                }

                this.RefreshStatus();
            }

            return false;
        }

        /// <summary>
        ///     The stop server.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool StopServer()
        {
            try
            {
                this.Enabled = false;
                if (this.listener != null)
                {
                    if (this.listener.Status != ProxyController.ControllerStatus.None)
                    {
                        this.listener.Stop();
                    }

                    this.Status = CurrentStatus.Disconnected;

                    this.ReConfig(false);
                }

                this.SaveSettings();
                Program.Notify.Text = @"Z A Я A - Stopped";
                this.RefreshStatus();
                return true;
            }
            catch (Exception ex)
            {
                this.ShowDialog("Error: " + ex.Message, "Can't stop server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.RefreshStatus();
            }

            return false;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The release capture.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        /// <summary>
        ///     The send message.
        /// </summary>
        /// <param name="winH">
        ///     The window handler.
        /// </param>
        /// <param name="msg">
        ///     The message.
        /// </param>
        /// <param name="paramW">
        ///     The W parameter.
        /// </param>
        /// <param name="paramL">
        ///     The L parameter.
        /// </param>
        /// <returns>
        ///     The <see cref="int" />.
        /// </returns>
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr winH, int msg, int paramW, int paramL);

        /// <summary>
        ///     The split by space.
        /// </summary>
        /// <param name="str">
        ///     The string.
        /// </param>
        /// <param name="a1">
        ///     The a 1.
        /// </param>
        /// <param name="a2">
        ///     The a 2.
        /// </param>
        private static void SplitBySpace(string str, Label a1, Label a2)
        {
            if (str.IndexOf(" ", StringComparison.Ordinal) > -1)
            {
                a1.Text = str.Split(' ')[0];
                a2.Text = str.Split(' ')[1] + @"ps";
            }
        }

        /// <summary>
        ///     The split by space.
        /// </summary>
        /// <param name="str">
        ///     The string.
        /// </param>
        /// <param name="a1">
        ///     The a 1.
        /// </param>
        private static void SplitBySpace(string str, CircularProgressBar a1)
        {
            if (str.IndexOf(" ", StringComparison.Ordinal) > -1)
            {
                if (str.Split(' ')[0].IndexOf('.') > -1)
                {
                    a1.Caption = str.Split(' ')[0].Split('.')[0];
                    a1.SubText = "." + str.Split(' ')[0].Split('.')[1];
                }
                else
                {
                    a1.Caption = str.Split(' ')[0];
                }

                a1.SupText = str.Split(' ')[1];
            }
        }

        /// <summary>
        ///     The disconnect click.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void BtnDisconnectClick(object sender, EventArgs e)
        {
            this.SaveSettings();
            if (this.Status == CurrentStatus.Connected)
            {
                this.StopServer();
            }
        }

        /// <summary>
        ///     The exit click.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void BtnExitClick(object sender, EventArgs e)
        {
            this.StopServer();
            Application.Exit();
        }

        /// <summary>
        ///     The login click.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void BtnLoginClick(object sender, EventArgs e)
        {
            this.SaveSettings();
            if (this.Status != CurrentStatus.Connected)
            {
                this.StartServer();
            }
        }

        /// <summary>
        ///     The minimize click.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void BtnMinimizeClick(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                this.Hide();
            }
            else
            {
                this.Show();
            }

            Program.Notify.Visible = !this.Visible;
        }

        /// <summary>
        ///     The mouse down.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void DragMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WmNclbuttondown, HtCaption, 0);
            }
        }

        /// <summary>
        ///     The fail disconnected.
        /// </summary>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void FailDisconnected(EventArgs e)
        {
            this.listener.AutoDisconnect = false;
            this.Invoke(
                (SimpleVoidDelegate)delegate
                    {
                        this.ShowDialog(
                            "Connection to the server interrupted.",
                            "Z A Я A",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Stop);
                        Program.Notify.ShowBalloonTip(
                            1000,
                            "Z A Я A",
                            "Connection to the server interrupted.",
                            ToolTipIcon.Error);
                        this.StopServer();
                        this.RefreshStatus();
                    },
                new object[] { });
        }

        /// <summary>
        ///     The main form closing.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void FrmMainFormClosing(object sender, FormClosingEventArgs e)
        {
            this.StopServer();
        }

        /// <summary>
        ///     The main form load
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void FrmMainLoad(object sender, EventArgs e)
        {
            if (Settings.Default.FirstRun)
            {
                if (Settings.Default.UpdateSettings)
                {
                    Settings.Default.Upgrade();
                }

                Settings.Default.FirstRun = false;
                Settings.Default.Save();
            }

            WindowsModule platform = new WindowsModule();
            platform.RegisterPlatform();
            this.ReloadSettings();

            this.updaterWorker = new BackgroundWorker();
            this.skipUpdate = !Settings.Default.AutoUpdate;
            this.updaterWorker.DoWork += (s, args) =>
                {
                    this.latestVersion = null;
                    try
                    {
                        if (!this.skipUpdate)
                        {
                            Updater updaterObject = new Updater(
                                "ZARA",
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
            this.updaterWorker.RunWorkerCompleted += (s, args) =>
                {
                    try
                    {
                        if (this.latestVersion != null)
                        {
                            VDialog updaterDialog = new VDialog
                                                        {
                                                            Content =
                                                                "New version of this application is available for download, do you want us to open the release page so you can download it?",
                                                            WindowTitle = "Auto Update Check",
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

                            this.skipUpdate = updaterDialog.Show() != VDialogResult.Yes;
                            if (!this.skipUpdate)
                            {
                                Process.Start(this.latestVersion.PageLink);
                            }
                        }
                    }
                    catch
                    {
                    }
                };

            this.updaterWorker.RunWorkerAsync();

            try
            {
                Program.Notify.ContextMenu =
                    new ContextMenu(
                        new[]
                            {
                                new MenuItem(
                                    "Show Window",
                                    delegate
                                        {
                                            Program.Notify.GetType()
                                                .GetMethod(
                                                    "OnDoubleClick",
                                                    BindingFlags.Instance | BindingFlags.NonPublic)
                                                .Invoke(Program.Notify, new object[] { null });
                                        })
                                    {
                                        DefaultItem = true
                                    },
                                new MenuItem("-", (EventHandler)null), new MenuItem(
                                                                           "Exit",
                                                                           delegate
                                                                               {
                                                                                   this.StopServer();
                                                                                   Application.Exit();
                                                                               })
                            });
                Program.Notify.Icon = Resources.Icon;
                Program.Notify.Text = @"Z A Я A - Stopped";
                Program.Notify.DoubleClick += this.BtnMinimizeClick;
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        ///     The move page to.
        /// </summary>
        /// <param name="pos">
        ///     The position.
        /// </param>
        private void MovePageTo(int pos)
        {
            this.ani.Stop();
            if (pos != this.pnl_main.Location.X)
            {
                this.ani.SetPaths(
                    new Path2D(
                        this.pnl_main.Location,
                        new Point(pos, this.pnl_main.Location.Y),
                        600,
                        300,
                        Functions.CubicEaseOut));
                this.ani.Play(this.pnl_main, x => x.Location);
            }
        }

        /// <summary>
        ///     The refresh status.
        /// </summary>
        private void RefreshStatus()
        {
            switch (this.Status)
            {
                case CurrentStatus.Connected:
                    this.btn_login.Enabled = false;
                    this.txt_password.Enabled = false;
                    this.txt_server.Enabled = false;
                    this.txt_username.Enabled = false;
                    this.btn_disconnect.Enabled = true;
                    this.lbl_status.Text = @"CONNECTED";
                    this.pb_status.Image = Resources.connected;
                    this.MovePageTo(0);
                    break;
                default:
                    this.btn_login.Enabled = true;
                    this.txt_password.Enabled = true;
                    this.txt_server.Enabled = true;
                    this.txt_username.Enabled = true;
                    this.btn_disconnect.Enabled = false;
                    this.lbl_status.Text = @"DISCONNECTED";
                    this.pb_status.Image = Resources.disconnected;
                    this.MovePageTo(-200);
                    break;
            }

            this.Enabled = true;
        }

        /// <summary>
        ///     The reload settings.
        /// </summary>
        private void ReloadSettings()
        {
            if (Settings.Default.ServerAddress != string.Empty)
            {
                this.txt_server.Text = Settings.Default.ServerAddress;
                if (!Settings.Default.ShowHost)
                {
                    this.pnl_host.Visible = false;
                }
            }

            if (Settings.Default.UserAndPassword_User != string.Empty)
            {
                this.txt_username.Text = Settings.Default.UserAndPassword_User;
            }

            if (Settings.Default.UserAndPassword_Pass != string.Empty)
            {
                this.txt_password.Text = Settings.Default.UserAndPassword_Pass;
                this.txt_password.PasswordChar = 'x';
            }
        }

        /// <summary>
        ///     The save settings.
        /// </summary>
        private void SaveSettings()
        {
            Settings.Default.ServerAddress = this.txt_server.Text;
            Settings.Default.UserAndPassword_User = this.txt_username.Text;
            Settings.Default.UserAndPassword_Pass = this.txt_password.Text;
            Settings.Default.Save();
        }

        /// <summary>
        ///     The show dialog.
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="title">
        ///     The title.
        /// </param>
        /// <param name="messageBoxButtons">
        ///     The message box buttons.
        /// </param>
        /// <param name="messageBoxIcon">
        ///     The message box icon.
        /// </param>
        private void ShowDialog(
            string message,
            string title,
            MessageBoxButtons messageBoxButtons,
            MessageBoxIcon messageBoxIcon)
        {
            VDialog.Show(this, message, title, messageBoxButtons, messageBoxIcon);
        }

        /// <summary>
        ///     The stat timer_ tick.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void StatTimerTick(object sender, EventArgs e)
        {
            if (this.listener == null)
            {
                return;
            }

            this.downloadSpeed = (this.listener.AverageReceivingSpeed + this.downloadSpeed) / 2;
            this.uploadSpeed = (this.listener.AverageSendingSpeed + this.uploadSpeed) / 2;
            if (this.listener != null && this.listener.Status.HasFlag(ProxyController.ControllerStatus.Proxy))
            {
                Program.Notify.Text = string.Format(
                    "Z A Я A\r\nCurrent Transfer Rate: {0}/s",
                    Common.FormatFileSizeAsString(this.downloadSpeed + this.uploadSpeed));
            }

            this.downloadPoints.Enqueue(new ChartPoint(this.downloadSpeed));
            this.uploadPoints.Enqueue(new ChartPoint(this.uploadSpeed));

            if (!this.Visible)
            {
                return;
            }

            this.lbl_stat_acceptingthreads.Text =
                this.listener.AcceptingConnections.ToString(CultureInfo.InvariantCulture);
            this.lbl_stat_activeconnections.Text =
                this.listener.RoutingConnections.ToString(CultureInfo.InvariantCulture);
            SplitBySpace(
                Common.FormatFileSizeAsString(this.listener.ReceivedBytes),
                this.lbl_stat_downloaded,
                this.lbl_stat_downloaded_v);
            SplitBySpace(
                Common.FormatFileSizeAsString(this.listener.SentBytes),
                this.lbl_stat_uploaded,
                this.lbl_stat_uploaded_v);
            SplitBySpace(Common.FormatFileSizeAsString(this.downloadSpeed), this.cpb_stat_downloadrate);
            SplitBySpace(Common.FormatFileSizeAsString(this.uploadSpeed), this.cpb_stat_uploadrate);
            TimeSpan last5Min = new TimeSpan(Environment.TickCount - (4 * 60 * 1000));
            this.cpb_stat_downloadrate.Value = this.downloadSpeed
                                               / Math.Max(
                                                   (float)
                                                   this.downloadPoints.Where(t => t.Time > last5Min).Max(t => t.Data),
                                                   1) * 100;

            this.cpb_stat_uploadrate.Value = this.uploadSpeed
                                             / Math.Max(
                                                 (float)this.uploadPoints.Where(t => t.Time > last5Min).Max(t => t.Data),
                                                 1) * 100;
        }

        /// <summary>
        ///     The txt_ leave.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void TxtLeave(object sender, EventArgs e)
        {
            this.SaveSettings();
        }

        /// <summary>
        ///     The txt_password_ enter.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void TxtPasswordEnter(object sender, EventArgs e)
        {
            if (this.txt_password.PasswordChar == '\0')
            {
                this.txt_password.Text = string.Empty;
                this.txt_password.PasswordChar = 'x';
            }
        }

        /// <summary>
        ///     The txt_server_ leave.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        private void TxtServerLeave(object sender, EventArgs e)
        {
            if (this.txt_server.Text == string.Empty)
            {
                return;
            }

            try
            {
                this.txt_server.Text = this.txt_server.Text.Replace("\\", "/");
                if (this.txt_server.Text.IndexOf("://", StringComparison.Ordinal) == -1)
                {
                    this.txt_server.Text = @"http://" + this.txt_server.Text;
                }

                Uri uri = new Uri(this.txt_server.Text);
                this.txt_server.Text = uri.Host;
            }
            catch (Exception ex)
            {
                VDialog.Show(
                    "Value is not acceptable.\r\n" + ex.Message,
                    "Data Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                this.txt_server.Text = string.Empty;
                new Thread(
                    delegate()
                        {
                            Thread.Sleep(10);
                            this.Invoke((SimpleVoidDelegate)(() => this.txt_server.Focus()), new object[] { });
                        }) { IsBackground = true }.Start();
            }

            this.SaveSettings();
        }

        #endregion
    }
}