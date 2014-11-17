// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProxyController.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   //   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    using PeaRoxy.ClientLibrary.ServerModules;

    /// <summary>
    ///     The proxy controller class which handle all incoming connections and is the starting point of the library
    /// </summary>
    public class ProxyController : IDisposable
    {
        public delegate void AutoDisconnectedDueToFailureNotifyDelegate(EventArgs e);

        public delegate void NewLogNotifyDelegate(string message, EventArgs e);

        public delegate void OperationWithErrorMessageFinishedDelegate(bool success, string error);

        /// <summary>
        ///     The supported mime types for auto configuration.
        /// </summary>
        public enum AutoConfigMimeType
        {
            Netscape = 1,

            Javascript = 2,
        }

        /// <summary>
        ///     The Controller statuses
        /// </summary>
        [Flags]
        public enum ControllerStatus
        {
            None = 0,

            Proxy = 1,

            AutoConfig = 2,
        }

        internal readonly List<ProxyClient> ConnectedClients = new List<ProxyClient>();

        private readonly BackgroundWorker acceptingWorker;

        private readonly List<ServerType> routingClients = new List<ServerType>();

        private readonly BackgroundWorker routingWorker;

        private readonly Dictionary<ThreadStart, int> scheduledTasks = new Dictionary<ThreadStart, int>();

        private int acceptingCycle;

        private int acceptingCycleLastTime = 1;

        private int failAttempts;

        private long lastReceivedBytes;

        private long lastReceivingSpeedCalculationTime;

        private long lastSendingSpeedCalculationTime;

        private long lastSentBytes;

        private Socket listenerSocket;

        private int routingCycle;

        private int routingCycleLastTime = 1;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProxyController" /> class.
        /// </summary>
        /// <param name="activeServer">
        ///     The active server to use for handling the requests.
        /// </param>
        /// <param name="ip">
        ///     The IP address to listen on.
        /// </param>
        /// <param name="port">
        ///     The port number to bind to.
        /// </param>
        public ProxyController(ServerType activeServer, IPAddress ip, ushort port)
        {
            this.lastSendingSpeedCalculationTime = this.lastReceivingSpeedCalculationTime = Environment.TickCount;
            this.lastSentBytes = this.lastReceivedBytes = this.ReceivedBytes = this.SentBytes = 0;
            this.AutoDisconnect = true;
            this.LastConnectedClient = 0;
            this.AutoConfigMime = AutoConfigMimeType.Javascript;
            this.ErrorRenderer = new ErrorRenderer();
            this.SmartPear = new SmartPear();
            this.IsHttpSupported = true;
            this.IsHttpsSupported = true;
            this.IsSocksSupported = true;
            this.DnsResolver = new DnsResolver(this);
            this.Ip = ip;
            this.Port = port;
            this.Status = ControllerStatus.None;
            this.IsAutoConfigEnable = false;
            this.AutoConfigPath = string.Empty;
            this.ActiveServer = activeServer;
            this.acceptingWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
            this.acceptingWorker.DoWork += this.AcceptingWorkerDoWork;
            this.routingWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
            this.routingWorker.DoWork += this.RoutingWorkerDoWork;
            this.ReceivePacketSize = this.SendPacketSize = 8 * 1024;
        }

        /// <summary>
        /// Gets the accepting clock or in other word the number of times we had ran the accepting process per second.
        /// </summary>
        public int AcceptingClock
        {
            get
            {
                int ret = 1000;
                if (Environment.TickCount != this.acceptingCycleLastTime)
                {
                    ret = (this.acceptingCycle * 1000) / (Environment.TickCount - this.acceptingCycleLastTime);
                }

                this.acceptingCycleLastTime = Environment.TickCount;
                this.acceptingCycle = 0;
                return ret;
            }
        }

        /// <summary>
        ///     Gets or sets the active server to handle the requests
        /// </summary>
        public ServerType ActiveServer { get; set; }

        /// <summary>
        ///     Gets or sets the auto configuration's mime type.
        /// </summary>
        public AutoConfigMimeType AutoConfigMime { get; set; }

        /// <summary>
        ///     Gets or sets the auto configuration's script path.
        /// </summary>
        public string AutoConfigPath { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether we should auto disconnect if we failed to restore a connection.
        /// </summary>
        public bool AutoDisconnect { get; set; }

        /// <summary>
        ///     Gets the average receiving speed in bytes/second
        /// </summary>
        public long AverageReceivingSpeed
        {
            get
            {
                long bytesReceived = this.ReceivedBytes - this.lastReceivedBytes;
                this.lastReceivedBytes = this.ReceivedBytes;
                double timeE = (Environment.TickCount - this.lastReceivingSpeedCalculationTime) / (double)1000;
                if (timeE > 0)
                {
                    this.lastReceivingSpeedCalculationTime = Environment.TickCount;
                    return (long)(bytesReceived / timeE);
                }

                return 0;
            }
        }

        /// <summary>
        ///     Gets the average sending speed in bytes/second
        /// </summary>
        public long AverageSendingSpeed
        {
            get
            {
                long bytesSent = this.SentBytes - this.lastSentBytes;
                this.lastSentBytes = this.SentBytes;
                double timeE = (Environment.TickCount - this.lastSendingSpeedCalculationTime) / (double)1000;
                if (!(timeE > 0))
                {
                    return 0;
                }

                this.lastSendingSpeedCalculationTime = Environment.TickCount;
                return (long)(bytesSent / timeE);
            }
        }

        /// <summary>
        ///     Gets the number bytes received till now
        /// </summary>
        public long ReceivedBytes { get; internal set; }

        /// <summary>
        ///     Gets the number bytes sent till now
        /// </summary>
        public long SentBytes { get; internal set; }

        /// <summary>
        ///     Gets the DNS resolver object
        /// </summary>
        public DnsResolver DnsResolver { get; private set; }

        /// <summary>
        ///     Gets the error renderer object
        /// </summary>
        public ErrorRenderer ErrorRenderer { get; private set; }

        /// <summary>
        ///     Gets or sets number of fail attempts till now.
        /// </summary>
        internal int FailAttempts
        {
            get
            {
                return this.failAttempts;
            }

            set
            {
                this.failAttempts = value;
                if (this.AutoDisconnect && this.FailAttempts > 30 && this.Status.HasFlag(ControllerStatus.Proxy))
                {
                    this.FailAttempts = 0;
                    this.Stop();
                    if (this.AutoDisconnectedDueToFailureNotify != null)
                    {
                        this.AutoDisconnectedDueToFailureNotify(new EventArgs());
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets the IP address to listen on.
        /// </summary>
        public IPAddress Ip { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether we have auto configuring script enabled
        /// </summary>
        public bool IsAutoConfigEnable { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether we are listening for HTTPS requests
        /// </summary>
        public bool IsHttpsSupported { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether we are listening for HTTP requests
        /// </summary>
        public bool IsHttpSupported { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether we are listening for SOCKS requests
        /// </summary>
        public bool IsSocksSupported { get; set; }

        /// <summary>
        ///     Gets the last connected client time stamp.
        /// </summary>
        public int LastConnectedClient { get; private set; }

        /// <summary>
        ///     Gets or sets the port to bind to.
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        ///     Gets or sets the receive packet size.
        /// </summary>
        public int ReceivePacketSize { get; set; }

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

        /// <summary>
        ///     Gets the routing cycles in each second.
        /// </summary>
        public int RoutingClock
        {
            get
            {
                int ret = 1000;
                if (Environment.TickCount != this.routingCycleLastTime)
                {
                    ret = (this.routingCycle * 1000) / (Environment.TickCount - this.routingCycleLastTime);
                }

                this.routingCycleLastTime = Environment.TickCount;
                this.routingCycle = 0;
                return ret;
            }
        }

        /// <summary>
        ///     Gets or sets the send packet size.
        /// </summary>
        public int SendPacketSize { get; set; }

        /// <summary>
        ///     Gets the SmartPear object
        /// </summary>
        public SmartPear SmartPear { get; private set; }

        /// <summary>
        ///     Gets the status of the controller
        /// </summary>
        public ControllerStatus Status { get; private set; }

        /// <summary>
        ///     Gets the number of connections in accepting state
        /// </summary>
        public int AcceptingConnections
        {
            get
            {
                return Math.Max(this.ConnectedClients.Count - this.routingClients.Count, 0);
            }
        }

        public void Dispose()
        {
            this.acceptingWorker.Dispose();
            this.listenerSocket.Dispose();
            this.routingWorker.Dispose();
        }

        /// <summary>
        ///     Notify you when new entry added to the log
        /// </summary>
        public static event NewLogNotifyDelegate LogNotify;

        /// <summary>
        ///     Notify you when controller automatically disconnect doe to failure in restoring the connection to the server
        /// </summary>
        public event AutoDisconnectedDueToFailureNotifyDelegate AutoDisconnectedDueToFailureNotify;

        /// <summary>
        ///     The add a task to the scheduled tasks queue
        /// </summary>
        /// <param name="threadStart">
        ///     The action method.
        /// </param>
        /// <param name="id">
        ///     The id.
        /// </param>
        public void AddTasksToQueue(ThreadStart threadStart, int id)
        {
            lock (this.scheduledTasks) this.scheduledTasks.Add(threadStart, id);
        }

        /// <summary>
        ///     Get a safe copy of connected clients.
        /// </summary>
        /// <returns>
        ///     A list of connected clients.
        /// </returns>
        public IEnumerable<ProxyClient> GetConnectedClients()
        {
            List<ProxyClient> clients = new List<ProxyClient>();
            lock (this.ConnectedClients) clients.AddRange(this.ConnectedClients);
            return clients;
        }

        /// <summary>
        ///     Remove a task from the scheduled tasks queue
        /// </summary>
        /// <param name="threadStart">
        ///     The action method.
        /// </param>
        public void RemoveTaskFromQueue(ThreadStart threadStart)
        {
            lock (this.scheduledTasks) this.scheduledTasks.Remove(threadStart);
        }

        /// <summary>
        ///     Listening for new requests and answering them
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" /> value indicating success of the process
        /// </returns>
        /// <exception cref="Exception">
        ///     No supported proxy protocol selected
        /// </exception>
        public bool Start()
        {
            this.lastSendingSpeedCalculationTime = this.lastReceivingSpeedCalculationTime = Environment.TickCount;
            this.lastSentBytes = this.lastReceivedBytes = this.ReceivedBytes = this.SentBytes = 0;
            if (!this.IsHttpSupported && !this.IsHttpsSupported && !this.IsSocksSupported)
            {
                throw new Exception(
                    "We can't start without any supported protocol. If we can't produce anything, so why you want us to start working?!");
            }

            if (this.ActiveServer != null && this.ActiveServer.GetType().Name == typeof(NoServer).Name)
            {
                this.SmartPear.ForwarderSocksEnable = false;
                this.SmartPear.ForwarderHttpsEnable = false;
            }

            if (this.IsAutoConfigEnable && this.Status.HasFlag(ControllerStatus.AutoConfig)
                && !this.Status.HasFlag(ControllerStatus.Proxy))
            {
                this.Status |= ControllerStatus.Proxy;
                return true;
            }

            if (this.Status == ControllerStatus.None)
            {
                if (!this.acceptingWorker.IsBusy)
                {
                    this.listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPEndPoint localIp = new IPEndPoint(this.Ip, this.Port);
                    this.listenerSocket.Bind(localIp);
                    this.listenerSocket.Listen(256);
                    if (this.Port == 0)
                    {
                        this.Port = (ushort)((IPEndPoint)this.listenerSocket.LocalEndPoint).Port;
                    }

                    this.DnsResolver.Start();
                    this.acceptingWorker.RunWorkerAsync();
                    this.routingWorker.RunWorkerAsync();
                }
                this.Status = ControllerStatus.Proxy;
                if (this.IsAutoConfigEnable)
                {
                    this.Status |= ControllerStatus.AutoConfig;
                }
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Stop listening to the new requests or suspend the answering process
        /// </summary>
        public void Stop()
        {
            if (!this.IsAutoConfigEnable)
            {
                this.Status = ControllerStatus.None;
                this.routingWorker.CancelAsync();
                while (this.routingWorker.IsBusy)
                {
                    Application.DoEvents();
                }

                this.acceptingWorker.CancelAsync();
                while (this.acceptingWorker.IsBusy)
                {
                    Application.DoEvents();
                }
            }
            else if (this.Status != ControllerStatus.None)
            {
                this.Status = ControllerStatus.AutoConfig;
                this.CloseAllClients();
            }
        }

        /// <summary>
        ///     Test the currently selected server
        /// </summary>
        public void TestServer()
        {
            this.TestServer(this.ActiveServer);
        }

        /// <summary>
        ///     Test the specified server
        /// </summary>
        /// <param name="activeServer">
        ///     The server
        /// </param>
        /// <exception cref="Exception">
        ///     Failed to connect to the server or server response is invalid
        /// </exception>
        public void TestServer(ServerType activeServer)
        {
            try
            {
                activeServer = activeServer.Clone();
                activeServer.Establish(
                    "google.com",
                    80,
                    new ProxyClient(null, this),
                    Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\n\r\n"));
                int timeout = 25000;
                while (timeout != 0)
                {
                    Thread.Sleep(100);
                    if (activeServer.ParentClient.LastError != string.Empty)
                    {
                        throw new Exception(activeServer.ParentClient.LastError);
                    }

                    if (activeServer.IsServerValid)
                    {
                        return;
                    }

                    if (activeServer.ParentClient.IsClosed)
                    {
                        throw new Exception("Connection dropped by server.");
                    }

                    timeout -= 100;
                }
                throw new Exception("No acceptable response.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        ///     Test the currently selected server asynchronously
        /// </summary>
        /// <param name="callback">
        ///     The callback method.
        /// </param>
        public void TestServerAsyc(OperationWithErrorMessageFinishedDelegate callback)
        {
            this.TestServerAsyc(this.ActiveServer, callback);
        }

        /// <summary>
        ///     Test the specified server asynchronously
        /// </summary>
        /// <param name="activeServer">
        ///     The server
        /// </param>
        /// <param name="callback">
        ///     The callback method.
        /// </param>
        public void TestServerAsyc(ServerType activeServer, OperationWithErrorMessageFinishedDelegate callback)
        {
            new Thread(
                delegate()
                    {
                        string mes = string.Empty;
                        try
                        {
                            this.TestServer(activeServer);
                        }
                        catch (Exception ex)
                        {
                            mes = ex.Message;
                        }

                        if (callback != null)
                        {
                            callback.Invoke(mes == string.Empty, mes);
                        }
                    }) { IsBackground = true }.Start();
        }

        [DebuggerStepThrough]
        internal static void LogIt(string message)
        {
            if (LogNotify == null)
            {
                return;
            }

            foreach (Delegate del in LogNotify.GetInvocationList())
            {
                if (del.Target == null || ((ISynchronizeInvoke)del.Target).InvokeRequired == false)
                {
                    del.DynamicInvoke(new object[] { message, new EventArgs() });
                }
                else
                {
                    ((ISynchronizeInvoke)del.Target).Invoke(del, new object[] { message, new EventArgs() });
                }
            }
        }

        internal void ClientDisconnected(ProxyClient client)
        {
            lock (this.routingClients)
                for (int i = 0; i < this.routingClients.Count; i++)
                {
                    if (this.routingClients[i].ParentClient == null
                        || this.routingClients[i].ParentClient.Equals(client))
                    {
                        this.routingClients.RemoveAt(i);
                        i--;
                    }
                }

            lock (this.ConnectedClients)
                if (this.ConnectedClients.Contains(client))
                {
                    this.ConnectedClients.Remove(client);
                }
        }

        internal void ClientMoveToRouting(ServerType clientServer)
        {
            lock (this.routingClients) this.routingClients.Add(clientServer);
        }

        private void AcceptingWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            while (!this.acceptingWorker.CancellationPending)
            {
                try
                {
                    Thread.Sleep(1);
                    this.acceptingCycle++;
                    if (this.listenerSocket.Poll(0, SelectMode.SelectRead))
                    {
                        this.LastConnectedClient = Environment.TickCount;
                        lock (this.ConnectedClients)
                            this.ConnectedClients.Add(
                                new ProxyClient(this.listenerSocket.Accept(), this)
                                    {
                                        ReceivePacketSize =
                                            this.ReceivePacketSize,
                                        SendPacketSize =
                                            this.SendPacketSize
                                    });
                    }

                    if (this.ConnectedClients.Count > 0)
                    {
                        foreach (ProxyClient t in this.GetConnectedClients())
                        {
                            if (t != null && !t.IsSendingStarted)
                            {
                                t.Accepting();
                            }
                        }
                    }

                    if (this.scheduledTasks.Count > 0)
                    {
                        lock (this.scheduledTasks)
                            for (int i = 0; i < this.scheduledTasks.Count; i++)
                            {
                                if (this.scheduledTasks.Values.ElementAt(i) != 0)
                                {
                                    this.scheduledTasks[this.scheduledTasks.Keys.ElementAt(i)]--;
                                }
                                else
                                {
                                    this.scheduledTasks.Keys.ElementAt(i).Invoke();
                                    this.scheduledTasks.Remove(this.scheduledTasks.Keys.ElementAt(i));
                                }
                            }
                    }

                    this.DnsResolver.Accepting();
                    if (this.AutoDisconnect && this.LastConnectedClient != 0
                        && Environment.TickCount - this.LastConnectedClient >= 60000)
                    {
                        this.LastConnectedClient = Environment.TickCount;
                        this.TestServerAsyc(
                            delegate(bool suc, string mes)
                                {
                                    if (suc)
                                    {
                                        this.failAttempts = 0;
                                    }
                                    else
                                    {
                                        this.failAttempts++;
                                    }
                                });
                    }
                }
                catch (Exception ex)
                {
                    LogIt("AcceptingWorker: " + ex.Message + "\r\n" + ex.StackTrace);
                }
            }
            try
            {
                this.listenerSocket.Close();
                this.DnsResolver.Stop();
                this.CloseAllClients();
            }
            catch (Exception)
            {
            }
        }

        private void CloseAllClients()
        {
            try
            {
                foreach (ProxyClient client in this.GetConnectedClients())
                {
                    try
                    {
                        client.Close();
                    }
                    catch
                    {
                    }
                }
                lock (this.routingClients)
                {
                    this.routingClients.Clear();
                }
            }
            catch
            {
            }
        }

        private void RoutingWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            while (!this.routingWorker.CancellationPending)
            {
                try
                {
                    Thread.Sleep(1);
                    this.routingCycle++;
                    if (this.routingClients.Count > 0)
                    {
                        ServerType[] st;
                        lock (this.routingClients) st = this.routingClients.ToArray();
                        foreach (ServerType server in st)
                        {
                            if (server != null && !server.IsClosed)
                            {
                                server.Route();
                            }
                            else if (server != null)
                            {
                                lock (this.routingClients) this.routingClients.Remove(server);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogIt("RoutingWorker: " + ex.Message + "\r\n" + ex.StackTrace);
                }
            }

            this.CloseAllClients();
        }
    }
}