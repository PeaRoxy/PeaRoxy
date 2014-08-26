// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProxyController.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   //   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The proxy_ controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary
{
    #region

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

    #endregion

    /// <summary>
    ///     The proxy controller.
    /// </summary>
    public class ProxyController : IDisposable
    {
        #region Fields

        /// <summary>
        ///     The connected clients.
        /// </summary>
        internal readonly List<ProxyClient> ConnectedClients = new List<ProxyClient>();

        /// <summary>
        ///     The accepting worker.
        /// </summary>
        private readonly BackgroundWorker acceptingWorker;

        /// <summary>
        ///     The routing clients.
        /// </summary>
        private readonly List<ServerType> routingClients = new List<ServerType>();

        /// <summary>
        ///     The routing worker.
        /// </summary>
        private readonly BackgroundWorker routingWorker;

        /// <summary>
        ///     The scheduled tasks.
        /// </summary>
        private readonly Dictionary<ThreadStart, int> scheduledTasks = new Dictionary<ThreadStart, int>();

        /// <summary>
        ///     The accepting cycle last time.
        /// </summary>
        private int acceptingCycleLastTime = 1;

        /// <summary>
        ///     The listener socket.
        /// </summary>
        private Socket listenerSocket;

        /// <summary>
        ///     The routing cycle last time.
        /// </summary>
        private int routingCycleLastTime = 1;

        /// <summary>
        ///     The accepting cycle.
        /// </summary>
        private int acceptingCycle;

        /// <summary>
        ///     The fail attempts.
        /// </summary>
        private int failAttempts;

        /// <summary>
        ///     The number of bytes we received (old)
        /// </summary>
        private long oldReceivedBytes;

        /// <summary>
        ///     The receive speed (old)
        /// </summary>
        private long oldReceiveSpeed;

        /// <summary>
        ///     The number of bytes we sent (old)
        /// </summary>
        private long oldSentBytes;

        /// <summary>
        ///     The send speed (old)
        /// </summary>
        private long oldSendSpeed;

        /// <summary>
        ///     The routing cycle.
        /// </summary>
        private int routingCycle;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyController"/> class.
        /// </summary>
        /// <param name="activeServer">
        /// The active server.
        /// </param>
        /// <param name="ip">
        /// The IP.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        public ProxyController(ServerType activeServer, IPAddress ip, ushort port)
        {
            this.oldSendSpeed = this.oldReceiveSpeed = Environment.TickCount;
            this.oldSentBytes = this.oldReceivedBytes = this.ReceivedBytes = this.SentBytes = 0;
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
        }

        #endregion

        #region Delegates

        /// <summary>
        ///     The fail disconnected del.
        /// </summary>
        /// <param name="e">
        ///     The e.
        /// </param>
        public delegate void FailDisconnectedDel(EventArgs e);

        /// <summary>
        ///     The new log delegate.
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        public delegate void NewLogDelegate(string message, EventArgs e);

        /// <summary>
        ///     The operation with error message finished.
        /// </summary>
        /// <param name="success">
        ///     The success.
        /// </param>
        /// <param name="error">
        ///     The error.
        /// </param>
        public delegate void OperationWithErrorMessageFinished(bool success, string error);

        #endregion

        #region Public Events

        /// <summary>
        ///     The new log.
        /// </summary>
        public static event NewLogDelegate NewLog;

        /// <summary>
        ///     The fail disconnected.
        /// </summary>
        public event FailDisconnectedDel FailDisconnected;

        #endregion

        #region Enums

        /// <summary>
        ///     The auto config mime type.
        /// </summary>
        public enum AutoConfigMimeType
        {
            /// <summary>
            ///     The netscape.
            /// </summary>
            Netscape = 1, 

            /// <summary>
            ///     The java script.
            /// </summary>
            Javascript = 2, 
        }

        /// <summary>
        ///     The controller status enum.
        /// </summary>
        [Flags]
        public enum ControllerStatus
        {
            None = 0,

            Proxy = 1,

            AutoConfig = 2,

            ProxyAndAutoConfig = Proxy | AutoConfig,
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the accepting cycle.
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
        ///     Gets or sets the active server.
        /// </summary>
        public ServerType ActiveServer { get; set; }

        /// <summary>
        ///     Gets or sets the auto config mime.
        /// </summary>
        public AutoConfigMimeType AutoConfigMime { get; set; }

        /// <summary>
        ///     Gets or sets the auto config path.
        /// </summary>
        public string AutoConfigPath { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether auto disconnect.
        /// </summary>
        public bool AutoDisconnect { get; set; }

        /// <summary>
        ///     Gets the average receiving speed.
        /// </summary>
        public long AverageReceivingSpeed
        {
            get
            {
                long bytesReceived = this.ReceivedBytes - this.oldReceivedBytes;
                this.oldReceivedBytes = this.ReceivedBytes;
                double timeE = (Environment.TickCount - this.oldReceiveSpeed) / (double)1000;
                if (timeE > 0)
                {
                    this.oldReceiveSpeed = Environment.TickCount;
                    return (long)(bytesReceived / timeE);
                }

                return 0;
            }
        }

        /// <summary>
        ///     Gets the average sending speed.
        /// </summary>
        public long AverageSendingSpeed
        {
            get
            {
                long bytesSent = this.SentBytes - this.oldSentBytes;
                this.oldSentBytes = this.SentBytes;
                double timeE = (Environment.TickCount - this.oldSendSpeed) / (double)1000;
                if (!(timeE > 0))
                {
                    return 0;
                }

                this.oldSendSpeed = Environment.TickCount;
                return (long)(bytesSent / timeE);
            }
        }

        /// <summary>
        ///     Gets the bytes received.
        /// </summary>
        public long ReceivedBytes { get; internal set; }

        /// <summary>
        ///     Gets the bytes sent.
        /// </summary>
        public long SentBytes { get; internal set; }

        /// <summary>
        ///     Gets the DNS resolver.
        /// </summary>
        public DnsResolver DnsResolver { get; private set; }

        /// <summary>
        ///     Gets the error renderer.
        /// </summary>
        public ErrorRenderer ErrorRenderer { get; private set; }

        /// <summary>
        ///     Gets or sets the fail attempts.
        /// </summary>
        public int FailAttempts
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
                    if (this.FailDisconnected != null)
                    {
                        this.FailDisconnected(new EventArgs());
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets the IP.
        /// </summary>
        public IPAddress Ip { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is auto config enable.
        /// </summary>
        public bool IsAutoConfigEnable { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is http s_ supported.
        /// </summary>
        public bool IsHttpsSupported { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is http supported.
        /// </summary>
        public bool IsHttpSupported { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is sock s_ supported.
        /// </summary>
        public bool IsSocksSupported { get; set; }

        /// <summary>
        ///     Gets the last connected client.
        /// </summary>
        public int LastConnectedClient { get; private set; }

        /// <summary>
        ///     Gets or sets the port.
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        ///     Gets or sets the receive packet size.
        /// </summary>
        public int ReceivePacketSize { get; set; }

        /// <summary>
        ///     Gets the routing connections.
        /// </summary>
        public int RoutingConnections
        {
            get
            {
                return this.routingClients.Count;
            }
        }

        /// <summary>
        ///     Gets the routing cycle.
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
        ///     Gets the smart pear.
        /// </summary>
        public SmartPear SmartPear { get; private set; }

        /// <summary>
        ///     Gets the status.
        /// </summary>
        public ControllerStatus Status { get; private set; }

        /// <summary>
        ///     Gets the accepting connections.
        /// </summary>
        public int AcceptingConnections
        {
            get
            {
                return Math.Max(this.ConnectedClients.Count - this.routingClients.Count, 0);
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add to scheduled tasks.
        /// </summary>
        /// <param name="threadStart">
        /// The thread.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        public void AddToScheduledTasks(ThreadStart threadStart, int id)
        {
            lock (this.scheduledTasks) this.scheduledTasks.Add(threadStart, id);
        }

        /// <summary>
        ///     The dispose.
        /// </summary>
        public void Dispose()
        {
            this.acceptingWorker.Dispose();
            this.listenerSocket.Dispose();
            this.routingWorker.Dispose();
        }

        /// <summary>
        ///     The get connected clients.
        /// </summary>
        /// <returns>
        ///     The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public IEnumerable<ProxyClient> GetConnectedClients()
        {
            List<ProxyClient> clients = new List<ProxyClient>();
            lock (this.ConnectedClients) clients.AddRange(this.ConnectedClients);
            return clients;
        }

        /// <summary>
        /// The remove from scheduled tasks.
        /// </summary>
        /// <param name="threadStart">
        /// The thread.
        /// </param>
        public void RemoveFromScheduledTasks(ThreadStart threadStart)
        {
            lock (this.scheduledTasks) this.scheduledTasks.Remove(threadStart);
        }

        /// <summary>
        ///     The start.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        /// <exception cref="Exception">
        /// No supported proxy protocol selected
        /// </exception>
        public bool Start()
        {
            this.oldSendSpeed = this.oldReceiveSpeed = Environment.TickCount;
            this.oldSentBytes = this.oldReceivedBytes = this.ReceivedBytes = this.SentBytes = 0;
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

            if (this.IsAutoConfigEnable && this.Status.HasFlag(ControllerStatus.AutoConfig) && !this.Status.HasFlag(ControllerStatus.Proxy))
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
                    this.Status |= ControllerStatus.AutoConfig;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     The stop.
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
                CloseAllClients();
            }
        }

        /// <summary>
        ///     The test server.
        /// </summary>
        public void TestServer()
        {
            this.TestServer(this.ActiveServer);
        }

        /// <summary>
        /// The test server.
        /// </summary>
        /// <param name="activeServer">
        /// The active server.
        /// </param>
        /// <exception cref="Exception">
        /// Failed to connect to the server
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

                if (activeServer.GetType() != typeof(PeaRoxy))
                {
                    // PeaRoxy problem with Forger and underlying IO
                    throw new Exception("No acceptable response.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// The test server asynchronous.
        /// </summary>
        /// <param name="callback">
        /// The callback.
        /// </param>
        public void TestServerAsyc(OperationWithErrorMessageFinished callback)
        {
            this.TestServerAsyc(this.ActiveServer, callback);
        }

        /// <summary>
        /// The test server asynchronous.
        /// </summary>
        /// <param name="activeServer">
        /// The active server.
        /// </param>
        /// <param name="callback">
        /// The callback.
        /// </param>
        public void TestServerAsyc(ServerType activeServer, OperationWithErrorMessageFinished callback)
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
                    }) {
                          IsBackground = true 
                       }.Start();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The log it.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        [DebuggerStepThrough]
        internal static void LogIt(string message)
        {
            if (NewLog == null)
            {
                return;
            }

            foreach (Delegate del in NewLog.GetInvocationList())
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

        /// <summary>
        /// The i disconnected.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        internal void Disconnected(ProxyClient client)
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

        /// <summary>
        /// Moves the server to the routing list
        /// </summary>
        /// <param name="server">
        /// The server object
        /// </param>
        internal void MoveToRouting(ServerType server)
        {
            lock (this.routingClients) this.routingClients.Add(server);
        }

        /// <summary>
        /// The accepting worker code
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
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
                CloseAllClients();
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

        /// <summary>
        /// The routing worker code
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
                                server.DoRoute();
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

            CloseAllClients();
        }

        #endregion
    }
}