// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PeaRoxyClient.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The pea roxy client.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Server
{
    #region

    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    using PeaRoxy.CommonLibrary;
    using PeaRoxy.CoreProtocol;

    #endregion

    /// <summary>
    /// The pea roxy client.
    /// </summary>
    public class PeaRoxyClient : IDisposable
    {
        #region Constants

        /// <summary>
        /// The server pea roxy version.
        /// </summary>
        private const int ServerPeaRoxyVersion = 1;

        #endregion

        #region Fields

        /// <summary>
        /// The protocol.
        /// </summary>
        internal PeaRoxyProtocol Protocol;

        /// <summary>
        /// The underlying client compression type.
        /// </summary>
        private readonly Common.CompressionTypes underlyingClientCompression;

        /// <summary>
        /// The underlying client encryption type.
        /// </summary>
        private readonly Common.EncryptionTypes underlyingClientEncryption;

        /// <summary>
        /// The underlying client receive packet size.
        /// </summary>
        private readonly int underlyingClientReceivePacketSize;

        /// <summary>
        /// The underlying client send packet size.
        /// </summary>
        private readonly int underlyingClientSendPacketSize;

        /// <summary>
        /// The current timeout.
        /// </summary>
        private int currentTimeout;

        /// <summary>
        /// The forger.
        /// </summary>
        private HttpForger forger;

        /// <summary>
        /// The forwarded.
        /// </summary>
        private bool isForwarded;

        /// <summary>
        /// The forwarder.
        /// </summary>
        private HttpForwarder forwarder;

        /// <summary>
        /// The destination socket.
        /// </summary>
        private Socket destinationSocket;

        /// <summary>
        /// The write buffer.
        /// </summary>
        private byte[] writeBuffer = new byte[0];

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PeaRoxyClient"/> class.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="parent">
        /// The parent.
        /// </param>
        /// <param name="encType">
        /// The enc type.
        /// </param>
        /// <param name="comTypes">
        /// The com type.
        /// </param>
        /// <param name="receivePacketSize">
        /// The receive packet size.
        /// </param>
        /// <param name="sendPacketSize">
        /// The send packet size.
        /// </param>
        /// <param name="selectedAuthMode">
        /// The selected auth mode.
        /// </param>
        /// <param name="noDataTimeout">
        /// The no data timeout.
        /// </param>
        /// <param name="clientSupportedEncryptionType">
        /// The client supported encryption type.
        /// </param>
        /// <param name="clientSupportedCompressionType">
        /// The client supported compression type.
        /// </param>
        public PeaRoxyClient(
            Socket client, 
            Controller parent, 
            Common.EncryptionTypes encType = Common.EncryptionTypes.None, 
            Common.CompressionTypes comTypes = Common.CompressionTypes.None, 
            int receivePacketSize = 8192, 
            int sendPacketSize = 1024, 
            int selectedAuthMode = 255, 
            int noDataTimeout = 6000, 
            Common.EncryptionTypes clientSupportedEncryptionType = Common.EncryptionTypes.AllDefaults,
            Common.CompressionTypes clientSupportedCompressionType = Common.CompressionTypes.AllDefaults)
        {
            this.UserId = "Anonymous"; // Use Anonymous as temporary user name until client introduce it-self
            this.CurrentStage = ClientStage.Connected;
            this.SelectedAuthMode = selectedAuthMode;
            this.NoDataTimeout = noDataTimeout;
            this.Controller = parent;
            this.UnderlyingSocket = client;
            this.UnderlyingSocket.Blocking = false;
            this.underlyingClientEncryption = encType;
            this.underlyingClientCompression = comTypes;
            this.ClientSupportedEncryptionType = clientSupportedEncryptionType;
            this.ClientSupportedCompressionType = clientSupportedCompressionType;
            this.underlyingClientReceivePacketSize = receivePacketSize;
            this.underlyingClientSendPacketSize = sendPacketSize;
            this.currentTimeout = this.NoDataTimeout * 1000;
        }

        #endregion

        #region Enums

        /// <summary>
        /// The client_ stage.
        /// </summary>
        public enum ClientStage
        {
            /// <summary>
            /// The connected.
            /// </summary>
            Connected, 

            /// <summary>
            /// The waiting for forger.
            /// </summary>
            WaitingForForger, 

            /// <summary>
            /// The waiting for welcome message.
            /// </summary>
            WaitingForWelcomeMessage, 

            /// <summary>
            /// The connecting to server.
            /// </summary>
            ConnectingToServer, 

            /// <summary>
            /// The routing.
            /// </summary>
            Routing, 

            /// <summary>
            /// The resolving local server.
            /// </summary>
            ResolvingLocalServer
        }

        #endregion

        #region Public Properties

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
        /// Gets the controller.
        /// </summary>
        public Controller Controller { get; private set; }

        /// <summary>
        /// Gets the current stage.
        /// </summary>
        public ClientStage CurrentStage { get; private set; }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the underlying socket.
        /// </summary>
        public Socket UnderlyingSocket { get; private set; }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        public string UserId { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the client supported compression type.
        /// </summary>
        private Common.CompressionTypes ClientSupportedCompressionType { get; set; }

        /// <summary>
        /// Gets or sets the client supported encryption type.
        /// </summary>
        private Common.EncryptionTypes ClientSupportedEncryptionType { get; set; }

        /// <summary>
        /// Gets or sets the no data timeout.
        /// </summary>
        private int NoDataTimeout { get; set; }

        /// <summary>
        /// Gets or sets the selected auth mode.
        /// </summary>
        private int SelectedAuthMode { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The accepting.
        /// </summary>
        public void Accepting()
        {
            try
            {
                if (Common.IsSocketConnected(this.UnderlyingSocket) && this.currentTimeout > 0)
                {
                    switch (this.CurrentStage)
                    {
                        case ClientStage.Connected:
                            string clientAddress = this.UnderlyingSocket.RemoteEndPoint.ToString();
                            if (
                                ConfigReader.GetBlackList()
                                    .Any(blacklist => Common.DoesMatchWildCard(clientAddress, blacklist)))
                            {
                                this.Close("Blacklisted Client: " + clientAddress);
                                return;
                            }
                            this.currentTimeout = this.NoDataTimeout * 1000;
                            this.forger = new HttpForger(this.UnderlyingSocket, this.Controller.Domain, true);
                            this.CurrentStage = ClientStage.WaitingForForger;
                            break;
                        case ClientStage.WaitingForForger:
                            bool notRelated;
                            bool result = this.forger.ReceiveRequest(out notRelated);
                            if (notRelated)
                            {
                                if (this.Controller.HttpForwardingPort == 0)
                                {
                                    this.Close();
                                }
                                else
                                {
                                    this.isForwarded = true;
                                    this.CurrentStage = ClientStage.ResolvingLocalServer;
                                    this.Id = Screen.ClientConnected(
                                        this.UserId, 
                                        "F." + this.UnderlyingSocket.RemoteEndPoint);
                                        
                                        // Report that a new client connected
                                }
                            }
                            else if (result)
                            {
                                this.forger.SendResponse();
                                this.Protocol = new PeaRoxyProtocol(
                                    this.UnderlyingSocket, 
                                    this.underlyingClientEncryption, 
                                    this.underlyingClientCompression)
                                                    {
                                                        ReceivePacketSize =
                                                            this.underlyingClientReceivePacketSize, 
                                                        SendPacketSize =
                                                            this.underlyingClientSendPacketSize, 
                                                        CloseCallback = this.CloseCal, 
                                                        ClientSupportedCompressionType =
                                                            this.ClientSupportedCompressionType, 
                                                        ClientSupportedEncryptionType =
                                                            this.ClientSupportedEncryptionType
                                                    };
                                this.Id = Screen.ClientConnected(
                                    this.UserId, 
                                    "C." + this.Protocol.UnderlyingSocket.RemoteEndPoint);
                                    
                                    // Report that a new client connected
                                this.currentTimeout = this.NoDataTimeout * 1000;
                                this.CurrentStage = ClientStage.WaitingForWelcomeMessage;
                            }

                            break;
                        case ClientStage.WaitingForWelcomeMessage:
                            if (this.Protocol.IsDataAvailable())
                            {
                                byte serverErrorCode = 0;
                                byte[] clientRequest = this.Protocol.Read(); // Read data from client
                                if (clientRequest == null || clientRequest.Length <= 0)
                                {
                                    // Check if data is correct
                                    this.Close("8. " + "No data received. Connection Timeout.");
                                    return;
                                }

                                ConfigUser acceptedUser = null;

                                // Select Auth Type
                                if (clientRequest[0] != this.SelectedAuthMode)
                                {
                                    serverErrorCode = 99;
                                }
                                else if (this.SelectedAuthMode == 0)
                                {
                                    // Nothing to auth. Just accept user
                                    Array.Copy(clientRequest, 1, clientRequest, 0, clientRequest.Length - 1);
                                }
                                else if (this.SelectedAuthMode == 1)
                                {
                                    // Auth using user name and password hash
                                    string username = Encoding.ASCII.GetString(clientRequest, 2, clientRequest[1]);
                                        
                                        // Read UserName
                                    byte[] passwordHash = new byte[clientRequest[clientRequest[1] + 2]];
                                        
                                        // Init Password Hash Byte Array
                                    Array.Copy(
                                        clientRequest, 
                                        clientRequest[1] + 3, 
                                        passwordHash, 
                                        0, 
                                        passwordHash.Length); // Read Password Hash

                                    // Search out users to find out if we have this user in users.ini
                                    foreach (ConfigUser user in ConfigReader.GetUsers())
                                    {
                                        if (user.Username.ToLower() == username.ToLower()
                                            && user.Hash.SequenceEqual(passwordHash))
                                        {
                                            // Check each user name and password hash
                                            acceptedUser = user;
                                            break;
                                        }
                                    }

                                    if (acceptedUser == null)
                                    {
                                        // Let check if we have a fail result with auth, If so, Close Connection
                                        serverErrorCode = 99;
                                    }
                                    else
                                    {
                                        Screen.ChangeUser(this.UserId, acceptedUser.Username, this.Id);
                                            
                                            // Let inform that user is changed, We are not "Anonymous" anymore
                                        this.UserId = acceptedUser.Username;
                                            
                                            // Save user name in userId field for later access to screen
                                        Array.Copy(
                                            clientRequest, 
                                            clientRequest[1] + passwordHash.Length + 3, 
                                            clientRequest, 
                                            0, 
                                            clientRequest.Length - (clientRequest[1] + passwordHash.Length + 3));
                                    }
                                }

                                string clientRequestedAddress = null;
                                ushort clientRequestedPort = 0;
                                if (serverErrorCode == 0)
                                {
                                    // Auth ok

                                    if (clientRequest[0] != ServerPeaRoxyVersion)
                                    {
                                        // Check again if client use same version as we are
                                        this.Close(); // "6. " + "Unknown version, Expected " + version.ToString());
                                        return;
                                    }

                                    byte clientAddressType = clientRequest[3];
                                        
                                        // Read address type client want to connect
                                    byte[] clientPlainRequestedAddress;
                                    switch (clientAddressType)
                                    {
                                            // Getting request address and port depending to address type
                                        case 1: // IPv4
                                            clientPlainRequestedAddress = new byte[4];
                                            Array.Copy(clientRequest, 4, clientPlainRequestedAddress, 0, 4);
                                            clientRequestedAddress =
                                                new IPAddress(clientPlainRequestedAddress).ToString();
                                            clientRequestedPort =
                                                (ushort)((ushort)(clientRequest[8] * 256) + clientRequest[9]);
                                            break;
                                        case 3: // Domain Name
                                            clientPlainRequestedAddress = new byte[clientRequest[4]];
                                            Array.Copy(
                                                clientRequest, 
                                                5, 
                                                clientPlainRequestedAddress, 
                                                0, 
                                                clientRequest[4]);
                                            clientRequestedAddress =
                                                Encoding.ASCII.GetString(clientPlainRequestedAddress);
                                            clientRequestedPort =
                                                (ushort)
                                                ((ushort)(clientRequest[5 + clientRequest[4]] * 256)
                                                 + clientRequest[5 + clientRequest[4] + 1]);
                                            break;
                                        case 4: // IPv6
                                            clientPlainRequestedAddress = new byte[16];
                                            Array.Copy(clientRequest, 4, clientPlainRequestedAddress, 0, 16);
                                            clientRequestedAddress =
                                                new IPAddress(clientPlainRequestedAddress).ToString();
                                            clientRequestedPort =
                                                (ushort)((ushort)(clientRequest[20] * 256) + clientRequest[21]);
                                            break;
                                        default:
                                            serverErrorCode = 8; // This type of address is not supported
                                            break;
                                    }

                                    if (clientRequestedAddress != null)
                                    {
                                        string clientRequestedConnectionString = clientRequestedAddress.ToLower().Trim() + ":" + clientRequestedPort;

                                        if (
                                            ConfigReader.GetBlackList()
                                                .Any(
                                                    blacklist =>
                                                    Common.DoesMatchWildCard(clientRequestedConnectionString, blacklist)))
                                        {
                                            this.Close("Blacklisted Request: " + clientRequestedAddress);
                                            return;
                                        }
                                    }
                                }

                                // Init server response to this request
                                byte[] serverResponse = new byte[2];
                                serverResponse[0] = ServerPeaRoxyVersion;
                                serverResponse[1] = serverErrorCode;
                                this.Protocol.Write(serverResponse, true); // Send response to client

                                if (serverErrorCode != 0 || clientRequestedAddress == null)
                                {
                                    // Check if we have any problem with request
                                    this.Close("5. " + "response Error, Code: " + serverErrorCode);
                                    return;
                                }

                                if (acceptedUser != null)
                                {
                                    this.Protocol.EncryptionKey = Encoding.ASCII.GetBytes(acceptedUser.Password);
                                }

                                Screen.SetRequestIpAddress(
                                    this.UserId, 
                                    this.Id, 
                                    clientRequestedAddress + ":" + clientRequestedPort);
                                    
                                    // Inform that we have a request for an address
                                this.destinationSocket = new Socket(
                                    AddressFamily.InterNetwork, 
                                    SocketType.Stream, 
                                    ProtocolType.Tcp);
                                this.destinationSocket.BeginConnect(
                                    clientRequestedAddress, 
                                    clientRequestedPort, 
                                    delegate(IAsyncResult ar)
                                        {
                                            try
                                            {
                                                this.destinationSocket.EndConnect(ar);
                                                this.destinationSocket.Blocking = false;
                                                this.currentTimeout = this.NoDataTimeout * 1000;
                                                this.Controller.MoveToQ(this);
                                                this.CurrentStage = ClientStage.Routing;
                                            }
                                            catch (Exception)
                                            {
                                                this.Close();
                                            }
                                        }, 
                                    null);

                                this.currentTimeout = this.NoDataTimeout * 1000;
                                this.CurrentStage = ClientStage.ConnectingToServer;
                            }

                            break;
                        case ClientStage.ResolvingLocalServer:
                            Screen.ChangeUser(this.UserId, "Forwarder", this.Id);
                            this.UserId = "Forwarder";
                            Screen.SetRequestIpAddress(
                                this.UserId, 
                                this.Id,
                                this.Controller.HttpForwardingIp + this.Controller.HttpForwardingPort);
                                
                                // Inform that we have a request for an address
                            this.destinationSocket = new Socket(
                                AddressFamily.InterNetwork, 
                                SocketType.Stream, 
                                ProtocolType.Tcp);
                            this.destinationSocket.BeginConnect(
                                this.Controller.HttpForwardingIp, 
                                this.Controller.HttpForwardingPort, 
                                delegate(IAsyncResult ar)
                                    {
                                        try
                                        {
                                            this.destinationSocket.EndConnect(ar);
                                            this.forwarder = new HttpForwarder(
                                                this.destinationSocket, 
                                                this.underlyingClientReceivePacketSize, 
                                                this.underlyingClientSendPacketSize);
                                            this.forwarder.Write(this.forger.HeaderBytes);
                                            this.currentTimeout = this.NoDataTimeout * 1000;
                                            this.Controller.MoveToQ(this);
                                            this.CurrentStage = ClientStage.Routing;
                                        }
                                        catch (Exception e)
                                        {
                                            this.Close("9. " + e.Message + "\r\n" + e.StackTrace);
                                        }
                                    }, 
                                null);

                            this.currentTimeout = this.NoDataTimeout * 1000;
                            this.CurrentStage = ClientStage.ConnectingToServer;
                            break;
                        case ClientStage.ConnectingToServer:
                            break;
                    }

                    this.currentTimeout--;
                }
                else
                {
                    this.Close();
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
                    this.Close("4. " + e.Message + "\r\n" + e.StackTrace);
                }
            }
        }

        /// <summary>
        /// The close.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="async">
        /// The async.
        /// </param>
        public void Close(string message = null, bool async = true)
        {
            if (message != null)
            {
                // If we have message with this function, Send it to screen
                Screen.LogMessage(message);
            }

            if (this.Protocol != null)
            {
                this.Protocol.Close(null, async);
            }

            if (this.forwarder != null)
            {
                this.forwarder.Close(null, async);
            }

            this.Controller.Dissconnected(this);
            try
            {
                if (async)
                {
                    byte[] db = new byte[0];
                    if (this.destinationSocket != null)
                    {
                        this.destinationSocket.BeginSend(
                            db, 
                            0, 
                            db.Length, 
                            SocketFlags.None, 
                            delegate(IAsyncResult ar)
                                {
                                    try
                                    {
                                        this.destinationSocket.Close(); // Close request connection it-self
                                        this.destinationSocket.EndSend(ar);
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
                    if (this.destinationSocket != null)
                    {
                        // Close request connection it-self
                        this.destinationSocket.Close();
                    }
                }
            }
            catch (Exception)
            {
            }

            Screen.ClientDisconnected(this.UserId, this.Id); // Inform that we have nothing to do after this.
        }

        /// <summary>
        /// The close cal.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="async">
        /// The async.
        /// </param>
        public void CloseCal(string message = null, bool async = true)
        {
            this.Close(message, async);
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
        public void DoRoute()
        {
            try
            {
                if (!this.isForwarded)
                {
                    if ((this.Protocol.BusyWrite || this.BusyWrite
                         || (Common.IsSocketConnected(this.destinationSocket)
                             && Common.IsSocketConnected(this.Protocol.UnderlyingSocket))) && this.currentTimeout > 0)
                    {
                        // While we have both sides connected and no timeout happened
                        if (!this.Protocol.BusyWrite && this.destinationSocket.Available > 0)
                        {
                            // If any new data in request connection
                            this.currentTimeout = this.NoDataTimeout * 1000; // Reset timeout variable
                            byte[] buffer = this.Read();
                            if (buffer != null && buffer.Length > 0)
                            {
                                // If we have any data
                                this.Protocol.Write(buffer, true); // Write data to client
                                Screen.DataReceived(this.UserId, this.Id, buffer.Length);
                                    
                                    // Inform that we have received new data
                            }
                        }

                        if (!this.BusyWrite && this.Protocol.IsDataAvailable())
                        {
                            // If any new data in client connection
                            this.currentTimeout = this.NoDataTimeout * 1000; // Reset timeout variable
                            byte[] buffer = this.Protocol.Read(); // Read data from client
                            if (buffer != null && buffer.Length > 0)
                            {
                                // If we have any data
                                this.Write(buffer);
                                Screen.DataSent(this.UserId, this.Id, buffer.Length);
                                    
                                    // Inform that we have Sent new data
                            }
                        }

                        this.currentTimeout--;
                    }
                    else
                    {
                        this.Close(null, false);
                    }
                }
                else
                {
                    if (Common.IsSocketConnected(this.destinationSocket)
                        && Common.IsSocketConnected(this.forwarder.UnderlyingSocket) && this.currentTimeout > 0)
                    {
                        // While we have both sides connected and no timeout happened
                        if (!this.forwarder.BusyWrite && this.destinationSocket.Available > 0)
                        {
                            // If any new data in request connection
                            this.currentTimeout = this.NoDataTimeout * 1000; // Reset timeout variable
                            byte[] buffer = this.Read();
                            if (buffer != null && buffer.Length > 0)
                            {
                                // If we have any data
                                this.forwarder.Write(buffer); // Write data to client
                                Screen.DataSent(this.UserId, this.Id, buffer.Length);
                                    
                                    // Inform that we have received new data
                            }
                        }

                        if (!this.BusyWrite && this.forwarder.UnderlyingSocket.Available > 0)
                        {
                            // If any new data in client connection
                            this.currentTimeout = this.NoDataTimeout * 1000; // Reset timeout variable
                            byte[] buffer = this.forwarder.Read(); // Read data from client
                            if (buffer != null && buffer.Length > 0)
                            {
                                // If we have any data
                                this.Write(buffer);
                                Screen.DataReceived(this.UserId, this.Id, buffer.Length);
                                    
                                    // Inform that we have Sent new data
                            }
                        }

                        this.currentTimeout--;
                    }
                    else
                    {
                        this.Close(null, false);
                    }
                }
            }
            catch (Exception e)
            {
                this.Close("3. " + e.Message + "\r\n" + e.StackTrace);
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
        public byte[] Read()
        {
            try
            {
                int i = 60; // Time out by second

                i = i * 100;
                while (i > 0)
                {
                    if (this.destinationSocket.Available > 0)
                    {
                        byte[] buffer = new byte[this.underlyingClientReceivePacketSize];
                        int bytes = this.destinationSocket.Receive(buffer);
                        Array.Resize(ref buffer, bytes);
                        return buffer;
                    }

                    Thread.Sleep(10);
                    i--;
                }
            }
            catch (Exception e)
            {
                this.Close("1. " + e.Message + "\r\n" + e.StackTrace);
            }

            return null;
        }

        /// <summary>
        /// The write.
        /// </summary>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        public void Write(byte[] bytes)
        {
            try
            {
                if (bytes != null)
                {
                    Array.Resize(ref this.writeBuffer, this.writeBuffer.Length + bytes.Length);
                    Array.Copy(bytes, 0, this.writeBuffer, this.writeBuffer.Length - bytes.Length, bytes.Length);
                }

                if (this.writeBuffer.Length > 0 && this.destinationSocket.Poll(0, SelectMode.SelectWrite))
                {
                    int bytesWritten = this.destinationSocket.Send(this.writeBuffer, SocketFlags.None);
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
                this.Close("2. " + e.Message + "\r\n" + e.StackTrace);
            }
        }

        #endregion
    }
}