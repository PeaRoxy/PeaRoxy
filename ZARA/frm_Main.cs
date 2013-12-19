using LukeSw.Windows.Forms;
using PeaRoxy.ClientLibrary;
using PeaRoxy.ClientLibrary.Server_Types;
using PeaRoxy.Windows;
using PeaRoxy.Windows.Network.TAP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ZARA
{
    public partial class frm_Main : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        

        public enum CurrentStatus
        {
            Disconnected,
            Connected,
            Sleep,
        }

        private WinFormAnimation.Animator2D Ani = new WinFormAnimation.Animator2D(WinFormAnimation.Timer.eFPSLimit.FPS60);
        private Proxy_Controller Listener;
        private TapTunnel tapTunnel;
        public CurrentStatus Status { get; set; }
        private delegate void SimpleVoid_Delegate();
        
        public frm_Main()
        {
            InitializeComponent();
            this.MaximumSize = this.MinimumSize = this.Size = new Size(496, 316);
            pnl_main.Location = new Point(-200, 0);
        }

        private void frm_Main_Load(object sender, EventArgs e)
        {
            if (ZARA.Properties.Settings.Default.FirstRun)
            {
                if (ZARA.Properties.Settings.Default.UpdateSettings)
                    ZARA.Properties.Settings.Default.Upgrade();
                ZARA.Properties.Settings.Default.FirstRun = false;
                ZARA.Properties.Settings.Default.Save();
            }

            WindowsModule platform = new WindowsModule();
            platform.RegisterPlatform();
            ReloadSettings();
            try
            {
                Program.Notify.ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[] {
                    new System.Windows.Forms.MenuItem("Show / Hide", (EventHandler)delegate{
                        Program.Notify.GetType().GetMethod("OnDoubleClick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(Program.Notify, new object[] { null });
                    }) {DefaultItem = true},
                    new System.Windows.Forms.MenuItem("-", (EventHandler)null),
                    new System.Windows.Forms.MenuItem("Exit", (EventHandler)delegate{
                        StopServer();
                        Application.Exit();
                    })
                });
                Program.Notify.Icon = ZARA.Properties.Resources.Icon;
                Program.Notify.Text = "Z A Я A - Stopped";
                Program.Notify.DoubleClick  += btn_minimize_Click;
            }
            catch (Exception) { }
        }

        private void MovePageTo(int pos)
        {
            Ani.Stop();
            if (pos != pnl_main.Location.X)
            {
                Ani.SetPaths(new WinFormAnimation.Path2D(pnl_main.Location, new Point(pos, pnl_main.Location.Y), 600, 300, WinFormAnimation.Functions.CubicEaseOut));
                Ani.Play(pnl_main, x => x.Location);
            }
        }

        private void Drag_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void btn_minimize_Click(object sender, EventArgs e)
        {
            if (this.Visible)
                this.Hide();
            else
                this.Show();
            Program.Notify.Visible = !this.Visible;
        }

        public bool StartServer()
        {
            try
            {
                if (string.IsNullOrEmpty(ZARA.Properties.Settings.Default.UserAndPassword_User))
                    throw new ArgumentException("Invalid value.", "UserName");

                if (Listener != null)
                    StopServer();
                else
                {
                    Listener = new Proxy_Controller(null, null, 0);
                    Listener.FailDisconnected += FailDisconnected;
                }
                this.Enabled = false;

                Listener.IP = System.Net.IPAddress.Loopback;
                Listener.Port = 0;
                Listener.IsAutoConfigEnable = false;
                Listener.IsHTTP_Supported = false;
                Listener.IsHTTPS_Supported = false;
                Listener.IsSOCKS_Supported = true;
                Listener.SendPacketSize = (int)ZARA.Properties.Settings.Default.Connection_SendPacketSize;
                Listener.ReceivePacketSize = (int)ZARA.Properties.Settings.Default.Connection_RecPacketSize;
                Listener.AutoDisconnect = ZARA.Properties.Settings.Default.Connection_StopOnInterrupt;

                Listener.ErrorRenderer.HTTPErrorRendering = false;
                Listener.ErrorRenderer.DirectErrorRendering_Port80 = false;
                Listener.ErrorRenderer.DirectErrorRendering_Port443 = false;

                Listener.DNSResolver.DNSResolver_Supported = Listener.DNSResolver.DNSResolver_UDPSupported = true;
                Listener.DNSResolver.DNSResolver_ServerIP = System.Net.IPAddress.Parse(ZARA.Properties.Settings.Default.DNS_IPAddress);

                Listener.SmartPear.Detector_Direct_Port80AsHTTP = false;
                Listener.SmartPear.Forwarder_HTTP_Enable = false;
                Listener.SmartPear.Detector_HTTP_Enable = false;
                Listener.SmartPear.Detector_HTTP_Pattern = "";
                Listener.SmartPear.Detector_DNSGrabber_Pattern = "";
                Listener.SmartPear.Detector_DNSGrabber_Enable = false;
                Listener.SmartPear.Forwarder_HTTPS_Enable = false;
                Listener.SmartPear.Detector_Timeout_Enable = false;
                Listener.SmartPear.Detector_Timeout = 0;
                Listener.SmartPear.Forwarder_Direct_Port80AsHTTP = false;
                Listener.SmartPear.Forwarder_SOCKS_Enable = false;
                Listener.SmartPear.Forwarder_HTTP_List.Clear();
                Listener.SmartPear.Forwarder_Direct_List.Clear();

                string ServerAddress = ZARA.Properties.Settings.Default.ServerAddress;
                ServerType ser = new Server_PeaRoxy(ZARA.Properties.Settings.Default.ServerAddress, ZARA.Properties.Settings.Default.ServerPort, "", ZARA.Properties.Settings.Default.UserAndPassword_User, ZARA.Properties.Settings.Default.UserAndPassword_Pass, PeaRoxy.CommonLibrary.Common.Encryption_Type.SimpleXOR, PeaRoxy.CommonLibrary.Common.Compression_Type.None);

                ser.NoDataTimeout = ZARA.Properties.Settings.Default.Connection_NoDataTimeout;
                Listener.ActiveServer = ser;

                Listener.TestServerAsyc((PeaRoxy.ClientLibrary.Proxy_Controller.OperationWithErrorMessageFinished)delegate(bool suc, string mes)
                {
                    this.Invoke((SimpleVoid_Delegate)delegate()
                    {
                        try
                        {
                            if (suc)
                            {
                                Listener.Start();

                                List<System.Net.IPAddress> hostIps = new List<System.Net.IPAddress>();
                                if (PeaRoxy.CommonLibrary.Common.IsIPAddress(ServerAddress))
                                    hostIps.Add(System.Net.IPAddress.Parse(ServerAddress));
                                else
                                    hostIps.AddRange(System.Net.Dns.GetHostEntry(ServerAddress).AddressList);
                                tapTunnel = new PeaRoxy.Windows.Network.TAP.TapTunnel();
                                tapTunnel.AdapterAddressRange = System.Net.IPAddress.Parse(ZARA.Properties.Settings.Default.TAP_IPRange);
                                try
                                {
                                    System.Net.Dns.GetHostAddresses("google.com");
                                    tapTunnel.DNSResolvingAddress = System.Net.IPAddress.Parse(ZARA.Properties.Settings.Default.DNS_IPAddress);
                                    tapTunnel.DNSResolvingAddress2 = System.Net.IPAddress.Loopback;
                                }
                                catch (Exception)
                                {
                                    tapTunnel.DNSResolvingAddress = System.Net.IPAddress.Loopback;
                                }
                                tapTunnel.ExceptionIPs = new System.Net.IPAddress[] {
                                    //System.Net.IPAddress.Parse(ZARA.Properties.Settings.Default.DNS_IPAddress),
                                    }.Concat(hostIps).ToArray();
                                tapTunnel.SocksProxyEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, Listener.Port);
                                tapTunnel.TunnelName = "ZARA Tunnel";
                                if (!tapTunnel.StartTunnel())
                                {
                                    tapTunnel.StopTunnel();
                                    throw new Exception("Failed to start TAP Adapter.");
                                }

                                Program.Notify.ShowBalloonTip(2000, "Z A Я A", "Connected to the server\r\n" + ser.ToString(), System.Windows.Forms.ToolTipIcon.Info);
                                Program.Notify.Text = "Z A Я A - Working";
                                Status = CurrentStatus.Connected;
                                RefreshStatus();
                            }
                            else
                            {
                                throw new Exception(mes);
                            }
                        }
                        catch (Exception ex)
                        {
                            ShowDialog("Error: " + ex.Message + ex.StackTrace, "Start Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                            if (Listener != null)
                                Listener.Stop();
                            RefreshStatus();
                        }
                    }, new object[] { });
                });

                return true;
            }
            catch (Exception ex)
            {
                ShowDialog("Error: " + ex.Message, "Start Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                if (Listener != null)
                    Listener.Stop();
                RefreshStatus();
            }
            return false;
        }

        public bool StopServer()
        {
            try
            {
                this.Enabled = false;
                if (Listener != null)
                {
                    if (Listener.Status != PeaRoxy.ClientLibrary.Proxy_Controller.ControllerStatus.Stopped)
                        Listener.Stop();

                    Status = CurrentStatus.Disconnected;

                    if (tapTunnel != null)
                        tapTunnel.StopTunnel();
                }
                SaveSettings();
                Program.Notify.Text = "Z A Я A - Stopped";
                RefreshStatus();
                return true;
            }
            catch (Exception ex)
            {
                ShowDialog("Error: " + ex.Message, "Can't stop server", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                RefreshStatus();
            }
            return false;
        }

        private void SaveSettings()
        {
            ZARA.Properties.Settings.Default.ServerAddress = txt_server.Text;
            ZARA.Properties.Settings.Default.UserAndPassword_User = txt_username.Text;
            ZARA.Properties.Settings.Default.UserAndPassword_Pass = txt_password.Text;
            ZARA.Properties.Settings.Default.Save();
        }

        private void ReloadSettings()
        {
            if (ZARA.Properties.Settings.Default.ServerAddress != string.Empty)
            {
                txt_server.Text = ZARA.Properties.Settings.Default.ServerAddress;
                if (!ZARA.Properties.Settings.Default.ShowHost)
                    pnl_host.Visible = false;
            }
            if (ZARA.Properties.Settings.Default.UserAndPassword_User != string.Empty)
                txt_username.Text = ZARA.Properties.Settings.Default.UserAndPassword_User;
            if (ZARA.Properties.Settings.Default.UserAndPassword_Pass != string.Empty)
            {
                txt_password.Text = ZARA.Properties.Settings.Default.UserAndPassword_Pass;
                txt_password.PasswordChar = 'x';
            }
        }

        private void RefreshStatus()
        {
            switch (this.Status)
            {
                case CurrentStatus.Connected:
                    btn_login.Enabled = false;
                    txt_password.Enabled = false;
                    txt_server.Enabled = false;
                    txt_username.Enabled = false;
                    btn_disconnect.Enabled = true;
                    lbl_status.Text = "CONNECTED";
                    pb_status.Image = ZARA.Properties.Resources.connected;
                    MovePageTo(0);
                    break;
                case CurrentStatus.Disconnected:
                default:
                    btn_login.Enabled = true;
                    txt_password.Enabled = true;
                    txt_server.Enabled = true;
                    txt_username.Enabled = true;
                    btn_disconnect.Enabled = false;
                    lbl_status.Text = "DISCONNECTED";
                    pb_status.Image = ZARA.Properties.Resources.disconnected;
                    MovePageTo(-200);
                    break;
            }
            this.Enabled = true;
        }

        public void FailDisconnected(EventArgs e)
        {
            Listener.AutoDisconnect = false;
            this.Invoke((SimpleVoid_Delegate)delegate()
            {
                ShowDialog("Connection to the server interrupted.", "Z A Я A", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
                Program.Notify.ShowBalloonTip(1000, "Z A Я A", "Connection to the server interrupted.", System.Windows.Forms.ToolTipIcon.Error);
                StopServer();
                RefreshStatus();
            }, new object[] { });
        }

        private void ShowDialog(string message, string title, MessageBoxButtons messageBoxButtons, MessageBoxIcon messageBoxIcon)
        {
            VDialog.Show(this, message, title, messageBoxButtons, messageBoxIcon);
        }

        private void btn_disconnect_Click(object sender, EventArgs e)
        {
            SaveSettings();
            if (this.Status == CurrentStatus.Connected)
                StopServer();
        }

        private void btn_login_Click(object sender, EventArgs e)
        {
            SaveSettings();
            if (this.Status != CurrentStatus.Connected)
                StartServer();
        }

        private void txt_server_Leave(object sender, EventArgs e)
        {
            if (txt_server.Text == "")
                return;
            try
            {
                txt_server.Text = txt_server.Text.Replace("\\", "/");
                if (txt_server.Text.IndexOf("://") == -1)
                    txt_server.Text = "http://" + txt_server.Text;
                Uri uri = new Uri(txt_server.Text);
                txt_server.Text = uri.Host;
            }
            catch (Exception ex)
            {
                VDialog.Show("Value is not acceptable.\r\n" + ex.Message, "Data Validation", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                txt_server.Text = "";
                new System.Threading.Thread(delegate()
                {
                    System.Threading.Thread.Sleep((int)(10));
                    this.Invoke((SimpleVoid_Delegate)delegate()
                    {
                        txt_server.Focus();
                    }, new object[] { });
                }) { IsBackground = true }.Start();
            }

            SaveSettings();
        }

        private void txt_Leave(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void frm_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopServer();
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            StopServer();
            Application.Exit();
        }

        private long DownSpeed = 0;
        private long UpSpeed = 0;
        private void statTimer_Tick(object sender, EventArgs e)
        {
            if (Listener != null)
            {
                DownSpeed = (Listener.AvgReceivingSpeed + DownSpeed) / 2;
                UpSpeed = (Listener.AvgSendingSpeed + UpSpeed) / 2;
                if (Listener != null && (Listener.Status == PeaRoxy.ClientLibrary.Proxy_Controller.ControllerStatus.OnlyProxy || Listener.Status == PeaRoxy.ClientLibrary.Proxy_Controller.ControllerStatus.Both))
                    Program.Notify.Text = "Z A Я A\r\nCurrent Transfer Rate: " + PeaRoxy.CommonLibrary.Common.FormatFileSizeAsString(DownSpeed + UpSpeed) + "/s";

                if (this.Visible)
                {
                    lbl_stat_acceptingthreads.Text = Listener.WaitingAcceptionConnections.ToString();
                    lbl_stat_activeconnections.Text = Listener.RoutingConnections.ToString();
                    SplitBySpace(PeaRoxy.CommonLibrary.Common.FormatFileSizeAsString(Listener.BytesReceived), lbl_stat_downloaded, lbl_stat_downloaded_v);
                    SplitBySpace(PeaRoxy.CommonLibrary.Common.FormatFileSizeAsString(Listener.BytesSent), lbl_stat_uploaded, lbl_stat_uploaded_v);
                    SplitBySpace(PeaRoxy.CommonLibrary.Common.FormatFileSizeAsString(DownSpeed), cpb_stat_downloadrate);
                    SplitBySpace(PeaRoxy.CommonLibrary.Common.FormatFileSizeAsString(UpSpeed), cpb_stat_uploadrate);
                }
            }
        }

        private void SplitBySpace(string str, Label a1, Label a2)
        {
            if (str.IndexOf(" ") > -1)
            {
                a1.Text = str.Split(' ')[0];
                a2.Text = str.Split(' ')[1] + "ps";
            }
            return;
        }

        private void SplitBySpace(string str, CircularProgressBar.CircularProgressBar a1)
        {
            if (str.IndexOf(" ") > -1)
            {
                if (str.Split(' ')[0].IndexOf('.') > -1)
                {
                    a1.Caption = str.Split(' ')[0].Split('.')[0];
                    a1.SubText = "." + str.Split(' ')[0].Split('.')[1];
                }else
                    a1.Caption = str.Split(' ')[0];
                a1.SupText = str.Split(' ')[1];
            }
            return;
        }

        private void txt_password_Enter(object sender, EventArgs e)
        {
            if (txt_password.PasswordChar == '\0')
            {
                txt_password.Text = "";
                txt_password.PasswordChar = 'x';
            }
        }
    }
}
