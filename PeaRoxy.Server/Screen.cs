// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Screen.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Server
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using PeaRoxy.CommonLibrary;

    internal static class Screen
    {
        private static readonly Dictionary<string, User> UsersCollection = new Dictionary<string, User>();

        private static StreamWriter errorLog;

        private static bool usageLogEnable;

        private static string usageLogAddress = string.Empty;

        private static int lastConnectedClientId;

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

        public static void LogMessage(string message)
        {
            if (errorLog != null)
            {
                errorLog.WriteLine(DateTime.Now + " - " + message);
                errorLog.Flush();
            }
        }

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

        public static void StartScreen(bool logErrors, bool logUsage, string logUsageAddress)
        {
            if (logErrors)
            {
                try
                {
                    errorLog = new StreamWriter(Process.GetCurrentProcess().Id + "_" + "errors.log", true);
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

        public static void ReDraw(PeaRoxyController ctrl, bool show = true)
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
                    "Accepting Cycle: "
                    + PrintWithSpaceAfter(ctrl.AcceptingCycle.ToString(CultureInfo.InvariantCulture))
                    + "Routing Cycle: " + PrintWithSpaceAfter(ctrl.RoutingCycle.ToString(CultureInfo.InvariantCulture)));
            }
            catch (Exception)
            {
            }
        }

        private static void ClearConsole()
        {
            Console.Clear();
        }

        private static string PrintWithSpaceAfter(string text, int afterSpaces = 10)
        {
            afterSpaces = afterSpaces - text.Length;
            for (int i = 0; i < afterSpaces; i++)
            {
                text += " ";
            }

            return text;
        }

        private static void WriteHr(char chr = '-')
        {
            string st = string.Empty;
            for (int i = 0; i < Console.BufferWidth; i++)
            {
                st += chr;
            }

            Console.WriteLine(st);
        }

        public class User : IDisposable
        {
            private readonly StreamWriter usageLog;

            private int lrC;

            public User(bool logUsage, string usageLogAddress, string id)
            {
                this.Id = id;
                if (logUsage)
                {
                    this.usageLog = new StreamWriter(usageLogAddress + id + ".log", false);
                }

                this.Clients = new Dictionary<int, Client>();
            }

            public long ByteReceived { get; set; }

            public long ByteSent { get; set; }

            public Dictionary<int, Client> Clients { get; set; }

            public long DownSpeed { get; set; }

            public string Id { get; set; }

            public long ResByteReceived { get; set; }

            public long ResByteSent { get; set; }

            public long UpSpeed { get; set; }

            public void Dispose()
            {
                if (this.usageLog != null)
                {
                    this.usageLog.Close();
                    this.usageLog.Dispose();
                }
            }

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

            public class Client
            {
                private long oldBytesReceived;

                private long oldBytesSent;

                private int oldSpeedUpdate;

                public Client()
                {
                    this.Log = new List<string>();
                    this.oldSpeedUpdate = Environment.TickCount;
                }

                public long ByteReceived { get; set; }

                public long ByteSent { get; set; }

                public long DownSpeed { get; set; }

                public int Id { get; set; }

                public List<string> Log { get; set; }

                public string RemoteAddress { get; set; }

                public string RequestAddress { get; set; }

                public long UpSpeed { get; set; }

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
            }
        }
    }
}