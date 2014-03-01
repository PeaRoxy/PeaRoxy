// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PeaRoxy.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The server_ pea roxy.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary.ServerModules
{
    #region

    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Text;

    using global::PeaRoxy.CommonLibrary;

    using global::PeaRoxy.CoreProtocol;

    #endregion

    /// <summary>
    ///     The PeaRoxy server module
    /// </summary>
    public sealed class PeaRoxy : ServerType, IDisposable
    {
        #region Constants

        /// <summary>
        /// The protocol version.
        /// </summary>
        private const byte ProtocolVersion = 1;

        #endregion

        #region Fields

        /// <summary>
        ///     The compression type.
        /// </summary>
        private readonly Common.CompressionType compressionType = Common.CompressionType.None;

        /// <summary>
        ///     The encryption type.
        /// </summary>
        private readonly Common.EncryptionType encryptionType = Common.EncryptionType.None;

        /// <summary>
        ///     The current timeout.
        /// </summary>
        private int currentTimeout;

        /// <summary>
        ///     The address.
        /// </summary>
        private string destinationAddress = string.Empty;

        /// <summary>
        ///     The port.
        /// </summary>
        private ushort destinationPort;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PeaRoxy"/> class.
        /// </summary>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <param name="domain">
        /// The domain.
        /// </param>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <param name="encType">
        /// The encryption type.
        /// </param>
        /// <param name="comType">
        /// The compression type.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Invalid server address
        /// </exception>
        public PeaRoxy(
            string address, 
            ushort port, 
            string domain, 
            string username = "", 
            string password = "", 
            Common.EncryptionType encType = Common.EncryptionType.None, 
            Common.CompressionType comType = Common.CompressionType.None)
        {
            if (string.IsNullOrEmpty(address)) throw new ArgumentException(@"Invalid value.", "address");

            IPAddress ip;
            if (IPAddress.TryParse(address, out ip))
            {
                address = ip.ToString();
            }
            else
            {
                try
                {
                    ip = Dns.GetHostAddresses(address)[0];
                    address = ip.ToString();
                }
                catch
                {
                }
            }

            this.ServerDomain = domain.ToLower().Trim();
            if (this.ServerDomain == string.Empty)
            {
                this.ServerDomain = "~";
            }

            this.IsServerValid = false;
            this.ServerAddress = address;
            this.ServerPort = port;
            this.Username = username;
            this.Password = password;
            this.encryptionType = encType;
            this.compressionType = comType;
            this.NoDataTimeout = 60;
            this.IsDataSent = false;
            this.IsDisconnected = false;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets a value indicating whether is data sent.
        /// </summary>
        public override bool IsDataSent { get; protected set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is disconnected.
        /// </summary>
        public override bool IsDisconnected { get; protected set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is server valid.
        /// </summary>
        public override bool IsServerValid { get; protected set; }

        /// <summary>
        ///     Gets or sets the no data timeout.
        /// </summary>
        public override int NoDataTimeout { get; set; }

        /// <summary>
        ///     Gets or sets the parent client.
        /// </summary>
        public override ProxyClient ParentClient { get; protected set; }

        /// <summary>
        ///     Gets or sets the password of the server
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Gets the underlying protocol object
        /// </summary>
        public PeaRoxyProtocol Protocol { get; private set; }

        /// <summary>
        ///     Gets or sets the address of the server
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        ///     Gets or sets the domain id of the server
        /// </summary>
        public string ServerDomain { get; set; }

        /// <summary>
        ///     Gets or sets the port of the server
        /// </summary>
        public ushort ServerPort { get; set; }

        /// <summary>
        ///     Gets or sets the underlying socket.
        /// </summary>
        public override Socket UnderlyingSocket { get; protected set; }

        /// <summary>
        ///     Gets or sets the username of the server
        /// </summary>
        public string Username { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The clone.
        /// </summary>
        /// <returns>
        ///     The <see cref="ServerType" />.
        /// </returns>
        public override ServerType Clone()
        {
            return new PeaRoxy(
                this.ServerAddress, 
                this.ServerPort, 
                this.ServerDomain, 
                this.Username, 
                this.Password, 
                this.encryptionType, 
                this.compressionType) 
                {
                    NoDataTimeout = this.NoDataTimeout 
                };
        }

        /// <summary>
        ///     The dispose.
        /// </summary>
        public void Dispose()
        {
            this.UnderlyingSocket.Close();
        }

        /// <summary>
        ///     The do route.
        /// </summary>
        public override void DoRoute()
        {
            try
            {
                if ((this.ParentClient.BusyWrite || this.Protocol.BusyWrite
                     || (Common.IsSocketConnected(this.UnderlyingSocket)
                         && Common.IsSocketConnected(this.ParentClient.Client))) && this.currentTimeout > 0)
                {
                    if (this.ParentClient.IsSmartForwarderEnable && this.ParentClient.SmartResponseBuffer.Length > 0
                        && (this.currentTimeout <= this.NoDataTimeout * 500
                            || this.currentTimeout
                            <= ((this.NoDataTimeout
                                 - this.ParentClient.Controller.SmartPear.DetectorHttpResponseBufferTimeout) * 1000))
                        && this.ParentClient.SmartResponseBuffer.Length > 0)
                    {
                        this.currentTimeout = this.NoDataTimeout * 1000;
                        this.ParentClient.IsSmartForwarderEnable = false;
                        this.ParentClient.Write(this.ParentClient.SmartResponseBuffer);
                    }

                    if (!this.ParentClient.BusyWrite && this.Protocol.IsDataAvailable())
                    {
                        this.IsDataSent = true;
                        this.currentTimeout = this.NoDataTimeout * 1000;
                        byte[] buffer = this.Protocol.Read();
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

                    if (!this.Protocol.BusyWrite && this.ParentClient.Client.Available > 0)
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
                                this.Protocol.Write(buffer, true);
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
        /// <param name="sndCallback">
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
            DataCallbackDelegate sndCallback = null, 
            DataCallbackDelegate receiveDataDelegate = null, 
            ConnectionCallbackDelegate connectionStatusCallback = null)
        {
            try
            {
                this.destinationAddress = address;
                this.destinationPort = port;
                this.ParentClient = client;
                this.SendDataDelegate = sndCallback;
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
                                HttpForger forger = new HttpForger(this.UnderlyingSocket);
                                forger.SendRequest(this.ServerDomain);
                                if (!forger.ReceiveResponse())
                                {
                                    this.Close(
                                        "HTTPForger failed to validate server response.", 
                                        null, 
                                        ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                                    return;
                                }

                                this.Protocol = new PeaRoxyProtocol(
                                    this.UnderlyingSocket, 
                                    this.encryptionType, 
                                    this.compressionType)
                                                    {
                                                        ReceivePacketSize = this.ParentClient.ReceivePacketSize, 
                                                        SendPacketSize = this.ParentClient.SendPacketSize, 
                                                        CloseCallback = this.Close
                                                    };

                                byte[] clientRequest;
                                if (this.Username.Trim() != string.Empty && this.Password != string.Empty)
                                {
                                    byte[] username = Encoding.ASCII.GetBytes(this.Username.Trim());
                                    byte[] password = Encoding.ASCII.GetBytes(this.Password);
                                    password = MD5.Create().ComputeHash(password);
                                    if (username.Length > 255)
                                    {
                                        this.Close(
                                            "Username is to long.", 
                                            null, 
                                            ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                                        return;
                                    }

                                    clientRequest = new byte[3 + username.Length + password.Length];
                                    clientRequest[0] = 1; // Password Auth type
                                    clientRequest[1] = (byte)username.Length;
                                    clientRequest[username.Length + 2] = (byte)password.Length;

                                    Array.Copy(username, 0, clientRequest, 2, username.Length);
                                    Array.Copy(password, 0, clientRequest, username.Length + 3, password.Length);
                                }
                                else
                                {
                                    clientRequest = new byte[1];
                                    clientRequest[0] = 0; // No Auth
                                }

                                byte clientAddressType;
                                IPAddress clientIp;
                                byte[] clientAddressBytes;
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

                                byte[] clientRequestAddress =
                                    new byte[6 + clientAddressBytes.Length + ((clientAddressType == 3) ? 1 : 0)];
                                clientRequestAddress[0] = ProtocolVersion;
                                clientRequestAddress[1] = 0;
                                clientRequestAddress[2] = 0;
                                clientRequestAddress[3] = clientAddressType;
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

                                    clientRequestAddress[4] = (byte)clientAddressBytes.Length;
                                }

                                Array.Copy(
                                    clientAddressBytes, 
                                    0, 
                                    clientRequestAddress, 
                                    4 + ((clientAddressType == 3) ? 1 : 0), 
                                    clientAddressBytes.Length);
                                clientRequestAddress[clientRequestAddress.GetUpperBound(0) - 1] =
                                    (byte)Math.Floor(port / 256d);
                                clientRequestAddress[clientRequestAddress.GetUpperBound(0)] = (byte)(port % 256);
                                Array.Resize(ref clientRequest, clientRequestAddress.Length + clientRequest.Length);
                                Array.Copy(
                                    clientRequestAddress, 
                                    0, 
                                    clientRequest, 
                                    clientRequest.Length - clientRequestAddress.Length, 
                                    clientRequestAddress.Length);

                                this.Protocol.Write(clientRequest, false);
                                byte[] serverResponse = this.Protocol.Read();
                                if (serverResponse == null)
                                {
                                    this.Close(
                                        "Connection closed by server or timed out.", 
                                        null, 
                                        ErrorRenderer.HttpHeaderCode.C504GatewayTimeout);
                                    return;
                                }

                                if (serverResponse[0] != clientRequestAddress[0])
                                {
                                    this.Close(
                                        string.Format(
                                            "Server version is different from what we expect. Server's version: {0}, Expected: {1}", 
                                            serverResponse[0], 
                                            clientRequestAddress[0]), 
                                        null, 
                                        ErrorRenderer.HttpHeaderCode.C502BadGateway);
                                    return;
                                }

                                if (serverResponse[1] == 99)
                                {
                                    this.Close(
                                        "Connection failed, Error Code: Auth Failed.", 
                                        null, 
                                        ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                                    return;
                                }

                                if (serverResponse[1] != 0)
                                {
                                    this.Close(
                                        "Connection failed, Error Code: " + serverResponse[1], 
                                        null, 
                                        ErrorRenderer.HttpHeaderCode.C502BadGateway);
                                    return;
                                }

                                if (this.Password != string.Empty)
                                {
                                    this.Protocol.EncryptionKey = Encoding.ASCII.GetBytes(this.Password);
                                }

                                this.IsServerValid = true;
                                if (headerData != null && Common.IsSocketConnected(this.UnderlyingSocket))
                                {
                                    this.Protocol.Write(headerData, true);
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
        ///     The get address.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public override string GetAddress()
        {
            return this.destinationAddress;
        }

        /// <summary>
        ///     The get port.
        /// </summary>
        /// <returns>
        ///     The <see cref="ushort" />.
        /// </returns>
        public override ushort GetPort()
        {
            return this.destinationPort;
        }

        /// <summary>
        ///     The to string.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public override string ToString()
        {
            return "PeaRoxy Module " + this.ServerAddress + ":" + this.ServerPort
                   + ((this.Username != string.Empty) ? " Using USN/PSW" : " Open");
        }

        #endregion

        #region Methods

        /// <summary>
        /// The close.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="async">
        /// The async.
        /// </param>
        private void Close(string title, bool async)
        {
            this.Close(title, null, ErrorRenderer.HttpHeaderCode.C500ServerError, async);
        }

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
                this.IsDisconnected = true;
                if (this.Protocol != null)
                {
                    this.Protocol.Close(title, async);
                }

                if (this.ParentClient != null)
                {
                    this.ParentClient.Close(title, message, code, async);
                }
            }
            catch
            {
            }
        }

        #endregion
    }
}