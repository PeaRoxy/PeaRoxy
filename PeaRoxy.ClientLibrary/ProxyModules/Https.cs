// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Https.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary.ProxyModules
{
    using System;
    using System.Text;

    using PeaRoxy.ClientLibrary.ServerModules;

    /// <summary>
    ///     The HTTPS Proxy Handler Module
    /// </summary>
    internal static class Https
    {
        public static void DirectHandle(
            ProxyClient client,
            string clientConnectionAddress,
            ushort clientConnectionPort,
            byte[] firstResponse)
        {
            if (client.IsSmartForwarderEnable && client.Controller.SmartPear.ForwarderHttpsEnable)
            {
                // Is Forwarder is Enable and Client need to be forwarded by NoServer
                ServerType ac = new NoServer { NoDataTimeout = client.Controller.ActiveServer.NoDataTimeout };
                if (client.Controller.SmartPear.DetectorTimeoutEnable)
                {
                    // If we have Timeout Detector then let change timeout
                    ac.NoDataTimeout = client.Controller.SmartPear.DetectorTimeout;
                }

                client.SmartDataSentCallbackForDirectConnections(firstResponse, false);
                ac.Establish(
                    clientConnectionAddress,
                    clientConnectionPort,
                    client,
                    firstResponse,
                    (ref byte[] data, ServerType thisactiveServer, ProxyClient thisclient) =>
                    thisclient.SmartDataSentCallbackForDirectConnections(data, false),
                    (ref byte[] data, ServerType thisactiveServer, ProxyClient thisclient) =>
                    thisclient.SmartDataReceivedCallbackForDirrectConnections(ref data, thisactiveServer, false),
                    (success, thisactiveServer, thisclient) =>
                    thisclient.SmartStatusCallbackForDirectConnections(thisactiveServer, success, false));
            }
            else
            {
                // Forwarder is Disable or Client is in proxy connection mode
                ServerType ac = client.Controller.ActiveServer.Clone();
                if (client.Controller.SmartPear.ForwarderHttpsEnable)
                {
                    // If we have forwarding enabled then we need to have a receive callback
                    ac.Establish(
                        clientConnectionAddress,
                        clientConnectionPort,
                        client,
                        firstResponse,
                        null,
                        (ref byte[] data, ServerType thisactiveServer, ProxyClient thisclient) =>
                        thisclient.SmartDataReceivedCallbackForDirrectConnections(ref data, thisactiveServer, false));
                }
                else
                {
                    // If we don't so SmartPear is disabled
                    ac.Establish(clientConnectionAddress, clientConnectionPort, client, firstResponse);
                }
            }
        }

        public static void Handle(byte[] firstResponse, ProxyClient client)
        {
            if (!client.Controller.IsHttpsSupported || !IsHttps(firstResponse))
            {
                client.Close();
                return;
            }

            string textData = Encoding.ASCII.GetString(firstResponse);

            // Getting Information From HTTPS Header
            string[] parts = textData.Split('\n')[0].Split(' ');
            Uri url = new Uri("https://" + parts[1].Trim());

            string clientConnectionAddress = url.Host;
            ushort clientConnectionPort = (ushort)url.Port;
            client.RequestAddress = "https://" + clientConnectionAddress + ":" + clientConnectionPort;
            if (textData.ToUpper().IndexOf("FASTCONNECT ", StringComparison.OrdinalIgnoreCase) != 0)
            {
                byte[] serverResponse = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
                client.Write(serverResponse);
                firstResponse = new byte[0];
            }
            else
            {
                int endLine = textData.IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4;
                Array.Copy(firstResponse, endLine, firstResponse, 0, firstResponse.Length - endLine);
                Array.Resize(ref firstResponse, firstResponse.Length - endLine);
            }

            client.IsReceivingStarted = false;
            if (client.Controller.Status.HasFlag(ProxyController.ControllerStatus.Proxy))
            {
                DirectHandle(client, clientConnectionAddress, clientConnectionPort, firstResponse);
            }
            else
            {
                client.Close(
                    "Currently we only serve AutoConfig files. Try restarting your browser.",
                    null,
                    ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
            }
        }

        public static bool IsHttps(byte[] firstresponse)
        {
            string textData = Encoding.ASCII.GetString(firstresponse);
            if (textData.IndexOf("\r\n\r\n", StringComparison.Ordinal) == -1)
            {
                return false;
            }

            return textData.ToUpper().IndexOf("CONNECT ", StringComparison.OrdinalIgnoreCase) == 0
                   || textData.ToUpper().IndexOf("FASTCONNECT ", StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}