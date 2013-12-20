// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DnsResolver.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The dns resolver.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary
{
    #region

    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    using PeaRoxy.CommonLibrary;

    #endregion

    /// <summary>
    /// The DNS resolver controller
    /// </summary>
    public class DnsResolver : IDisposable
    {
        #region Fields

        /// <summary>
        /// The parent.
        /// </summary>
        private readonly ProxyController parent;

        /// <summary>
        /// The DNS TCP listener socket
        /// </summary>
        private Socket dnsTcpListenerSocket;

        /// <summary>
        /// The DNS UDP listener socket
        /// </summary>
        private Socket dnsUdpListenerSocket;

        /// <summary>
        /// The local DNS IP.
        /// </summary>
        private IPEndPoint localDnsIp;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsResolver"/> class.
        /// </summary>
        /// <param name="parent">
        /// The parent.
        /// </param>
        public DnsResolver(ProxyController parent)
        {
            this.parent = parent;
            this.DnsResolverServerIp = IPAddress.Parse("8.8.4.4");
            this.DnsResolverSupported = true;
            this.DnsResolverUdpSupported = true;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the DNS resolver server IP.
        /// </summary>
        public IPAddress DnsResolverServerIp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether TCP DNS resolving supported.
        /// </summary>
        public bool DnsResolverSupported { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether UDP DNS resolving supported.
        /// </summary>
        public bool DnsResolverUdpSupported { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The accepting.
        /// </summary>
        public void Accepting()
        {
            try
            {
                if (this.DnsResolverSupported && this.dnsTcpListenerSocket.Poll(0, SelectMode.SelectRead))
                {
                    ProxyClient c = new ProxyClient(this.dnsTcpListenerSocket.Accept(), this.parent, true)
                                         {
                                             ReceivePacketSize
                                                 =
                                                 this
                                                 .parent
                                                 .ReceivePacketSize, 
                                             SendPacketSize
                                                 =
                                                 this
                                                 .parent
                                                 .SendPacketSize, 
                                             NoDataTimeOut
                                                 =
                                                 10
                                         };
                    c.Type = ProxyClient.ClientType.Dns;
                    lock (this.parent.ConnectedClients) this.parent.ConnectedClients.Add(c);
                    this.parent.ActiveServer.Clone().Establish(this.DnsResolverServerIp.ToString(), 53, c);
                }

                if (this.DnsResolverSupported && this.DnsResolverUdpSupported)
                {
                    if (this.dnsUdpListenerSocket == null)
                    {
                        this.dnsUdpListenerSocket = new Socket(
                            AddressFamily.InterNetwork, 
                            SocketType.Dgram, 
                            ProtocolType.Udp);
                        this.dnsUdpListenerSocket.EnableBroadcast = true;
                        this.dnsUdpListenerSocket.Bind(this.localDnsIp);
                    }

                    try
                    {
                        if (this.dnsUdpListenerSocket.Poll(0, SelectMode.SelectRead))
                        {
                            byte[] globalBuffer = new byte[500];
                            EndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
                            int i = this.dnsUdpListenerSocket.ReceiveFrom(globalBuffer, ref remoteEp);
                            byte[] buffer = new byte[i + 2];
                            Array.Copy(globalBuffer, 0, buffer, 2, i);
                            new Thread(
                                delegate()
                                    {
                                        try
                                        {
                                            buffer[0] = (byte)Math.Floor((double)i / 256);
                                            buffer[1] = (byte)(i % 256);
                                            IPAddress ip = this.parent.Ip;
                                                
                                                // Connecting to our self on same port, But TCP
                                            if (ip.Equals(IPAddress.Any))
                                            {
                                                ip = IPAddress.Loopback;
                                            }

                                            Socket tcpConnector = new Socket(
                                                AddressFamily.InterNetwork, 
                                                SocketType.Stream, 
                                                ProtocolType.Tcp);
                                            tcpConnector.Connect(ip, 53);
                                            tcpConnector.Send(buffer);
                                            buffer = new byte[tcpConnector.ReceiveBufferSize];
                                            i = tcpConnector.Receive(buffer);
                                            if (i != 0)
                                            {
                                                int neededBytes = (buffer[0] * 256) + buffer[1] + 2;
                                                Array.Resize(ref buffer, Math.Max(i, neededBytes));
                                                if (i < neededBytes)
                                                {
                                                    int timeout = 2000;
                                                    int received = i;
                                                    while (received < neededBytes && timeout > 0
                                                           && Common.IsSocketConnected(tcpConnector))
                                                    {
                                                        i = tcpConnector.Receive(
                                                            buffer, 
                                                            received, 
                                                            buffer.Length - received, 
                                                            SocketFlags.None);
                                                        received += i;
                                                        if (i == 0)
                                                        {
                                                            break;
                                                        }

                                                        timeout--;
                                                        Thread.Sleep(1);
                                                    }
                                                }
                                            }

                                            tcpConnector.Close();
                                            this.dnsUdpListenerSocket.SendTo(
                                                buffer, 
                                                2, 
                                                buffer.Length - 2, 
                                                SocketFlags.None, 
                                                remoteEp);
                                        }
                                        catch (Exception e)
                                        {
                                            ProxyController.LogIt("DNS Resolver UDP Error: " + e.Message);
                                        }
                                    }) {
                                          IsBackground = true 
                                       }.Start();
                        }
                    }
                    catch (Exception)
                    {
                        this.dnsUdpListenerSocket.Close();
                        this.dnsUdpListenerSocket = null;
                    }
                }
            }
            catch (Exception)
            {
                // Stat.LogIt("DNS Resolver Error: " + e.Message);
            }
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.dnsTcpListenerSocket.Dispose();
            this.dnsUdpListenerSocket.Dispose();
        }

        /// <summary>
        /// The start.
        /// </summary>
        public void Start()
        {
            if (this.DnsResolverSupported)
            {
                this.localDnsIp = new IPEndPoint(this.parent.Ip, 53);
                this.dnsTcpListenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.dnsTcpListenerSocket.Bind(this.localDnsIp);
                this.dnsTcpListenerSocket.Listen(256);
            }
        }

        /// <summary>
        /// The stop.
        /// </summary>
        public void Stop()
        {
            try
            {
                if (this.DnsResolverSupported)
                {
                    this.dnsTcpListenerSocket.Close();
                    if (this.DnsResolverUdpSupported && this.dnsUdpListenerSocket != null)
                    {
                        this.dnsUdpListenerSocket.Close();
                        this.dnsUdpListenerSocket = null;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}