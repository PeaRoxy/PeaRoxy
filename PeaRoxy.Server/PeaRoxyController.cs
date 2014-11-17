// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Controller.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Server
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     The Controller Class is responsible for listening for new connections and managing them as well as controlling the
    ///     accepting and routing threads for the server.
    /// </summary>
    public class PeaRoxyController : IDisposable
    {
        /// <summary>
        ///     The Controller statuses
        /// </summary>
        public enum ControllerStatuses
        {
            Stopped,

            Working,
        }

        private readonly int acceptingWait = 1;

        private readonly BackgroundWorker acceptingWorker;

        private readonly List<PeaRoxyClient> connectedClients = new List<PeaRoxyClient>();

        private readonly List<PeaRoxyClient> routingClients = new List<PeaRoxyClient>();

        private readonly int routingWait = 1;

        private readonly BackgroundWorker routingWorker;

        private int acceptingClock;

        private int acceptingCycleLastTime = 1;

        private Socket listeningServer;

        private int routingClock;

        private int routingCycleLastTime = 1;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PeaRoxyController" /> class.
        /// </summary>
        /// <param name="settings">
        ///     The Settings object to read the settings from
        /// </param>
        public PeaRoxyController(Settings settings)
        {
            this.Settings = settings;

            this.Port = (ushort)this.Settings.ServerPort;

            this.Ip = IPAddress.Parse(this.Settings.ServerIp);

            this.acceptingWait = Math.Min(Math.Max(1000 / this.Settings.MaxAcceptingClock, 1), 1000);
            this.routingWait = Math.Min(Math.Max(1000 / this.Settings.MaxRoutingClock, 1), 1000);

            Screen.StartScreen(
                (this.Settings.LogErrors ?? (bool?)true).Value,
                !string.IsNullOrEmpty(this.Settings.UsersUsageLogAddress),
                this.Settings.UsersUsageLogAddress);

            new Task(
                () =>
                    {
                        try
                        {
                            if ((this.Settings.PingMasterServer ?? (bool?)true).Value)
                            {
                                TcpClient pingTcp = new TcpClient();
                                pingTcp.Connect("pearoxy.com", 80);
                                pingTcp.BeginConnect(
                                    "reporting.pearoxy.com",
                                    80,
                                    delegate(IAsyncResult ar)
                                        {
                                            try
                                            {
                                                pingTcp.EndConnect(ar);
                                                NetworkStream pingStream = pingTcp.GetStream();

                                                const string Rn = "\r\n";
                                                string pingRequest =
                                                    string.Format(
                                                        "GET /ping?do=register&address={0}&port={1}&authmode={2} HTTP/1.1",
                                                        this.Ip,
                                                        this.Port,
                                                        this.Settings.AuthMethod) + Rn + "Host: cloud.pearoxy.com"
                                                    + Rn + Rn;
                                                byte[] pingRequestBytes = Encoding.ASCII.GetBytes(pingRequest);
                                                pingStream.BeginWrite (
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
                    }).Start();
            this.acceptingWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
            this.acceptingWorker.DoWork += this.AcceptingWorkerDoWork;
            this.routingWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
            this.routingWorker.DoWork += this.RoutingWorkerDoWork;
        }

        /// <summary>
        ///     Get the Settings object used by controller to read the settings from
        /// </summary>
        public Settings Settings { get; private set; }

        /// <summary>
        ///     Gets the port number we bind to.
        /// </summary>
        public ushort Port { get; private set; }

        /// <summary>
        ///     Gets the IP address we listening to.
        /// </summary>
        public IPAddress Ip { get; private set; }

        /// <summary>
        ///     Gets the accepting clock or in other word the number of times we had ran the accepting process per second.
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
        ///     Gets the routing clock or in other word the number of times we had ran the routing process per second.
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
        ///     Gets the current status of the controller.
        /// </summary>
        public ControllerStatuses Status { get; private set; }

        /// <summary>
        ///     Gets the number of connections in accepting state
        /// </summary>
        public int AcceptingConnections
        {
            get
            {
                return Math.Max(this.connectedClients.Count - this.routingClients.Count, 0);
            }
        }

        /// <summary>
        ///     Gets the number of connections in routing state
        /// </summary>
        public int RoutingConnections
        {
            get
            {
                return this.routingClients.Count;
            }
        }

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
        ///     Listening for new requests and answering them
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" /> value indicating success of the process
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
                this.acceptingWorker.RunWorkerAsync();
                this.routingWorker.RunWorkerAsync();
                this.Status = ControllerStatuses.Working;
                return true; // Every thing is good
            }

            return false; // We are already running
        }

        /// <summary>
        ///     Stop listening to the new requests or suspend the answering process
        /// </summary>
        public void Stop()
        {
            this.Status = ControllerStatuses.Stopped;
            if (this.acceptingWorker.IsBusy)
            {
                // If we are working
                this.acceptingWorker.CancelAsync();
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

        internal void ClientDisconnected(PeaRoxyClient client)
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

        internal void ClientMoveToRouting(PeaRoxyClient client)
        {
            lock (this.routingClients) this.routingClients.Add(client);
        }

        private void AcceptingWorkerDoWork(object sender, DoWorkEventArgs e)
        {
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
                                    this.Settings.EncryptionType,
                                    this.Settings.CompressionType,
                                    this.Settings.ReceivePacketSize,
                                    this.Settings.SendPacketSize,
                                    this.Settings.AuthMethod,
                                    this.Settings.NoDataConnectionTimeOut,
                                    this.Settings.SupportedEncryptionTypes,
                                    this.Settings.SupportedCompressionTypes));
                    }

                    if (this.connectedClients.Count > 0)
                    {
                        PeaRoxyClient[] st;
                        lock (this.connectedClients) st = this.connectedClients.ToArray();
                        foreach (PeaRoxyClient client in
                            st.Where(
                                client => client != null && client.CurrentStage != PeaRoxyClient.RequestStages.Routing))
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

            this.listeningServer.Close();
        }

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
                        foreach (PeaRoxyClient client in
                            st.Where(
                                client => client != null && client.CurrentStage == PeaRoxyClient.RequestStages.Routing))
                        {
                            client.Route();
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
    }
}