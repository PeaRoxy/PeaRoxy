// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Socks5.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The Socks 5 server handler
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary.ServerModules
{
    #region

    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    using global::PeaRoxy.CommonLibrary;

    #endregion

    /// <summary>
    /// The Socks 5 server module
    /// </summary>
    public sealed class Socks5 : ServerType, IDisposable
    {
        /// <summary>
        /// The protocol version
        /// </summary>
        public const byte ProtocolVersion = 5;

        #region Fields

        /// <summary>
        /// The address of the requested connection
        /// </summary>
        private string destinationAddress = string.Empty;

        /// <summary>
        /// The current timeout.
        /// </summary>
        private int currentTimeout;

        /// <summary>
        /// The port of the requested connected
        /// </summary>
        private ushort destinationPort;

        /// <summary>
        /// The write buffer.
        /// </summary>
        private byte[] writeBuffer = new byte[0];

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Socks5"/> class.
        /// </summary>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Invalid or Empty address
        /// </exception>
        public Socks5(string address, ushort port, string username = "", string password = "")
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException(@"Invalid value.", "address");

            this.IsServerValid = false;
            this.ServerAddress = address;
            this.ServerPort = port;
            this.Username = username;
            this.Password = password;
            this.NoDataTimeout = 60;
            this.IsDataSent = false;
            this.IsDisconnected = false;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the password of the server
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the address of the server
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        /// Gets or sets the port of the server
        /// </summary>
        public ushort ServerPort { get; set; }

        /// <summary>
        /// Gets or sets the username of the server
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets a value indicating whether we are busy writing
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
        /// Gets or sets a value indicating whether data is sent
        /// </summary>
        public override bool IsDataSent { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether we are disconnected
        /// </summary>
        public override bool IsDisconnected { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether server is valid
        /// </summary>
        public override bool IsServerValid { get; protected set; }

        /// <summary>
        /// Gets or sets the no data timeout.
        /// </summary>
        public override int NoDataTimeout { get; set; }

        /// <summary>
        /// Gets or sets the parent client.
        /// </summary>
        public override ProxyClient ParentClient { get; protected set; }

        /// <summary>
        /// Gets or sets the underlying socket.
        /// </summary>
        public override Socket UnderlyingSocket { get; protected set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The clone.
        /// </summary>
        /// <returns>
        /// The <see cref="ServerType"/>.
        /// </returns>
        public override ServerType Clone()
        {
            return new Socks5(this.ServerAddress, this.ServerPort, this.Username, this.Password)
                       {
                           NoDataTimeout
                               =
                               this
                               .NoDataTimeout
                       };
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            if (this.UnderlyingSocket != null)
            {
                this.UnderlyingSocket.Dispose();
            }
        }

        /// <summary>
        /// The do route.
        /// </summary>
        public override void DoRoute()
        {
            try
            {
                if ((this.ParentClient.BusyWrite || this.BusyWrite
                     || (Common.IsSocketConnected(this.UnderlyingSocket)
                         && Common.IsSocketConnected(this.ParentClient.Client))) && this.currentTimeout > 0)
                {
                    if (this.ParentClient.IsSmartForwarderEnable && this.ParentClient.SmartResponseBuffer.Length > 0
                        && (this.currentTimeout <= this.NoDataTimeout * 500
                            || this.currentTimeout
                            <= ((this.NoDataTimeout
                                 - this.ParentClient.Controller.SmartPear.DetectorHttpResponseBufferTimeout) * 1000)))
                    {
                        this.currentTimeout = this.NoDataTimeout * 1000;
                        this.ParentClient.IsSmartForwarderEnable = false;
                        this.ParentClient.Write(this.ParentClient.SmartResponseBuffer);
                    }

                    if (!this.ParentClient.BusyWrite && this.UnderlyingSocket.Available > 0)
                    {
                        this.IsDataSent = true;
                        this.currentTimeout = this.NoDataTimeout * 1000;
                        byte[] buffer = this.Read();
                        if (buffer != null && buffer.Length > 0)
                        {
                            if (this.ReceiveDataDelegate != null
                                && this.ReceiveDataDelegate.Invoke(ref buffer, this, this.ParentClient) == false)
                            {
                                this.ParentClient = null;
                                this.Close();
                                return;
                            }

                            if (buffer.Length > 0)
                            {
                                this.ParentClient.Write(buffer);
                            }
                        }
                    }

                    if (!this.BusyWrite && this.ParentClient.Client.Available > 0)
                    {
                        this.IsDataSent = true;
                        this.currentTimeout = this.NoDataTimeout * 1000;
                        byte[] buffer = this.ParentClient.Read();
                        if (buffer != null && buffer.Length > 0)
                        {
                            if (this.SendDataDelegate != null
                                && this.SendDataDelegate.Invoke(ref buffer, this, this.ParentClient) == false)
                            {
                                this.ParentClient = null;
                                this.Close();
                                return;
                            }

                            if (buffer.Length > 0)
                            {
                                this.Write(buffer);
                            }
                        }
                    }

                    this.currentTimeout--;
                }
                else
                {
                    if (this.IsDataSent == false && this.ConnectionStatusCallback != null
                        && this.ConnectionStatusCallback.Invoke(false, this, this.ParentClient) == false)
                    {
                        this.ParentClient = null;
                    }

                    this.Close(false);
                }
            }
            catch (Exception e)
            {
                if (e.TargetSite.Name == "Receive")
                {
                    this.Close();
                }
                else
                {
                    this.Close(e.Message, e.StackTrace);
                }
            }
        }

        /// <summary>
        /// The establish.
        /// </summary>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="headerData">
        /// The header data.
        /// </param>
        /// <param name="sendDataDelegate">
        /// The send callback.
        /// </param>
        /// <param name="receiveDataDelegate">
        /// The receive callback.
        /// </param>
        /// <param name="connectionStatusCallback">
        /// The connection status callback.
        /// </param>
        public override void Establish(
            string address, 
            ushort port, 
            ProxyClient client, 
            byte[] headerData = null, 
            DataCallbackDelegate sendDataDelegate = null, 
            DataCallbackDelegate receiveDataDelegate = null,
            ConnectionCallbackDelegate connectionStatusCallback = null)
        {
            try
            {
                this.destinationAddress = address;
                this.destinationPort = port;
                this.ParentClient = client;
                this.SendDataDelegate = sendDataDelegate;
                this.ReceiveDataDelegate = receiveDataDelegate;
                this.ConnectionStatusCallback = connectionStatusCallback;
                this.UnderlyingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.UnderlyingSocket.BeginConnect(
                    this.ServerAddress, 
                    this.ServerPort, 
                    delegate(IAsyncResult ar)
                        {
                            try
                            {
                                this.UnderlyingSocket.EndConnect(ar);
                                this.ParentClient.Controller.FailAttempts = 0;
                                this.UnderlyingSocket.Blocking = false;
                                byte[] clientRequest = new byte[0];
                                if (this.Username.Trim() != string.Empty && this.Password != string.Empty)
                                {
                                    Array.Resize(ref clientRequest, 4);
                                    clientRequest[0] = ProtocolVersion; // version
                                    clientRequest[1] = 2; // 2 auth method
                                    clientRequest[2] = 0; // No auth
                                    clientRequest[3] = 2; // User and pass
                                }
                                else
                                {
                                    Array.Resize(ref clientRequest, 3);
                                    clientRequest[0] = ProtocolVersion; // version
                                    clientRequest[1] = 1; // 1 auth method
                                    clientRequest[2] = 0; // No auth
                                }

                                this.Write(clientRequest, false);
                                byte[] serverResponse = this.Read();
                                if (serverResponse == null || serverResponse.Length < 2)
                                {
                                    this.Close(
                                        "Connection timeout.", 
                                        null, 
                                        ErrorRenderer.HttpHeaderCode.C504GatewayTimeout);
                                    return;
                                }

                                if (serverResponse[0] != ProtocolVersion)
                                {
                                    this.Close(
                                        "Unsupported version of proxy.", 
                                        null, 
                                        ErrorRenderer.HttpHeaderCode.C502BadGateway);
                                    return;
                                }

                                if ((serverResponse[1] != 0 && serverResponse[1] != 2)
                                    || (serverResponse[1] == 2
                                        && !(this.Username.Trim() != string.Empty && this.Password != string.Empty)))
                                {
                                    this.Close(
                                        "Unsupported authentication method.", 
                                        null, 
                                        ErrorRenderer.HttpHeaderCode.C502BadGateway);
                                    return;
                                }

                                if (serverResponse[1] == 2 && this.Username.Trim() != string.Empty
                                    && this.Password != string.Empty)
                                {
                                    byte[] username = Encoding.ASCII.GetBytes(this.Username.Trim());
                                    byte[] password = Encoding.ASCII.GetBytes(this.Password);
                                    if (username.Length > byte.MaxValue)
                                    {
                                        this.Close(
                                            "Username is to long.", 
                                            null, 
                                            ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                                        return;
                                    }

                                    Array.Resize(ref clientRequest, username.Length + password.Length + 3);
                                    clientRequest[0] = 1; // Auth version
                                    clientRequest[1] = (byte)username.Length;
                                    clientRequest[username.Length + 2] = (byte)password.Length;
                                    Array.Copy(username, 0, clientRequest, 2, username.Length);
                                    Array.Copy(password, 0, clientRequest, username.Length + 3, password.Length);
                                    this.Write(clientRequest, false);
                                    serverResponse = this.Read();
                                    if (serverResponse == null || serverResponse.Length < 2)
                                    {
                                        this.Close(
                                            "Connection timeout.", 
                                            null, 
                                            ErrorRenderer.HttpHeaderCode.C504GatewayTimeout);
                                        return;
                                    }

                                    if (serverResponse[0] != 1)
                                    {
                                        this.Close(
                                            "Unsupported version of proxy's user/pass authentication method.", 
                                            null, 
                                            ErrorRenderer.HttpHeaderCode.C502BadGateway);
                                        return;
                                    }

                                    if (serverResponse[1] != 0)
                                    {
                                        this.Close(
                                            "Authentication failed.", 
                                            null, 
                                            ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                                        return;
                                    }
                                }

                                IPAddress clientIp;
                                byte[] clientAddressBytes;
                                byte clientAddressType;
                                if (IPAddress.TryParse(address, out clientIp))
                                {
                                    clientAddressBytes = clientIp.GetAddressBytes();
                                    if (clientAddressBytes.Length == 16)
                                    {
                                        clientAddressType = 4;
                                    }
                                    else if (clientAddressBytes.Length == 4)
                                    {
                                        clientAddressType = 1;
                                    }
                                    else
                                    {
                                        this.Close(
                                            "Unknown IP Type.", 
                                            null, 
                                            ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                                        return;
                                    }
                                }
                                else
                                {
                                    clientAddressType = 3;
                                    clientAddressBytes = Encoding.ASCII.GetBytes(address);
                                }

                                clientRequest =
                                    new byte[6 + clientAddressBytes.Length + ((clientAddressType == 3) ? 1 : 0)];
                                clientRequest[0] = ProtocolVersion;
                                clientRequest[1] = 1;
                                clientRequest[2] = 0;
                                clientRequest[3] = clientAddressType;
                                if (clientAddressType == 3)
                                {
                                    if (clientAddressBytes.Length > 255)
                                    {
                                        this.Close(
                                            "Hostname is too long.", 
                                            null, 
                                            ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                                        return;
                                    }

                                    clientRequest[4] = (byte)clientAddressBytes.Length;
                                }

                                Array.Copy(
                                    clientAddressBytes, 
                                    0, 
                                    clientRequest, 
                                    4 + ((clientAddressType == 3) ? 1 : 0), 
                                    clientAddressBytes.Length);
                                clientRequest[clientRequest.GetUpperBound(0) - 1] = (byte)Math.Floor(port / 256d);
                                clientRequest[clientRequest.GetUpperBound(0)] = (byte)(port % 256);

                                this.Write(clientRequest, false);
                                serverResponse = this.Read();
                                if (serverResponse == null)
                                {
                                    this.Close(
                                        "Connection timeout.", 
                                        null, 
                                        ErrorRenderer.HttpHeaderCode.C504GatewayTimeout);
                                    return;
                                }

                                if (serverResponse[0] != clientRequest[0])
                                {
                                    this.Close(
                                        string.Format("Server version is different from what we expect. Server's version: {0}, Expected: {1}", serverResponse[0], clientRequest[0]), 
                                        null, 
                                        ErrorRenderer.HttpHeaderCode.C502BadGateway);
                                    return;
                                }

                                if (serverResponse[1] != 0)
                                {
                                    switch (serverResponse[1])
                                    {
                                        case 1:
                                            this.Close(
                                                "Connection failed, Error Code: " + serverResponse[1], 
                                                null, 
                                                ErrorRenderer.HttpHeaderCode.C502BadGateway);
                                            break;
                                        case 2:
                                            this.Close(
                                                "SOCKS Error Message: General failure.", 
                                                null, 
                                                ErrorRenderer.HttpHeaderCode.C502BadGateway);
                                            break;
                                        case 3:
                                            this.Close(
                                                "SOCKS Error Message: Connection not allowed by rule set.", 
                                                null, 
                                                ErrorRenderer.HttpHeaderCode.C502BadGateway);
                                            break;
                                        case 4:
                                            this.Close(
                                                "SOCKS Error Message: Network unreachable.", 
                                                null, 
                                                ErrorRenderer.HttpHeaderCode.C502BadGateway);
                                            break;
                                        case 5:
                                            this.Close(
                                                "SOCKS Error Message: Connection refused by destination host.", 
                                                null, 
                                                ErrorRenderer.HttpHeaderCode.C502BadGateway);
                                            break;
                                        case 6:
                                            this.Close(
                                                "SOCKS Error Message: TTL expired.", 
                                                null, 
                                                ErrorRenderer.HttpHeaderCode.C502BadGateway);
                                            break;
                                        case 7:
                                            this.Close(
                                                "SOCKS Error Message: Command not supported / protocol error.", 
                                                null, 
                                                ErrorRenderer.HttpHeaderCode.C502BadGateway);
                                            break;
                                        case 8:
                                            this.Close(
                                                "SOCKS Error Message: Address type not supported.", 
                                                null, 
                                                ErrorRenderer.HttpHeaderCode.C502BadGateway);
                                            break;
                                        default:
                                            this.Close(
                                                "Connection failed, Error Code: " + serverResponse[1], 
                                                null, 
                                                ErrorRenderer.HttpHeaderCode.C502BadGateway);
                                            break;
                                    }

                                    return;
                                }

                                this.IsServerValid = true;
                                if (headerData != null && Common.IsSocketConnected(this.UnderlyingSocket))
                                {
                                    this.Write(headerData);
                                }

                                this.currentTimeout = this.NoDataTimeout * 1000;
                                client.Controller.MoveToRouting(this);
                            }
                            catch (Exception ex)
                            {
                                this.ParentClient.Controller.FailAttempts++;
                                this.Close(
                                    ex.Message, 
                                    ex.StackTrace, 
                                    ErrorRenderer.HttpHeaderCode.C504GatewayTimeout);
                            }
                        }, 
                    null);
            }
            catch (Exception e)
            {
                this.Close(e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// The get address.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string GetAddress()
        {
            return this.destinationAddress;
        }

        /// <summary>
        /// The get port.
        /// </summary>
        /// <returns>
        /// The <see cref="ushort"/>.
        /// </returns>
        public override ushort GetPort()
        {
            return this.destinationPort;
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return "Socks5 Module " + this.ServerAddress + ":" + this.ServerPort
                   + ((this.Username != string.Empty) ? " Using USN/PSW" : " Open");
        }

        #endregion

        #region Methods

        /// <summary>
        /// The close.
        /// </summary>
        /// <param name="async">
        /// The async.
        /// </param>
        private void Close(bool async)
        {
            this.Close(null, null, ErrorRenderer.HttpHeaderCode.C500ServerError, async);
        }

        /// <summary>
        /// The close.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="code">
        /// The code.
        /// </param>
        /// <param name="async">
        /// The async.
        /// </param>
        private void Close(
            string title = null, 
            string message = null, 
            ErrorRenderer.HttpHeaderCode code = ErrorRenderer.HttpHeaderCode.C500ServerError, 
            bool async = false)
        {
            try
            {
                if (this.ParentClient != null)
                {
                    this.ParentClient.Close(title, message, code, async);
                }

                this.IsDisconnected = true;

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
                                    catch
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
            catch
            {
            }
        }

        /// <summary>
        /// The read.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>byte[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        private byte[] Read()
        {
            try
            {
                int i = 60; // Time out by second

                i = i * 100;
                while (i > 0)
                {
                    if (this.UnderlyingSocket.Available > 0)
                    {
                        byte[] buffer = new byte[this.ParentClient.ReceivePacketSize];
                        int bytes = this.UnderlyingSocket.Receive(buffer);
                        Array.Resize(ref buffer, bytes);
                        return buffer;
                    }

                    Thread.Sleep(10);
                    i--;
                }
            }
            catch (Exception e)
            {
                this.Close(e.Message, e.StackTrace);
            }

            return null;
        }

        /// <summary>
        /// The write.
        /// </summary>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        /// <param name="async">
        /// The async.
        /// </param>
        private void Write(byte[] bytes, bool async = true)
        {
            try
            {
                if (bytes != null)
                {
                    Array.Resize(ref this.writeBuffer, this.writeBuffer.Length + bytes.Length);
                    Array.Copy(bytes, 0, this.writeBuffer, this.writeBuffer.Length - bytes.Length, bytes.Length);
                }

                if (this.writeBuffer.Length > 0 && this.UnderlyingSocket.Poll(0, SelectMode.SelectWrite))
                {
                    int bytesWritten = this.UnderlyingSocket.Send(this.writeBuffer, SocketFlags.None);
                    Array.Copy(
                        this.writeBuffer, 
                        bytesWritten, 
                        this.writeBuffer, 
                        0, 
                        this.writeBuffer.Length - bytesWritten);
                    Array.Resize(ref this.writeBuffer, this.writeBuffer.Length - bytesWritten);
                }

                if (!async)
                {
                    int i = 60; // Time out by second
                    i = i * 100;
                    while (i > 0 && this.writeBuffer.Length > 0)
                    {
                        int wbl = this.writeBuffer.Length;
                        this.Write(null);
                        if (this.writeBuffer.Length == wbl)
                        {
                            Thread.Sleep(10);
                            i--;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.Close(e.Message, e.StackTrace);
            }
        }

        #endregion
    }
}