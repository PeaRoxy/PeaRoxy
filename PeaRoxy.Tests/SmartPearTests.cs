using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PeaRoxy.Tests
{
    using System.Net;

    using PeaRoxy.ClientLibrary;
    using PeaRoxy.ClientLibrary.ServerModules;
    using PeaRoxy.Windows;

    [TestClass]
    public class SmartPearTests
    {
        [TestMethod]
        public void TestHttpDetection()
        {
            ProxyController client = InitClient();
            WebProxy proxy = CommonMethods.CreateProxy(client.Ip, client.Port);
            bool? success = null;
            client.SmartPear.DetectorHttpCheckEnable = true;
            client.SmartPear.DetectorHttpMaxBuffering = 100;
            client.SmartPear.DetectorHttpPattern = "^HTTP/1.1 403 FORBIDDEN*";
            client.SmartPear.ForwarderListUpdated += delegate(string rule, bool direct, EventArgs args)
                { success = rule == ""; };
            using (WebClient webclient = new WebClient { Proxy = proxy })
            {
                try
                {
                    string result = webclient.DownloadString("http://httpbin.org/status/403");
                }
                catch (Exception e)
                {
                    if (!(e is WebException))
                    {
                        throw;
                    }
                }
            }

            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            while (success == null)
            {
                System.Threading.Thread.Sleep(100);
            }
            client.Stop();
        }
        public ProxyController InitClient()
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
            return client;
        }
    }
}
