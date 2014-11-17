namespace PeaRoxy.Tests
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PeaRoxy.ClientLibrary;
    using PeaRoxy.ClientLibrary.ServerModules;
    using PeaRoxy.CommonLibrary;
    using PeaRoxy.Server;
    using PeaRoxy.Windows;

    using Common = PeaRoxy.CommonLibrary.Common;

    [TestClass]
    public class PeaRoxyIntegrityTests
    {
        [TestMethod]
        [Timeout(600000)]
        public void StringDownloadTest()
        {
            this.DoTest(new Action<WebProxy>[] { CommonMethods.StringDownloadTest });
        }

        [TestMethod]
        [Timeout(600000)]
        public void SmallFileDownloadTest()
        {
            this.DoTest(new Action<WebProxy>[] { CommonMethods.SmallFileDownloadTest });
        }

        [TestMethod]
        [Timeout(600000)]
        public void BigFileDownloadTest()
        {
            this.DoTest(new Action<WebProxy>[] { CommonMethods.BigFileDownloadTest });
        }

        [TestMethod]
        [Timeout(600000)]
        public void StringUploadTest()
        {
            this.DoTest(new Action<WebProxy>[] { CommonMethods.UploadStringTest });
        }

        private void DoTest(Action<WebProxy>[] toRunActions)
        {
            new WindowsModule().RegisterPlatform();
            const string Username = "PeaRoxy";
            const string Password = "PeaRoxy@2014";
            var combinations =
                from authMethod in
                    Enum.GetValues(typeof(Common.AuthenticationMethods)).Cast<Common.AuthenticationMethods>()
                from serverCompression in
                    Enum.GetValues(typeof(Common.CompressionTypes)).Cast<Common.CompressionTypes>()
                from serverEncryption in Enum.GetValues(typeof(Common.EncryptionTypes)).Cast<Common.EncryptionTypes>()
                from clientCompression in
                    Enum.GetValues(typeof(Common.CompressionTypes)).Cast<Common.CompressionTypes>()
                from clientEncryption in Enum.GetValues(typeof(Common.EncryptionTypes)).Cast<Common.EncryptionTypes>()
                select
                    new
                        {
                            AuthMethod = authMethod,
                            ServerCompression = serverCompression,
                            ServerEncryption = serverEncryption,
                            ClientCompression = clientCompression,
                            ClientEncryption = clientEncryption,
                        };
            foreach (var combination in
                combinations.Where(combination => combination.AuthMethod != Common.AuthenticationMethods.Invalid))
            {
                Trace.WriteLine("Testing: " + combination);
                Settings serverSettings = new Settings
                                              {
                                                  AuthMethod = combination.AuthMethod,
                                                  CompressionType = combination.ServerCompression,
                                                  EncryptionType = combination.ServerEncryption,
                                                  ServerPort = 9980,
                                                  ConfigFileAddress = string.Empty,
                                                  BlackListFileAddress = string.Empty,
                                                  UsersFileAddress = string.Empty,
                                                  ServerIp = IPAddress.Loopback.ToString()
                                              };

                PeaRoxy clientServer = new PeaRoxy(
                    serverSettings.ServerIp,
                    (ushort)serverSettings.ServerPort,
                    string.Empty,
                    serverSettings.AuthMethod != Common.AuthenticationMethods.None ? Username : string.Empty,
                    serverSettings.AuthMethod != Common.AuthenticationMethods.None ? Password : string.Empty,
                    combination.ClientEncryption,
                    combination.ClientCompression);

                if (serverSettings.AuthMethod != Common.AuthenticationMethods.None)
                {
                    serverSettings.AthorizedUsers.Add(new ConfigUser(Username, Password));
                }

                PeaRoxyController server = new PeaRoxyController(serverSettings);
                ProxyController client = new ProxyController(clientServer, IPAddress.Loopback, 0)
                                             {
                                                 IsAutoConfigEnable
                                                     = false
                                             };
                if (!server.Start())
                {
                    throw new Exception("Failed to start the server");
                }
                client.TestServer();
                if (!client.Start())
                {
                    throw new Exception("Failed to start the client");
                }
                WebProxy proxy = CommonMethods.CreateProxy(client.Ip, client.Port);
                foreach (Action<WebProxy> action in toRunActions)
                {
                    action.Invoke(proxy);
                }
                client.Stop();
                server.Stop();
            }
        }
    }
}