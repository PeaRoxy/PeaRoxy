// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProxyClient.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The proxy client object
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary
{
    #region

    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    using PeaRoxy.ClientLibrary.ProxyModules;
    using PeaRoxy.ClientLibrary.ServerModules;
    using PeaRoxy.CommonLibrary;
    using PeaRoxy.Platform;

    using Https = PeaRoxy.ClientLibrary.ProxyModules.Https;
    using Socks5 = PeaRoxy.ClientLibrary.ProxyModules.Socks5;

    #endregion

    /// <summary>
    ///     The proxy client object
    /// </summary>
    public class ProxyClient
    {
        #region Fields

        /// <summary>
        ///     The current timeout.
        /// </summary>
        private int currentTimeout;

        /// <summary>
        ///     The extended info.
        /// </summary>
        private ConnectionInfo extendedInfo;

        /// <summary>
        ///     The speed of receiving data (old)
        /// </summary>
        private long oldReceiveSpeed;

        /// <summary>
        ///     The number of bytes we received (old)
        /// </summary>
        private long oldReceivedBytes;

        /// <summary>
        ///     The speed of sending data (old)
        /// </summary>
        private long oldSendSpeed;

        /// <summary>
        ///     The number of bytes we sent (old)
        /// </summary>
        private long oldSentBytes;

        /// <summary>
        ///     The request buffer.
        /// </summary>
        private byte[] reqBuffer = new byte[0];

        /// <summary>
        ///     The request address.
        /// </summary>
        private string requestAddress = string.Empty;

        /// <summary>
        ///     The write buffer.
        /// </summary>
        private byte[] writeBuffer = new byte[0];

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProxyClient" /> class.
        /// </summary>
        /// <param name="client">
        ///     The client.
        /// </param>
        /// <param name="parent">
        ///     The parent.
        /// </param>
        /// <param name="isDirectConnection">
        ///     The is direct connection.
        /// </param>
        public ProxyClient(Socket client, ProxyController parent, bool isDirectConnection = false)
        {
            this.SmartRequestBuffer = new byte[0];
            this.SmartResponseBuffer = new byte[0];
            this.oldSendSpeed = this.oldReceiveSpeed = Environment.TickCount;
            this.oldSentBytes = this.oldReceivedBytes = this.ReceivedBytes = this.SentBytes = 0;
            if (client != null && client.ProtocolType == ProtocolType.Tcp)
            {
                this.Type = ClientType.Tcp;
            }
            else if (client != null && client.ProtocolType == ProtocolType.Udp)
            {
                this.Type = ClientType.Udp;
            }

            this.Status = StatusCodes.Connected;
            this.LastError = string.Empty;
            this.NoDataTimeOut = 60;
            this.IsSmartForwarderEnable = parent.SmartPear.ForwarderHttpEnable || parent.SmartPear.ForwarderHttpsEnable
                                          || parent.SmartPear.ForwarderSocksEnable;
            this.SendPacketSize = 1024;
            this.ReceivePacketSize = 8192;
            this.IsClosed = false;
            this.Controller = parent;
            this.Client = client;
            if (this.Client != null)
            {
                this.Client.Blocking = false;
            }

            this.IsSendingStarted = isDirectConnection;
        }

        #endregion

        #region Enums

        /// <summary>
        ///     The e type.
        /// </summary>
        public enum ClientType
        {
            /// <summary>
            ///     This is a TCP Client
            /// </summary>
            Tcp,

            /// <summary>
            ///     This is a UDP Client
            /// </summary>
            Udp,

            /// <summary>
            ///     this is a DNS Client
            /// </summary>
            Dns
        }

        /// <summary>
        ///     The e status.
        /// </summary>
        public enum StatusCodes
        {
            /// <summary>
            ///     Client is connected
            /// </summary>
            Connected,

            /// <summary>
            ///     Client is waiting
            /// </summary>
            Waiting,

            /// <summary>
            ///     Client is routing
            /// </summary>
            Routing,

            /// <summary>
            ///     Client is closing
            /// </summary>
            Closing
        }

        #endregion

        #region Public Properties

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
                if (timeE <= 0)
                {
                    return 0;
                }
                this.oldReceiveSpeed = Environment.TickCount;
                return (long)(bytesReceived / timeE);
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
                if (timeE <= 0)
                {
                    return 0;
                }
                this.oldSendSpeed = Environment.TickCount;
                return (long)(bytesSent / timeE);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether we are busy writing.
        /// </summary>
        public bool BusyWrite
        {
            get
            {
                if (this.writeBuffer.Length > 0)
                {
                    this.Write(null);
                }

                return this.writeBuffer.Length > 0;
            }
        }

        /// <summary>
        ///     Gets or sets the client.
        /// </summary>
        public Socket Client { get; set; }

        /// <summary>
        ///     Gets the controller.
        /// </summary>
        public ProxyController Controller { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether is disconnected.
        /// </summary>
        public bool IsClosed { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether is receiving started.
        /// </summary>
        public bool IsReceivingStarted { get; internal set; }

        /// <summary>
        ///     Gets a value indicating whether is sending started.
        /// </summary>
        public bool IsSendingStarted { get; internal set; }

        /// <summary>
        ///     Gets a value indicating whether is smart forwarder enable.
        /// </summary>
        public bool IsSmartForwarderEnable { get; internal set; }

        /// <summary>
        ///     Gets the last error.
        /// </summary>
        public string LastError { get; private set; }

        /// <summary>
        ///     Gets or sets the no data time out.
        /// </summary>
        public int NoDataTimeOut { get; set; }

        /// <summary>
        ///     Gets or sets the receive packet size.
        /// </summary>
        public int ReceivePacketSize { get; set; }

        /// <summary>
        ///     Gets the bytes received.
        /// </summary>
        public long ReceivedBytes { get; private set; }

        /// <summary>
        ///     Gets or sets the request address.
        /// </summary>
        public string RequestAddress
        {
            get
            {
                return this.requestAddress;
            }

            set
            {
                this.requestAddress = value;
                this.IsSmartForwarderEnable = !this.IsNeedForwarding();
            }
        }

        /// <summary>
        ///     Gets or sets the send packet size.
        /// </summary>
        public int SendPacketSize { get; set; }

        /// <summary>
        ///     Gets the bytes sent.
        /// </summary>
        public long SentBytes { get; private set; }

        /// <summary>
        ///     Gets the status.
        /// </summary>
        public StatusCodes Status { get; internal set; }

        /// <summary>
        ///     Gets the type.
        /// </summary>
        public ClientType Type { get; internal set; }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets smart request buffer.
        /// </summary>
        internal byte[] SmartRequestBuffer { get; set; }

        /// <summary>
        ///     Gets or sets smart response buffer.
        /// </summary>
        internal byte[] SmartResponseBuffer { get; set; }

        #endregion

        #region Public Methods and Operators

        public bool IsConnected
        {
            get
            {
                return !this.IsClosed && this.Client != null && Common.IsSocketConnected(this.Client);
            }
        }

        /// <summary>
        ///     The direct_ connection status callback.
        /// </summary>
        /// <param name="currentActiveServer">
        ///     The current active server.
        /// </param>
        /// <param name="success">
        ///     The success.
        /// </param>
        /// <param name="isSocks">
        ///     The is socks.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool DirectConnectionStatusCallback(ServerType currentActiveServer, bool success, bool isSocks)
        {
            if (this.IsSmartForwarderEnable)
            {
                SmartPear smart = this.Controller.SmartPear;
                if (!success)
                {
                    if (smart.ForwarderHttpsEnable && (!isSocks || smart.ForwarderSocksEnable)
                        && smart.DetectorTimeoutEnable)
                    {
                        this.IsSmartForwarderEnable = false;
                        if (isSocks)
                        {
                            Socks5.DirectHandle(
                                this,
                                currentActiveServer.GetAddress(),
                                currentActiveServer.GetPort(),
                                this.SmartRequestBuffer);
                        }
                        else
                        {
                            Https.DirectHandle(
                                this,
                                currentActiveServer.GetAddress(),
                                currentActiveServer.GetPort(),
                                this.SmartRequestBuffer);
                        }

                        return false;
                    }
                }
                else
                {
                    if (smart.ForwarderHttpsEnable && (!isSocks || smart.ForwarderSocksEnable)
                        && smart.DetectorDnsGrabberEnable && currentActiveServer.UnderlyingSocket != null)
                    {
                        if (currentActiveServer.UnderlyingSocket.RemoteEndPoint != null
                            && smart.DetectorDnsGrabberRegEx.IsMatch(
                                ((IPEndPoint)currentActiveServer.UnderlyingSocket.RemoteEndPoint).Address.ToString()))
                        {
                            this.IsSmartForwarderEnable = false;
                            if (isSocks)
                            {
                                Socks5.DirectHandle(
                                    this,
                                    currentActiveServer.GetAddress(),
                                    currentActiveServer.GetPort(),
                                    this.SmartRequestBuffer);
                            }
                            else
                            {
                                Https.DirectHandle(
                                    this,
                                    currentActiveServer.GetAddress(),
                                    currentActiveServer.GetPort(),
                                    this.SmartRequestBuffer);
                            }

                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     The direct_ data received callback.
        /// </summary>
        /// <param name="binary">
        ///     The binary.
        /// </param>
        /// <param name="currentActiveServer">
        ///     The current active server.
        /// </param>
        /// <param name="isSocks">
        ///     The is socks.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool DirectDataReceivedCallback(ref byte[] binary, ServerType currentActiveServer, bool isSocks)
        {
            SmartPear smart = this.Controller.SmartPear;
            if (smart.ForwarderHttpsEnable && (!isSocks || smart.ForwarderSocksEnable))
            {
                // If HTTPS Forwarded
                if (!this.IsSmartForwarderEnable
                    && ((smart.DetectorDirectPort80AsHttp && smart.ForwarderDirectPort80AsHttp)
                        || smart.DetectorStatusDnsGrabber || smart.DetectorStatusTimeout))
                {
                    // If using proxy, and forwarder HTTPS is enable
                    Uri url;
                    if (this.RequestAddress != string.Empty
                        && Uri.TryCreate(this.RequestAddress, UriKind.Absolute, out url))
                    {
                        if (smart.DetectorDirectPort80AsHttp && smart.ForwarderDirectPort80AsHttp && url.Port == 80
                            && Http.IsHttp(this.SmartRequestBuffer))
                        {
                            smart.AddRuleToHttpForwarder(
                                "* | *" + url.Host.ToLower().TrimEnd(new[] { '/', '\\' }) + "*");
                        }
                        else
                        {
                            smart.AddRuleToDirectForwarder("* | *" + url.Host.ToLower() + ":" + url.Port);
                        }
                    }

                    currentActiveServer.ReceiveDataDelegate = null;
                    return true;
                }

                if (smart.ForwarderDirectPort80AsHttp && smart.DetectorDirectPort80AsHttp
                    && this.RequestAddress.EndsWith(":80") && Http.IsHttp(this.SmartRequestBuffer))
                {
                    // If we have Forwarder Enabled
                    bool blocked = false;
                    if (smart.DetectorStatusHttp && this.SmartResponseBuffer.Length < smart.DetectorHttpMaxBuffering)
                    {
                        // If detector is enable and responce is less than buffer size
                        byte[] smartResponseBuffer = this.SmartResponseBuffer;
                        Array.Resize(ref smartResponseBuffer, smartResponseBuffer.Length + binary.Length);
                        Array.Copy(
                            binary,
                            0,
                            smartResponseBuffer,
                            smartResponseBuffer.Length - binary.Length,
                            binary.Length);
                        Array.Resize(ref binary, 0);
                        if (smart.DetectorHttpRegEx.IsMatch(Encoding.ASCII.GetString(smartResponseBuffer)))
                        {
                            // If Responce is FILTERED
                            blocked = true;
                        }

                        this.SmartResponseBuffer = smartResponseBuffer;
                    }

                    if (blocked)
                    {
                        currentActiveServer.ReceiveDataDelegate = null;
                        if (this.IsSmartForwarderEnable)
                        {
                            // If client use NoServer
                            byte[] localReqBackup = new byte[this.SmartRequestBuffer.Length];
                            Array.Copy(this.SmartRequestBuffer, localReqBackup, localReqBackup.Length);
                            this.ForwarderClean();
                            this.IsSmartForwarderEnable = false;
                            Http.DirectHandle(
                                this,
                                currentActiveServer.GetAddress(),
                                currentActiveServer.GetPort(),
                                localReqBackup);
                            return false;
                        }
                    }
                    else
                    {
                        // Responce is OK
                        if (!this.IsSmartForwarderEnable
                            && (smart.DetectorStatusHttp || smart.DetectorStatusDnsGrabber
                                || smart.DetectorStatusTimeout))
                        {
                            // If client use Proxy and one of possible detectors is enable
                            Uri url;
                            if (this.RequestAddress != string.Empty
                                && Uri.TryCreate(this.RequestAddress, UriKind.Absolute, out url))
                            {
                                smart.AddRuleToHttpForwarder(
                                    "* | *" + url.Host.ToLower().TrimEnd(new[] { '/', '\\' }) + "*");

                                // Bug: I dont have time to solve it and also not importante as if a address is blocked, so other case of that address is also blocked.
                            }

                            currentActiveServer.ReceiveDataDelegate = null;
                        }
                    }

                    if (this.SmartResponseBuffer.Length > 0
                        && (!this.IsSmartForwarderEnable
                            || this.SmartResponseBuffer.Length >= smart.DetectorHttpMaxBuffering))
                    {
                        // If we have any thing in Responce and (Client use Proxy or Client use NoServer but Responce buffer is bigger than buffer)
                        this.ForwarderFlush(smart.ForwarderHttpEnable);
                        currentActiveServer.ReceiveDataDelegate = null;
                    }
                }
            }
            else
            {
                currentActiveServer.ReceiveDataDelegate = null;
            }

            return true;
        }

        /// <summary>
        ///     The direct_ data sent callback.
        /// </summary>
        /// <param name="binary">
        ///     The binary.
        /// </param>
        /// <param name="isSocks">
        ///     The is socks.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool DirectDataSentCallback(byte[] binary, bool isSocks)
        {
            SmartPear smart = this.Controller.SmartPear;
            if (this.IsSmartForwarderEnable && smart.ForwarderHttpsEnable && (!isSocks || smart.ForwarderSocksEnable))
            {
                byte[] smartRequestBuffer = this.SmartRequestBuffer;
                Array.Resize(ref smartRequestBuffer, smartRequestBuffer.Length + binary.Length);
                Array.Copy(binary, 0, smartRequestBuffer, smartRequestBuffer.Length - binary.Length, binary.Length);
                this.SmartRequestBuffer = smartRequestBuffer;
            }
            else if (this.SmartResponseBuffer.Length > 0)
            {
                this.ForwarderFlush(smart.ForwarderHttpsEnable && (!isSocks || smart.ForwarderSocksEnable));
            }

            return true;
        }

        /// <summary>
        ///     The forwarder_ clean.
        /// </summary>
        /// <param name="enable">
        ///     The enable.
        /// </param>
        public void ForwarderClean(bool? enable = null)
        {
            this.IsSmartForwarderEnable = (enable == null)
                                              ? this.Controller.SmartPear.ForwarderHttpEnable
                                              : (bool)enable;
            this.SmartResponseBuffer = new byte[0];
            this.SmartRequestBuffer = new byte[0];
        }

        /// <summary>
        ///     The forwarder_ flush.
        /// </summary>
        /// <param name="enable">
        ///     The enable.
        /// </param>
        public void ForwarderFlush(bool enable)
        {
            this.Write(this.SmartResponseBuffer);
            this.ForwarderClean(enable);
            this.IsSmartForwarderEnable = false;
        }

        /// <summary>
        ///     The http connection status callback.
        /// </summary>
        /// <param name="currentActiveServer">
        ///     The current active server.
        /// </param>
        /// <param name="success">
        ///     The success.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool HttpConnectionStatusCallback(ServerType currentActiveServer, bool success)
        {
            if (this.IsSmartForwarderEnable)
            {
                SmartPear smart = this.Controller.SmartPear;
                if (!success)
                {
                    if (smart.ForwarderHttpEnable && smart.DetectorTimeoutEnable)
                    {
                        this.IsSmartForwarderEnable = false;
                        Http.DirectHandle(
                            this,
                            currentActiveServer.GetAddress(),
                            currentActiveServer.GetPort(),
                            this.SmartRequestBuffer);
                        return false;
                    }
                }
                else
                {
                    if (smart.ForwarderHttpEnable && smart.DetectorDnsGrabberEnable
                        && currentActiveServer.UnderlyingSocket != null)
                    {
                        if (currentActiveServer.UnderlyingSocket.RemoteEndPoint != null
                            && smart.DetectorDnsGrabberRegEx.IsMatch(
                                ((IPEndPoint)currentActiveServer.UnderlyingSocket.RemoteEndPoint).Address.ToString()))
                        {
                            this.IsSmartForwarderEnable = false;
                            Http.DirectHandle(
                                this,
                                currentActiveServer.GetAddress(),
                                currentActiveServer.GetPort(),
                                this.SmartRequestBuffer);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     The http data received callback.
        /// </summary>
        /// <param name="binary">
        ///     The binary.
        /// </param>
        /// <param name="currentActiveServer">
        ///     The current active server.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool HttpDataReceivedCallback(ref byte[] binary, ServerType currentActiveServer)
        {
            SmartPear smart = this.Controller.SmartPear;
            if (smart.ForwarderHttpEnable)
            {
                // If we have Forwarder Enabled
                bool blocked = false;
                if (smart.DetectorStatusHttp && this.SmartResponseBuffer.Length < smart.DetectorHttpMaxBuffering)
                {
                    // If detector is enable and responce is less than buffer size
                    byte[] smartResponseBuffer = this.SmartResponseBuffer;
                    Array.Resize(ref smartResponseBuffer, smartResponseBuffer.Length + binary.Length);
                    Array.Copy(
                        binary,
                        0,
                        smartResponseBuffer,
                        smartResponseBuffer.Length - binary.Length,
                        binary.Length);
                    Array.Resize(ref binary, 0);
                    if (smart.DetectorHttpRegEx.IsMatch(Encoding.ASCII.GetString(smartResponseBuffer)))
                    {
                        // If Responce is FILTERED
                        blocked = true;
                    }

                    this.SmartResponseBuffer = smartResponseBuffer;
                }

                if (blocked)
                {
                    currentActiveServer.ReceiveDataDelegate = null;
                    if (this.IsSmartForwarderEnable)
                    {
                        // If client use NoServer
                        byte[] localReqBackup = new byte[this.SmartRequestBuffer.Length];
                        Array.Copy(this.SmartRequestBuffer, localReqBackup, localReqBackup.Length);
                        this.ForwarderClean();
                        this.IsSmartForwarderEnable = false;
                        Http.DirectHandle(
                            this,
                            currentActiveServer.GetAddress(),
                            currentActiveServer.GetPort(),
                            localReqBackup);
                        return false;
                    }
                }
                else
                {
                    // Responce is OK
                    if (!this.IsSmartForwarderEnable
                        && (smart.DetectorStatusHttp || smart.DetectorStatusDnsGrabber || smart.DetectorStatusTimeout))
                    {
                        // If client use Proxy and one of possible detectors is enable
                        Uri url;
                        if (this.RequestAddress != string.Empty
                            && Uri.TryCreate(this.RequestAddress, UriKind.Absolute, out url))
                        {
                            smart.AddRuleToHttpForwarder(
                                "* | *" + url.Host.ToLower().TrimEnd(new[] { '/', '\\' }) + "*");

                            // Bug: I dont have time to solve it and also not importante as if a address is blocked, so other case of that address is also blocked.
                        }

                        currentActiveServer.ReceiveDataDelegate = null;
                    }
                }

                if (this.SmartResponseBuffer.Length > 0
                    && (!this.IsSmartForwarderEnable
                        || this.SmartResponseBuffer.Length >= smart.DetectorHttpMaxBuffering))
                {
                    // If we have any thing in Responce and (Client use Proxy or Client use NoServer but Responce buffer is bigger than buffer)
                    this.ForwarderFlush(smart.ForwarderHttpEnable);
                    currentActiveServer.ReceiveDataDelegate = null;
                }
            }

            return true;
        }

        /// <summary>
        ///     The http data sent callback.
        /// </summary>
        /// <param name="binary">
        ///     The binary.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool HttpDataSentCallback(byte[] binary)
        {
            SmartPear smart = this.Controller.SmartPear;
            if (smart.ForwarderHttpEnable && (smart.DetectorHttpEnable || smart.DetectorDnsGrabberEnable))
            {
                // If we have Forwarder Enabled
                if (this.IsSmartForwarderEnable)
                {
                    // If Client using NoServer
                    byte[] smartRequestBuffer = this.SmartRequestBuffer;
                    Array.Resize(ref smartRequestBuffer, smartRequestBuffer.Length + binary.Length);
                    Array.Copy(binary, 0, smartRequestBuffer, smartRequestBuffer.Length - binary.Length, binary.Length);
                    this.SmartRequestBuffer = smartRequestBuffer;
                }
                else if (this.SmartResponseBuffer.Length > 0)
                {
                    // If client use Proxy and there is a responce already.
                    this.ForwarderFlush(smart.ForwarderHttpEnable);
                }
            }

            return true;
        }

        /// <summary>
        ///     The is need forwarding.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool IsNeedForwarding()
        {
            if (this.RequestAddress == string.Empty)
            {
                return false;
            }

            string name = "Unknown | ";

            ConnectionInfo conInfo = this.GetExtendedInfo();
            if (conInfo != null && conInfo.ProcessName != string.Empty)
            {
                name = conInfo.ProcessName + " | ";
            }

            SmartPear smart = this.Controller.SmartPear;
            string p = this.RequestAddress.ToLower();
            if (p.IndexOf("http://", StringComparison.OrdinalIgnoreCase) == 0)
            {
                Uri parsedUrl = new Uri(p);
                p = name + parsedUrl.Host;
                if (smart.ForwarderHttpList.Any(t => Common.IsMatchWildCard(p, t)))
                {
                    return true;
                }
            }
            else if (p.IndexOf("socks://", StringComparison.OrdinalIgnoreCase) == 0
                     || p.IndexOf("https://", StringComparison.OrdinalIgnoreCase) == 0)
            {
                p = name + p.Substring(p.IndexOf("://", StringComparison.Ordinal) + 3);
                if (smart.ForwarderDirectList.Any(t => Common.IsMatchWildCard(p, t)))
                {
                    return true;
                }

                if (smart.ForwarderDirectPort80AsHttp && p.IndexOf(":", StringComparison.Ordinal) != -1
                    && p.Substring(p.IndexOf(":", StringComparison.Ordinal) + 1) == "80")
                {
                    p = p.Substring(0, p.IndexOf(":", StringComparison.Ordinal));
                    if (smart.ForwarderHttpList.Any(t => Common.IsMatchWildCard(p, t)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     The accepting process
        /// </summary>
        public void Accepting()
        {
            try
            {
                if (Common.IsSocketConnected(this.Client))
                {
                    if (this.IsSendingStarted)
                    {
                        return;
                    }
                    if (this.Client.Available > 0)
                    {
                        this.Status = StatusCodes.Waiting;
                        this.currentTimeout = 0;
                        byte[] clientData = this.Read();
                        if (clientData == null || clientData.Length == 0)
                        {
                            this.Close();
                            return;
                        }

                        Array.Resize(ref this.reqBuffer, this.reqBuffer.Length + clientData.Length);
                        Array.Copy(
                            clientData,
                            0,
                            this.reqBuffer,
                            this.reqBuffer.Length - clientData.Length,
                            clientData.Length);
                        this.IsSendingStarted = true;
                        if (this.Controller.Status == ProxyController.ControllerStatus.None)
                        {
                            this.Close();
                        }
                        else if (Http.IsHttp(this.reqBuffer))
                        {
                            Http.Handle(this.reqBuffer, this);
                        }
                        else if (Https.IsHttps(this.reqBuffer))
                        {
                            Https.Handle(this.reqBuffer, this);
                        }
                        else if (Socks5.IsSocks(this.reqBuffer)
                                 && this.Controller.Status.HasFlag(ProxyController.ControllerStatus.Proxy))
                        {
                            Socks5.Handle(this.reqBuffer, this);
                        }
                        else
                        {
                            this.IsSendingStarted = false;
                        }
                    }
                    else if (this.currentTimeout == Math.Min(this.NoDataTimeOut * 1000, 30000)
                             || this.reqBuffer.Length >= 100)
                    {
                        if (this.reqBuffer.Length != 0)
                        {
                            this.Close(
                                "Unknown proxy connection",
                                "Header: " + Encoding.ASCII.GetString(this.reqBuffer),
                                ErrorRenderer.HttpHeaderCode.C501NotImplemented);
                        }
                    }

                    this.currentTimeout++;
                }
                else
                {
                    this.Close();
                }
            }
            catch (Exception e)
            {
                this.Close(e.Message, e.StackTrace);
            }
        }

        /// <summary>
        ///     The close.
        /// </summary>
        /// <param name="title">
        ///     The title.
        /// </param>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="code">
        ///     The code.
        /// </param>
        /// <param name="async">
        ///     The async.
        /// </param>
        /// <param name="sslstream">
        ///     The SSL stream.
        /// </param>
        public void Close(
            string title = null,
            string message = null,
            ErrorRenderer.HttpHeaderCode code = ErrorRenderer.HttpHeaderCode.C500ServerError,
            bool async = false,
            SslStream sslstream = null)
        {
            this.Status = StatusCodes.Closing;
            try
            {
                if (title != null)
                {
                    if (message == null)
                    {
                        message = "No more information.";
                    }

                    this.LastError = title + "\r\n" + message;
                }

                if (this.Client != null)
                {
                    // Testing or not
                    if (this.LastError != string.Empty && title != null)
                    {
                        ProxyController.LogIt(title);
                    }

                    if (title == null
                        || !this.Controller.ErrorRenderer.RenderError(this, title, message, code, sslstream))
                    {
                        if (async)
                        {
                            byte[] db = new byte[0];
                            if (this.Client != null)
                            {
                                this.Client.BeginSend(
                                    db,
                                    0,
                                    db.Length,
                                    SocketFlags.None,
                                    delegate(IAsyncResult ar)
                                        {
                                            try
                                            {
                                                this.Client.Close(); // Close request connection it-self
                                                this.Client.EndSend(ar);
                                            }
                                            catch (Exception)
                                            {
                                            }
                                        },
                                    null);
                            }
                        }
                        else
                        {
                            if (this.Client != null)
                            {
                                this.Client.Close();
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            this.IsClosed = true;
            this.Controller.Disconnected(this);
        }

        /// <summary>
        ///     The get extended info.
        /// </summary>
        /// <returns>
        ///     The <see cref="ConnectionInfo" />.
        /// </returns>
        public ConnectionInfo GetExtendedInfo()
        {
            try
            {
                if (this.extendedInfo != null)
                {
                    return this.extendedInfo;
                }

                if (this.Client != null)
                {
                    if (ClassRegistry.GetClass<ConnectionInfo>().IsSupported())
                    {
                        if (this.Type == ClientType.Tcp)
                        {
                            this.extendedInfo =
                                ClassRegistry.GetClass<ConnectionInfo>()
                                    .GetTcpConnectionByLocalAddress(
                                        ((IPEndPoint)this.Client.RemoteEndPoint).Address,
                                        (ushort)((IPEndPoint)this.Client.RemoteEndPoint).Port);
                        }
                        else if (this.Type == ClientType.Udp)
                        {
                            this.extendedInfo =
                                ClassRegistry.GetClass<ConnectionInfo>()
                                    .GetUdpConnectionByLocalAddress(
                                        ((IPEndPoint)this.Client.RemoteEndPoint).Address,
                                        (ushort)((IPEndPoint)this.Client.RemoteEndPoint).Port);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        /// <summary>
        ///     The read.
        /// </summary>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>byte[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        public byte[] Read()
        {
            try
            {
                int i = 60; // Time out by second

                i = i * 100;
                while (i > 0)
                {
                    if (this.Client.Available > 0)
                    {
                        byte[] buffer = new byte[this.Client.ReceiveBufferSize];
                        int bytes = this.Client.Receive(buffer);
                        Array.Resize(ref buffer, bytes);
                        this.Status = StatusCodes.Routing;
                        this.DataSent(buffer.Length);
                        return buffer;
                    }

                    Thread.Sleep(10);
                    i--;
                }
            }
            catch (Exception)
            {
                this.Close();
            }

            return null;
        }

        /// <summary>
        ///     The write.
        /// </summary>
        /// <param name="bytes">
        ///     The bytes.
        /// </param>
        /// <param name="toStream">
        ///     The to stream.
        /// </param>
        public void Write(byte[] bytes, Stream toStream = null)
        {
            try
            {
                this.IsReceivingStarted = true;
                if (toStream == null)
                {
                    if (bytes != null)
                    {
                        this.Status = StatusCodes.Routing;
                        Array.Resize(ref this.writeBuffer, this.writeBuffer.Length + bytes.Length);
                        Array.Copy(bytes, 0, this.writeBuffer, this.writeBuffer.Length - bytes.Length, bytes.Length);
                    }

                    if (this.writeBuffer.Length > 0 && this.Client.Poll(0, SelectMode.SelectWrite))
                    {
                        int bytesWritten = this.Client.Send(this.writeBuffer, SocketFlags.None);
                        this.DataReceived(this.writeBuffer.Length);
                        Array.Copy(
                            this.writeBuffer,
                            bytesWritten,
                            this.writeBuffer,
                            0,
                            this.writeBuffer.Length - bytesWritten);
                        Array.Resize(ref this.writeBuffer, this.writeBuffer.Length - bytesWritten);
                    }
                }
                else
                {
                    toStream.Write(bytes, 0, bytes.Length);
                    this.DataReceived(bytes.Length);
                }
            }
            catch (Exception e)
            {
                this.Close(e.Message, e.StackTrace);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The data received.
        /// </summary>
        /// <param name="p">
        ///     The p.
        /// </param>
        private void DataReceived(int p)
        {
            this.ReceivedBytes += p;
            if (this.Controller != null)
            {
                this.Controller.ReceivedBytes += p;
            }
        }

        /// <summary>
        ///     The data sent.
        /// </summary>
        /// <param name="p">
        ///     The p.
        /// </param>
        private void DataSent(int p)
        {
            this.SentBytes += p;
            if (this.Controller != null)
            {
                this.Controller.SentBytes += p;
            }
        }

        #endregion
    }
}