using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Threading;
using PeaRoxy.CommonLibrary;
using PeaRoxy.ClientLibrary;
using PeaRoxy.ClientLibrary.Server_Types;
using LukeSw.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.Threading;
using Microsoft.WindowsAPICodePack.ApplicationServices;
namespace PeaRoxy.Windows.WPFClient
{
    public partial class MainWindow : Window, ISynchronizeInvoke, System.Windows.Forms.IWin32Window
    {
        public enum CurrentStatus
        {
            Disconnected,
            Connected,
            Sleep,
        }
        private Proxy_Controller Listener;
        private Network.TapTunnel tapTunnel;

        System.Drawing.Icon connectedIcon;
        public CurrentStatus Status { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            if (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.FirstRun)
            {
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Upgrade();
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.FirstRun = false;
                PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Save();
            }

            Windows.WindowsModule platform = new WindowsModule();
            platform.RegisterPlatform();

            try
            {
                App.Notify.ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[] {
                    new System.Windows.Forms.MenuItem("Show / Hide", (EventHandler)delegate{
                        App.Notify.GetType().GetMethod("OnDoubleClick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(App.Notify, new object[] { null });
                    }) {DefaultItem = true},
                    new System.Windows.Forms.MenuItem("-", (EventHandler)null),
                    new System.Windows.Forms.MenuItem("Exit", (EventHandler)delegate{
                        StopServer();
                        App.End();
                    })
                });
                System.IO.Stream st = Application.GetResourceStream(new Uri("pack://application:,,,/PeaRoxy.Windows.WPFClient;component/Images/Icons/Pear_Red_Notify.ico")).Stream;
                App.Notify.Icon = new System.Drawing.Icon(st);
                st.Close();
                st.Dispose();
                App.Notify.Text = "PeaRoxy Client - Stopped";
            }
            catch (Exception) { }
            connectedIcon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/PeaRoxy.Windows.WPFClient;component/Images/Icons/Connected.ico")).Stream);
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Tick += new EventHandler(UpdateStats);
            timer.Start();
            
            Status = CurrentStatus.Disconnected;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                if (TaskbarManager.IsPlatformSupported)
                {
                    // Thumbnail
                    TabbedThumbnail tt = new TabbedThumbnail(this.Handle, this.Handle);
                    TaskbarManager.Instance.TabbedThumbnail.AddThumbnailPreview(tt);
                    tt.DisplayFrameAroundBitmap = false;
                    tt.ClippingRectangle = new System.Drawing.Rectangle(0, 0, (int)MainPage.ThumbnileChart.ActualWidth, (int)MainPage.ThumbnileChart.ActualHeight);
                    tt.Title = this.Title;
                    System.IO.Stream st = Application.GetResourceStream(new Uri("pack://application:,,,/PeaRoxy.Windows.WPFClient;component/Images/Icons/Pear_Notify.ico")).Stream;
                    tt.SetWindowIcon(new System.Drawing.Icon(st));
                    tt.PeekOffset = new Vector(5000, 5000);
                    tt.TabbedThumbnailBitmapRequested += TabbedThumbnailBitmapRequested;
                    Windows7ShellPreviewWindowFixer.Fix(this.Title, System.Diagnostics.Process.GetCurrentProcess());

                    // JumpList
                    JumpList jl = JumpList.CreateJumpListForIndividualWindow("", this.Handle);
                    jl.KnownCategoryToDisplay = JumpListKnownCategoryType.Neither;
                    jl.ClearAllUserTasks();
                    string ourFileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    JumpListLink quitLink = new JumpListLink(ourFileName, "Quit PeaRoxy");
                    quitLink.IconReference = new Microsoft.WindowsAPICodePack.Shell.IconReference(ourFileName, 1);
                    quitLink.Arguments = "/quit";
                    jl.AddUserTasks(new JumpListTask[] { quitLink });
                    jl.Refresh();
                }
            }
            catch (Exception) { }
        }

        private void TabbedThumbnailBitmapRequested(object sender, TabbedThumbnailBitmapRequestedEventArgs e)
        {
            try
            {
                TabbedThumbnail tt = TaskbarManager.Instance.TabbedThumbnail.GetThumbnailPreview(this.Handle);
                BitmapSource bs = MainPage.ThumbnileChart.CreateScreenshot();
                tt.SetImage(bs);
                ThreadPool.QueueUserWorkItem(c =>
                {
                    Thread.Sleep(1000);
                    this.Dispatcher.Invoke((SimpleVoid_Delegate)delegate()
                    {
                        try
                        {
                            tt.InvalidatePreview();
                        }
                        catch (Exception) { }
                    }, new object[] { });
                });
                e.Handled = true;
                return;
            }
            catch (Exception) { }
            e.Handled = false;
        }

        public bool StartServer(bool silent = false)
        {
            try
            {
                MainPage.IsEnabled = false;
                if (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Auth_Type == 2)
                    if (string.IsNullOrEmpty(PeaRoxy.Windows.WPFClient.Properties.Settings.Default.UserAndPassword_User))
                        throw new ArgumentException("Invalid value.", "UserName");

                if (Listener != null)
                    StopServer();
                else
                {
                    Listener = new ClientLibrary.Proxy_Controller(null, null, 0);
                    Listener.SmartPear.Forwarder_ListUpdated += Smart_ListUpdated;
                    Listener.FailDisconnected += FailDisconnected;
                }

                Listener.IP = ((PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_Address == "*") ? System.Net.IPAddress.Any : System.Net.IPAddress.Parse(PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_Address));
                Listener.Port = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_Port;
                Listener.IsAutoConfigEnable = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.AutoConfig_Enable;
                Listener.AutoConfigMime = (ClientLibrary.Proxy_Controller.AutoConfigMimeType)PeaRoxy.Windows.WPFClient.Properties.Settings.Default.AutoConfig_Mime;
                Listener.AutoConfigPath = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.AutoConfig_Address;
                Listener.IsHTTP_Supported = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_HTTP;
                Listener.IsHTTPS_Supported = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_HTTPS;
                Listener.IsSOCKS_Supported = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_SOCKS;
                Listener.SendPacketSize = (int)PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_SendPacketSize;
                Listener.ReceivePacketSize = (int)PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_RecPacketSize;
                Listener.AutoDisconnect = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_StopOnInterrupt;

                Listener.ErrorRenderer.HTTPErrorRendering = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ErrorRenderer_EnableHTTP;
                Listener.ErrorRenderer.DirectErrorRendering_Port80 = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ErrorRenderer_Enable80;
                Listener.ErrorRenderer.DirectErrorRendering_Port443 = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ErrorRenderer_Enable443;

                Listener.DNSResolver.DNSResolver_Supported = Listener.DNSResolver.DNSResolver_UDPSupported = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.DNS_Enable;
                Listener.DNSResolver.DNSResolver_ServerIP = System.Net.IPAddress.Parse(PeaRoxy.Windows.WPFClient.Properties.Settings.Default.DNS_IPAddress);

                Listener.SmartPear.Detector_Direct_Port80AsHTTP = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_Direct_AutoRoutePort80AsHTTP;
                Listener.SmartPear.Forwarder_HTTP_Enable = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTP_Enable;
                Listener.SmartPear.Detector_HTTP_Enable = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTP_AutoRoute_Enable;
                Listener.SmartPear.Detector_HTTP_Pattern = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTP_AutoRoute_Pattern;
                Listener.SmartPear.Detector_DNSGrabber_Pattern = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_AntiDNSPattern;
                Listener.SmartPear.Detector_DNSGrabber_Enable = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_AntiDNS_Enable;
                Listener.SmartPear.Forwarder_HTTPS_Enable = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTPS_Enable;
                Listener.SmartPear.Detector_Timeout_Enable = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_Timeout_Enable;
                Listener.SmartPear.Detector_Timeout = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_Timeout_Value;
                Listener.SmartPear.Forwarder_Direct_Port80AsHTTP = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_Direct_Port80Router;
                Listener.SmartPear.Forwarder_SOCKS_Enable = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_SOCKS_Enable;
                Listener.SmartPear.Forwarder_HTTP_List.Clear();
                Listener.SmartPear.Forwarder_Direct_List.Clear();
                foreach (string s in PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_HTTP_List.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    Listener.SmartPear.Forwarder_HTTP_List.Add(s.ToLower());
                foreach (string s in PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Smart_Direct_List.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    Listener.SmartPear.Forwarder_Direct_List.Add(s.ToLower());

                ServerType ser;
                string ServerAddress = "";
                switch (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Server_Type)
                {
                    case 0:
                        ser = new Server_NoServer();
                        break;
                    case 1:
                        ServerAddress = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.PeaRoxySocks_Address;
                        ser = new Server_PeaRoxy(
                            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.PeaRoxySocks_Address,
                            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.PeaRoxySocks_Port,
                            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.PeaRoxySocks_Domain,
                            ((PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Auth_Type == 2) ? PeaRoxy.Windows.WPFClient.Properties.Settings.Default.UserAndPassword_User : string.Empty),
                            ((PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Auth_Type == 2) ? PeaRoxy.Windows.WPFClient.Properties.Settings.Default.UserAndPassword_Pass : string.Empty),
                            (CommonLibrary.Common.Encryption_Type)PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_Encryption,
                            (CommonLibrary.Common.Compression_Type)PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_Compression
                            );
                        break;
                    case 2:
                        ServerAddress = new Uri(PeaRoxy.Windows.WPFClient.Properties.Settings.Default.PeaRoxyWeb_Address).DnsSafeHost;
                        ser = new Server_PeaRoxyWeb(
                            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.PeaRoxyWeb_Address,
                            ((PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Auth_Type == 2) ? PeaRoxy.Windows.WPFClient.Properties.Settings.Default.UserAndPassword_User : string.Empty),
                            ((PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Auth_Type == 2) ? PeaRoxy.Windows.WPFClient.Properties.Settings.Default.UserAndPassword_Pass : string.Empty),
                            (CommonLibrary.Common.Encryption_Type)PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_Encryption
                            );
                        Listener.SmartPear.Forwarder_HTTPS_Enable = false;
                        break;
                    case 3:
                        ServerAddress = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ProxyServer_Address;
                        switch (WPFClient.Properties.Settings.Default.ProxyServer_Type)
                        {
                            case 0:
                                ser = new Server_HTTPS(
                                    PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ProxyServer_Address,
                                    PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ProxyServer_Port,
                                    ((PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Auth_Type == 2) ? PeaRoxy.Windows.WPFClient.Properties.Settings.Default.UserAndPassword_User : string.Empty),
                                    ((PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Auth_Type == 2) ? PeaRoxy.Windows.WPFClient.Properties.Settings.Default.UserAndPassword_Pass : string.Empty)
                                    );
                                break;
                            case 1:
                                ser = new Server_SOCKS5(
                                    PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ProxyServer_Address,
                                    PeaRoxy.Windows.WPFClient.Properties.Settings.Default.ProxyServer_Port,
                                    ((PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Auth_Type == 2) ? PeaRoxy.Windows.WPFClient.Properties.Settings.Default.UserAndPassword_User : string.Empty),
                                    ((PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Auth_Type == 2) ? PeaRoxy.Windows.WPFClient.Properties.Settings.Default.UserAndPassword_Pass : string.Empty)
                                    );
                                break;
                            default:
                                throw new Exception("No known server type.");
                        }
                        break;
                    default:
                        throw new Exception("No known server type.");
                }

                ser.NoDataTimeout = PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Connection_NoDataTimeout;
                Listener.ActiveServer = ser;
                IndfferentForm();

                // Grabber Pre-Listen Settings
                switch (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Grabber)
                {
                    case 0:
                        // Do nothing
                        break;
                    case 1:
                        if (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Server_Type == 0)
                            throw new Exception("You can't select No Server when using TAP Adapter grabber.");
                        if (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Server_Type == 2)
                            throw new Exception("You can't select PeaRoxyWeb Server when using TAP Adapter grabber.");
                        Listener.DNSResolver.DNSResolver_Supported = Listener.DNSResolver.DNSResolver_UDPSupported = true;
                        if (Listener.SmartPear.Forwarder_HTTP_Enable || Listener.SmartPear.Forwarder_HTTPS_Enable || Listener.SmartPear.Forwarder_SOCKS_Enable)
                        {
                            if (!silent)
                                VDialog.Show(this, "We have disabled SmartPear functionality because of using TAP Adapter grabber.", "SmartPear", System.Windows.Forms.MessageBoxButtons.OK);
                            Listener.SmartPear.Forwarder_HTTP_Enable = false;
                            Listener.SmartPear.Forwarder_HTTPS_Enable = false;
                            Listener.SmartPear.Forwarder_SOCKS_Enable = false;
                        }
                        break;
                    case 2:
                        // Do nothing
                        break;
                    default:
                        break;
                }
                if (!PeaRoxy.Windows.WPFClient.Properties.Settings.Default.DNS_Enable && !typeof(Server_NoServer).Equals(ser) && !typeof(Server_PeaRoxyWeb).Equals(ser))
                {
                    try
                    {
                        System.Net.Dns.GetHostAddresses("google.com");
                    }
                    catch (Exception)
                    {
                        if (silent || VDialog.Show("It seems that your network can't resolve host names. PeaRoxy, PeaRoxyWeb, HTTPS and SOCKS all support requests with host name instead of IP too;\nbut in case that you want to have complete compatibility for all applications, it is recomended to enable DNS Resolver and edit your network adaptor's settings to use 'localhost' as resolving server.\n\nNow, do want us to enable DNS Resolver for you temporary?", "DNS Resolver", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                            Listener.DNSResolver.DNSResolver_Supported = Listener.DNSResolver.DNSResolver_UDPSupported = true;
                    }
                }
                Listener.TestServerAsyc((ClientLibrary.Proxy_Controller.OperationWithErrorMessageFinished)delegate(bool suc, string mes)
                {
                    this.Dispatcher.Invoke((SimpleVoid_Delegate)delegate()
                    {
                        try
                        {
                            if (suc)
                            {
                                if (Listener.SmartPear.Forwarder_HTTP_Enable || Listener.SmartPear.Forwarder_HTTPS_Enable || Listener.SmartPear.Forwarder_SOCKS_Enable)
                                {
                                    try
                                    {
                                        Listener.TestServer(new Server_NoServer());
                                    }
                                    catch (Exception ex)
                                    {
                                        if (silent || VDialog.Show(this, "It seems you have SmartPear enabled but no direct internet connection, Do you want us to disable SmartPear temporary?! \r\n\r\n" + ex.Message, "Smart Pear", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                                        {
                                            Listener.SmartPear.Forwarder_HTTP_Enable = false;
                                            Listener.SmartPear.Forwarder_HTTPS_Enable = false;
                                            Listener.SmartPear.Forwarder_SOCKS_Enable = false;
                                        }
                                    }
                                }

                                Listener.Start();
                                // Grabber Pro-Listen Settings
                                switch (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Grabber)
                                {
                                    case 0:
                                        reConfig(true);
                                        break;
                                    case 1:
                                        List<System.Net.IPAddress> hostIps = new List<System.Net.IPAddress>();
                                        if (CommonLibrary.Common.IsIPAddress(ServerAddress))
                                            hostIps.Add(System.Net.IPAddress.Parse(ServerAddress));
                                        else
                                            hostIps.AddRange(System.Net.Dns.GetHostEntry(ServerAddress).AddressList);
                                        tapTunnel = new Network.TapTunnel();
                                        tapTunnel.AdapterAddressRange = System.Net.IPAddress.Parse(PeaRoxy.Windows.WPFClient.Properties.Settings.Default.TAP_IPRange);
                                        try
                                        {
                                            System.Net.Dns.GetHostAddresses("google.com");
                                            tapTunnel.DNSResolvingAddress = System.Net.IPAddress.Parse(PeaRoxy.Windows.WPFClient.Properties.Settings.Default.DNS_IPAddress);
                                        }
                                        catch (Exception)
                                        {
                                            tapTunnel.DNSResolvingAddress = System.Net.IPAddress.Parse(PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_Address);
                                        }
                                        tapTunnel.ExceptionIPs = new System.Net.IPAddress[] {
                                            System.Net.IPAddress.Parse(PeaRoxy.Windows.WPFClient.Properties.Settings.Default.DNS_IPAddress),
                                            }.Concat(hostIps).ToArray();
                                        tapTunnel.SocksProxyEndPoint = new System.Net.IPEndPoint(
                                            System.Net.IPAddress.Parse(PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_Address),
                                            PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_Port);
                                        tapTunnel.TunnelName = "PeaRoxy Tunnel";
                                        if (!tapTunnel.StartTunnel())
                                        {
                                            tapTunnel.StopTunnel();
                                            if (!silent)
                                                VDialog.Show(this, "Failed to start TAP Adapter. Skipped.", "TAP Driver", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                                        }
                                        break;
                                    case 2:
                                        // Do Nothing
                                        break;
                                    default:
                                        break;
                                }

                                App.Notify.ShowBalloonTip(2000, "PeaRoxy Client", "Connected to the server\r\n" + ser.ToString(), System.Windows.Forms.ToolTipIcon.Info);
                                App.Notify.Text = "PeaRoxy Client - Working";
                                System.IO.Stream st = Application.GetResourceStream(new Uri("pack://application:,,,/PeaRoxy.Windows.WPFClient;component/Images/Icons/Pear_Notify.ico")).Stream;
                                App.Notify.Icon = new System.Drawing.Icon(st);
                                st.Close();
                                st.Dispose();
                                Status = CurrentStatus.Connected;
                                RefreshStatus(true);
                            }
                            else
                            {
                                throw new Exception(mes);
                            }
                        }
                        catch (Exception ex)
                        {
                            VDialog.Show(this, "Error: " + ex.Message + "\r\n" + ex.StackTrace, "Start Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                            if (Listener != null)
                                Listener.Stop();
                            MainPage.IsEnabled = true;
                            RefreshStatus(true);
                        }
                    }, new object[] { });
                });

                return true;
            }
            catch (Exception ex)
            {
                VDialog.Show(this, "Error: " + ex.Message + "\r\n" + ex.StackTrace, "Start Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                if (Listener != null)
                    Listener.Stop();
                MainPage.IsEnabled = true;
                RefreshStatus(true);
            }
            return false;
        }

        private void Smart_ListUpdated(string item, bool https, EventArgs e)
        {
            this.Dispatcher.Invoke((SimpleVoid_Delegate)delegate()
            {
                ((SettingTabs.SmartPear)Options.SmartPear.SettingsPage).AddToSmartList(item, https);
                App.Notify.ShowBalloonTip(10, ((https) ? "PeaRoxy Client - Https/Socks" : "PeaRoxy Client - Http"), "Added to SmartPear list: \r\n" + item, System.Windows.Forms.ToolTipIcon.Info);
            }, new object[] { });
        }

        public void reConfig(bool silent = false)
        {
            if (Listener == null || Listener.Status == Proxy_Controller.ControllerStatus.Stopped || Listener.Status == Proxy_Controller.ControllerStatus.OnlyAutoConfig)
            {
                if (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.AutoConfig_Enable)
                {
                    WindowsProxy.RefreshProxySettings();
                    //ni.ShowBalloonTip(2000, "PeaRoxy Client", "Ask Windows to refresh proxy settings: Done", System.Windows.Forms.ToolTipIcon.Info);
                    // Nothing to Do
                }

                if (WindowsProxy.Windows_DisableProxy())
                    App.Notify.ShowBalloonTip(2000, "PeaRoxy Client", "Ask Windows to ignore PeaRoxy settings: Done", System.Windows.Forms.ToolTipIcon.Info);
                else
                    if (!silent && VDialog.Show(this, "Failed, You need to logoff and relogin to your account and try again or configure your system manually.\r\nDo want us to logoff your user account?!", "Auto Config", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                        Windows.Common.LogOffUser();
            }
            else
            {
                if (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.AutoConfig_Enable)
                {
                    string autoProxyAddress = "http://" + ((PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_Address == "*") ? Environment.MachineName : PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_Address) + ":" + PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_Port.ToString() + "/" + PeaRoxy.Windows.WPFClient.Properties.Settings.Default.AutoConfig_Address;
                    if (!WindowsProxy.Windows_SetActiveProxyAutoConfig(autoProxyAddress))
                    {
                        if (!silent && VDialog.Show(this, "Failed, You need to logoff and relogin to your account and try again or configure your system manually.\r\nDo want us to logoff your user account?!", "Auto Config", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                            Windows.Common.LogOffUser();
                        else
                            App.Notify.ShowBalloonTip(2000, "PeaRoxy Client", "Failed to configure Windows", System.Windows.Forms.ToolTipIcon.Warning);
                        return;
                    }
                }
                bool isDone = false;
                if (!WindowsProxy.Windows_SetActiveProxy(PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_Address,
                    PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Proxy_Port,
                    Listener.IsHTTP_Supported,
                    Listener.IsHTTPS_Supported,
                    Listener.IsSOCKS_Supported))
                    if (!silent && VDialog.Show(this, "Failed, You need to logoff and relogin to your account and try again or configure your system manually.\r\nDo want us to logoff your user account?!", "Auto Config", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                        Windows.Common.LogOffUser();
                    else
                        App.Notify.ShowBalloonTip(2000, "PeaRoxy Client", "Failed to configure Windows", System.Windows.Forms.ToolTipIcon.Warning);
                else
                    isDone = true;
                if (isDone)
                    App.Notify.ShowBalloonTip(2000, "PeaRoxy Client", "Ask Windows to use PeaRoxy to connect to internet: Done", System.Windows.Forms.ToolTipIcon.Info);
            }
            if (WindowsProxy.IsFirefoxNeedReconfig() && !WindowsProxy.ForceFirefoxToUseSystemSettings())
                if (!silent)
                    VDialog.Show(this, "Failed to change firefox settings, Are you running firefox?! Please close it and try again.", "Auto Config", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                else
                    App.Notify.ShowBalloonTip(2000, "PeaRoxy Client", "Failed to configure firefox", System.Windows.Forms.ToolTipIcon.Warning);
        }

        public void FailDisconnected(EventArgs e)
        {
            Listener.AutoDisconnect = false;
            this.Dispatcher.Invoke((SimpleVoid_Delegate)delegate()
            {
                VDialog.Show(this, "Connection to the server interrupted.", "PeaRoxy Client", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
                App.Notify.ShowBalloonTip(1000, "PeaRoxy Client", "Connection to the server interrupted.", System.Windows.Forms.ToolTipIcon.Error);
                Controls_StopClick(null, null);
            }, new object[] { });
        }

        public bool StopServer()
        {
            try
            {
                if (Listener != null)
                {
                    if (Listener.Status != ClientLibrary.Proxy_Controller.ControllerStatus.Stopped)
                    {
                        if (Listener.IsAutoConfigEnable && !PeaRoxy.Windows.WPFClient.Properties.Settings.Default.AutoConfig_KeepRuning)
                            Listener.IsAutoConfigEnable = false;
                        Listener.Stop();
                    }

                    if (Listener.Status == ClientLibrary.Proxy_Controller.ControllerStatus.OnlyAutoConfig || Listener.Status == ClientLibrary.Proxy_Controller.ControllerStatus.Both)
                        Status = CurrentStatus.Sleep;
                    else
                        Status = CurrentStatus.Disconnected;

                    switch (PeaRoxy.Windows.WPFClient.Properties.Settings.Default.Grabber)
                    {
                        case 0:
                            reConfig(true);
                            break;
                        case 1:
                            if (tapTunnel != null)
                                tapTunnel.StopTunnel();
                            break;
                        case 2:
                            // Do Nothing
                            break;
                        default:
                            break;
                    }

                }
                Options.SaveSettings();
                MainPage.SaveSettings();
                ((SettingTabs.SmartPear)Options.SmartPear.SettingsPage).FillSmartList();
                System.IO.Stream st = Application.GetResourceStream(new Uri("pack://application:,,,/PeaRoxy.Windows.WPFClient;component/Images/Icons/Pear_Red_Notify.ico")).Stream;
                App.Notify.Icon = new System.Drawing.Icon(st);
                st.Close();
                st.Dispose();
                App.Notify.Text = "PeaRoxy Client - Stopped";
                return true;
            }
            catch (Exception ex)
            {
                VDialog.Show(this, "Error: " + ex.Message + "\r\n" + ex.StackTrace, "Can't stop server", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return false;
        }

        private long DownSpeed = 0;
        private long UpSpeed = 0;
        private void UpdateStats(object sender, EventArgs e)
        {
            if (Listener != null)
            {
                DownSpeed = (Listener.AvgReceivingSpeed + DownSpeed) / 2;
                UpSpeed = (Listener.AvgSendingSpeed + UpSpeed) / 2;
                downPoints.Add(new chartPoint(DownSpeed / 1024));
                upPoints.Add(new chartPoint(UpSpeed / 1024));
                if (Listener != null && (Listener.Status == ClientLibrary.Proxy_Controller.ControllerStatus.OnlyProxy || Listener.Status == ClientLibrary.Proxy_Controller.ControllerStatus.Both))
                    App.Notify.Text = "PeaRoxy Client\r\nCurrent Transfer Rate: " + CommonLibrary.Common.FormatFileSizeAsString(DownSpeed + UpSpeed) + "/s";
                try
                {
                    if (TaskbarManager.IsPlatformSupported)
                        if (Listener.Status == Proxy_Controller.ControllerStatus.OnlyProxy || Listener.Status == Proxy_Controller.ControllerStatus.Both)
                            TaskbarManager.Instance.SetOverlayIcon(connectedIcon, "Connected");
                        else
                            TaskbarManager.Instance.SetOverlayIcon(null, "Stopped");
                }
                catch (Exception) { }
                if (this.IsVisible && Options.IsEnabled)
                {
                    ((SettingTabs.General)Options.General.SettingsPage).UpdateStats(Listener, DownSpeed, UpSpeed);
                    ((SettingTabs.ActiveConnections)Options.Connections.SettingsPage).UpdateConnections(Listener);
                }
            }
        }

        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            throw new NotImplementedException();
        }
        public object EndInvoke(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
        public object Invoke(Delegate method, object[] args)
        {
            return this.Dispatcher.Invoke(method, args);
        }
        public bool InvokeRequired
        {
            get { return true; }
        }

        public IntPtr Handle
        {
            get
            {
                var interopHelper = new WindowInteropHelper(this);
                return interopHelper.Handle;
            }
        }
    }
}