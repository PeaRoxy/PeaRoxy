using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeaRoxy.CommonLibrary;
namespace PeaRoxy.Server
{
    class Screen
    {
        static int lastConnectedClientId = 0;
        static Dictionary<string,User> usersCollection = new Dictionary<string,User>();
        static System.IO.StreamWriter ErrorLog = null;
        static bool LogUsage = false;
        static string UsageLogAddress = string.Empty;
        public static void StartScreen(bool LogErrors, bool LogUsage, string UsageLogAddress)
        {
            if (LogErrors)
                try
                {
                    ErrorLog = new System.IO.StreamWriter("errors.log", true);
                }
                catch (Exception)
                {
                    ErrorLog = null;
                }

           
            if (LogUsage)
                try
                {
                    System.IO.Directory.CreateDirectory(UsageLogAddress);
                    System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(UsageLogAddress);
                    di = di.CreateSubdirectory(System.Diagnostics.Process.GetCurrentProcess().Id + "_" + System.IO.Path.GetRandomFileName());
                    Screen.UsageLogAddress = di.FullName + "/";
                    Screen.LogUsage = true;
                }
                catch (Exception)
                {
                    Screen.LogUsage = false;
                }
        }

        public static int ClientConnected(string UserId, string RemoteIPAddress, string IPAddress = "")
        {
            lock (usersCollection)
            {
                lastConnectedClientId++;
                if (!usersCollection.ContainsKey(UserId))
                    usersCollection.Add(UserId, new User(Screen.LogUsage, Screen.UsageLogAddress) { Id = UserId });

                usersCollection[UserId].Clients.Add(lastConnectedClientId, new User.Client() { Id = lastConnectedClientId, RequestAddress = IPAddress, RemoteAddress = RemoteIPAddress });
            }
            return lastConnectedClientId;

        }

        public static void SetRequestIPAddress(string UserId, int ClientId, String IPAddress)
        {
            if (!usersCollection.ContainsKey(UserId) || !usersCollection[UserId].Clients.ContainsKey(ClientId))
                return;
            lock (usersCollection)
            {
                usersCollection[UserId].Clients[ClientId].RequestAddress = IPAddress;
            }
        }

        public static void ClientDisconnected(string UserId, int ClientId)
        {
            if (!usersCollection.ContainsKey(UserId) || !usersCollection[UserId].Clients.ContainsKey(ClientId))
                return;

            User.Client client = usersCollection[UserId].Clients[ClientId];
            lock (usersCollection)
            {
                usersCollection[UserId].Clients.Remove(ClientId);
                usersCollection[UserId].ResByteReceived += client.ByteReceived;
                usersCollection[UserId].ResByteSent += client.ByteSent;
            }
        }

        public static void ChangeUser(string LastUserId, string NewUserId,int ClientId)
        {
            if (!usersCollection.ContainsKey(LastUserId) || !usersCollection[LastUserId].Clients.ContainsKey(ClientId))
                return;

            lock (usersCollection)
            {
                if (!usersCollection.ContainsKey(NewUserId))
                    usersCollection.Add(NewUserId, new User(Screen.LogUsage, Screen.UsageLogAddress) { Id = NewUserId });

                User.Client client = usersCollection[LastUserId].Clients[ClientId];
                usersCollection[LastUserId].Clients.Remove(ClientId);
                usersCollection[NewUserId].Clients.Add(ClientId, client);
            }
        }


        public static void LogMessage(string Message)
        {
            if (ErrorLog != null)
            {
                ErrorLog.WriteLine(Message);
                ErrorLog.Flush();
            }
        }

        public static void DataReceived(string UserId, int ClientId, int Bytes)
        {
            if (!usersCollection.ContainsKey(UserId) || !usersCollection[UserId].Clients.ContainsKey(ClientId))
                return;

            lock (usersCollection)
            {
                usersCollection[UserId].Clients[ClientId].ByteReceived += Bytes;
            }
        }

        public static void DataSent(string UserId, int ClientId, int Bytes)
        {
            if (!usersCollection.ContainsKey(UserId) || !usersCollection[UserId].Clients.ContainsKey(ClientId))
                return;

            lock (usersCollection)
            {
                usersCollection[UserId].Clients[ClientId].ByteSent += Bytes;
            }
        }

        private static string PrintWithSpaceAfter(string text, int afterSpaces = 10)
        {
            afterSpaces = afterSpaces - text.Length;
            for (int i = 0; i < afterSpaces; i++)
                text += " ";
            return text;
        }

        private static void WriteHR(char chr = '-')
        {
            string st = string.Empty;
            for (int i = 0; i < Console.BufferWidth; i++)
                st += chr;

            Console.WriteLine(st);
        }
        private static void ClearConsole()
        {
                Console.Clear();
        }

        public static void reDraw(Controller ctrl,bool show = true)
        {
            ClearConsole();
            try
            {
                long ByteReceived = 0;
                long ByteSent = 0;
                long DownSpeed = 0;
                long UpSpeed = 0;
                lock (usersCollection)
                {
                    for (int i = 0; i < usersCollection.Values.Count; i++)
                    {
                        User user = usersCollection.Values.ElementAt(i);
                        user.Update(show);
                        if (show && user.Clients.Count > 0)
                        {
                            Console.WriteLine("     " + PrintWithSpaceAfter(user.Id) + "Active Connections: " + PrintWithSpaceAfter(user.Clients.Count.ToString(), 4) + PrintWithSpaceAfter(Common.FormatFileSizeAsString(user.ByteReceived).ToString()) +
                                                            PrintWithSpaceAfter(Common.FormatFileSizeAsString(user.ByteSent).ToString()) + PrintWithSpaceAfter(Common.FormatFileSizeAsString(user.DownSpeed).ToString()) +
                                                            PrintWithSpaceAfter(Common.FormatFileSizeAsString(user.UpSpeed).ToString()));
                            Console.WriteLine(PrintWithSpaceAfter("IP", 19) + PrintWithSpaceAfter("REQ", 20) + PrintWithSpaceAfter("DOWN") +
                                                            PrintWithSpaceAfter("UP") + PrintWithSpaceAfter("DOWN PS") + PrintWithSpaceAfter("UP PS"));
                            try
                            {
                                for (int j = 0; j < user.Clients.Values.Count; j++)
                                {
                                    User.Client client = user.Clients.Values.ElementAt(j);
                                    string remoteAddress = client.RemoteAddress;
                                    if (remoteAddress.Length > 18)
                                        remoteAddress = remoteAddress.Substring(0, 18);
                                    string requestAddress = client.RequestAddress;
                                    if (requestAddress.Length > 19)
                                        requestAddress = requestAddress.Substring(0, 19);
                                    Console.WriteLine(PrintWithSpaceAfter(remoteAddress, 19) + PrintWithSpaceAfter(requestAddress, 20) + PrintWithSpaceAfter(Common.FormatFileSizeAsString(client.ByteReceived).ToString()) +
                                                                    PrintWithSpaceAfter(Common.FormatFileSizeAsString(client.ByteSent).ToString()) + PrintWithSpaceAfter(Common.FormatFileSizeAsString(client.DownSpeed).ToString()) +
                                                                    PrintWithSpaceAfter(Common.FormatFileSizeAsString(client.UpSpeed).ToString()));
                                }
                            }
                            catch (Exception) { }
                            WriteHR();
                            WriteHR(' ');
                        }
                        ByteReceived += user.ByteReceived;
                        ByteSent += user.ByteSent;
                        DownSpeed += user.DownSpeed;
                        UpSpeed += user.UpSpeed;
                    }
                }
                Console.WriteLine("Downloaded: " + PrintWithSpaceAfter(Common.FormatFileSizeAsString(ByteReceived).ToString()) +
                "Uploaded: " + PrintWithSpaceAfter(Common.FormatFileSizeAsString(ByteSent).ToString()) +
                "Down PS: " + PrintWithSpaceAfter(Common.FormatFileSizeAsString(DownSpeed).ToString()) +
                "Up PS: " + PrintWithSpaceAfter(Common.FormatFileSizeAsString(UpSpeed).ToString()));
                Console.WriteLine("Accepting Cycle: " + PrintWithSpaceAfter(ctrl.AcceptingCycle.ToString()) +
                                    "Routing Cycle: " + PrintWithSpaceAfter(ctrl.RoutingCycle.ToString()));
            }
            catch (Exception) { }
        }

        public class User : IDisposable
	    {
            public string Id { get; set; }
            public Dictionary<int, Client> Clients { get; set; }
            public long ByteReceived { get; set; }
            public long ByteSent { get; set; }
            public long ResByteReceived { get; set; }
            public long ResByteSent { get; set; }
            public long DownSpeed { get; set; }
            public long UpSpeed { get; set; }
            private System.IO.StreamWriter UsageLog = null;
            private int lrC = 0;
            public User(bool LogUsage, string UsageLogAddress)
            {
                if (LogUsage)
                    UsageLog = new System.IO.StreamWriter(UsageLogAddress + Id + ".log", false);
                Clients = new Dictionary<int, Client>();
            }
            public void Update(bool showConnections = false)
            {
                this.ByteReceived = this.ResByteReceived;
                this.ByteSent = this.ResByteSent;
                this.DownSpeed = 0;
                this.UpSpeed = 0;
                if (UsageLog != null)
                {
                    UsageLog.BaseStream.SetLength(Math.Max(UsageLog.BaseStream.Length - 76, 0));
                    UsageLog.BaseStream.Position = UsageLog.BaseStream.Length;
                }
                foreach (KeyValuePair<int, User.Client> client in this.Clients)
                {
                    client.Value.Update();
                    if (showConnections)
                        if (UsageLog != null && client.Value.Id > lrC && client.Value.RequestAddress != string.Empty)
                        {
                            lrC = client.Value.Id;
                            UsageLog.WriteLine(client.Value.RemoteAddress + " => " + client.Value.RequestAddress);
                        }
                    this.ByteReceived += client.Value.ByteReceived;
                    this.ByteSent += client.Value.ByteSent;
                    this.DownSpeed += client.Value.DownSpeed;
                    this.UpSpeed += client.Value.UpSpeed;
                }
                if (UsageLog != null)
                {
                    UsageLog.Write("\r\n" + "Downloaded:     " + PrintWithSpaceAfter(Common.FormatFileSizeAsString(this.ByteReceived),20));
                    UsageLog.Write("\r\n" + "Uploaded:       " + PrintWithSpaceAfter(Common.FormatFileSizeAsString(this.ByteSent),20));
                    UsageLog.Flush();
                }
            }
            public class Client
            {
                public int Id { get; set; }
                public long ByteReceived { get; set; }
                public long ByteSent { get; set; }
                public long DownSpeed { get; set; }
                public long UpSpeed { get; set; }
                private long lastBytesReceived = 0;
                private long lastBytesSent = 0;
                private int lastSpeedUpdate = 0;
                public List<string> Log { get; set; }
                public string RemoteAddress { get; set; }
                public string RequestAddress { get; set; }
                public Client()
                {
                    Log = new List<string>();
                    lastSpeedUpdate = Environment.TickCount;
                }
                public void Update()
                {
                    long BytesR = this.ByteReceived - this.lastBytesReceived;
                    long BytesS = this.ByteSent - this.lastBytesSent;
                    this.lastBytesReceived = this.ByteReceived;
                    this.lastBytesSent = this.ByteSent;
                    double timeE = (double)(Environment.TickCount - lastSpeedUpdate) / (double)1000;
                    if (timeE <= 0)
                        return;
                    lastSpeedUpdate = Environment.TickCount;

                    DownSpeed = ((int)(BytesR / timeE) + DownSpeed) / 2;
                    UpSpeed = ((int)(BytesS / timeE) + UpSpeed) / 2;
                }
            }

            public void Dispose()
            {
                if (this.UsageLog != null)
                    this.UsageLog.Close(); this.UsageLog.Dispose();
            }
        }
    }
}
