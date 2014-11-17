namespace PeaRoxy.Tests
{
    using System;
    using System.Net;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PeaRoxy.ClientLibrary;
    using PeaRoxy.ClientLibrary.ServerModules;
    using PeaRoxy.Windows;

    [TestClass]
    public class NoServerTests
    {
        [TestMethod]
        [Timeout(600000)]
        public void NoServerTest()
        {
            new WindowsModule().RegisterPlatform();

            NoServer clientServer = new NoServer();

            ProxyController client = new ProxyController(clientServer, IPAddress.Loopback, 0)
                                         {
                                             IsAutoConfigEnable =
                                                 false
                                         };
            client.TestServer();
            if (!client.Start())
            {
                throw new Exception("Failed to start the client");
            }

            WebProxy proxy = CommonMethods.CreateProxy(client.Ip, client.Port);
            CommonMethods.StringDownloadTest(proxy);
            CommonMethods.SmallFileDownloadTest(proxy);
            CommonMethods.BigFileDownloadTest(proxy);
            CommonMethods.UploadStringTest(proxy);
            client.Stop();
        }
    }
}