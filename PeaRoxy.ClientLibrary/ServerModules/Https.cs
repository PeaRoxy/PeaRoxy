// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Https.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The server_ https.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary.ServerModules
{
    #region

    using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    using global::PeaRoxy.CommonLibrary;

    #endregion

    /// <summary>
    /// The Http and Https server module
    /// </summary>
    public sealed class Https : ServerType, IDisposable
    {
        #region Fields
        /// <summary>
        /// The address.
        /// </summary>
        private string destinationAddress = string.Empty;

        /// <summary>
        /// The current timeout.
        /// </summary>
        private int currentTimeout;

        /// <summary>
        /// The port.
        /// </summary>
        private ushort destinationPort;

        /// <summary>
        /// The write buffer.
        /// </summary>
        private byte[] writeBuffer = new byte[0];

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Https"/> class.
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
        /// Invalid server IP
        /// </exception>
        public Https(string address, ushort port, string username = "", string password = "")
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
            this.IsClosed = false;
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
        /// Gets a value indicating whether busy write.
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
        /// Gets or sets a value indicating whether is data sent.
        /// </summary>
        public override bool IsDataSent { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether is disconnected.
        /// </summary>
        public override bool IsClosed { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether is server valid.
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
            return new Https(this.ServerAddress, this.ServerPort, this.Username, this.Password)
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
            this.UnderlyingSocket.Dispose();
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
                         && this.ParentClient.Client != null && Common.IsSocketConnected(this.ParentClient.Client))) && this.currentTimeout > 0)
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
                                this.UnderlyingSocket.Blocking = false;
                                this.ParentClient.Controller.FailAttempts = 0;

                                string httpsHeader = "CONNECT " + address + ":" + port + " HTTP/1.1" + "\r\n";
                                if (this.Username != string.Empty)
                                {
                                    httpsHeader += "Proxy-Authorization: Basic "
                                                   + Convert.ToBase64String(
                                                       Encoding.UTF8.GetBytes(this.Username + ":" + this.Password))
                                                   + "\r\n";
                                }

                                httpsHeader += "\r\n";
                                bool firstTry = true;

                                sendHeader:
                                this.Write(Encoding.ASCII.GetBytes(httpsHeader));
                                this.currentTimeout = this.NoDataTimeout * 1000;
                                while (this.UnderlyingSocket.Available <= 0)
                                {
                                    if ((client.Client != null && !Common.IsSocketConnected(client.Client))
                                        || !Common.IsSocketConnected(this.UnderlyingSocket))
                                    {
                                        this.Close();
                                        return;
                                    }

                                    if (!this.BusyWrite)
                                    {
                                        this.currentTimeout--;
                                        if (this.currentTimeout == 0)
                                        {
                                            this.Close(
                                                "No response, Timeout.", 
                                                null, 
                                                ErrorRenderer.HttpHeaderCode.C504GatewayTimeout);
                                            return;
                                        }
                                    }

                                    Thread.Sleep(1);
                                }

                                string response = Encoding.ASCII.GetString(this.Read()).ToLower();
                                if (firstTry && this.Username != string.Empty
                                    && (response.StartsWith("HTTP/1.1 407".ToLower())
                                        || response.StartsWith("HTTP/1.0 407".ToLower())))
                                {
                                    firstTry = false;
                                    goto sendHeader;
                                }

                                if (response.StartsWith("HTTP/1.1 200".ToLower())
                                         || response.StartsWith("HTTP/1.0 200".ToLower()))
                                {
                                    this.IsServerValid = true;
                                    if (headerData != null && Common.IsSocketConnected(this.UnderlyingSocket))
                                    {
                                        this.Write(headerData);
                                    }

                                    this.currentTimeout = this.NoDataTimeout * 1000;
                                    client.Controller.MoveToRouting(this);
                                }
                                else
                                {
                                    this.Close(
                                        "Connection failed", 
                                        "response: " + response.Substring(0, Math.Min(response.Length, 50)), 
                                        ErrorRenderer.HttpHeaderCode.C502BadGateway);
                                }
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
            return "HTTPS Module " + this.ServerAddress + ":" + this.ServerPort
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

                this.IsClosed = true;

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
        private void Write(byte[] bytes)
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
            }
            catch (Exception e)
            {
                this.Close(e.Message, e.StackTrace);
            }
        }

        #endregion
    }
}