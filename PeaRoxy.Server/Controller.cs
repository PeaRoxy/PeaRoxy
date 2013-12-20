// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Controller.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Server
{
    #region

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    using PeaRoxy.CommonLibrary;

    #endregion

    /// <summary>
    /// The controller.
    /// </summary>
    public class Controller : IDisposable
    {
        #region Fields

        /// <summary>
        /// The accepting cycle last time.
        /// </summary>
        private int acceptingCycleLastTime = 1;

        /// <summary>
        /// The accepting wait.
        /// </summary>
        private readonly int acceptingWait = 1;

        /// <summary>
        /// The accepting worker.
        /// </summary>
        private readonly BackgroundWorker acceptingWorker;

        /// <summary>
        /// The connected clients.
        /// </summary>
        private readonly List<PeaRoxyClient> connectedClients = new List<PeaRoxyClient>();

        /// <summary>
        /// The listening server.
        /// </summary>
        private Socket listeningServer;

        /// <summary>
        /// The routing clients.
        /// </summary>
        private readonly List<PeaRoxyClient> routingClients = new List<PeaRoxyClient>();

        /// <summary>
        /// The routing cycle last time.
        /// </summary>
        private int routingCycleLastTime = 1;

        /// <summary>
        /// The routing wait.
        /// </summary>
        private readonly int routingWait = 1;

        /// <summary>
        /// The routing worker.
        /// </summary>
        private readonly BackgroundWorker routingWorker;

        /// <summary>
        /// The accepting cycle.
        /// </summary>
        private int acceptingClock;

        /// <summary>
        /// The routing cycle.
        /// </summary>
        private int routingClock;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Controller"/> class.
        /// </summary>
        public Controller()
        {
            this.Domain = !ConfigReader.GetSettings().ContainsKey("PeaRoxyDomain".ToLower()) ? string.Empty : ConfigReader.GetSettings()["PeaRoxyDomain".ToLower()];

            IPAddress ipz;
            if (!ConfigReader.GetSettings().ContainsKey("HTTPForwardingIP".ToLower())
                || !IPAddress.TryParse(ConfigReader.GetSettings()["HTTPForwardingIP".ToLower()], out ipz))
            {
                this.HttpForwardingIp = IPAddress.Any;
            }
            else
            {
                this.HttpForwardingIp = ipz;
            }

            ushort fp;
            if (!ConfigReader.GetSettings().ContainsKey("HTTPForwardingPort".ToLower())
                || !ushort.TryParse(ConfigReader.GetSettings()["HTTPForwardingPort".ToLower()], out fp))
            {
                this.HttpForwardingPort = 0;
            }
            else
            {
                this.HttpForwardingPort = fp;
            }

            ushort p;
            if (!ConfigReader.GetSettings().ContainsKey("ServerPort".ToLower())
                || !ushort.TryParse(ConfigReader.GetSettings()["ServerPort".ToLower()], out p))
            {
                // Read Listening port from config
                this.Port = 1080;
            }
            else
            {
                this.Port = p;
            }

            IPAddress ip;
            if (!ConfigReader.GetSettings().ContainsKey("ServerIP".ToLower())
                || !IPAddress.TryParse(ConfigReader.GetSettings()["ServerIP".ToLower()], out ip))
            {
                // Read Listening IP from config
                this.Ip = IPAddress.Any;
            }
            else
            {
                this.Ip = ip;
            }

            int errorLog;
            if (!ConfigReader.GetSettings().ContainsKey("LogErrors".ToLower())
                || !int.TryParse(ConfigReader.GetSettings()["LogErrors".ToLower()], out errorLog))
            {
                errorLog = 0;
            }

            int usageLog;
            if (!ConfigReader.GetSettings().ContainsKey("LogUsersUsage".ToLower())
                || !int.TryParse(ConfigReader.GetSettings()["LogUsersUsage".ToLower()], out usageLog))
            {
                usageLog = 0;
            }

            if (!ConfigReader.GetSettings().ContainsKey("MaxAcceptingClock".ToLower())
                || !int.TryParse(ConfigReader.GetSettings()["MaxAcceptingClock".ToLower()], out this.acceptingWait))
            {
                this.acceptingWait = 10;
            }
            else
            {
                this.acceptingWait = Math.Min(Math.Max(1000 / this.acceptingWait, 0), 1000);
            }

            if (!ConfigReader.GetSettings().ContainsKey("MaxRoutingClock".ToLower())
                || !int.TryParse(ConfigReader.GetSettings()["MaxRoutingClock".ToLower()], out this.routingWait))
            {
                this.routingWait = 1;
            }
            else
            {
                this.routingWait = Math.Min(Math.Max(1000 / this.routingWait, 0), 1000);
            }

            string usageLogAddress = !ConfigReader.GetSettings().ContainsKey("LogUsersUsageAddress".ToLower()) ? "." : ConfigReader.GetSettings()["LogUsersUsageAddress".ToLower()];

            Screen.StartScreen(errorLog == 1, usageLog == 1, usageLogAddress);

            bool pingMasterServer;
            if (!ConfigReader.GetSettings().ContainsKey("PingMasterServer".ToLower())
                || !bool.TryParse(ConfigReader.GetSettings()["PingMasterServer".ToLower()], out pingMasterServer))
            {
                pingMasterServer = false;
            }

            try
            {
                if (pingMasterServer)
                {
                    TcpClient pingTcp = new TcpClient();
                    pingTcp.Connect("pearoxy.com", 80);
                    pingTcp.BeginConnect(
                        "pearoxy.com", 
                        80, 
                        delegate(IAsyncResult ar)
                            {
                                try
                                {
                                    pingTcp.EndConnect(ar);
                                    NetworkStream pingStream = pingTcp.GetStream();
                                    byte configSelectedAuth;
                                    if (!ConfigReader.GetSettings().ContainsKey("AuthMethod".ToLower())
                                        || !byte.TryParse(
                                            ConfigReader.GetSettings()["AuthMethod".ToLower()], 
                                            out configSelectedAuth))
                                    {
                                        // Read config about how to auth user 
                                        configSelectedAuth = 255;
                                    }

                                    string rn = "\r\n";
                                    string pingRequest = "GET /ping.php?do=register&address=" + this.Ip
                                                         + "&port=" + this.Port + "&authmode="
                                                         + configSelectedAuth + " HTTP/1.1" + rn
                                                         + "Host: www.pearoxy.com" + rn + rn;
                                    byte[] pingRequestBytes = Encoding.ASCII.GetBytes(pingRequest);
                                    pingStream.BeginWrite(
                                        pingRequestBytes, 
                                        0, 
                                        pingRequestBytes.Length, 
                                        delegate(IAsyncResult ar2)
                                            {
                                                try
                                                {
                                                    pingStream.EndWrite(ar2);
                                                    pingStream.Close();
                                                    pingTcp.Close();
                                                }
                                                catch (Exception)
                                                {
                                                }
                                            }, 
                                        null);
                                }
                                catch (Exception)
                                {
                                }
                            }, 
                        null);
                }
            }
            catch (Exception)
            {
            }

            this.acceptingWorker = new BackgroundWorker { WorkerSupportsCancellation = true }; // Init a thread for listening for incoming requests
            this.acceptingWorker.DoWork += this.AcceptingWorkerDoWork; // Add function to run async
            this.routingWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
            this.routingWorker.DoWork += this.RoutingWorkerDoWork;
        }

        #endregion

        #region Enums

        /// <summary>
        /// The controller status.
        /// </summary>
        public enum ControllerStatus
        {
            /// <summary>
            /// The stopped.
            /// </summary>
            Stopped, 

            /// <summary>
            /// The working.
            /// </summary>
            Working, 
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the accepting cycle.
        /// </summary>
        public int AcceptingCycle
        {
            get
            {
                int ret = (this.acceptingClock * 1000) / (Environment.TickCount - this.acceptingCycleLastTime);
                this.acceptingCycleLastTime = Environment.TickCount;
                this.acceptingClock = 0;
                return ret;
            }
        }

        /// <summary>
        /// Gets the http forwarding ip.
        /// </summary>
        public IPAddress HttpForwardingIp { get; private set; }

        /// <summary>
        /// Gets the http forwarding port.
        /// </summary>
        public ushort HttpForwardingPort { get; private set; }

        /// <summary>
        /// Gets the ip.
        /// </summary>
        public IPAddress Ip { get; private set; }

        /// <summary>
        /// Gets the pearoxy domain.
        /// </summary>
        public string Domain { get; private set; }

        /// <summary>
        /// Gets the port.
        /// </summary>
        public ushort Port { get; private set; }

        /// <summary>
        /// Gets the routing cycle.
        /// </summary>
        public int RoutingCycle
        {
            get
            {
                int ret = (this.routingClock * 1000) / (Environment.TickCount - this.routingCycleLastTime);
                this.routingCycleLastTime = Environment.TickCount;
                this.routingClock = 0;
                return ret;
            }
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ControllerStatus Status { get; private set; }

        /// <summary>
        /// Gets the waiting acception connections.
        /// </summary>
        public int WaitingAcceptionConnections
        {
            get
            {
                return Math.Max(this.connectedClients.Count - this.routingClients.Count, 0);
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            if (this.acceptingWorker != null)
            {
                this.acceptingWorker.Dispose();
            }

            if (this.listeningServer != null)
            {
                this.listeningServer.Dispose();
            }

            if (this.routingWorker != null)
            {
                this.routingWorker.Dispose();
            }
        }

        /// <summary>
        /// The start.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Start()
        {
            if (!this.acceptingWorker.IsBusy)
            {
                // If we are not working before
                this.listeningServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipLocal = new IPEndPoint(this.Ip, this.Port);
                this.listeningServer.Bind(ipLocal);
                this.listeningServer.Listen(256);

                this.acceptingWorker.RunWorkerAsync(); // Start client acceptation thread
                this.routingWorker.RunWorkerAsync();
                this.Status = ControllerStatus.Working;
                return true; // Every thing is good
            }

            return false; // We are already running
        }

        /// <summary>
        /// The stop.
        /// </summary>
        public void Stop()
        {
            this.Status = ControllerStatus.Stopped;
            if (this.acceptingWorker.IsBusy)
            {
                // If we are working
                this.acceptingWorker.CancelAsync(); // Sending cancel to acceptation thread
                this.routingWorker.CancelAsync();
            }

            PeaRoxyClient[] cls;
            lock (this.connectedClients)
            {
                cls = new PeaRoxyClient[this.connectedClients.Count];
                this.connectedClients.CopyTo(cls);
                this.connectedClients.Clear();
            }

            foreach (PeaRoxyClient client in cls)
            {
                client.Close();
            }

            lock (this.routingClients) this.routingClients.Clear();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The client disconnected callback.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        internal void Dissconnected(PeaRoxyClient client)
        {
            lock (this.routingClients)
                if (this.routingClients.Contains(client))
                {
                    this.routingClients.Remove(client);
                }

            lock (this.connectedClients)
                if (this.connectedClients.Contains(client))
                {
                    this.connectedClients.Remove(client);
                }
        }

        /// <summary>
        /// The client routing ready callback.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        internal void MoveToQ(PeaRoxyClient client)
        {
            lock (this.routingClients) this.routingClients.Add(client);
        }

        /// <summary>
        /// The accepting worker_ do work.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        private void AcceptingWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            int ti; // Temporary variable
            if (
                !(ConfigReader.GetSettings().ContainsKey("NoDataConnectionTimeOut".ToLower())
                  && int.TryParse(ConfigReader.GetSettings()["NoDataConnectionTimeOut".ToLower()], out ti)))
            {
                // Read settings about timeout
                ti = 600;
            }

            int sp; // Temporary variable
            if (
                !(ConfigReader.GetSettings().ContainsKey("SendPacketSize".ToLower())
                  && int.TryParse(ConfigReader.GetSettings()["SendPacketSize".ToLower()], out sp)))
            {
                // Read Settings about buffer size of connections
                sp = 10 * 1024;
            }

            int rp; // Temporary variable
            if (
                !(ConfigReader.GetSettings().ContainsKey("ReceivePacketSize".ToLower())
                  && int.TryParse(ConfigReader.GetSettings()["ReceivePacketSize".ToLower()], out rp)))
            {
                // Read Settings about buffer size of connections
                rp = 10 * 1024;
            }

            byte enc; // Temporary variable
            if (
                !(ConfigReader.GetSettings().ContainsKey("EncryptionType".ToLower())
                  && byte.TryParse(ConfigReader.GetSettings()["EncryptionType".ToLower()], out enc)))
            {
                // Try to set selected encryption type in settings
                enc = (byte)Common.EncryptionType.None;
            }

            int clientSupportedEncryptionType;
            if (!ConfigReader.GetSettings().ContainsKey("SupportedEncryptionTypes".ToLower())
                || !int.TryParse(
                    ConfigReader.GetSettings()["SupportedEncryptionTypes".ToLower()], 
                    out clientSupportedEncryptionType))
            {
                // If we don't have any setting about supported types of encryption set it to def, -1 mean any type
                clientSupportedEncryptionType = -1;
            }

            byte com; // Temporary variable
            if (
                !(ConfigReader.GetSettings().ContainsKey("CompressionType".ToLower())
                  && byte.TryParse(ConfigReader.GetSettings()["CompressionType".ToLower()], out com)))
            {
                // Try to set selected compression type in settings
                com = (byte)Common.CompressionType.None;
            }

            int clientSupportedCompressionType;
            if (!ConfigReader.GetSettings().ContainsKey("SupportedCompressionTypes".ToLower())
                || !int.TryParse(
                    ConfigReader.GetSettings()["SupportedCompressionTypes".ToLower()], 
                    out clientSupportedCompressionType))
            {
                // If we don't have any setting about supported types of compression set it to def, -1 mean any type
                clientSupportedCompressionType = -1;
            }

            byte configSelectedAuth;
            if (!ConfigReader.GetSettings().ContainsKey("AuthMethod".ToLower())
                || !byte.TryParse(ConfigReader.GetSettings()["AuthMethod".ToLower()], out configSelectedAuth))
            {
                // Read config about how to auth user 
                throw new Exception("Bad AuthMethod in config file.");
            }

            while (!this.acceptingWorker.CancellationPending)
            {
                try
                {
                    this.acceptingClock++;
                    if (this.listeningServer.Poll(0, SelectMode.SelectRead))
                    {
                        lock (this.connectedClients)
                            this.connectedClients.Add(
                                new PeaRoxyClient(
                                    this.listeningServer.Accept(), 
                                    this, 
                                    (Common.EncryptionType)enc, 
                                    (Common.CompressionType)com, 
                                    rp, 
                                    sp, 
                                    configSelectedAuth, 
                                    ti, 
                                    clientSupportedEncryptionType, 
                                    clientSupportedCompressionType));
                                
                                // Create a client and send TCPClient to it, Let add this client to list too, So we can close it when needed
                    }

                    if (this.connectedClients.Count > 0)
                    {
                        PeaRoxyClient[] st;
                        lock (this.connectedClients) st = this.connectedClients.ToArray();
                        foreach (PeaRoxyClient client in st.Where(client => client != null && client.CurrentStage != PeaRoxyClient.ClientStage.Routing))
                        {
                            client.Accepting();
                        }
                    }

                    Thread.Sleep(this.acceptingWait);
                }
                catch (Exception ex)
                {
                    Screen.LogMessage("AcceptingWorker: " + ex.Message + "\r\n" + ex.StackTrace);
                }
            }

            this.listeningServer.Close(); // Not bad to check if we have stopped listening server
        }

        /// <summary>
        /// The routing worker process.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void RoutingWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            while (!this.routingWorker.CancellationPending)
            {
                try
                {
                    this.routingClock++;
                    if (this.routingClients.Count > 0)
                    {
                        PeaRoxyClient[] st;
                        lock (this.routingClients) st = this.routingClients.ToArray();
                        foreach (PeaRoxyClient client in st.Where(client => client != null && client.CurrentStage == PeaRoxyClient.ClientStage.Routing))
                        {
                            client.DoRoute();
                        }
                    }

                    Thread.Sleep(this.routingWait);
                }
                catch (Exception ex)
                {
                    Screen.LogMessage("RoutingWorker: " + ex.Message + "\r\n" + ex.StackTrace);
                }
            }

            this.listeningServer.Close();
        }

        #endregion
    }
}