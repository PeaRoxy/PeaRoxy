// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProxyClient.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary
{
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
    using Socks = PeaRoxy.ClientLibrary.ProxyModules.Socks;

    /// <summary>
    ///     The proxy client object is representation of an incoming connection
    /// </summary>
    public class ProxyClient
    {
        /// <summary>
        ///     Supported Request Types
        /// </summary>
        public enum RequestTypes
        {
            Tcp,

            Udp,

            Dns
        }

        /// <summary>
        ///     The statuses
        /// </summary>
        public enum StatusCodes
        {
            Connected,

            Waiting,

            Routing,

            Closing
        }

        private int currentTimeout;

        private ConnectionInfo extendedInfo;

        private long lastReceivingSpeedCalculationTime;

        private long lastSendingSpeedCalculationTime;

        private long oldReceivedBytes;

        private long oldSentBytes;

        private byte[] reqBuffer = new byte[0];

        private string requestAddress = string.Empty;

        private byte[] writeBuffer = new byte[0];

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProxyClient" /> class.
        /// </summary>
        /// <param name="client">
        ///     The underlaying Net.Sockets.Socket
        /// </param>
        /// <param name="parent">
        ///     The parent ProxyController class
        /// </param>
        public ProxyClient(Socket client, ProxyController parent)
        {
            this.SmartRequestBuffer = new byte[0];
            this.SmartResponseBuffer = new byte[0];
            this.lastSendingSpeedCalculationTime = this.lastReceivingSpeedCalculationTime = Environment.TickCount;
            this.oldSentBytes = this.oldReceivedBytes = this.ReceivedBytes = this.SentBytes = 0;
            if (client != null && client.ProtocolType == ProtocolType.Tcp)
            {
                this.RequestType = RequestTypes.Tcp;
            }
            else if (client != null && client.ProtocolType == ProtocolType.Udp)
            {
                this.RequestType = RequestTypes.Udp;
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
            this.UnderlyingSocket = client;
            if (this.UnderlyingSocket != null)
            {
                this.UnderlyingSocket.Blocking = false;
                this.UnderlyingSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DontFragment, true);
            }
        }

        /// <summary>
        ///     Gets the average receiving speed.
        /// </summary>
        public long AverageReceivingSpeed
        {
            get
            {
                long bytesReceived = this.ReceivedBytes - this.oldReceivedBytes;
                this.oldReceivedBytes = this.ReceivedBytes;
                double timeE = (Environment.TickCount - this.lastReceivingSpeedCalculationTime) / (double)1000;
                if (timeE <= 0)
                {
                    return 0;
                }
                this.lastReceivingSpeedCalculationTime = Environment.TickCount;
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
                double timeE = (Environment.TickCount - this.lastSendingSpeedCalculationTime) / (double)1000;
                if (timeE <= 0)
                {
                    return 0;
                }
                this.lastSendingSpeedCalculationTime = Environment.TickCount;
                return (long)(bytesSent / timeE);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether we are busy writing data to the underlying Socket.
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
        ///     Gets the underlying socket to the client.
        /// </summary>
        public Socket UnderlyingSocket { get; set; }

        /// <summary>
        ///     Gets the active Proxy Controller object.
        /// </summary>
        public ProxyController Controller { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether we had a close request on the underlying connection
        /// </summary>
        public bool IsClosed { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether we received any thing from the other end
        /// </summary>
        public bool IsReceivingStarted { get; internal set; }

        /// <summary>
        ///     Gets a value indicating whether we sent any thing to the other end
        /// </summary>
        public bool IsSendingStarted { get; internal set; }

        /// <summary>
        ///     Gets a value indicating whether we have smart forwarding enabled
        /// </summary>
        public bool IsSmartForwarderEnable { get; internal set; }

        /// <summary>
        ///     Gets the last error.
        /// </summary>
        public string LastError { get; private set; }

        /// <summary>
        ///     Gets or sets the number of seconds after last data transmission to close the connection
        /// </summary>
        public int NoDataTimeOut { get; set; }

        /// <summary>
        ///     Gets or sets the receive packet size.
        /// </summary>
        public int ReceivePacketSize { get; set; }

        /// <summary>
        ///     Gets the bytes received until now.
        /// </summary>
        public long ReceivedBytes { get; private set; }

        /// <summary>
        ///     Gets or sets the requested address to connect which may or may not be a URL
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
                this.IsSmartForwarderEnable = !this.SmartIsNeedForwarding;
            }
        }

        /// <summary>
        ///     Gets or sets the send packet size.
        /// </summary>
        public int SendPacketSize { get; set; }

        /// <summary>
        ///     Gets the bytes sent until now.
        /// </summary>
        public long SentBytes { get; private set; }

        /// <summary>
        ///     Gets the current status of the client.
        /// </summary>
        public StatusCodes Status { get; internal set; }

        /// <summary>
        ///     Gets the request type of the client.
        /// </summary>
        public RequestTypes RequestType { get; internal set; }

        internal byte[] SmartRequestBuffer { get; set; }

        internal byte[] SmartResponseBuffer { get; set; }

        /// <summary>
        ///     Gets the value indicating whether we are still connected to the other end
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return !this.IsClosed && this.UnderlyingSocket != null
                       && Common.IsSocketConnected(this.UnderlyingSocket);
            }
        }

        public bool SmartIsNeedForwarding
        {
            get
            {
                if (this.RequestAddress == string.Empty)
                {
                    return false;
                }
                
                SmartPear smart = this.Controller.SmartPear;
                string p = this.RequestAddress.ToLower();
                if (p.IndexOf("HTTP://", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Uri parsedUrl = new Uri(p);
                    if (smart.ForwarderHttpList.Any(t => Common.DoesMatchWildCard(parsedUrl.Host, t)))
                    {
                        return true;
                    }
                }
                else if (p.IndexOf("SOCKS://", StringComparison.OrdinalIgnoreCase) == 0
                         || p.IndexOf("HTTP://", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    p = p.Substring(p.IndexOf("://", StringComparison.Ordinal) + 3);
                    if (smart.ForwarderDirectList.Any(t => Common.DoesMatchWildCard(p, t)))
                    {
                        return true;
                    }

                    if (smart.ForwarderTreatPort80AsHttp && p.IndexOf(":", StringComparison.Ordinal) != -1
                        && p.Substring(p.IndexOf(":", StringComparison.Ordinal) + 1) == "80")
                    {
                        p = p.Substring(0, p.IndexOf(":", StringComparison.Ordinal));
                        if (smart.ForwarderHttpList.Any(t => Common.DoesMatchWildCard(p, t)))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public bool SmartStatusCallbackForDirectConnections(ServerType currentActiveServer, bool success, bool isSocks)
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
                            Socks.DirectHandle(
                                this,
                                currentActiveServer.GetRequestedAddress(),
                                currentActiveServer.GetRequestedPort(),
                                this.SmartRequestBuffer);
                        }
                        else
                        {
                            Https.DirectHandle(
                                this,
                                currentActiveServer.GetRequestedAddress(),
                                currentActiveServer.GetRequestedPort(),
                                this.SmartRequestBuffer);
                        }

                        return false;
                    }
                }
                else
                {
                    if (smart.ForwarderHttpsEnable && (!isSocks || smart.ForwarderSocksEnable)
                        && smart.DetectorDnsPoisoningEnable && currentActiveServer.UnderlyingSocket != null)
                    {
                        if (currentActiveServer.UnderlyingSocket.RemoteEndPoint != null
                            && smart.DetectorDnsPoisoningRegEx.IsMatch(
                                ((IPEndPoint)currentActiveServer.UnderlyingSocket.RemoteEndPoint).Address.ToString()))
                        {
                            this.IsSmartForwarderEnable = false;
                            if (isSocks)
                            {
                                Socks.DirectHandle(
                                    this,
                                    currentActiveServer.GetRequestedAddress(),
                                    currentActiveServer.GetRequestedPort(),
                                    this.SmartRequestBuffer);
                            }
                            else
                            {
                                Https.DirectHandle(
                                    this,
                                    currentActiveServer.GetRequestedAddress(),
                                    currentActiveServer.GetRequestedPort(),
                                    this.SmartRequestBuffer);
                            }

                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public bool SmartDataReceivedCallbackForDirrectConnections(
            ref byte[] binary,
            ServerType currentActiveServer,
            bool isSocks)
        {
            SmartPear smart = this.Controller.SmartPear;
            if (smart.ForwarderHttpsEnable && (!isSocks || smart.ForwarderSocksEnable))
            {
                // If HTTPS Forwarded
                if (!this.IsSmartForwarderEnable
                    && ((smart.DetectorTreatPort80AsHttp && smart.ForwarderTreatPort80AsHttp)
                        || smart.DetectorStatusDnsPoisoning || smart.DetectorStatusTimeout))
                {
                    // If using proxy, and forwarder HTTPS is enable
                    Uri url;
                    if (this.RequestAddress != string.Empty
                        && Uri.TryCreate(this.RequestAddress, UriKind.Absolute, out url))
                    {
                        if (smart.DetectorTreatPort80AsHttp && smart.ForwarderTreatPort80AsHttp && url.Port == 80
                            && Http.IsHttp(this.SmartRequestBuffer))
                        {
                            smart.AddRuleToHttpForwarder(
                                string.Format("*{0}*", url.Host.ToLower().TrimEnd(new[] { '/', '\\' })));
                        }
                        else
                        {
                            smart.AddRuleToDirectForwarder(string.Format("*{0}:{1}", url.Host.ToLower(), url.Port));
                        }
                    }

                    currentActiveServer.ReceiveDataDelegate = null;
                    return true;
                }

                if (smart.ForwarderTreatPort80AsHttp && smart.DetectorTreatPort80AsHttp
                    && this.RequestAddress.EndsWith(":80") && Http.IsHttp(this.SmartRequestBuffer))
                {
                    // If we have Forwarder Enabled
                    bool blocked = false;
                    if (smart.DetectorStatusHttp && this.SmartResponseBuffer.Length < smart.DetectorHttpMaxBuffering)
                    {
                        // If detector is enable and response is less than buffer size
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
                            // If response is blocked
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
                            this.SmartCleanTheForwarder();
                            this.IsSmartForwarderEnable = false;
                            Http.DirectHandle(
                                this,
                                currentActiveServer.GetRequestedAddress(),
                                currentActiveServer.GetRequestedPort(),
                                localReqBackup);
                            return false;
                        }
                    }
                    else
                    {
                        // Response is OK
                        if (!this.IsSmartForwarderEnable
                            && (smart.DetectorStatusHttp || smart.DetectorStatusDnsPoisoning
                                || smart.DetectorStatusTimeout))
                        {
                            // If client use Proxy and one of possible detectors is enable
                            Uri url;
                            if (this.RequestAddress != string.Empty
                                && Uri.TryCreate(this.RequestAddress, UriKind.Absolute, out url))
                            {
                                smart.AddRuleToHttpForwarder(
                                    string.Format("*{0}*", url.Host.ToLower().TrimEnd(new[] { '/', '\\' })));
                            }

                            currentActiveServer.ReceiveDataDelegate = null;
                        }
                    }

                    if (this.SmartResponseBuffer.Length > 0
                        && (!this.IsSmartForwarderEnable
                            || this.SmartResponseBuffer.Length >= smart.DetectorHttpMaxBuffering))
                    {
                        // If we have any thing in Response and (Client use Proxy or Client use NoServer but Response buffer is bigger than buffer)
                        this.SmartFlushTheForwarder(smart.ForwarderHttpEnable);
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

        public bool SmartDataSentCallbackForDirectConnections(byte[] binary, bool isSocks)
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
                this.SmartFlushTheForwarder(smart.ForwarderHttpsEnable && (!isSocks || smart.ForwarderSocksEnable));
            }

            return true;
        }

        public void SmartCleanTheForwarder(bool? enable = null)
        {
            this.IsSmartForwarderEnable = (enable == null)
                                              ? this.Controller.SmartPear.ForwarderHttpEnable
                                              : (bool)enable;
            this.SmartResponseBuffer = new byte[0];
            this.SmartRequestBuffer = new byte[0];
        }

        public void SmartFlushTheForwarder(bool enable)
        {
            this.Write(this.SmartResponseBuffer);
            this.SmartCleanTheForwarder(enable);
            this.IsSmartForwarderEnable = false;
        }

        public bool SmartStatusCallbackForHttpConnections(ServerType currentActiveServer, bool success)
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
                            currentActiveServer.GetRequestedAddress(),
                            currentActiveServer.GetRequestedPort(),
                            this.SmartRequestBuffer);
                        return false;
                    }
                }
                else
                {
                    if (smart.ForwarderHttpEnable && smart.DetectorDnsPoisoningEnable
                        && currentActiveServer.UnderlyingSocket != null)
                    {
                        if (currentActiveServer.UnderlyingSocket.RemoteEndPoint != null
                            && smart.DetectorDnsPoisoningRegEx.IsMatch(
                                ((IPEndPoint)currentActiveServer.UnderlyingSocket.RemoteEndPoint).Address.ToString()))
                        {
                            this.IsSmartForwarderEnable = false;
                            Http.DirectHandle(
                                this,
                                currentActiveServer.GetRequestedAddress(),
                                currentActiveServer.GetRequestedPort(),
                                this.SmartRequestBuffer);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public bool SmartDataReceivedCallbackForHttpConnections(ref byte[] binary, ServerType currentActiveServer)
        {
            SmartPear smart = this.Controller.SmartPear;
            if (smart.ForwarderHttpEnable)
            {
                // If we have Forwarder Enabled
                bool blocked = false;
                if (smart.DetectorStatusHttp && this.SmartResponseBuffer.Length < smart.DetectorHttpMaxBuffering)
                {
                    // If detector is enable and response is less than buffer size
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
                        // If Response is blocked
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
                        this.SmartCleanTheForwarder();
                        this.IsSmartForwarderEnable = false;
                        Http.DirectHandle(
                            this,
                            currentActiveServer.GetRequestedAddress(),
                            currentActiveServer.GetRequestedPort(),
                            localReqBackup);
                        return false;
                    }
                }
                else
                {
                    // Response is OK
                    if (!this.IsSmartForwarderEnable
                        && (smart.DetectorStatusHttp || smart.DetectorStatusDnsPoisoning || smart.DetectorStatusTimeout))
                    {
                        // If client use Proxy and one of possible detectors is enable
                        Uri url;
                        if (this.RequestAddress != string.Empty
                            && Uri.TryCreate(this.RequestAddress, UriKind.Absolute, out url))
                        {
                            smart.AddRuleToHttpForwarder(
                                string.Format("*{0}*", url.Host.ToLower().TrimEnd(new[] { '/', '\\' })));
                        }

                        currentActiveServer.ReceiveDataDelegate = null;
                    }
                }

                if (this.SmartResponseBuffer.Length > 0
                    && (!this.IsSmartForwarderEnable
                        || this.SmartResponseBuffer.Length >= smart.DetectorHttpMaxBuffering))
                {
                    // If we have any thing in Response and (Client use Proxy or Client use NoServer but Response buffer is bigger than buffer)
                    this.SmartFlushTheForwarder(smart.ForwarderHttpEnable);
                    currentActiveServer.ReceiveDataDelegate = null;
                }
            }

            return true;
        }

        public bool SmartDataSentCallbackForHttpConnections(byte[] binary)
        {
            SmartPear smart = this.Controller.SmartPear;
            if (smart.ForwarderHttpEnable && (smart.DetectorHttpCheckEnable || smart.DetectorDnsPoisoningEnable))
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
                    // If client use Proxy and there is a response already.
                    this.SmartFlushTheForwarder(smart.ForwarderHttpEnable);
                }
            }

            return true;
        }

        internal void Accepting()
        {
            try
            {
                if (Common.IsSocketConnected(this.UnderlyingSocket))
                {
                    if (this.IsSendingStarted)
                    {
                        return;
                    }
                    if (this.UnderlyingSocket.Available > 0)
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
                        else if (Socks.IsSocks(this.reqBuffer)
                                 && this.Controller.Status.HasFlag(ProxyController.ControllerStatus.Proxy))
                        {
                            Socks.Handle(this.reqBuffer, this);
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
        ///     The close method which supports mentioning a message about the reason
        /// </summary>
        /// <param name="title">
        ///     The title.
        /// </param>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="code">
        ///     The HTTP code.
        /// </param>
        /// <param name="async">
        ///     Indicating if the closing process should treat the client as an asynchronous client
        /// </param>
        /// <param name="sslstream">
        ///     The SSL stream if any.
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

                if (this.UnderlyingSocket != null)
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
                            if (this.UnderlyingSocket != null)
                            {
                                this.UnderlyingSocket.BeginSend(
                                    db,
                                    0,
                                    db.Length,
                                    SocketFlags.None,
                                    delegate(IAsyncResult ar)
                                        {
                                            try
                                            {
                                                this.UnderlyingSocket.Close(); // Close request connection it-self
                                                this.UnderlyingSocket.EndSend(ar);
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
                            if (this.UnderlyingSocket != null)
                            {
                                this.UnderlyingSocket.Close();
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            this.IsClosed = true;
            this.Controller.ClientDisconnected(this);
        }

        /// <summary>
        ///     The get extended info method is used to query any information about the program that made the request.
        /// </summary>
        /// <returns>
        ///     The <see cref="ConnectionInfo" /> object.
        /// </returns>
        public ConnectionInfo GetExtendedInfo()
        {
            try
            {
                if (this.extendedInfo != null)
                {
                    return this.extendedInfo;
                }

                if (this.UnderlyingSocket != null)
                {
                    if (ClassRegistry.GetClass<ConnectionInfo>().IsSupported())
                    {
                        if (this.RequestType == RequestTypes.Tcp)
                        {
                            this.extendedInfo =
                                ClassRegistry.GetClass<ConnectionInfo>()
                                    .GetTcpConnectionByLocalAddress(
                                        ((IPEndPoint)this.UnderlyingSocket.RemoteEndPoint).Address,
                                        (ushort)((IPEndPoint)this.UnderlyingSocket.RemoteEndPoint).Port);
                        }
                        else if (this.RequestType == RequestTypes.Udp)
                        {
                            this.extendedInfo =
                                ClassRegistry.GetClass<ConnectionInfo>()
                                    .GetUdpConnectionByLocalAddress(
                                        ((IPEndPoint)this.UnderlyingSocket.RemoteEndPoint).Address,
                                        (ushort)((IPEndPoint)this.UnderlyingSocket.RemoteEndPoint).Port);
                        }
                        if (this.extendedInfo != null)
                        {
                            return this.extendedInfo;
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
        ///     The read method to read the data from the other end
        /// </summary>
        /// <returns>
        ///     Received data in form of
        ///     <see>
        ///         <cref>byte[]</cref>
        ///     </see>
        /// </returns>
        public byte[] Read()
        {
            try
            {
                int i = 60; // Time out by second

                i = i * 100;
                while (i > 0)
                {
                    if (this.UnderlyingSocket.Available > 0)
                    {
                        byte[] buffer = new byte[this.UnderlyingSocket.ReceiveBufferSize];
                        int bytes = this.UnderlyingSocket.Receive(buffer);
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
        ///     The write method to write the data to the other end.
        /// </summary>
        /// <param name="bytes">
        ///     The data to write.
        /// </param>
        /// <param name="toStream">
        ///     A Stream to write to instead of directly talking with other end
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

                    if (this.writeBuffer.Length > 0 && this.UnderlyingSocket.Poll(0, SelectMode.SelectWrite))
                    {
                        int bytesWritten = this.UnderlyingSocket.Send(this.writeBuffer, SocketFlags.None);
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
                if (e is ObjectDisposedException)
                {
                    this.Close();
                }
                else
                {
                    this.Close(e.Message, e.StackTrace);
                }
            }
        }

        private void DataReceived(int p)
        {
            this.ReceivedBytes += p;
            if (this.Controller != null)
            {
                this.Controller.ReceivedBytes += p;
            }
        }

        private void DataSent(int p)
        {
            this.SentBytes += p;
            if (this.Controller != null)
            {
                this.Controller.SentBytes += p;
            }
        }
    }
}