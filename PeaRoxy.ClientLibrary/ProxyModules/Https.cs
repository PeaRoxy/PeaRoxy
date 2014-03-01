// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Https.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The proxy_ https.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary.ProxyModules
{
    #region

    using System;
    using System.Text;

    using PeaRoxy.ClientLibrary.ServerModules;

    #endregion

    /// <summary>
    /// The https proxy module
    /// </summary>
    internal static class Https
    {
        #region Public Methods and Operators

        /// <summary>
        /// The direct handle.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="clientConnectionAddress">
        /// The client connection address.
        /// </param>
        /// <param name="clientConnectionPort">
        /// The client connection port.
        /// </param>
        /// <param name="firstResponse">
        /// The first response.
        /// </param>
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

                client.DirectDataSentCallback(firstResponse, false);
                ac.Establish(
                    clientConnectionAddress, 
                    clientConnectionPort, 
                    client, 
                    firstResponse, 
                    delegate(ref byte[] data, ServerType thisactiveServer, ProxyClient thisclient)
                        {
                            return thisclient.DirectDataSentCallback(data, false);
                        }, 
                    delegate(ref byte[] data, ServerType thisactiveServer, ProxyClient thisclient)
                        {
                            return thisclient.DirectDataReceivedCallback(ref data, thisactiveServer, false);
                        }, 
                    delegate(bool success, ServerType thisactiveServer, ProxyClient thisclient)
                        {
                            return thisclient.DirectConnectionStatusCallback(thisactiveServer, success, false);
                        });
            }
            else
            {
                // Forwarder is Disable or Client is in proxy connection mode
                ServerType ac = client.Controller.ActiveServer.Clone();
                if (client.Controller.SmartPear.ForwarderHttpsEnable)
                {
                    // If we have forwarding enable then we need to send rcv callback
                    ac.Establish(
                        clientConnectionAddress, 
                        clientConnectionPort, 
                        client, 
                        firstResponse, 
                        null, 
                        delegate(ref byte[] data, ServerType thisactiveServer, ProxyClient thisclient)
                            {
                                return thisclient.DirectDataReceivedCallback(ref data, thisactiveServer, false);
                            });
                }
                else
                {
                    // If we dont so SmartPear is disabled
                    ac.Establish(clientConnectionAddress, clientConnectionPort, client, firstResponse);
                }
            }
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="firstResponse">
        /// The first response.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
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

        /// <summary>
        /// The is https.
        /// </summary>
        /// <param name="firstresponse">
        /// The first response.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsHttps(byte[] firstresponse)
        {
            string textData = Encoding.ASCII.GetString(firstresponse);
            if (textData.IndexOf("\r\n\r\n", StringComparison.Ordinal) == -1)
            {
                return false;
            }

            return textData.ToUpper().IndexOf("CONNECT ", StringComparison.OrdinalIgnoreCase) == 0 || textData.ToUpper().IndexOf("FASTCONNECT ", StringComparison.OrdinalIgnoreCase) == 0;
        }

        #endregion
    }
}