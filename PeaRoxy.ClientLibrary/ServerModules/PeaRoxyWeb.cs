// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PeaRoxyWeb.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The server_ pea roxy web.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary.ServerModules
{
    #region

    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;

    using global::PeaRoxy.ClientLibrary.ProxyModules;

    using global::PeaRoxy.CommonLibrary;

    using global::PeaRoxy.CoreProtocol.Cryptors;

    #endregion

    /// <summary>
    ///     The PeaRoxyWeb server module
    /// </summary>
    public sealed class PeaRoxyWeb : ServerType, IDisposable
    {
        #region Static Fields

        /// <summary>
        ///     The random byte generator
        /// </summary>
        private static readonly RNGCryptoServiceProvider Random = new RNGCryptoServiceProvider();

        #endregion

        #region Fields

        /// <summary>
        ///     The encryption salt bytes.
        /// </summary>
        private readonly byte[] encryptionSaltBytes = new byte[4];

        /// <summary>
        ///     The encryption type.
        /// </summary>
        private readonly Common.EncryptionType encryptionType = Common.EncryptionType.None;

        /// <summary>
        ///     The chunked is first.
        /// </summary>
        private bool chunkedIsFirst = true;

        /// <summary>
        ///     The chunked needed.
        /// </summary>
        private int chunkedNeeded;

        /// <summary>
        ///     The client secure stream.
        /// </summary>
        private SslStream clientSslStream;

        /// <summary>
        ///     The content bytes.
        /// </summary>
        private int contentBytes = -1;

        /// <summary>
        ///     The encryptor class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        private Cryptor cryptor = new Cryptor();

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

        /// <summary>
        ///     The encryption key.
        /// </summary>
        private byte[] encryptionKey;

        /// <summary>
        ///     The is chunked.
        /// </summary>
        private bool isChunked;

        /// <summary>
        ///     The is done for routing.
        /// </summary>
        private bool isDoneForRouting;

        /// <summary>
        ///     The is https.
        /// </summary>
        private bool isHttps;

        /// <summary>
        ///     The peer encryptor class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        private Cryptor peerCryptor = new Cryptor();

        /// <summary>
        ///     The read bytes.
        /// </summary>
        private int readedBytes;

        /// <summary>
        ///     The server encryption type.
        /// </summary>
        private Common.EncryptionType serverEncryptionType = Common.EncryptionType.None;

        /// <summary>
        ///     The write buffer.
        /// </summary>
        private byte[] writeBuffer = new byte[0];

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PeaRoxyWeb"/> class.
        /// </summary>
        /// <param name="address">
        /// The address.
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
        /// <param name="addressChecked">
        /// The address checked.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Invalid address or encryption type
        /// </exception>
        public PeaRoxyWeb(
            string address, 
            string username = "", 
            string password = "", 
            Common.EncryptionType encType = Common.EncryptionType.None, 
            bool addressChecked = false)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentException(@"Invalid value.", "address");
            }

            if (encType != Common.EncryptionType.SimpleXor && encType != Common.EncryptionType.None)
            {
                throw new ArgumentException(@"Invalid value.", "encType");
            }

            this.IsServerValid = false;
            this.ServerAddress = address;
            this.ServerUri = new Uri(address);

            if (!addressChecked)
            {
                HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create(this.ServerUri);
                wreq.AllowAutoRedirect = true;
                wreq.Proxy = null;
                wreq.Method = "HEAD";
                wreq.Headers.Add("X-Requested-With", "NOREDIRECT");
                HttpWebResponse wres = (HttpWebResponse)wreq.GetResponse();
                wres.Close();
                if (wres.StatusCode != HttpStatusCode.OK)
                {
                    throw new ArgumentException(@"Server responsed with status code " + wres.StatusCode, "address");
                }

                this.ServerUri = wres.ResponseUri;
                this.ServerAddress = this.ServerUri.ToString();
            }

            this.Username = username;
            this.Password = password;
            this.encryptionType = encType;
            this.NoDataTimeout = 60;
            this.IsDataSent = false;
            this.IsClosed = false;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets a value indicating whether busy write.
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
        ///     Gets or sets a value indicating whether is data sent.
        /// </summary>
        public override bool IsDataSent { get; protected set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is disconnected.
        /// </summary>
        public override bool IsClosed { get; protected set; }

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
        ///     Gets or sets the address of the server
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        ///     Gets or sets the address of the server
        /// </summary>
        public Uri ServerUri { get; set; }

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
            return new PeaRoxyWeb(this.ServerAddress, this.Username, this.Password, this.encryptionType, true)
                       {
                           NoDataTimeout
                               =
                               this
                               .NoDataTimeout
                       };
        }

        /// <summary>
        ///     The dispose.
        /// </summary>
        public void Dispose()
        {
            this.UnderlyingSocket.Dispose();
            if (this.clientSslStream != null)
            {
                this.clientSslStream.Dispose();
            }
        }

        /// <summary>
        ///     The do route.
        /// </summary>
        public override void DoRoute()
        {
            try
            {
                if (!this.isDoneForRouting)
                {
                    return;
                }

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
                        if (this.isHttps && this.clientSslStream != null)
                        {
                            this.clientSslStream.Write(this.ParentClient.SmartResponseBuffer);
                        }
                        else
                        {
                            this.ParentClient.Write(this.ParentClient.SmartResponseBuffer);
                        }
                    }

                    if (!this.ParentClient.BusyWrite && this.UnderlyingSocket.Available > 0)
                    {
                        this.IsDataSent = true;
                        this.currentTimeout = this.NoDataTimeout * 1000;
                        byte[] buffer = this.Read();
                        if (buffer != null && buffer.Length > 0)
                        {
                            buffer = this.CheckChunked(buffer);
                            buffer = this.peerCryptor.Decrypt(buffer);
                            if (this.ReceiveDataDelegate != null
                                && this.ReceiveDataDelegate.Invoke(ref buffer, this, this.ParentClient) == false)
                            {
                                this.ParentClient = null;
                                this.Close();
                                return;
                            }

                            if (buffer.Length > 0)
                            {
                                this.WriteToClient(buffer);
                            }
                        }
                    }

                    if (!this.BusyWrite && this.ParentClient.Client.Available > 0)
                    {
                        if (this.isHttps)
                        {
                            this.Close();
                        }

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

                            this.Close(false);
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
            catch (Exception)
            {
                // e)
                // if (e.TargetSite.Name == "Receive")
                // Close();
                // else
                // Close(e.Message, e.StackTrace);
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
                    this.ServerUri.Host, 
                    this.ServerUri.Port, 
                    delegate(IAsyncResult ar)
                        {
                            try
                            {
                                this.UnderlyingSocket.EndConnect(ar);
                                this.UnderlyingSocket.Blocking = false;
                                this.ParentClient.Controller.FailAttempts = 0;

                                if (!this.CheckConnectionTypeWithClient(ref headerData))
                                {
                                    return;
                                }

                                if (!this.SendRequestToServer(ref headerData))
                                {
                                    return;
                                }

                                if (!this.ReadServerResponse(ref headerData))
                                {
                                    return;
                                }

                                this.ParentClient.Controller.MoveToRouting(this);

                                if (!this.ReadHeaderOfActualResponse(ref headerData))
                                {
                                    return;
                                }

                                this.WriteToClient(headerData);
                                this.isDoneForRouting = true;
                                this.currentTimeout = this.NoDataTimeout * 1000;
                            }
                            catch (Exception ex)
                            {
                                this.ParentClient.Controller.FailAttempts++;
                                this.Close(ex.Message, ex.StackTrace);
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
            return "PeaRoxyWeb Module " + this.ServerAddress
                   + ((this.Username != string.Empty) ? " Using USN/PSW" : " Open");
        }

        #endregion

        #region Methods

        /// <summary>
        /// The check chunked.
        /// </summary>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        /// <returns>
        /// The
        ///     <see>
        ///         <cref>byte[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        private byte[] CheckChunked(byte[] bytes)
        {
            try
            {
                if (bytes.Length > 0)
                {
                    if (this.chunkedIsFirst)
                    {
                        int bodyLocation = Common.GetFirstBytePatternIndex(bytes, Encoding.ASCII.GetBytes("\r\n"));
                        this.isChunked = bodyLocation < 5 && bodyLocation > 0;
                        this.chunkedIsFirst = false;
                    }

                    if (this.isChunked)
                    {
                        while (this.chunkedNeeded == 0 && bytes.Length >= 2)
                        {
                            int digitPlace = Common.GetFirstBytePatternIndex(bytes, Encoding.ASCII.GetBytes("\r\n"));
                            if (digitPlace == -1)
                            {
                                this.isChunked = false;
                                return bytes;
                            }

                            if (digitPlace != 0)
                            {
                                this.chunkedNeeded = int.Parse(
                                    Encoding.ASCII.GetString(bytes, 0, digitPlace), 
                                    NumberStyles.HexNumber);
                            }

                            digitPlace += 2;
                            Array.Copy(bytes, digitPlace, bytes, 0, bytes.Length - digitPlace);
                            Array.Resize(ref bytes, bytes.Length - digitPlace);
                        }

                        byte[] newBytes;
                        if (bytes.Length >= this.chunkedNeeded)
                        {
                            newBytes = new byte[this.chunkedNeeded];
                            this.chunkedNeeded = 0;
                            Array.Copy(bytes, newBytes, newBytes.Length);
                            Array.Copy(bytes, newBytes.Length, bytes, 0, bytes.Length - newBytes.Length);
                            Array.Resize(ref bytes, bytes.Length - newBytes.Length);
                            bytes = this.CheckChunked(bytes);
                            Array.Resize(ref newBytes, newBytes.Length + bytes.Length);
                            Array.Copy(bytes, 0, newBytes, newBytes.Length - bytes.Length, bytes.Length);
                        }
                        else
                        {
                            newBytes = new byte[bytes.Length];
                            Array.Copy(bytes, newBytes, bytes.Length);
                            this.chunkedNeeded -= bytes.Length;
                            Array.Resize(ref bytes, 0);
                        }

                        return newBytes;
                    }
                }
            }
            catch (Exception)
            {
            }

            return bytes;
        }

        /// <summary>
        /// The check connection type with client.
        /// </summary>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool CheckConnectionTypeWithClient(ref byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                // IS HTTPS
                try
                {
                    Uri url;
                    if (!Uri.TryCreate(this.ParentClient.RequestAddress, UriKind.Absolute, out url))
                    {
                        this.Close(
                            "Unrecognizable address.", 
                            null, 
                            ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                        return false;
                    }

                    string certAddress = url.DnsSafeHost;
                    if (!Common.IsIpAddress(certAddress))
                    {
                        certAddress = Common.GetNextLevelDomain(certAddress);
                        if (string.IsNullOrEmpty(certAddress))
                        {
                            this.Close(
                                "Domain name is not acceptable for HTTPS connection.", 
                                url.ToString(), 
                                ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                            return false;
                        }
                    }

                    certAddress = ErrorRenderer.GetCertForDomain(certAddress);
                    if (string.IsNullOrEmpty(certAddress))
                    {
                        this.Close("No certificate available or failed to generate one.", certAddress);
                        return false;
                    }

                    X509Certificate certificate = new X509Certificate2(certAddress, string.Empty);
                    this.ParentClient.Client.Blocking = true;
                    Stream stream = new NetworkStream(this.ParentClient.Client);
                    this.clientSslStream = new SslStream(stream) { ReadTimeout = 30 * 1000, WriteTimeout = 30 * 1000 };
                    this.clientSslStream.AuthenticateAsServer(certificate);
                    bytes = new byte[16384];
                    string headerString = string.Empty;
                    bool headerRec = false;
                    int arraySize = 0;
                    do
                    {
                        if (arraySize >= bytes.Length)
                        {
                            this.Close(
                                "No header after 16KB of data.", 
                                null, 
                                ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                            return false;
                        }

                        int readCount = this.clientSslStream.Read(bytes, arraySize, bytes.Length - arraySize);
                        if (readCount > 0)
                        {
                            arraySize += readCount;
                        }
                        else
                        {
                            this.Close(
                                "Connection closed by client.", 
                                null, 
                                ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                            return false;
                        }

                        headerString += Encoding.ASCII.GetString(bytes, arraySize - readCount, readCount);
                        if (headerString.IndexOf("\r\n\r\n", StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            headerRec = true;
                        }
                    }
                    while (!headerRec);
                    Array.Resize(ref bytes, arraySize);
                    if (Http.IsHttp(bytes))
                    {
                        this.isHttps = true;
                    }
                    else
                    {
                        this.Close(
                            "Connection header is not HTTP.", 
                            null, 
                            ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                        return false;
                    }
                }
                catch (Exception)
                {
                    this.Close();
                    return false;
                }
            }
            else if (!Http.IsHttp(bytes))
            {
                this.Close(
                    "PeaRoxy supports only HTTPS and HTTP connections currently.", 
                    null, 
                    ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                return false;
            }

            return true;
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
                if (this.ParentClient != null)
                {
                    if (this.isHttps && this.clientSslStream != null)
                    {
                        this.ParentClient.Close(title, message, code, async, this.clientSslStream);
                    }
                    else
                    {
                        this.ParentClient.Close(title, message, code, async);
                    }
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
        ///     The read.
        /// </summary>
        /// <returns>
        ///     The
        ///     <see>
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
        /// The read header of actual response method
        /// </summary>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool ReadHeaderOfActualResponse(ref byte[] bytes)
        {
            bytes = this.CheckChunked(bytes);
            bytes = this.peerCryptor.Decrypt(bytes);

            string header = Encoding.ASCII.GetString(bytes);
            bool errorChecked = false;
            this.currentTimeout = this.NoDataTimeout * 1000;
            while (header.IndexOf("\r\n\r\n", StringComparison.Ordinal) == -1)
            {
                if (!errorChecked && header != string.Empty)
                {
                    errorChecked = true;
                    if (header.StartsWith("Server Error:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.Close(header, null, ErrorRenderer.HttpHeaderCode.C502BadGateway);
                        return false;
                    }
                }

                if ((this.ParentClient.Client != null && !Common.IsSocketConnected(this.ParentClient.Client))
                    || !Common.IsSocketConnected(this.UnderlyingSocket))
                {
                    this.Close();
                    return false;
                }

                if (this.UnderlyingSocket.Available > 0)
                {
                    this.currentTimeout = this.NoDataTimeout * 1000;
                    byte[] response = this.Read();
                    if (response == null || response.Length <= 0)
                    {
                        this.Close(
                            "No response from server, Read Failure.", 
                            null, 
                            ErrorRenderer.HttpHeaderCode.C502BadGateway);
                        return false;
                    }

                    response = this.CheckChunked(response);
                    response = this.peerCryptor.Decrypt(response);

                    Array.Resize(ref bytes, bytes.Length + response.Length);
                    Array.Copy(response, 0, bytes, bytes.Length - response.Length, response.Length);
                    header += Encoding.ASCII.GetString(response);
                }
                else
                {
                    this.currentTimeout--;
                    if (this.currentTimeout == 0)
                    {
                        this.Close(
                            "No response from server, Timeout.", 
                            null, 
                            ErrorRenderer.HttpHeaderCode.C504GatewayTimeout);
                        return false;
                    }

                    Thread.Sleep(1);
                }
            }

            this.IsServerValid = true;
            int endOfHeaderIndex = header.IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4;
            header = header.Substring(0, endOfHeaderIndex);
            int headerLenght = Encoding.ASCII.GetByteCount(header);
            Array.Copy(bytes, headerLenght, bytes, 0, bytes.Length - headerLenght);

            // headerLenght += 2;
            Array.Resize(ref bytes, bytes.Length - headerLenght);

            // --------------------------- It is better to add connection close to response header. Of course it may not solve our keep alive problem so we keep
            // content length of response too so we can close it later.
            int start = header.IndexOf("Content-Length:", StringComparison.InvariantCultureIgnoreCase);
            if (start != -1)
            {
                start += "Content-Length:".Length;
                int count = header.IndexOf("\r\n", start, StringComparison.Ordinal) - start;
                this.contentBytes = int.Parse(header.Substring(start, count).Trim());
            }

            int indexOfConnectionType = header.IndexOf("\r\nConnection:", StringComparison.InvariantCultureIgnoreCase);
            if (indexOfConnectionType != -1)
            {
                int countUntilEndOfLine = header.IndexOf("\r\n", indexOfConnectionType + 2, StringComparison.Ordinal)
                                          - indexOfConnectionType;
                header = header.Remove(indexOfConnectionType, countUntilEndOfLine);
            }

            header = header.Insert(header.Length - 2, "Connection: close\r\nProxy-Connection: close\r\n");

            byte[] headerBytes = Encoding.ASCII.GetBytes(header);
            if (this.contentBytes != -1)
            {
                this.contentBytes += headerBytes.Length;
            }

            Array.Resize(ref headerBytes, headerBytes.Length + bytes.Length);
            Array.Copy(bytes, 0, headerBytes, headerBytes.Length - bytes.Length, bytes.Length);
            bytes = headerBytes;
            return true;
        }

        /// <summary>
        /// The read server response.
        /// </summary>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        // ReSharper disable once RedundantAssignment
        private bool ReadServerResponse(ref byte[] bytes)
        {
            this.currentTimeout = this.NoDataTimeout * 1000; // let make it 60sec
            string responseHeader = string.Empty;
            bytes = new byte[0];
            while (responseHeader.IndexOf("\r\n\r\n", StringComparison.Ordinal) == -1)
            {
                if ((this.ParentClient.Client != null && !Common.IsSocketConnected(this.ParentClient.Client))
                    || !Common.IsSocketConnected(this.UnderlyingSocket))
                {
                    this.Close();
                    return false;
                }

                if (this.UnderlyingSocket.Available > 0)
                {
                    this.currentTimeout = this.NoDataTimeout * 1000;
                    byte[] response = this.Read();
                    if (response == null || response.Length <= 0)
                    {
                        this.Close(
                            "No response from server, Read Failure.", 
                            null, 
                            ErrorRenderer.HttpHeaderCode.C504GatewayTimeout);
                        return false;
                    }

                    Array.Resize(ref bytes, bytes.Length + response.Length);
                    Array.Copy(response, 0, bytes, bytes.Length - response.Length, response.Length);
                    responseHeader += Encoding.ASCII.GetString(response);
                }
                else
                {
                    if (!this.BusyWrite)
                    {
                        this.currentTimeout--;
                        if (this.currentTimeout == 0)
                        {
                            this.Close(
                                "No response from server, Timeout.", 
                                null, 
                                ErrorRenderer.HttpHeaderCode.C504GatewayTimeout);
                            return false;
                        }
                    }

                    Thread.Sleep(1);
                }
            }

            int endOfHeaderIndex = responseHeader.IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4;
            responseHeader = responseHeader.Substring(0, endOfHeaderIndex);
            if (!responseHeader.StartsWith("HTTP/1.1 200", StringComparison.InvariantCultureIgnoreCase)
                && !responseHeader.StartsWith("HTTP/1.0 200", StringComparison.InvariantCultureIgnoreCase))
            {
                this.Close(
                    "Bad server response. Is address of server ", 
                    responseHeader.Substring(0, Math.Min(responseHeader.Length, responseHeader.IndexOf("\r", StringComparison.Ordinal))), 
                    ErrorRenderer.HttpHeaderCode.C502BadGateway);
                return false;
            }

            int headerLenght = Encoding.ASCII.GetByteCount(responseHeader);
            Array.Copy(bytes, headerLenght, bytes, 0, bytes.Length - headerLenght);
            Array.Resize(ref bytes, bytes.Length - headerLenght);

            // --------------------------- We have header. Reading cookies for getting encryption type
            int indexOfCookies = responseHeader.IndexOf("\r\nSet-Cookie: ", StringComparison.InvariantCultureIgnoreCase);
            if (indexOfCookies != -1)
            {
                int startOfCookies = responseHeader.IndexOf(
                    "=", 
                    indexOfCookies, 
                    StringComparison.InvariantCultureIgnoreCase) + 1;
                int endOfCookies = responseHeader.IndexOf(
                    ";", 
                    indexOfCookies, 
                    StringComparison.InvariantCultureIgnoreCase);
                this.serverEncryptionType =
                    (Common.EncryptionType)
                    int.Parse(responseHeader.Substring(startOfCookies, endOfCookies - startOfCookies));
            }

            if (this.serverEncryptionType == this.encryptionType)
            {
                this.peerCryptor = this.cryptor;
            }
            else
            {
                switch (this.serverEncryptionType)
                {
                    case Common.EncryptionType.None:

                        // Do Nothing. It is OK
                        break;
                    case Common.EncryptionType.SimpleXor:
                        this.peerCryptor = new SimpleXorCryptor(this.encryptionKey, false);
                        this.peerCryptor.SetSalt(this.encryptionSaltBytes);
                        break;
                    default:
                        this.Close(
                            "Unsupported encryption method used by server.", 
                            null, 
                            ErrorRenderer.HttpHeaderCode.C502BadGateway);
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The send request to server.
        /// </summary>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool SendRequestToServer(ref byte[] bytes)
        {
            int requestContentLength = -1;
            bool needContentLength = false;
            string textData = Encoding.ASCII.GetString(bytes);
            int headerLocation = textData.IndexOf("\r\n\r\n", StringComparison.Ordinal);
            if (headerLocation != -1)
            {
                textData = textData.Substring(0, headerLocation + 4);
            }

            int start = textData.IndexOf("Content-Length:", StringComparison.InvariantCultureIgnoreCase);
            if (start != -1)
            {
                start += "Content-Length:".Length;
                int count = textData.IndexOf("\r\n", start, StringComparison.Ordinal) - start;
                requestContentLength = int.Parse(textData.Substring(start, count).Trim());
            }

            if (textData.StartsWith("POST", StringComparison.InvariantCultureIgnoreCase))
            {
                if (requestContentLength == -1)
                {
                    requestContentLength = 0;
                }
                else
                {
                    needContentLength = true;
                }
            }
            else
            {
                if (requestContentLength == -1)
                {
                    requestContentLength = 0;
                }

                needContentLength = true;
            }

            if (needContentLength)
            {
                requestContentLength += Encoding.ASCII.GetByteCount(textData);
            }

            // --------------------------- Appending Request Header
            string httpHeader = "POST " + this.ServerUri.PathAndQuery + " HTTP/1.1" + "\r\n";
            httpHeader += "Host: " + this.ServerUri.Host + "\r\n";
            httpHeader += "Content-Type: text/plain" + "\r\n";
            if (needContentLength)
            {
                httpHeader += "Content-Length: " + requestContentLength + "\r\n";
            }
            else
            {
                httpHeader += "TE: chunked" + "\r\n";
                httpHeader += "Transfer-Encoding: chunked" + "\r\n";
            }

            httpHeader += "Connection: close" + "\r\n";

            Random.GetNonZeroBytes(this.encryptionSaltBytes);
            this.encryptionKey = this.encryptionSaltBytes;

            if (this.Username != string.Empty)
            {
                httpHeader += "Authorization: Basic "
                              + Convert.ToBase64String(
                                  Encoding.ASCII.GetBytes(this.Username + ":" + Common.Md5(this.Password))) + "\r\n";
                this.encryptionKey = Encoding.ASCII.GetBytes(this.Password);
            }

            switch (this.encryptionType)
            {
                case Common.EncryptionType.SimpleXor:
                    this.cryptor = new SimpleXorCryptor(this.encryptionKey, false);
                    break;
            }

            this.cryptor.SetSalt(this.encryptionSaltBytes);
            byte[] hostBytes =
                Encoding.ASCII.GetBytes(
                    (this.isHttps ? "https://" : "http://") + this.destinationAddress + ":" + this.destinationPort);
            hostBytes = this.cryptor.Encrypt(hostBytes);
            byte[] encryptedOutput = new byte[hostBytes.Length + 5];
            Array.Copy(hostBytes, 0, encryptedOutput, 5, hostBytes.Length);
            Array.Copy(this.encryptionSaltBytes, 0, encryptedOutput, 0, this.encryptionSaltBytes.Length);
            encryptedOutput[4] = (byte)this.encryptionType;
            string cookieValue = Path.GetRandomFileName().Replace(".", string.Empty) + "="
                                 + Uri.EscapeDataString(Convert.ToBase64String(encryptedOutput)) + "; ";
            httpHeader += "Cookie: " + cookieValue + "\r\n";
            httpHeader += "\r\n";

            this.Write(Encoding.ASCII.GetBytes(httpHeader), false);
            this.Write(bytes);

            int readed = bytes.Length;
            int timeout = this.NoDataTimeout * 1000;
            while ((readed < requestContentLength || !needContentLength) && timeout > 0)
            {
                if (this.BusyWrite)
                {
                    timeout--;
                    Thread.Sleep(1);
                    continue;
                }

                byte[] buffer = new byte[8192];
                int readCount;
                if (this.isHttps)
                {
                    readCount = this.clientSslStream.Read(
                        buffer, 
                        0, 
                        Math.Min(buffer.Length, requestContentLength - readed));
                    Array.Resize(ref buffer, readCount);
                }
                else
                {
                    if (this.ParentClient.Client.Available > 0)
                    {
                        timeout = this.NoDataTimeout * 1000;
                        buffer = this.ParentClient.Read();
                        readCount = buffer.Length;
                    }
                    else
                    {
                        timeout--;
                        Thread.Sleep(1);
                        continue;
                    }
                }

                if (readCount == 0)
                {
                    this.Close(
                        "Connection closed by client.", 
                        null, 
                        ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                    return false;
                }

                readed += readCount;
                this.Write(buffer);

                if (!needContentLength && buffer.Length >= 5
                    && Common.GetFirstBytePatternIndex(buffer, Encoding.ASCII.GetBytes("0\r\n\r\n"), buffer.Length - 5)
                    != -1)
                {
                    break;
                }
            }

            this.IsDataSent = true;
            return true;
        }

        /// <summary>
        /// The write.
        /// </summary>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        /// <param name="encryption">
        /// The encryption.
        /// </param>
        private void Write(byte[] bytes, bool encryption = true)
        {
            try
            {
                if (bytes != null)
                {
                    if (encryption)
                    {
                        bytes = this.cryptor.Encrypt(bytes);
                    }

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

        /// <summary>
        /// The write to client.
        /// </summary>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        private void WriteToClient(byte[] bytes)
        {
            if (this.isHttps && this.clientSslStream != null)
            {
                this.ParentClient.Write(bytes, this.clientSslStream);
            }
            else
            {
                this.ParentClient.Write(bytes);
            }

            if (this.contentBytes != -1)
            {
                this.readedBytes += bytes.Length;
                if (this.readedBytes >= this.contentBytes)
                {
                    this.UnderlyingSocket.Close();
                }
            }
            else if (bytes.Length >= 5
                     && Common.GetFirstBytePatternIndex(bytes, Encoding.ASCII.GetBytes("0\r\n\r\n"), bytes.Length - 5)
                     != -1)
            {
                this.UnderlyingSocket.Close();
            }
        }

        #endregion
    }
}