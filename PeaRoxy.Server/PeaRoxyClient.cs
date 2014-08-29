// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PeaRoxyClient.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Server
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    using PeaRoxy.CommonLibrary;
    using PeaRoxy.CoreProtocol;

    /// <summary>
    ///     The PeaRoxy client object is representation of an incoming connection
    /// </summary>
    public class PeaRoxyClient : IDisposable
    {
        /// <summary>
        ///     The stages of request
        /// </summary>
        public enum RequestStages
        {
            JustConnected,

            WaitingForForger,

            WaitingForWelcomeMessage,

            ConnectingToTheServer,

            Routing,

            ResolvingLocalServer
        }

        private const int ServerPeaRoxyVersion = 1;

        private readonly Common.CompressionTypes underlyingClientCompression;

        private readonly Common.EncryptionTypes underlyingClientEncryption;

        private readonly int underlyingClientReceivePacketSize;

        private readonly int underlyingClientSendPacketSize;

        internal PeaRoxyProtocol Protocol;

        private int currentTimeout;

        private Socket destinationSocket;

        private HttpForger forger;

        private Forwarder forwarder;

        private bool isForwarded;

        private byte[] writeBuffer = new byte[0];

        /// <summary>
        ///     Initializes a new instance of the <see cref="PeaRoxyClient" /> class.
        /// </summary>
        /// <param name="client">
        ///     The client's socket.
        /// </param>
        /// <param name="parent">
        ///     The parent controller object.
        /// </param>
        /// <param name="encType">
        ///     The sending encryption type.
        /// </param>
        /// <param name="comTypes">
        ///     The sending compression type.
        /// </param>
        /// <param name="receivePacketSize">
        ///     The receiving packet size.
        /// </param>
        /// <param name="sendPacketSize">
        ///     The sending packet size.
        /// </param>
        /// <param name="selectedAuthMode">
        ///     The selected authentication mode.
        /// </param>
        /// <param name="noDataTimeout">
        ///     The no data timeout value.
        /// </param>
        /// <param name="clientSupportedEncryptionType">
        ///     The supported client encryption types.
        /// </param>
        /// <param name="clientSupportedCompressionType">
        ///     The supported client compression types.
        /// </param>
        public PeaRoxyClient(
            Socket client,
            PeaRoxyController parent,
            Common.EncryptionTypes encType = Common.EncryptionTypes.None,
            Common.CompressionTypes comTypes = Common.CompressionTypes.None,
            int receivePacketSize = 8192,
            int sendPacketSize = 1024,
            Common.AuthenticationMethods selectedAuthMode = Common.AuthenticationMethods.Invalid,
            int noDataTimeout = 6000,
            Common.EncryptionTypes clientSupportedEncryptionType = Common.EncryptionTypes.AllDefaults,
            Common.CompressionTypes clientSupportedCompressionType = Common.CompressionTypes.AllDefaults)
        {
            this.Username = "Anonymous"; // Use Anonymous as temporary user name until client introduce it-self
            this.CurrentStage = RequestStages.JustConnected;
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
        ///     Gets the active Proxy Controller object.
        /// </summary>
        public PeaRoxyController Controller { get; private set; }

        /// <summary>
        ///     Gets the current stage.
        /// </summary>
        public RequestStages CurrentStage { get; private set; }

        /// <summary>
        ///     Gets the Id number of this connection.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        ///     Gets the underlying socket to the client.
        /// </summary>
        public Socket UnderlyingSocket { get; private set; }

        /// <summary>
        ///     Gets the username used to establish this connection
        /// </summary>
        public string Username { get; private set; }

        private Common.CompressionTypes ClientSupportedCompressionType { get; set; }

        private Common.EncryptionTypes ClientSupportedEncryptionType { get; set; }

        private int NoDataTimeout { get; set; }

        private Common.AuthenticationMethods SelectedAuthMode { get; set; }

        public void Dispose()
        {
            if (this.UnderlyingSocket != null)
            {
                this.UnderlyingSocket.Dispose();
            }
        }

        /// <summary>
        ///     The method to handle the accepting process, should call repeatedly
        /// </summary>
        public void Accepting()
        {
            try
            {
                if (Common.IsSocketConnected(this.UnderlyingSocket) && this.currentTimeout > 0)
                {
                    switch (this.CurrentStage)
                    {
                        case RequestStages.JustConnected:
                            string clientAddress = this.UnderlyingSocket.RemoteEndPoint.ToString();
                            if (
                                this.Controller.Settings.BlackListedAddresses.Any(
                                    blacklist => Common.DoesMatchWildCard(clientAddress, blacklist)))
                            {
                                this.Close("Blacklisted Client: " + clientAddress);
                                return;
                            }
                            this.currentTimeout = this.NoDataTimeout * 1000;
                            this.forger = new HttpForger(
                                this.UnderlyingSocket,
                                this.Controller.Settings.PeaRoxyDomain,
                                true);
                            this.CurrentStage = RequestStages.WaitingForForger;
                            break;
                        case RequestStages.WaitingForForger:
                            bool notRelated;
                            bool result = this.forger.ReceiveRequest(out notRelated);
                            if (notRelated)
                            {
                                if (this.Controller.Settings.HttpForwardingPort == 0)
                                {
                                    this.Close();
                                }
                                else
                                {
                                    this.isForwarded = true;
                                    this.CurrentStage = RequestStages.ResolvingLocalServer;
                                    this.Id = Screen.ClientConnected(
                                        this.Username,
                                        "F." + this.UnderlyingSocket.RemoteEndPoint);
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
                                                        CloseCallback = this.Close,
                                                        ClientSupportedCompressionType =
                                                            this.ClientSupportedCompressionType,
                                                        ClientSupportedEncryptionType =
                                                            this.ClientSupportedEncryptionType
                                                    };
                                this.Id = Screen.ClientConnected(
                                    this.Username,
                                    "C." + this.Protocol.UnderlyingSocket.RemoteEndPoint);

                                // Report that a new client connected
                                this.currentTimeout = this.NoDataTimeout * 1000;
                                this.CurrentStage = RequestStages.WaitingForWelcomeMessage;
                            }

                            break;
                        case RequestStages.WaitingForWelcomeMessage:
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

                                // Select Authentication Type
                                if ((byte)this.SelectedAuthMode != clientRequest[0])
                                {
                                    serverErrorCode = 99;
                                }
                                else
                                {
                                    switch (this.SelectedAuthMode)
                                    {
                                        case Common.AuthenticationMethods.None:
                                            Array.Copy(clientRequest, 1, clientRequest, 0, clientRequest.Length - 1);
                                            break;
                                        case Common.AuthenticationMethods.UserPass:
                                            {
                                                // Authentication using user name and password hash
                                                string username = Encoding.ASCII.GetString(
                                                    clientRequest,
                                                    2,
                                                    clientRequest[1]);

                                                // Read UserName
                                                byte[] passwordHash = new byte[clientRequest[clientRequest[1] + 2]];

                                                // Initialize Password Hash Byte Array
                                                Array.Copy(
                                                    clientRequest,
                                                    clientRequest[1] + 3,
                                                    passwordHash,
                                                    0,
                                                    passwordHash.Length); // Read Password Hash

                                                acceptedUser =
                                                    this.Controller.Settings.AthorizedUsers.FirstOrDefault(
                                                        user =>
                                                        user.Username.Equals(
                                                            username,
                                                            StringComparison.OrdinalIgnoreCase)
                                                        && user.Hash.SequenceEqual(passwordHash));

                                                if (acceptedUser == null)
                                                {
                                                    // Let check if we have a fail result with authentication, If so, Close Connection
                                                    serverErrorCode = 99;
                                                }
                                                else
                                                {
                                                    Screen.ChangeUser(this.Username, acceptedUser.Username, this.Id);

                                                    // Let inform that user is changed, We are not "Anonymous" anymore
                                                    this.Username = acceptedUser.Username;

                                                    // Save user name in userId field for later access to screen
                                                    Array.Copy(
                                                        clientRequest,
                                                        clientRequest[1] + passwordHash.Length + 3,
                                                        clientRequest,
                                                        0,
                                                        clientRequest.Length
                                                        - (clientRequest[1] + passwordHash.Length + 3));
                                                }
                                            }
                                            break;
                                    }
                                }

                                string clientRequestedAddress = null;
                                ushort clientRequestedPort = 0;
                                if (serverErrorCode == 0)
                                {
                                    // Authentication OK
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
                                        string clientRequestedConnectionString = clientRequestedAddress.ToLower().Trim()
                                                                                 + ":" + clientRequestedPort;

                                        if (
                                            this.Controller.Settings.BlackListedAddresses.Any(
                                                blacklist =>
                                                Common.DoesMatchWildCard(clientRequestedConnectionString, blacklist)))
                                        {
                                            this.Close("Blacklisted Request: " + clientRequestedAddress);
                                            return;
                                        }
                                    }
                                }

                                // Initialize server response to this request
                                byte[] serverResponse = new byte[2];
                                serverResponse[0] = ServerPeaRoxyVersion;
                                serverResponse[1] = serverErrorCode;
                                this.Protocol.Write(serverResponse, true); // Send response to client

                                if (serverErrorCode != 0 || clientRequestedAddress == null)
                                {
                                    // Check if we have any problem with request
                                    this.Close("5. " + "Response Error, Code: " + serverErrorCode);
                                    return;
                                }

                                if (acceptedUser != null)
                                {
                                    this.Protocol.EncryptionKey = Encoding.ASCII.GetBytes(acceptedUser.Password);
                                }

                                Screen.SetRequestIpAddress(
                                    this.Username,
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
                                                this.Controller.ClientMoveToRouting(this);
                                                this.CurrentStage = RequestStages.Routing;
                                            }
                                            catch (Exception)
                                            {
                                                this.Close();
                                            }
                                        },
                                    null);

                                this.currentTimeout = this.NoDataTimeout * 1000;
                                this.CurrentStage = RequestStages.ConnectingToTheServer;
                            }

                            break;
                        case RequestStages.ResolvingLocalServer:
                            Screen.ChangeUser(this.Username, "Forwarder", this.Id);
                            this.Username = "Forwarder";
                            Screen.SetRequestIpAddress(
                                this.Username,
                                this.Id,
                                this.Controller.Settings.HttpForwardingIp + ":"
                                + this.Controller.Settings.HttpForwardingPort);

                            // Inform that we have a request for an address
                            this.destinationSocket = new Socket(
                                AddressFamily.InterNetwork,
                                SocketType.Stream,
                                ProtocolType.Tcp);
                            this.destinationSocket.BeginConnect(
                                this.Controller.Settings.HttpForwardingIp,
                                this.Controller.Settings.HttpForwardingPort,
                                delegate(IAsyncResult ar)
                                    {
                                        try
                                        {
                                            this.destinationSocket.EndConnect(ar);
                                            this.forwarder = new Forwarder(
                                                this.destinationSocket,
                                                this.underlyingClientReceivePacketSize);
                                            this.forwarder.Write(this.forger.HeaderBytes);
                                            this.currentTimeout = this.NoDataTimeout * 1000;
                                            this.Controller.ClientMoveToRouting(this);
                                            this.CurrentStage = RequestStages.Routing;
                                        }
                                        catch (Exception e)
                                        {
                                            this.Close("9. " + e.Message + "\r\n" + e.StackTrace);
                                        }
                                    },
                                null);

                            this.currentTimeout = this.NoDataTimeout * 1000;
                            this.CurrentStage = RequestStages.ConnectingToTheServer;
                            break;
                        case RequestStages.ConnectingToTheServer:
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
        ///     The close method which supports mentioning a message about the reason
        /// </summary>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="async">
        ///     Indicating if the closing process should treat the client as an asynchronous client
        /// </param>
        public void Close(string message = null, bool async = true)
        {
            if (message != null)
            {
                // If there is a message here, Send it to screen
                Screen.LogMessage(message);
            }

            if (this.Protocol != null)
            {
                this.Protocol.Close(null, async);
            }

            if (this.forwarder != null)
            {
                this.forwarder.Close(async);
            }

            this.Controller.ClientDisconnected(this);
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
                                        this.destinationSocket.Close(); // Close request's connection
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
                        // Close request's connection
                        this.destinationSocket.Close();
                    }
                }
            }
            catch (Exception)
            {
            }

            Screen.ClientDisconnected(this.Username, this.Id); // Inform that we have nothing to do after this.
        }

        /// <summary>
        ///     The method to handle the routing process, should call repeatedly
        /// </summary>
        public void Route()
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
                                Screen.DataReceived(this.Username, this.Id, buffer.Length);

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
                                Screen.DataSent(this.Username, this.Id, buffer.Length);

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
                                Screen.DataSent(this.Username, this.Id, buffer.Length);

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
                                Screen.DataReceived(this.Username, this.Id, buffer.Length);

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
        ///     The write method to write the data to the other end.
        /// </summary>
        /// <param name="bytes">
        ///     The data to write.
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
    }
}