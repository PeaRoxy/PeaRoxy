using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.Runtime.InteropServices;
using PeaRoxy.CommonLibrary;
namespace PeaRoxy.Server
{
    public class Controller : IDisposable
    {
        public enum ControllerStatus
        {
            Stopped,
            Working,
        }
        private int acceptingCycle;
        private int routingCycle;
        private int AcceptingCycleLastTime = 1;
        private int RoutingCycleLastTime = 1;
        private BackgroundWorker AcceptingWorker;
        private BackgroundWorker RoutingWorker;
        private Socket ListeningServer;
        private List<PeaRoxyClient> ConnectedClients = new List<PeaRoxyClient>();
        private List<PeaRoxyClient> RoutingClients = new List<PeaRoxyClient>();
        public ControllerStatus Status { get; private set; }
        public ushort Port { get; private set; }
        public ushort HTTPForwardingPort { get; private set; }
        public string PeaRoxyDomain { get; private set; }
        public IPAddress HTTPForwardingIP { get; private set; }
        public IPAddress IP { get; private set; }
        public int AcceptingCycle
        {
            get
            {
                int ret = (acceptingCycle * 1000) / (Environment.TickCount - AcceptingCycleLastTime);
                AcceptingCycleLastTime = Environment.TickCount;
                acceptingCycle = 0;
                return ret;
            }
            private set
            {
                acceptingCycle = value;
            }
        }

        public int RoutingCycle
        {
            get
            {
                int ret = (routingCycle * 1000) / (Environment.TickCount - RoutingCycleLastTime);
                RoutingCycleLastTime = Environment.TickCount;
                routingCycle = 0;
                return ret;
            }
            private set
            {
                routingCycle = value;
            }
        }

        public int WaitingAcceptionConnections
        {
            get
            {
                return Math.Max(ConnectedClients.Count - RoutingClients.Count, 0);
            }
        }
        public int RoutingConnections
        {
            get
            {
                return RoutingClients.Count;
            }
        }
        public Controller()
        {
            if (!ConfigReader.GetSettings().ContainsKey("PeaRoxyDomain".ToLower()))
                this.PeaRoxyDomain = string.Empty;
            else
                this.PeaRoxyDomain = ConfigReader.GetSettings()["PeaRoxyDomain".ToLower()];

            IPAddress ipz;
            if (!ConfigReader.GetSettings().ContainsKey("HTTPForwardingIP".ToLower()) || !IPAddress.TryParse(ConfigReader.GetSettings()["HTTPForwardingIP".ToLower()], out ipz))
                this.HTTPForwardingIP = IPAddress.Any;
            else
                this.HTTPForwardingIP = ipz;

            ushort fp;
            if (!ConfigReader.GetSettings().ContainsKey("HTTPForwardingPort".ToLower()) || !ushort.TryParse(ConfigReader.GetSettings()["HTTPForwardingPort".ToLower()], out fp))
                this.HTTPForwardingPort = 0;
            else
                this.HTTPForwardingPort = fp;

            ushort p;
            if (!ConfigReader.GetSettings().ContainsKey("ServerPort".ToLower()) || !ushort.TryParse(ConfigReader.GetSettings()["ServerPort".ToLower()], out p)) // Read Listening port from config
                this.Port = 1080;
            else
                this.Port = p;

            IPAddress ip;
            if (!ConfigReader.GetSettings().ContainsKey("ServerIP".ToLower()) || !IPAddress.TryParse(ConfigReader.GetSettings()["ServerIP".ToLower()], out ip)) // Read Listening IP from config
                this.IP = IPAddress.Any;
            else
                this.IP = ip;

             int errorLog;
             if (!ConfigReader.GetSettings().ContainsKey("LogErrors".ToLower()) || !int.TryParse(ConfigReader.GetSettings()["LogErrors".ToLower()], out errorLog))
                errorLog = 0;

            int usageLog;
            if (!ConfigReader.GetSettings().ContainsKey("LogUsersUsage".ToLower()) || !int.TryParse(ConfigReader.GetSettings()["LogUsersUsage".ToLower()], out usageLog))
                usageLog = 0;

            string usageLogAddress;
            if (!ConfigReader.GetSettings().ContainsKey("LogUsersUsageAddress".ToLower()))
                usageLogAddress = ".";
            else
                usageLogAddress = ConfigReader.GetSettings()["LogUsersUsageAddress".ToLower()];

            Screen.StartScreen(errorLog == 1, usageLog == 1, usageLogAddress);

            bool pingMasterServer;
            if (!ConfigReader.GetSettings().ContainsKey("PingMasterServer".ToLower()) || !bool.TryParse(ConfigReader.GetSettings()["PingMasterServer".ToLower()], out pingMasterServer))
                pingMasterServer = false;

            try
            {
                if (pingMasterServer)
                {
                    TcpClient pingTcp = new TcpClient();
                    pingTcp.Connect("pearoxy.com", 80);
                    pingTcp.BeginConnect("pearoxy.com", 80, (AsyncCallback)delegate(IAsyncResult ar)
                    {
                        try
                        {
                            pingTcp.EndConnect(ar);
                            NetworkStream pingStream = pingTcp.GetStream();
                            byte config_SelectedAuth;
                            if (!ConfigReader.GetSettings().ContainsKey("AuthMethod".ToLower()) || !byte.TryParse(ConfigReader.GetSettings()["AuthMethod".ToLower()], out config_SelectedAuth)) // Read config about how to auth user 
                                config_SelectedAuth = 255;
                            string rn = "\r\n";
                            string pingRequest = "GET /ping.php?do=register&address=" + this.IP.ToString() + "&port=" + this.Port.ToString() + "&authmode=" + config_SelectedAuth.ToString() + " HTTP/1.1" + rn +
                                                    "Host: www.pearoxy.com" + rn + rn;
                            byte[] pingRequestBytes = Encoding.ASCII.GetBytes(pingRequest);
                            pingStream.BeginWrite(pingRequestBytes, 0, pingRequestBytes.Length, (AsyncCallback)delegate(IAsyncResult ar2)
                            {
                                try
                                {
                                    pingStream.EndWrite(ar2);
                                    pingStream.Close();
                                    pingTcp.Close();
                                }
                                catch (Exception) { }
                            }, null);
                        }
                        catch (Exception) { }
                    }, null);
                }
            }
            catch (Exception) { }
            this.AcceptingWorker = new BackgroundWorker(); // Init a thread for listening for incoming requests
            this.AcceptingWorker.WorkerSupportsCancellation = true; // We may want to cancel it
            this.AcceptingWorker.DoWork += AcceptingWorker_DoWork; // Add function to run async
            this.RoutingWorker = new BackgroundWorker();
            this.RoutingWorker.WorkerSupportsCancellation = true;
            this.RoutingWorker.DoWork += RoutingWorker_DoWork;
        }

        public bool Start()
        {
            if (!this.AcceptingWorker.IsBusy) // If we are not working before
            {
                this.ListeningServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipLocal = new IPEndPoint(IP, Port);
                this.ListeningServer.Bind(ipLocal);
                this.ListeningServer.Listen(256);

                this.AcceptingWorker.RunWorkerAsync(); // Start client acceptation thread
                this.RoutingWorker.RunWorkerAsync();
                int temporary_executer = this.RoutingCycle + this.AcceptingCycle;
                Status = ControllerStatus.Working;
                return true; // Every thing is good
            }
            return false; // We are already running
        }

        public void Stop()
        {
            Status = ControllerStatus.Stopped;
            if (this.AcceptingWorker.IsBusy) // If we are working
            {
                this.AcceptingWorker.CancelAsync(); // Sending cancel to acceptation thread
                this.RoutingWorker.CancelAsync();
            }

            PeaRoxyClient[] cls;
            lock (ConnectedClients)
            {
                cls = new PeaRoxyClient[ConnectedClients.Count];
                ConnectedClients.CopyTo(cls);
                this.ConnectedClients.Clear();
            }
            foreach (PeaRoxyClient client in cls)
                client.Close();

            lock (RoutingClients)
                this.RoutingClients.Clear();
        }

        private void AcceptingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int _ti; // Temporary variable
            if (!(ConfigReader.GetSettings().ContainsKey("NoDataConnectionTimeOut".ToLower()) && int.TryParse(ConfigReader.GetSettings()["NoDataConnectionTimeOut".ToLower()], out _ti))) // Read settings about timeout
                _ti = 600;

            int _sp; // Temporary variable
            if (!(ConfigReader.GetSettings().ContainsKey("SendPacketSize".ToLower()) && int.TryParse(ConfigReader.GetSettings()["SendPacketSize".ToLower()], out _sp))) // Read Settings about buffer size of connections
                _sp = 10 * 1024;

            int _rp; // Temporary variable
            if (!(ConfigReader.GetSettings().ContainsKey("ReceivePacketSize".ToLower()) && int.TryParse(ConfigReader.GetSettings()["ReceivePacketSize".ToLower()], out _rp))) // Read Settings about buffer size of connections
                _rp = 10 * 1024;

            byte enc; // Temporary variable
            if (!(ConfigReader.GetSettings().ContainsKey("EncryptionType".ToLower()) && byte.TryParse(ConfigReader.GetSettings()["EncryptionType".ToLower()], out enc))) // Try to set selected encryption type in settings
                enc = (byte)Common.Encryption_Type.None;

            int clientSupportedEncryptionType;
            if (!ConfigReader.GetSettings().ContainsKey("SupportedEncryptionTypes".ToLower()) || !int.TryParse(ConfigReader.GetSettings()["SupportedEncryptionTypes".ToLower()], out clientSupportedEncryptionType)) // If we don't have any setting about supported types of encryption set it to def, -1 mean any type
                clientSupportedEncryptionType = -1;

            byte com; // Temporary variable
            if (!(ConfigReader.GetSettings().ContainsKey("CompressionType".ToLower()) && byte.TryParse(ConfigReader.GetSettings()["CompressionType".ToLower()], out com))) // Try to set selected compression type in settings
                com = (byte)Common.Compression_Type.None;

            int clientSupportedCompressionType;
            if (!ConfigReader.GetSettings().ContainsKey("SupportedCompressionTypes".ToLower()) || !int.TryParse(ConfigReader.GetSettings()["SupportedCompressionTypes".ToLower()], out clientSupportedCompressionType)) // If we don't have any setting about supported types of compression set it to def, -1 mean any type
                clientSupportedCompressionType = -1;

            byte config_SelectedAuth;
            if (!ConfigReader.GetSettings().ContainsKey("AuthMethod".ToLower()) || !byte.TryParse(ConfigReader.GetSettings()["AuthMethod".ToLower()], out config_SelectedAuth)) // Read config about how to auth user 
                throw new Exception("Bad AuthMethod in config file.");

            while (!this.AcceptingWorker.CancellationPending)
            {
                try
                {
                    acceptingCycle++;
                    if (this.ListeningServer.Poll(0, SelectMode.SelectRead))
                    {
                        lock (ConnectedClients)
                            this.ConnectedClients.Add(new PeaRoxyClient(this.ListeningServer.Accept(),
                                this,
                                (Common.Encryption_Type)enc,
                                (Common.Compression_Type)com,
                                _rp,
                                _sp,
                                config_SelectedAuth,
                                _ti,
                                clientSupportedEncryptionType,
                                clientSupportedCompressionType
                                )); // Create a client and send TCPClient to it, Let add this client to list too, So we can close it when needed
                    }
                    if (ConnectedClients.Count > 0)
                    {
                        PeaRoxyClient[] st;
                        lock (ConnectedClients)
                            st = ConnectedClients.ToArray();
                        for (int i = 0; i < st.Length; i++)
                            if (st[i] != null && st[i].CurrentStage != PeaRoxyClient.Client_Stage.Routing)
                                st[i].Accepting();
                    }
                    System.Threading.Thread.Sleep(1);
                }
                catch (Exception ex)
                {
                    Screen.LogMessage("AcceptingWorker: " + ex.Message + "\r\n" + ex.StackTrace);
                }
            }
            this.ListeningServer.Close(); // Not bad to check if we have stopped listening server
        }

        private void RoutingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!this.RoutingWorker.CancellationPending)
            {
                try
                {
                    routingCycle++;
                    if (RoutingClients.Count > 0)
                    {
                        PeaRoxyClient[] st;
                        lock (RoutingClients)
                            st = RoutingClients.ToArray();
                        for (int i = 0; i < st.Length; i++)
                            if (st[i] != null && st[i].CurrentStage == PeaRoxyClient.Client_Stage.Routing)
                                st[i].DoRoute();
                    }
                    System.Threading.Thread.Sleep(1);
                }
                catch (Exception ex)
                {
                    Screen.LogMessage("RoutingWorker: " + ex.Message + "\r\n" + ex.StackTrace);
                }
            }
            this.ListeningServer.Close();
        }

        internal void iMoveToQ(PeaRoxyClient cl)
        {
            lock (RoutingClients)
                RoutingClients.Add(cl);
        }

        internal void iDissconnected(PeaRoxyClient client)
        {
            lock (RoutingClients)
                if (RoutingClients.Contains(client))
                    RoutingClients.Remove(client);

            lock (ConnectedClients)
                if (ConnectedClients.Contains(client))
                    ConnectedClients.Remove(client);
        }

        public void Dispose()
        {
            if (this.AcceptingWorker != null)
                this.AcceptingWorker.Dispose();
            if (this.ListeningServer != null)
                this.ListeningServer.Dispose();
            if (this.RoutingWorker != null)
                this.RoutingWorker.Dispose();
        }
    }
}
