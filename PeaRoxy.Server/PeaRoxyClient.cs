using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.Security.Cryptography;
using PeaRoxy.CommonLibrary;
namespace PeaRoxy.Server
{
    public class PeaRoxyClient : IDisposable
    {
        public enum Client_Stage
        {
            Connected,
            WaitingForForger,
            WaitingForWelcomeMessage,
            ConnectingToServer,
            Routing,
            ResolvingLocalServer
        }
        internal PeaRoxy.CoreProtocol.PeaRoxyProtocol Protocol;
        private Common.Encryption_Type UnderlyingClientEncType;
        private Common.Compression_Type UnderlyingClientComType;
        private int UnderlyingClientReceivePacketSize;
        private int UnderlyingClientSendPacketSize;
        private byte[] writeBuffer = new byte[0];
        private CoreProtocol.HTTPForger Forger;
        private HTTPForwarder Forwarder;
        private bool Forwarded = false;
        private Socket destinationSocket = null;
        private int CurrentTimeout = 0;
        private int NoDataTimeout { get; set; }
        private int ClientSupportedEncryptionType { get; set; }
        private int ClientSupportedCompressionType { get; set; }
        private int SelectedAuthMode { get; set; }
        public Client_Stage CurrentStage { get; private set; }
        public Controller Controller { get; private set; }
        public string UserId { get; private set; }
        public int Id { get; private set; }
        public Socket UnderlyingSocket { get; private set; }
        public bool BusyWrite
        {
            get
            {
                if (writeBuffer.Length > 0)
                    this.Write(null);
                return (writeBuffer.Length > 0);
            }
        }
        // Set some variable and init an other thread to start
        public PeaRoxyClient(
                Socket client,
                Controller parent,
                Common.Encryption_Type encType = Common.Encryption_Type.None,
                Common.Compression_Type comType = Common.Compression_Type.None,
                int ReceivePacketSize = 8192,
                int SendPacketSize = 1024,
                int SelectedAuthMode = 255,
                int NoDataTimeout = 6000,
                int ClientSupportedEncryptionType = -1,
                int ClientSupportedCompressionType = -1
            )
        {
            this.UserId = "Anonymous"; // Use Anonymous as temporary user name until client introduce it-self
            this.CurrentStage = Client_Stage.Connected;
            this.SelectedAuthMode = SelectedAuthMode;
            this.NoDataTimeout = NoDataTimeout;
            this.Controller = parent;
            this.UnderlyingSocket = client;
            this.UnderlyingSocket.Blocking = false;
            this.UnderlyingClientEncType = encType;
            this.UnderlyingClientComType = comType;
            this.ClientSupportedEncryptionType = ClientSupportedEncryptionType;
            this.ClientSupportedCompressionType = ClientSupportedCompressionType;
            this.UnderlyingClientReceivePacketSize = ReceivePacketSize;
            this.UnderlyingClientSendPacketSize = SendPacketSize;
            this.CurrentTimeout = this.NoDataTimeout * 1000;
        }

        public void Accepting()
        {
            try
            {
                if (Common.IsSocketConnected(this.UnderlyingSocket) && CurrentTimeout > 0)
                {
                    switch (this.CurrentStage)
                    {
                        case Client_Stage.Connected:
                            CurrentTimeout = NoDataTimeout * 1000;
                            Forger = new CoreProtocol.HTTPForger(UnderlyingSocket, Controller.PeaRoxyDomain, true);
                            this.CurrentStage = Client_Stage.WaitingForForger;
                            break;
                        case Client_Stage.WaitingForForger:
                            bool notRelated = false;
                            bool result = Forger.ReceiveRequest(out notRelated);
                            if (notRelated)
                            {
                                if (Controller.HTTPForwardingPort == 0)
                                    Close();
                                else
                                {
                                    Forwarded = true;
                                    this.CurrentStage = Client_Stage.ResolvingLocalServer;
                                    Id = Screen.ClientConnected(UserId, "F." + UnderlyingSocket.RemoteEndPoint.ToString()); // Report that a new client connected
                                }
                            }
                            else if (result)
                            {
                                CoreProtocol.HTTPForger.SendResponse(UnderlyingSocket);
                                Protocol = new CoreProtocol.PeaRoxyProtocol(UnderlyingSocket, UnderlyingClientEncType, UnderlyingClientComType)
                                {
                                    ReceivePacketSize = UnderlyingClientReceivePacketSize,
                                    SendPacketSize = UnderlyingClientSendPacketSize,
                                    CloseCallback = this.CloseCal,
                                    ClientSupportedCompressionType = (Common.Compression_Type)ClientSupportedCompressionType,
                                    ClientSupportedEncryptionType = (Common.Encryption_Type)ClientSupportedEncryptionType
                                };
                                Id = Screen.ClientConnected(UserId, "C." + Protocol.UnderlyingSocket.RemoteEndPoint.ToString()); // Report that a new client connected
                                CurrentTimeout = NoDataTimeout * 1000;
                                this.CurrentStage = Client_Stage.WaitingForWelcomeMessage;
                            }
                            break;
                        case Client_Stage.WaitingForWelcomeMessage:
                            if (Protocol.isDataAvailable())
                            {
                                byte version = 1;
                                byte server_Aresponse = 0;
                                byte[] client_data = Protocol.Read(); // Read data from client
                                if (client_data == null || client_data.Length == 0)
                                { // Check if data is correct
                                    Close("8. " + "No data received. Connection Timeout.");
                                    return;
                                }

                                ConfigReader.User Accepted_User = null;
                                // Select Auth Type
                                if (this.SelectedAuthMode != 0 && client_data[0] != this.SelectedAuthMode)
                                    server_Aresponse = 99;
                                else if (this.SelectedAuthMode == 0)// Nothing to auth. Just accept user
                                {
                                    if (client_data[0] == 0)
                                        Array.Copy(client_data, 1, client_data, 0, client_data.Length - 1);
                                    else if (client_data[0] == 1)
                                        Array.Copy(client_data, client_data[1] + client_data[client_data[1] + 2] + 3, client_data, 0, client_data.Length - (client_data[1] + client_data[client_data[1] + 2] + 3));
                                }
                                else if (this.SelectedAuthMode == 1) // Auth using user name and password hash
                                {
                                    string username = System.Text.Encoding.ASCII.GetString(client_data, 2, client_data[1]); // Read UserName
                                    byte[] passwordHash = new byte[client_data[client_data[1] + 2]]; // Init Password Hash Byte Array
                                    Array.Copy(client_data, client_data[1] + 3, passwordHash, 0, passwordHash.Length); // Read Password Hash
                                    
                                    
                                    // Search out users to find out if we have this user in users.ini
                                    foreach (ConfigReader.User user in ConfigReader.GetUsers())
                                        if (user.Username.ToLower() == username.ToLower() && user.Hash.SequenceEqual(passwordHash)) // Check each user name and password hash
                                        {
                                            Accepted_User = user;
                                            break;
                                        }

                                    if (Accepted_User == null) // Let check if we have a fail result with auth, If so, Close Connection
                                    {
                                        server_Aresponse = 99;
                                    }
                                    else
                                    {
                                        Screen.ChangeUser(this.UserId, Accepted_User.Username, this.Id); // Let inform that user is changed, We are not "Anonymous" anymore
                                        this.UserId = Accepted_User.Username; // Save user name in userId field for later access to screen
                                        Array.Copy(client_data, client_data[1] + passwordHash.Length + 3, client_data, 0, client_data.Length - (client_data[1] + passwordHash.Length + 3));
                                    }
                                }

                                string client_connectionAddress = null;
                                ushort client_connectionPort = 0;
                                if (server_Aresponse == 0)
                                {
                                    // ------------- Establishing connection
                                    if (client_data == null || client_data.Length <= 3)
                                    { // If response is acceptable
                                        Close("7. " + "No request received. Connection Timeout.");
                                        return;
                                    }

                                    if (client_data[0] != version)
                                    { // Check again if client use same version as we are
                                        Close(); //"6. " + "Unknown version, Expected " + version.ToString());
                                        return;
                                    }

                                    byte client_addressType = client_data[3]; // Read address type client want to connect

                                    byte[] client_unformatedConnectionAddress;
                                    switch (client_addressType) // Getting request address and port depending to address type
                                    {
                                        case 1: // IPv4
                                            client_unformatedConnectionAddress = new byte[4];
                                            Array.Copy(client_data, 4, client_unformatedConnectionAddress, 0, 4);
                                            client_connectionAddress = new IPAddress(client_unformatedConnectionAddress).ToString();
                                            client_connectionPort = (ushort)((ushort)(client_data[8] * 256) + (ushort)client_data[9]);
                                            break;
                                        case 3: // Domain Name
                                            client_unformatedConnectionAddress = new byte[client_data[4]];
                                            Array.Copy(client_data, 5, client_unformatedConnectionAddress, 0, client_data[4]);
                                            client_connectionAddress = System.Text.Encoding.ASCII.GetString(client_unformatedConnectionAddress);
                                            client_connectionPort = (ushort)((ushort)(client_data[(5 + client_data[4])] * 256) + (ushort)client_data[(5 + client_data[4] + 1)]);
                                            break;
                                        case 4: // IPv6
                                            client_unformatedConnectionAddress = new byte[16];
                                            Array.Copy(client_data, 4, client_unformatedConnectionAddress, 0, 16);
                                            client_connectionAddress = new IPAddress(client_unformatedConnectionAddress).ToString();
                                            client_connectionPort = (ushort)((ushort)(client_data[20] * 256) + (ushort)client_data[21]);
                                            break;
                                        default:
                                            server_Aresponse = 8; // This type of address is not supported
                                            break;
                                    }

                                    foreach (string blacklist in ConfigReader.GetBlackList())
                                        if (blacklist.ToLower() == client_connectionAddress.ToLower().Trim())
                                        {
                                            Close(" Blacklisted: " + client_connectionAddress);
                                            return;
                                        }
                                }

                                // Init server response to this request
                                byte[] server_response = new byte[2];
                                server_response[0] = version;
                                server_response[1] = server_Aresponse;
                                Protocol.Write(server_response, true); // Send response to client

                                if (server_Aresponse != 0) // Check if we have any problem with request
                                {
                                    Close("5. " + "response Error, Code: " + server_Aresponse.ToString());
                                    return;
                                }

                                if (Accepted_User != null)
                                    Protocol.EncryptionKey = System.Text.Encoding.ASCII.GetBytes(Accepted_User.Password);

                                Screen.SetRequestIPAddress(this.UserId, this.Id, client_connectionAddress + ":" + client_connectionPort); // Inform that we have a request for an address

                                destinationSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                destinationSocket.BeginConnect(client_connectionAddress, (int)client_connectionPort, (AsyncCallback)delegate(IAsyncResult ar)
                                {
                                    try
                                    {
                                        destinationSocket.EndConnect(ar);
                                        destinationSocket.Blocking = false;
                                        CurrentTimeout = NoDataTimeout * 1000;
                                        this.Controller.iMoveToQ(this);
                                        this.CurrentStage = Client_Stage.Routing;
                                    }
                                    catch (Exception)
                                    {
                                        Close();
                                    }
                                }, null);

                                CurrentTimeout = NoDataTimeout * 1000;
                                this.CurrentStage = Client_Stage.ConnectingToServer;
                            }
                            break;
                        case Client_Stage.ResolvingLocalServer:
                            Screen.ChangeUser(this.UserId, "Forwarder", this.Id);
                            this.UserId = "Forwarder";
                            Screen.SetRequestIPAddress(this.UserId, this.Id, Controller.HTTPForwardingIP.ToString() + Controller.HTTPForwardingPort.ToString()); // Inform that we have a request for an address
                            destinationSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            destinationSocket.BeginConnect(Controller.HTTPForwardingIP, Controller.HTTPForwardingPort, (AsyncCallback)delegate(IAsyncResult ar)
                            {
                                try
                                {
                                    destinationSocket.EndConnect(ar);
                                    Forwarder = new HTTPForwarder(destinationSocket, this.UnderlyingClientReceivePacketSize, this.UnderlyingClientSendPacketSize);
                                    Forwarder.Write(Forger.HeaderBytes);
                                    CurrentTimeout = NoDataTimeout * 1000;
                                    this.Controller.iMoveToQ(this);
                                    this.CurrentStage = Client_Stage.Routing;
                                }
                                catch (Exception e)
                                {
                                    Close("9. " + e.Message + "\r\n" + e.StackTrace);
                                }
                            }, null);

                            CurrentTimeout = NoDataTimeout * 1000;
                            this.CurrentStage = Client_Stage.ConnectingToServer;
                            break;
                        case Client_Stage.ConnectingToServer:
                            break;
                    }
                    CurrentTimeout--;
                }
                else
                    Close(null, true);
            }
            catch (Exception e)
            {
                if (e.TargetSite.Name == "Receive")
                    Close();
                else
                    Close("4. " + e.Message + "\r\n" + e.StackTrace);
            }
        }

        public void DoRoute()
        {
            try
            {
                if (!this.Forwarded)
                {
                    if ((Protocol.BusyWrite || this.BusyWrite || (Common.IsSocketConnected(destinationSocket) && Common.IsSocketConnected(Protocol.UnderlyingSocket))) && CurrentTimeout > 0) // While we have both sides connected and no timeout happened
                    {
                        if (!Protocol.BusyWrite && destinationSocket.Available > 0) // If any new data in request connection
                        {
                            CurrentTimeout = NoDataTimeout * 1000; // Reset timeout variable
                            byte[] buffer = this.Read();
                            if (buffer != null && buffer.Length > 0)
                            { // If we have any data
                                Protocol.Write(buffer, true); // Write data to client
                                Screen.DataReceived(this.UserId, this.Id, buffer.Length); // Inform that we have received new data
                            }
                        }

                        if (!this.BusyWrite && Protocol.isDataAvailable()) // If any new data in client connection
                        {
                            CurrentTimeout = NoDataTimeout * 1000; // Reset timeout variable
                            byte[] buffer = Protocol.Read(); // Read data from client
                            if (buffer != null && buffer.Length > 0)
                            { // If we have any data
                                this.Write(buffer);
                                Screen.DataSent(this.UserId, this.Id, buffer.Length); // Inform that we have Sent new data
                            }
                        }
                        CurrentTimeout--;
                    }
                    else
                        Close(null, false);
                }
                else
                {
                    if (Common.IsSocketConnected(destinationSocket) && Common.IsSocketConnected(Forwarder.UnderlyingSocket) && CurrentTimeout > 0) // While we have both sides connected and no timeout happened
                    {
                        if (!Forwarder.BusyWrite && destinationSocket.Available > 0) // If any new data in request connection
                        {
                            CurrentTimeout = NoDataTimeout * 1000; // Reset timeout variable
                            byte[] buffer = this.Read();
                            if (buffer != null && buffer.Length > 0)
                            { // If we have any data
                                Forwarder.Write(buffer); // Write data to client
                                Screen.DataSent(this.UserId, this.Id, buffer.Length); // Inform that we have received new data
                            }
                        }

                        if (!this.BusyWrite && Forwarder.UnderlyingSocket.Available > 0) // If any new data in client connection
                        {
                            CurrentTimeout = NoDataTimeout * 1000; // Reset timeout variable
                            byte[] buffer = Forwarder.Read(); // Read data from client
                            if (buffer != null && buffer.Length > 0)
                            { // If we have any data
                                this.Write(buffer);
                                Screen.DataReceived(this.UserId, this.Id, buffer.Length); // Inform that we have Sent new data
                            }
                        }
                        CurrentTimeout--;
                    }
                    else
                        Close(null, false);
                }
            }
            catch (Exception e)
            {
                Close("3. " + e.Message + "\r\n" + e.StackTrace);
            }
        }

        public void Write(byte[] bytes)
        {
            try
            {
                if (bytes != null)
                {
                    Array.Resize(ref writeBuffer, writeBuffer.Length + bytes.Length);
                    Array.Copy(bytes, 0, writeBuffer, writeBuffer.Length - bytes.Length, bytes.Length);
                }
                if (writeBuffer.Length > 0 && destinationSocket.Poll(0, SelectMode.SelectWrite))
                {
                    int bytesWritten = destinationSocket.Send(writeBuffer, SocketFlags.None);
                    Array.Copy(writeBuffer, bytesWritten, writeBuffer, 0, writeBuffer.Length - bytesWritten);
                    Array.Resize(ref writeBuffer, writeBuffer.Length - bytesWritten);
                }
            }
            catch (Exception e)
            {
                Close("2. " + e.Message + "\r\n" + e.StackTrace);
            }
        }

        public byte[] Read()
        {
            try
            {
                int i = 60; // Time out by second

                i = i * 100;
                while (i > 0)
                {
                    if (destinationSocket.Available > 0)
                    {
                        byte[] buffer = new byte[this.UnderlyingClientReceivePacketSize];
                        int bytes = destinationSocket.Receive(buffer);
                        Array.Resize(ref buffer, bytes);
                        return buffer;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(10);
                        i--;
                    }
                }
            }
            catch (Exception e)
            {
                Close("1. " + e.Message + "\r\n" + e.StackTrace);
            }
            return null;
        }

        public void CloseCal(string message = null, bool async = true)
        {
            Close(message, async);
        }

        public void Close(string message = null, bool async = true)
        {
            if (message != null) // If we have message with this function, Send it to screen
                Screen.LogMessage(message);

            if (Protocol != null)
                Protocol.Close(null, async);

            if (Forwarder != null)
                Forwarder.Close(null, async);

            Controller.iDissconnected(this);
            try
            {
                if (async)
                {
                    byte[] db = new byte[0];
                    if (destinationSocket != null)
                        destinationSocket.BeginSend(db, 0, db.Length, SocketFlags.None, (AsyncCallback)delegate(IAsyncResult ar)
                        {
                            try
                            {
                                destinationSocket.Close(); // Close request connection it-self
                                destinationSocket.EndSend(ar);
                            }
                            catch (Exception) { }
                        }, null);
                }
                else
                {
                    if (destinationSocket != null) // Close request connection it-self
                        destinationSocket.Close();
                }
            }
            catch (Exception) { }
            Screen.ClientDisconnected(this.UserId, this.Id); // Inform that we have nothing to do after this.
        }

        public void Dispose()
        {
            if (this.UnderlyingSocket != null)
                this.UnderlyingSocket.Dispose();
        }
    }
}
