// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Screen.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The screen.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Server
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using PeaRoxy.CommonLibrary;

    #endregion

    /// <summary>
    /// The screen.
    /// </summary>
    internal static class Screen
    {
        #region Static Fields

        /// <summary>
        /// The users collection.
        /// </summary>
        private static readonly Dictionary<string, User> UsersCollection = new Dictionary<string, User>();

        /// <summary>
        /// The error log.
        /// </summary>
        private static StreamWriter errorLog;

        /// <summary>
        /// The log usage.
        /// </summary>
        private static bool usageLogEnable;

        /// <summary>
        /// The usage log address.
        /// </summary>
        private static string usageLogAddress = string.Empty;

        /// <summary>
        /// The last connected client id.
        /// </summary>
        private static int lastConnectedClientId;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The change user.
        /// </summary>
        /// <param name="lastUserId">
        /// The last user id.
        /// </param>
        /// <param name="newUserId">
        /// The new user id.
        /// </param>
        /// <param name="clientId">
        /// The client id.
        /// </param>
        public static void ChangeUser(string lastUserId, string newUserId, int clientId)
        {
            if (!UsersCollection.ContainsKey(lastUserId) || !UsersCollection[lastUserId].Clients.ContainsKey(clientId))
            {
                return;
            }

            lock (UsersCollection)
            {
                if (!UsersCollection.ContainsKey(newUserId))
                {
                    UsersCollection.Add(newUserId, new User(usageLogEnable, usageLogAddress, newUserId));
                }

                User.Client client = UsersCollection[lastUserId].Clients[clientId];
                UsersCollection[lastUserId].Clients.Remove(clientId);
                UsersCollection[newUserId].Clients.Add(clientId, client);
            }
        }

        /// <summary>
        /// The client connected.
        /// </summary>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="remoteIpAddress">
        /// The remote ip address.
        /// </param>
        /// <param name="ipAddress">
        /// The ip address.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int ClientConnected(string userId, string remoteIpAddress, string ipAddress = "")
        {
            lock (UsersCollection)
            {
                lastConnectedClientId++;
                if (!UsersCollection.ContainsKey(userId))
                {
                    UsersCollection.Add(userId, new User(usageLogEnable, usageLogAddress, userId));
                }

                UsersCollection[userId].Clients.Add(
                    lastConnectedClientId, 
                    new User.Client
                        {
                            Id = lastConnectedClientId, 
                            RequestAddress = ipAddress, 
                            RemoteAddress = remoteIpAddress
                        });
            }

            return lastConnectedClientId;
        }

        /// <summary>
        /// The client disconnected.
        /// </summary>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="clientId">
        /// The client id.
        /// </param>
        public static void ClientDisconnected(string userId, int clientId)
        {
            if (!UsersCollection.ContainsKey(userId) || !UsersCollection[userId].Clients.ContainsKey(clientId))
            {
                return;
            }

            User.Client client = UsersCollection[userId].Clients[clientId];
            lock (UsersCollection)
            {
                UsersCollection[userId].Clients.Remove(clientId);
                UsersCollection[userId].ResByteReceived += client.ByteReceived;
                UsersCollection[userId].ResByteSent += client.ByteSent;
            }
        }

        /// <summary>
        /// The data received.
        /// </summary>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="clientId">
        /// The client id.
        /// </param>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        public static void DataReceived(string userId, int clientId, int bytes)
        {
            if (!UsersCollection.ContainsKey(userId) || !UsersCollection[userId].Clients.ContainsKey(clientId))
            {
                return;
            }

            lock (UsersCollection)
            {
                UsersCollection[userId].Clients[clientId].ByteReceived += bytes;
            }
        }

        /// <summary>
        /// The data sent.
        /// </summary>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="clientId">
        /// The client id.
        /// </param>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        public static void DataSent(string userId, int clientId, int bytes)
        {
            if (!UsersCollection.ContainsKey(userId) || !UsersCollection[userId].Clients.ContainsKey(clientId))
            {
                return;
            }

            lock (UsersCollection)
            {
                UsersCollection[userId].Clients[clientId].ByteSent += bytes;
            }
        }

        /// <summary>
        /// The log message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public static void LogMessage(string message)
        {
            if (errorLog != null)
            {
                errorLog.WriteLine(message);
                errorLog.Flush();
            }
        }

        /// <summary>
        /// The set request ip address.
        /// </summary>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="clientId">
        /// The client id.
        /// </param>
        /// <param name="ipAddress">
        /// The ip address.
        /// </param>
        public static void SetRequestIpAddress(string userId, int clientId, string ipAddress)
        {
            if (!UsersCollection.ContainsKey(userId) || !UsersCollection[userId].Clients.ContainsKey(clientId))
            {
                return;
            }

            lock (UsersCollection)
            {
                UsersCollection[userId].Clients[clientId].RequestAddress = ipAddress;
            }
        }

        /// <summary>
        /// The start screen.
        /// </summary>
        /// <param name="logErrors">
        /// The log errors.
        /// </param>
        /// <param name="logUsage">
        /// The log usage.
        /// </param>
        /// <param name="logUsageAddress">
        /// The usage log address.
        /// </param>
        public static void StartScreen(bool logErrors, bool logUsage, string logUsageAddress)
        {
            if (logErrors)
            {
                try
                {
                    errorLog = new StreamWriter("errors.log", true);
                }
                catch (Exception)
                {
                    errorLog = null;
                }
            }

            if (logUsage)
            {
                try
                {
                    Directory.CreateDirectory(logUsageAddress);
                    DirectoryInfo di = new DirectoryInfo(logUsageAddress);
                    di = di.CreateSubdirectory(Process.GetCurrentProcess().Id + "_" + Path.GetRandomFileName());
                    usageLogAddress = di.FullName + "/";
                    usageLogEnable = true;
                }
                catch (Exception)
                {
                    usageLogEnable = false;
                }
            }
        }

        /// <summary>
        /// The re draw.
        /// </summary>
        /// <param name="ctrl">
        /// The ctrl.
        /// </param>
        /// <param name="show">
        /// The show.
        /// </param>
        public static void ReDraw(Controller ctrl, bool show = true)
        {
            ClearConsole();
            try
            {
                long byteReceived = 0;
                long byteSent = 0;
                long downSpeed = 0;
                long upSpeed = 0;
                lock (UsersCollection)
                {
                    for (int i = 0; i < UsersCollection.Values.Count; i++)
                    {
                        User user = UsersCollection.Values.ElementAt(i);
                        user.Update(show);
                        if (show && user.Clients.Count > 0)
                        {
                            Console.WriteLine(
                                "     " + PrintWithSpaceAfter(user.Id) + "Active Connections: "
                                + PrintWithSpaceAfter(user.Clients.Count.ToString(CultureInfo.InvariantCulture), 4)
                                + PrintWithSpaceAfter(Common.FormatFileSizeAsString(user.ByteReceived))
                                + PrintWithSpaceAfter(Common.FormatFileSizeAsString(user.ByteSent))
                                + PrintWithSpaceAfter(Common.FormatFileSizeAsString(user.DownSpeed))
                                + PrintWithSpaceAfter(Common.FormatFileSizeAsString(user.UpSpeed)));
                            Console.WriteLine(
                                PrintWithSpaceAfter("IP", 19) + PrintWithSpaceAfter("REQ", 20)
                                + PrintWithSpaceAfter("DOWN") + PrintWithSpaceAfter("UP")
                                + PrintWithSpaceAfter("DOWN PS") + PrintWithSpaceAfter("UP PS"));
                            try
                            {
                                for (int j = 0; j < user.Clients.Values.Count; j++)
                                {
                                    User.Client client = user.Clients.Values.ElementAt(j);
                                    string remoteAddress = client.RemoteAddress;
                                    if (remoteAddress.Length > 18)
                                    {
                                        remoteAddress = remoteAddress.Substring(0, 18);
                                    }

                                    string requestAddress = client.RequestAddress;
                                    if (requestAddress.Length > 19)
                                    {
                                        requestAddress = requestAddress.Substring(0, 19);
                                    }

                                    Console.WriteLine(
                                        PrintWithSpaceAfter(remoteAddress, 19) + PrintWithSpaceAfter(requestAddress, 20)
                                        + PrintWithSpaceAfter(Common.FormatFileSizeAsString(client.ByteReceived))
                                        + PrintWithSpaceAfter(Common.FormatFileSizeAsString(client.ByteSent))
                                        + PrintWithSpaceAfter(Common.FormatFileSizeAsString(client.DownSpeed))
                                        + PrintWithSpaceAfter(Common.FormatFileSizeAsString(client.UpSpeed)));
                                }
                            }
                            catch (Exception)
                            {
                            }

                            WriteHr();
                            WriteHr(' ');
                        }

                        byteReceived += user.ByteReceived;
                        byteSent += user.ByteSent;
                        downSpeed += user.DownSpeed;
                        upSpeed += user.UpSpeed;
                    }
                }

                Console.WriteLine(
                    "Downloaded: " + PrintWithSpaceAfter(Common.FormatFileSizeAsString(byteReceived)) + "Uploaded: "
                    + PrintWithSpaceAfter(Common.FormatFileSizeAsString(byteSent)) + "Down PS: "
                    + PrintWithSpaceAfter(Common.FormatFileSizeAsString(downSpeed)) + "Up PS: "
                    + PrintWithSpaceAfter(Common.FormatFileSizeAsString(upSpeed)));
                Console.WriteLine(
                    "Accepting Cycle: " + PrintWithSpaceAfter(ctrl.AcceptingCycle.ToString(CultureInfo.InvariantCulture)) + "Routing Cycle: "
                    + PrintWithSpaceAfter(ctrl.RoutingCycle.ToString(CultureInfo.InvariantCulture)));
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The clear console.
        /// </summary>
        private static void ClearConsole()
        {
            Console.Clear();
        }

        /// <summary>
        /// The print with space after.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <param name="afterSpaces">
        /// The after spaces.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string PrintWithSpaceAfter(string text, int afterSpaces = 10)
        {
            afterSpaces = afterSpaces - text.Length;
            for (int i = 0; i < afterSpaces; i++)
            {
                text += " ";
            }

            return text;
        }

        /// <summary>
        /// The write horizontal line.
        /// </summary>
        /// <param name="chr">
        /// The chr.
        /// </param>
        private static void WriteHr(char chr = '-')
        {
            string st = string.Empty;
            for (int i = 0; i < Console.BufferWidth; i++)
            {
                st += chr;
            }

            Console.WriteLine(st);
        }

        #endregion

        /// <summary>
        /// The user.
        /// </summary>
        public class User : IDisposable
        {
            #region Fields

            /// <summary>
            /// The usage log.
            /// </summary>
            private readonly StreamWriter usageLog;

            /// <summary>
            /// The lr c.
            /// </summary>
            private int lrC;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="User"/> class.
            /// </summary>
            /// <param name="logUsage">
            /// The log usage.
            /// </param>
            /// <param name="usageLogAddress">
            /// The usage log address.
            /// </param>
            /// <param name="id">
            /// The id.
            /// </param>
            public User(bool logUsage, string usageLogAddress, string id)
            {
                this.Id = id;
                if (logUsage)
                {
                    this.usageLog = new StreamWriter(usageLogAddress + id + ".log", false);
                }

                this.Clients = new Dictionary<int, Client>();
            }

            #endregion

            #region Public Properties

            /// <summary>
            /// Gets or sets the byte received.
            /// </summary>
            public long ByteReceived { get; set; }

            /// <summary>
            /// Gets or sets the byte sent.
            /// </summary>
            public long ByteSent { get; set; }

            /// <summary>
            /// Gets or sets the clients.
            /// </summary>
            public Dictionary<int, Client> Clients { get; set; }

            /// <summary>
            /// Gets or sets the down speed.
            /// </summary>
            public long DownSpeed { get; set; }

            /// <summary>
            /// Gets or sets the id.
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets the res byte received.
            /// </summary>
            public long ResByteReceived { get; set; }

            /// <summary>
            /// Gets or sets the res byte sent.
            /// </summary>
            public long ResByteSent { get; set; }

            /// <summary>
            /// Gets or sets the up speed.
            /// </summary>
            public long UpSpeed { get; set; }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// The dispose.
            /// </summary>
            public void Dispose()
            {
                if (this.usageLog != null)
                {
                    this.usageLog.Close();
                    this.usageLog.Dispose();
                }
            }

            /// <summary>
            /// The update.
            /// </summary>
            /// <param name="showConnections">
            /// The show connections.
            /// </param>
            public void Update(bool showConnections = false)
            {
                this.ByteReceived = this.ResByteReceived;
                this.ByteSent = this.ResByteSent;
                this.DownSpeed = 0;
                this.UpSpeed = 0;
                if (this.usageLog != null)
                {
                    this.usageLog.BaseStream.SetLength(Math.Max(this.usageLog.BaseStream.Length - 76, 0));
                    this.usageLog.BaseStream.Position = this.usageLog.BaseStream.Length;
                }

                foreach (KeyValuePair<int, Client> client in this.Clients)
                {
                    client.Value.Update();
                    if (showConnections)
                    {
                        if (this.usageLog != null && client.Value.Id > this.lrC
                            && client.Value.RequestAddress != string.Empty)
                        {
                            this.lrC = client.Value.Id;
                            this.usageLog.WriteLine(client.Value.RemoteAddress + " => " + client.Value.RequestAddress);
                        }
                    }

                    this.ByteReceived += client.Value.ByteReceived;
                    this.ByteSent += client.Value.ByteSent;
                    this.DownSpeed += client.Value.DownSpeed;
                    this.UpSpeed += client.Value.UpSpeed;
                }

                if (this.usageLog != null)
                {
                    this.usageLog.Write(
                        "\r\n" + "Downloaded:     "
                        + PrintWithSpaceAfter(Common.FormatFileSizeAsString(this.ByteReceived), 20));
                    this.usageLog.Write(
                        "\r\n" + "Uploaded:       "
                        + PrintWithSpaceAfter(Common.FormatFileSizeAsString(this.ByteSent), 20));
                    this.usageLog.Flush();
                }
            }

            #endregion

            /// <summary>
            /// The client.
            /// </summary>
            public class Client
            {
                #region Fields

                /// <summary>
                /// The last bytes received.
                /// </summary>
                private long oldBytesReceived;

                /// <summary>
                /// The last bytes sent.
                /// </summary>
                private long oldBytesSent;

                /// <summary>
                /// The last speed update.
                /// </summary>
                private int oldSpeedUpdate;

                #endregion

                #region Constructors and Destructors

                /// <summary>
                /// Initializes a new instance of the <see cref="Client"/> class.
                /// </summary>
                public Client()
                {
                    this.Log = new List<string>();
                    this.oldSpeedUpdate = Environment.TickCount;
                }

                #endregion

                #region Public Properties

                /// <summary>
                /// Gets or sets the byte received.
                /// </summary>
                public long ByteReceived { get; set; }

                /// <summary>
                /// Gets or sets the byte sent.
                /// </summary>
                public long ByteSent { get; set; }

                /// <summary>
                /// Gets or sets the down speed.
                /// </summary>
                public long DownSpeed { get; set; }

                /// <summary>
                /// Gets or sets the id.
                /// </summary>
                public int Id { get; set; }

                /// <summary>
                /// Gets or sets the log.
                /// </summary>
                // ReSharper disable once UnusedAutoPropertyAccessor.Global
                public List<string> Log { get; set; }

                /// <summary>
                /// Gets or sets the remote address.
                /// </summary>
                public string RemoteAddress { get; set; }

                /// <summary>
                /// Gets or sets the request address.
                /// </summary>
                public string RequestAddress { get; set; }

                /// <summary>
                /// Gets or sets the up speed.
                /// </summary>
                public long UpSpeed { get; set; }

                #endregion

                #region Public Methods and Operators

                /// <summary>
                /// The update.
                /// </summary>
                public void Update()
                {
                    long bytesReceived = this.ByteReceived - this.oldBytesReceived;
                    long bytesSent = this.ByteSent - this.oldBytesSent;
                    this.oldBytesReceived = this.ByteReceived;
                    this.oldBytesSent = this.ByteSent;
                    double timeE = (Environment.TickCount - this.oldSpeedUpdate) / (double)1000;
                    if (timeE <= 0)
                    {
                        return;
                    }

                    this.oldSpeedUpdate = Environment.TickCount;

                    this.DownSpeed = ((int)(bytesReceived / timeE) + this.DownSpeed) / 2;
                    this.UpSpeed = ((int)(bytesSent / timeE) + this.UpSpeed) / 2;
                }

                #endregion
            }
        }
    }
}