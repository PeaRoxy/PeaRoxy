﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PeaRoxy.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary.ServerModules
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Text;

    using global::PeaRoxy.CommonLibrary;
    using global::PeaRoxy.CoreProtocol;

    /// <summary>
    ///     The PeaRoxy Server Module
    /// </summary>
    public sealed class PeaRoxy : ServerType, IDisposable
    {
        private const byte ProtocolVersion = 1;

        private readonly Common.CompressionTypes compressionTypes = Common.CompressionTypes.None;

        private readonly Common.EncryptionTypes encryptionType = Common.EncryptionTypes.None;

        private int currentTimeout;

        private string requestedAddress = string.Empty;

        private ushort requestedPort;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PeaRoxy" /> class.
        /// </summary>
        /// <param name="address">
        ///     The server address.
        /// </param>
        /// <param name="port">
        ///     The server port.
        /// </param>
        /// <param name="domain">
        ///     The server domain identifier.
        /// </param>
        /// <param name="username">
        ///     The user name to authenticate, leave empty for no authentication.
        /// </param>
        /// <param name="password">
        ///     The password to authenticate, leave empty for no authentication.
        /// </param>
        /// <param name="encType">
        ///     The client encryption type.
        /// </param>
        /// <param name="comTypes">
        ///     The client compression type.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     Invalid server address, port number, encryption or compression type
        /// </exception>
        public PeaRoxy(
            string address,
            ushort port,
            string domain,
            string username = "",
            string password = "",
            Common.EncryptionTypes encType = Common.EncryptionTypes.None,
            Common.CompressionTypes comTypes = Common.CompressionTypes.None)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentException(@"Invalid value.", "address");
            }

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

            this.IsServerValid = false;
            this.ServerAddress = address;
            this.ServerPort = port;
            this.Username = username;
            this.Password = password;
            this.encryptionType = encType;
            this.compressionTypes = comTypes;
            this.NoDataTimeout = 60;
            this.IsDataSent = false;
            this.IsClosed = false;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether we had sent some data to the server
        /// </summary>
        public override bool IsDataSent { get; protected set; }

        /// <summary>
        ///     Gets or sets a value indicating whether we had a close request on the underlying connection
        /// </summary>
        public override bool IsClosed { get; protected set; }

        /// <summary>
        ///     Gets or sets a value indicating whether server exists and is valid.
        /// </summary>
        public override bool IsServerValid { get; protected set; }

        /// <summary>
        ///     Gets or sets a value indicating whether we should use compatibility mode with http forger
        /// </summary>
        public bool ForgerCompatibility { get; set; }


        /// <summary>
        ///     Gets or sets the number of seconds after last data transmission to close the connection
        /// </summary>
        public override int NoDataTimeout { get; set; }

        /// <summary>
        ///     Gets the parent client.
        /// </summary>
        public override ProxyClient ParentClient { get; protected set; }

        /// <summary>
        ///     Gets or sets the password of the server
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Gets the underlying PeaRoxy protocol object
        /// </summary>
        public PeaRoxyProtocol Protocol { get; private set; }

        /// <summary>
        ///     Gets or sets the address of the server
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        ///     Gets or sets the domain identifier of the server
        /// </summary>
        public string ServerDomain { get; set; }

        /// <summary>
        ///     Gets or sets the port number of the server
        /// </summary>
        public ushort ServerPort { get; set; }

        /// <summary>
        ///     Gets the underlying socket to the server.
        /// </summary>
        public override Socket UnderlyingSocket { get; protected set; }

        /// <summary>
        ///     Gets or sets the user name of the server
        /// </summary>
        public string Username { get; set; }

        public void Dispose()
        {
            this.UnderlyingSocket.Close();
        }

        /// <summary>
        ///     The clone method is to make another copy of this class with exactly same settings
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
                this.compressionTypes) { NoDataTimeout = this.NoDataTimeout, ForgerCompatibility = this.ForgerCompatibility};
        }

        /// <summary>
        ///     The method to handle the routing process, should call repeatedly
        /// </summary>
        public override void Route()
        {
            try
            {
                if ((this.ParentClient.BusyWrite || this.Protocol.BusyWrite
                     || (Common.IsSocketConnected(this.UnderlyingSocket) && this.ParentClient.UnderlyingSocket != null
                         && Common.IsSocketConnected(this.ParentClient.UnderlyingSocket))) && this.currentTimeout > 0)
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

                    if (!this.Protocol.BusyWrite && this.ParentClient.UnderlyingSocket.Available > 0)
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

        /// <summary>
        ///     The establish method is used to connect to the server and try to request access to the specific address.
        /// </summary>
        /// <param name="address">
        ///     The requested address.
        /// </param>
        /// <param name="port">
        ///     The requested port.
        /// </param>
        /// <param name="client">
        ///     The client to connect with.
        /// </param>
        /// <param name="headerData">
        ///     The data to send after connecting.
        /// </param>
        /// <param name="sendDataDelegate">
        ///     The send callback.
        /// </param>
        /// <param name="receiveDataDelegate">
        ///     The receive callback.
        /// </param>
        /// <param name="connectionStatusCallback">
        ///     The connection status changed callback.
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
                this.requestedAddress = address;
                this.requestedPort = port;
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
                                this.UnderlyingSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DontFragment, true);
                                this.ParentClient.Controller.FailAttempts = 0;
                                HttpForger.SendRequest(
                                    this.UnderlyingSocket,
                                    string.IsNullOrWhiteSpace(this.ServerDomain) ? "~" : this.ServerDomain, "~",
                                    this.ForgerCompatibility ? "LINK" : "GET");
                                if (new HttpForger(this.UnderlyingSocket).ReceiveResponse()
                                    != HttpForger.CurrentState.ValidData)
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
                                    this.compressionTypes)
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
                                            "Username is too long.",
                                            null,
                                            ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                                        return;
                                    }

                                    clientRequest = new byte[3 + username.Length + password.Length];
                                    clientRequest[0] = (byte)Common.AuthenticationMethods.UserPass;
                                    clientRequest[1] = (byte)username.Length;
                                    clientRequest[username.Length + 2] = (byte)password.Length;

                                    Array.Copy(username, 0, clientRequest, 2, username.Length);
                                    Array.Copy(password, 0, clientRequest, username.Length + 3, password.Length);
                                }
                                else
                                {
                                    clientRequest = new byte[1];
                                    clientRequest[0] = (byte)Common.AuthenticationMethods.None;
                                }

                                byte clientAddressType;
                                IPAddress clientIp;
                                byte[] clientAddressBytes;
                                if (IPAddress.TryParse(this.requestedAddress, out clientIp))
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
                                    clientAddressBytes = Encoding.ASCII.GetBytes(this.requestedAddress);
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
                                    (byte)Math.Floor(this.requestedPort / 256d);
                                clientRequestAddress[clientRequestAddress.GetUpperBound(0)] =
                                    (byte)(this.requestedPort % 256);
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
                                        "Connection failed, Error Code: Authentication Failed.",
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
                                client.Controller.ClientMoveToRouting(this);
                            }
                            catch (Exception ex)
                            {
                                this.ParentClient.Controller.FailAttempts++;
                                this.Close(ex.Message, ex.StackTrace, ErrorRenderer.HttpHeaderCode.C504GatewayTimeout);
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
        ///     Get the latest requested address
        /// </summary>
        /// <returns>
        ///     The requested address as <see cref="string" />.
        /// </returns>
        public override string GetRequestedAddress()
        {
            return this.requestedAddress;
        }

        /// <summary>
        ///     Get the latest requested port number
        /// </summary>
        /// <returns>
        ///     The requested port number as <see cref="ushort" />.
        /// </returns>
        public override ushort GetRequestedPort()
        {
            return this.requestedPort;
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

        private void Close(string title, bool async)
        {
            this.Close(title, null, ErrorRenderer.HttpHeaderCode.C500ServerError, async);
        }

        private void Close(bool async)
        {
            this.Close(null, null, ErrorRenderer.HttpHeaderCode.C500ServerError, async);
        }

        private void Close(
            string title = null,
            string message = null,
            ErrorRenderer.HttpHeaderCode code = ErrorRenderer.HttpHeaderCode.C500ServerError,
            bool async = false)
        {
            try
            {
                this.IsClosed = true;
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
    }
}