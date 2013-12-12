using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using PeaRoxy.ClientLibrary.Server_Types;
using System.Windows.Forms;
namespace PeaRoxy.ClientLibrary
{
    public class Proxy_Controller : IDisposable
    {
        public enum ControllerStatus
        {
            Stopped,
            OnlyProxy,
            OnlyAutoConfig,
            Both,
        }
        public enum AutoConfigMimeType:int
        {
            Netscape = 1,
            Javascript = 2,
        }
        public delegate void FailDisconnectedDel(EventArgs e);
        public event FailDisconnectedDel FailDisconnected;
        public delegate void NewLogDelegate(string message, EventArgs e);
        public static event NewLogDelegate NewLog;
        private int failAttempts;
        private int AcceptingCycleLastTime = 1;
        private int RoutingCycleLastTime = 1;
        private int acceptingCycle = 0;
        private int routingCycle = 0;
        private long last_rcvSpeedReaded = 0;
        private long last_sntSpeedReaded = 0;
        private long last_rcvBytes = 0;
        private long last_sntBytes = 0;
        private BackgroundWorker AcceptingWorker;
        private BackgroundWorker RoutingWorker;
        private Socket ListenerSocket;
        private Dictionary<System.Threading.ThreadStart, int> ScheduledTasks = new Dictionary<System.Threading.ThreadStart,int>();
        private List<ServerType> RoutingClients = new List<ServerType>();
        internal List<Proxy_Client> ConnectedClients = new List<Proxy_Client>();
        public ControllerStatus Status { get; private set; }
        public SmartPear SmartPear { get; private set; }
        public ErrorRenderer ErrorRenderer { get; private set; }
        public DNSResolver DNSResolver { get; private set; }
        public AutoConfigMimeType AutoConfigMime { get; set; }
        public ServerType ActiveServer { get; set; }
        public IPAddress IP { get; set; }
        public int LastConnectedClient { get; private set; }
        public string AutoConfigPath { get; set; }
        public bool IsAutoConfigEnable { get; set; }
        public bool AutoDisconnect { get; set; }
        public int SendPacketSize { get; set; }
        public int ReceivePacketSize { get; set; }
        public ushort Port { get; set; }
        public bool IsHTTP_Supported { get; set; }
        public bool IsHTTPS_Supported { get; set; }
        public bool IsSOCKS_Supported { get; set; }
        public long BytesSent { get; internal set; }
        public long BytesReceived { get; internal set; }
        public long AvgSendingSpeed
        {
            get
            {
                long BytesS = this.BytesSent - this.last_sntBytes;
                this.last_sntBytes = this.BytesSent;
                double timeE = (double)(Environment.TickCount - last_sntSpeedReaded) / (double)1000;
                if (timeE > 0)
                {
                    last_sntSpeedReaded = Environment.TickCount;
                    return (long)(BytesS / timeE);
                }
                return 0;
            }
        }
        public long AvgReceivingSpeed
        {
            get
            {
                long BytesR = this.BytesReceived - this.last_rcvBytes;
                this.last_rcvBytes = this.BytesReceived;
                double timeE = (double)(Environment.TickCount - last_rcvSpeedReaded) / (double)1000;
                if (timeE > 0)
                {
                    last_rcvSpeedReaded = Environment.TickCount;
                    return (long)(BytesR / timeE);
                }
                return 0;
            }
        }
        public delegate void OperationWithErrorMessageFinished(bool success, string Error);
        public int AcceptingCycle
        {
            get
            {
                int ret = 1000;
                if (Environment.TickCount != AcceptingCycleLastTime)
                    ret = (acceptingCycle * 1000) / (Environment.TickCount - AcceptingCycleLastTime);
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
                int ret = 1000;
                if (Environment.TickCount != RoutingCycleLastTime)
                    ret = (routingCycle * 1000) / (Environment.TickCount - RoutingCycleLastTime);
                RoutingCycleLastTime = Environment.TickCount;
                routingCycle = 0;
                return ret;
            }
            private set
            {
                routingCycle = value;
            }
        }
        public int FailAttempts
        {
            get
            {
                return failAttempts;
            }
            set
            {
                failAttempts = value;
                if (AutoDisconnect && this.FailAttempts > 30 && this.Status != ControllerStatus.Stopped)
                {
                    this.FailAttempts = 0;
                    Stop();
                    if (FailDisconnected != null)
                        FailDisconnected(new EventArgs());
                }
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
        public Proxy_Controller(ServerType activeServer, IPAddress ip, ushort port)
        {
            last_sntSpeedReaded = last_rcvSpeedReaded = Environment.TickCount;
            last_sntBytes = last_rcvBytes = BytesReceived = BytesSent = 0;
            this.AutoDisconnect = true;
            this.LastConnectedClient = 0;
            this.AutoConfigMime = AutoConfigMimeType.Javascript;
            this.ErrorRenderer = new ErrorRenderer();
            this.SmartPear = new SmartPear();
            this.IsHTTP_Supported = true;
            this.IsHTTPS_Supported = true;
            this.IsSOCKS_Supported = true;
            this.DNSResolver = new DNSResolver(this);
            this.IP = ip;
            this.Port = port;
            this.Status = ControllerStatus.Stopped;
            this.IsAutoConfigEnable = false;
            this.AutoConfigPath = string.Empty;
            this.ActiveServer = activeServer;
            this.AcceptingWorker = new BackgroundWorker();
            this.AcceptingWorker.WorkerSupportsCancellation = true;
            this.AcceptingWorker.DoWork += AcceptingWorker_DoWork;
            this.RoutingWorker = new BackgroundWorker();
            this.RoutingWorker.WorkerSupportsCancellation = true;
            this.RoutingWorker.DoWork += RoutingWorker_DoWork;
        }
        public bool Start()
        {
            last_sntSpeedReaded = last_rcvSpeedReaded = Environment.TickCount;
            last_sntBytes = last_rcvBytes = BytesReceived = BytesSent = 0;
            if (!this.IsHTTP_Supported && !this.IsHTTPS_Supported && !this.IsSOCKS_Supported)
                throw new Exception("We can't start without any supported protocol. If we can't produce anything, so why you want us to start working?!");
            if (this.ActiveServer != null && this.ActiveServer.GetType().Name == typeof(Server_NoServer).Name)
            {
                this.SmartPear.Forwarder_SOCKS_Enable = false;
                this.SmartPear.Forwarder_HTTPS_Enable = false;
            }
            if (this.IsAutoConfigEnable && this.Status == ControllerStatus.OnlyAutoConfig)
            {
                Status = ControllerStatus.Both;
                return true;
            }
            else if (this.Status == ControllerStatus.Stopped)
            {
                int temporary_executer = this.RoutingCycle + this.AcceptingCycle;
                if (!this.AcceptingWorker.IsBusy)
                {
                    this.ListenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPEndPoint ipLocal = new IPEndPoint(this.IP, this.Port);
                    this.ListenerSocket.Bind(ipLocal);
                    this.ListenerSocket.Listen(256);
                    if (this.Port == 0)
                        this.Port = (ushort)((IPEndPoint)this.ListenerSocket.LocalEndPoint).Port;
                    DNSResolver.Start();
                    this.AcceptingWorker.RunWorkerAsync();
                    this.RoutingWorker.RunWorkerAsync();
                }
                if (this.IsAutoConfigEnable)
                    Status = ControllerStatus.Both;
                else
                    Status = ControllerStatus.OnlyProxy;

                return true;
            }
            return false;
        }
        public void Stop()
        {
            Status = ControllerStatus.Stopped;
            this.AcceptingWorker.CancelAsync();
            while (this.AcceptingWorker.IsBusy)
                Application.DoEvents();
            this.RoutingWorker.CancelAsync();
            while (this.RoutingWorker.IsBusy)
                Application.DoEvents();
            if (IsAutoConfigEnable)
            {
                this.Start();
                Status = ControllerStatus.OnlyAutoConfig;
            }
        }
        public void AddToScheduledTasks(System.Threading.ThreadStart str, int id)
        {
            lock (ScheduledTasks)
                ScheduledTasks.Add(str, id);
        }
        public void RemoveFromScheduledTasks(System.Threading.ThreadStart str)
        {
            lock (ScheduledTasks)
                ScheduledTasks.Remove(str);
        }
        private void AcceptingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!this.AcceptingWorker.CancellationPending)
            {
                try
                {
                    System.Threading.Thread.Sleep(1);
                    acceptingCycle++;
                    if (this.ListenerSocket.Poll(0, SelectMode.SelectRead))
                    {
                        LastConnectedClient = Environment.TickCount;
                        lock (ConnectedClients)
                            this.ConnectedClients.Add(new Proxy_Client(this.ListenerSocket.Accept(), this) { ReceivePacketSize = this.ReceivePacketSize, SendPacketSize = this.SendPacketSize }); // Create a client and send TCPClient to it, Let add this client to list too, So we can close it when needed
                    }
                    if (ConnectedClients.Count > 0)
                    {
                        Proxy_Client[] st;
                        lock (ConnectedClients)
                            st = ConnectedClients.ToArray();
                        for (int i = 0; i < st.Length; i++)
                            if (st[i] != null && !st[i].IsSendingStarted)
                                st[i].Accepting();
                    }
                    if (ScheduledTasks.Count > 0)
                        lock (ScheduledTasks)
                            for (int i = 0; i < ScheduledTasks.Count; i++)
                                if (ScheduledTasks.Values.ElementAt(i) != 0)
                                    ScheduledTasks[ScheduledTasks.Keys.ElementAt(i)]--;
                                else
                                {
                                    ScheduledTasks.Keys.ElementAt(i).Invoke();
                                    ScheduledTasks.Remove(ScheduledTasks.Keys.ElementAt(i));
                                }
                    DNSResolver.Accepting();
                    if (AutoDisconnect && LastConnectedClient != 0 && Environment.TickCount - LastConnectedClient >= 60000)
                    {
                        LastConnectedClient = Environment.TickCount;
                        TestServerAsyc((ClientLibrary.Proxy_Controller.OperationWithErrorMessageFinished)delegate(bool suc, string mes)
                        {
                            if (suc)
                                failAttempts = 0;
                            else
                                failAttempts++;
                        });
                    }
                }
                catch (Exception ex)
                {
                    Proxy_Controller.LogIt("AcceptingWorker: " + ex.Message + "\r\n" + ex.StackTrace);
                }
            }
            this.ListenerSocket.Close();
            DNSResolver.Stop();
            if (ConnectedClients.Count > 0)
            {
                Proxy_Client[] st = ConnectedClients.ToArray();
                for (int i = 0; i < st.Length; i++)
                    st[i].Close();
            }
        }
        private void RoutingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!this.RoutingWorker.CancellationPending)
            {
                try
                {
                    System.Threading.Thread.Sleep(1);
                    routingCycle++;
                    if (RoutingClients.Count > 0)
                    {
                        ServerType[] st;
                        lock (RoutingClients)
                            st = RoutingClients.ToArray();
                        for (int i = 0; i < st.Length; i++)
                            if (st[i] != null && !st[i].IsDisconnected)
                                st[i].DoRoute();
                            else if (st[i] != null)
                                lock (RoutingClients)
                                    RoutingClients.Remove(st[i]);
                    }
                }
                catch (Exception ex)
                {
                    Proxy_Controller.LogIt("RoutingWorker: " + ex.Message + "\r\n" + ex.StackTrace);
                }
            }
            lock (RoutingClients)
                this.RoutingClients.Clear();
        }
        internal void iDissconnected(Proxy_Client client)
        {
            lock (RoutingClients)
                for (int i = 0; i < RoutingClients.Count; i++)
                    if (RoutingClients[i].ParentClient == null || RoutingClients[i].ParentClient.Equals(client))
                    {
                        RoutingClients.RemoveAt(i);
                        i--;
                    }

            lock (ConnectedClients)
                if (ConnectedClients.Contains(client))
                    ConnectedClients.Remove(client);
        }
        internal void iMoveToRouting(ServerType ser)
        {
             lock (RoutingClients)
                RoutingClients.Add(ser);
        }
        public void TestServerAsyc(OperationWithErrorMessageFinished callback)
        {
            TestServerAsyc(this.ActiveServer, callback);
        }
        public void TestServerAsyc(ServerType ActiveServer, OperationWithErrorMessageFinished callback)
        {
            new System.Threading.Thread(delegate()
            {
                string mes = string.Empty;
                try
                {
                    this.TestServer(ActiveServer);
                }
                catch (Exception ex)
                {
                    mes = ex.Message;
                }
                if (callback != null)
                    callback.Invoke(mes == string.Empty, mes);
            }) { IsBackground = true }.Start();
        }
        public void TestServer()
        {
            TestServer(this.ActiveServer);
        }
        public void TestServer(ServerType ActiveServer)
        {
            try
            {
                ActiveServer = ActiveServer.Clone();
                ActiveServer.Establish("google.com", 80, new Proxy_Client(null, this), System.Text.Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\n\r\n"));
                int timeout = 25000;
                while (timeout != 0)
                {
                    System.Threading.Thread.Sleep(100);
                    if (ActiveServer.ParentClient.LastError != string.Empty)
                        throw new Exception(ActiveServer.ParentClient.LastError);
                    if (ActiveServer.IsServerValid)
                        return;
                    if (ActiveServer.ParentClient.IsDisconnected)
                        throw new Exception("Connection dropped by server.");
                    timeout -= 100;
                }
                if (ActiveServer.GetType() != typeof(Server_PeaRoxy)) // PeaRoxy problem with Forger and underlying IO
                    throw new Exception("No acceptable response.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public List<Proxy_Client> GetConnectedClients()
        {
            List<Proxy_Client> clients = new List<Proxy_Client>();
            lock (this.ConnectedClients)
                clients.AddRange(this.ConnectedClients);
            return clients;
        }

        [System.Diagnostics.DebuggerStepThrough]
        internal static void LogIt(string message)
        {
            if (NewLog == null)
                return;
            foreach (System.Delegate del in NewLog.GetInvocationList())
            {
                if (del.Target == null || ((System.ComponentModel.ISynchronizeInvoke)del.Target).InvokeRequired == false)
                    del.DynamicInvoke(new object[] { message, new EventArgs() });
                else
                    ((System.ComponentModel.ISynchronizeInvoke)del.Target).Invoke(del, new object[] { message, new EventArgs() });
            }
        }

        public void Dispose()
        {
            this.AcceptingWorker.Dispose();
            this.ListenerSocket.Dispose();
            this.RoutingWorker.Dispose();
        }
    }
}