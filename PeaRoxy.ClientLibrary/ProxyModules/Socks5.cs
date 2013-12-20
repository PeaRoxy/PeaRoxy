// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Socks5.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The proxy_ socks.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary.ProxyModules
{
    #region

    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    using PeaRoxy.ClientLibrary.ServerModules;

    #endregion

    /// <summary>
    /// The socks 5 proxy module.
    /// </summary>
    internal static class Socks5
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
            if (client.IsSmartForwarderEnable && client.Controller.SmartPear.ForwarderHttpsEnable
                && client.Controller.SmartPear.ForwarderSocksEnable)
            {
                // Is Forwarder is Enable and Client need to be forwarded by NoServer
                ServerType ac = new NoServer { NoDataTimeout = client.Controller.ActiveServer.NoDataTimeout };
                if (client.Controller.SmartPear.DetectorTimeoutEnable)
                {
                    // If we have Timeout Detector then let change timeout
                    ac.NoDataTimeout = client.Controller.SmartPear.DetectorTimeout;
                }

                client.DirectDataSentCallback(firstResponse, true);
                ac.Establish(
                    clientConnectionAddress, 
                    clientConnectionPort, 
                    client, 
                    firstResponse,
                    delegate(ref byte[] data, ServerType thisactiveServer, ProxyClient thisclient)
                        {
                            return thisclient.DirectDataSentCallback(data, true);
                        },
                    delegate(ref byte[] data, ServerType thisactiveServer, ProxyClient thisclient)
                        {
                            return thisclient.DirectDataReceivedCallback(ref data, thisactiveServer, true);
                        },
                    delegate(bool success, ServerType thisactiveServer, ProxyClient thisclient)
                        {
                            return thisclient.DirectConnectionStatusCallback(thisactiveServer, success, true);
                        });
            }
            else
            {
                // Forwarder is Disable or Client is in proxy connection mode
                ServerType ac = client.Controller.ActiveServer.Clone();
                if (client.Controller.SmartPear.ForwarderHttpsEnable
                    && client.Controller.SmartPear.ForwarderSocksEnable)
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
                                return thisclient.DirectDataReceivedCallback(ref data, thisactiveServer, true);
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
            if (!client.Controller.IsSocksSupported || !IsSocks(firstResponse))
            {
                client.Close();
                return;
            }

            // Responding with socks protocol
            byte clientVersion = firstResponse[0];
            if (clientVersion == 5)
            {
                byte[] clientSupportedAuths = new byte[firstResponse[1]];
                Array.Copy(firstResponse, 2, clientSupportedAuths, 0, firstResponse[1]);
                byte[] serverResponse = new byte[2];
                if (clientSupportedAuths.Length == 0)
                {
                    client.Close("No auth method found.", null, ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                    return;
                }

                serverResponse[0] = clientVersion;

                // -------------Selecting auth type
                byte serverSelectedAuth = 255;
                foreach (byte item in clientSupportedAuths)
                {
                    if (item == 0)
                    {
                        serverSelectedAuth = 0;
                    }
                }

                serverResponse[1] = serverSelectedAuth;
                client.Write(serverResponse);
                client.IsReceivingStarted = false;

                // -------------Doing auth
                if (serverSelectedAuth == 255)
                {
                    client.Close(
                        "SOCKS Connection only accept clients with no authentication information.", 
                        null, 
                        ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                    return;
                }

                // -------------Establishing connection
                byte[] response = new byte[client.Client.ReceiveBufferSize];
                client.Client.ReceiveTimeout = client.NoDataTimeOut;
                client.Client.BeginReceive(
                    response, 
                    0, 
                    response.Length, 
                    SocketFlags.None, 
                    delegate(IAsyncResult ar)
                        {
                            try
                            {
                                int bytes = client.Client.EndReceive(ar);
                                Array.Resize(ref response, bytes);

                                if (response == null || response.Length == 0)
                                {
                                    client.Close(
                                        "No request received. Connection Timeout.", 
                                        null, 
                                        ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                                    return;
                                }

                                if (response[0] != clientVersion)
                                {
                                    client.Close(
                                        "Unknown SOCKS version, Expected " + clientVersion, 
                                        null, 
                                        ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
                                    return;
                                }

                                byte clientConnectionType = response[1];
                                byte clientAddressType = response[3];
                                string clientConnectionAddress = null;
                                byte[] clientUnformatedConnectionAddress;
                                ushort clientConnectionPort = 0;
                                byte serverAresponse = 0;
                                if (clientConnectionType != 1)
                                {
                                    serverAresponse = 7;
                                }

                                switch (clientAddressType)
                                {
                                    case 1:
                                        clientUnformatedConnectionAddress = new byte[4];
                                        Array.Copy(response, 4, clientUnformatedConnectionAddress, 0, 4);
                                        clientConnectionAddress =
                                            new IPAddress(clientUnformatedConnectionAddress).ToString();
                                        clientConnectionPort =
                                            (ushort)((ushort)(response[8] * 256) + response[9]);
                                        break;
                                    case 3:
                                        clientUnformatedConnectionAddress = new byte[response[4]];
                                        Array.Copy(response, 5, clientUnformatedConnectionAddress, 0, response[4]);
                                        clientConnectionAddress =
                                            Encoding.ASCII.GetString(clientUnformatedConnectionAddress);
                                        clientConnectionPort =
                                            (ushort)
                                            ((ushort)(response[5 + response[4]] * 256)
                                             + response[5 + response[4] + 1]);
                                        break;
                                    case 4:
                                        clientUnformatedConnectionAddress = new byte[16];
                                        Array.Copy(response, 4, clientUnformatedConnectionAddress, 0, 16);
                                        clientConnectionAddress =
                                            new IPAddress(clientUnformatedConnectionAddress).ToString();
                                        clientConnectionPort =
                                            (ushort)((ushort)(response[20] * 256) + response[21]);
                                        break;
                                    default:
                                        serverAresponse = 8;
                                        break;
                                }

                                serverResponse = new byte[3 + (response.Length - 3)];
                                serverResponse[0] = clientVersion;
                                serverResponse[1] = serverAresponse;
                                serverResponse[2] = 0;
                                serverResponse[3] = 3;
                                Array.Copy(response, 3, serverResponse, 3, response.Length - 3);
                                client.RequestAddress = "socks://" + clientConnectionAddress + ":"
                                                        + clientConnectionPort;
                                client.Write(serverResponse);
                                client.IsReceivingStarted = false;
                                if (serverAresponse != 0)
                                {
                                    client.Close(
                                        "Response Error, Code: " + serverAresponse, 
                                        null, 
                                        ErrorRenderer.HttpHeaderCode.C417ExpectationFailed, 
                                        true);
                                    return;
                                }

                                firstResponse = new byte[0];
                                DirectHandle(client, clientConnectionAddress, clientConnectionPort, firstResponse);
                            }
                            catch (Exception e)
                            {
                                client.Close(e.Message, e.StackTrace);
                            }
                        }, 
                    response);
            }
            else if (clientVersion == 4)
            {
                try
                {
                    byte clientConnectionType = firstResponse[1];
                    byte serverAresponse = 90;
                    if (clientConnectionType != 1)
                    {
                        serverAresponse = 91;
                    }

                    ushort clientConnectionPort = (ushort)((ushort)(firstResponse[2] * 256) + firstResponse[3]);
                    byte[] clientUnformatedConnectionAddress = new byte[4];
                    Array.Copy(firstResponse, 4, clientUnformatedConnectionAddress, 0, 4);
                    string clientConnectionAddress = new IPAddress(clientUnformatedConnectionAddress).ToString();
                    if (clientConnectionAddress.StartsWith("0.0.0.") && !clientConnectionAddress.EndsWith(".0"))
                    {
                        int domainStart = 0, domainEnd = 0;
                        for (int i = 8; i < firstResponse.Length; i++)
                        {
                            if (firstResponse[i] == 0)
                            {
                                if (domainStart == 0)
                                {
                                    domainStart = i + 1;
                                }
                                else if (domainEnd == 0)
                                {
                                    domainEnd = i;
                                }
                            }
                        }

                        if (domainEnd != 0 && domainStart != 0)
                        {
                            clientConnectionAddress = Encoding.ASCII.GetString(
                                firstResponse, 
                                domainStart, 
                                domainEnd - domainStart);
                        }
                        else
                        {
                            serverAresponse = 91;
                        }
                    }

                    byte[] serverResponse = new byte[8];
                    serverResponse[0] = 0;
                    serverResponse[1] = serverAresponse;
                    Array.Copy(firstResponse, 2, serverResponse, 2, 2); // PORT
                    Array.Copy(firstResponse, 4, serverResponse, 4, 4); // IP
                    client.RequestAddress = "socks://" + clientConnectionAddress + ":"
                                            + clientConnectionPort;
                    client.Write(serverResponse);
                    client.IsReceivingStarted = false;
                    if (serverAresponse != 90)
                    {
                        client.Close(
                            "Response Error, Code: " + serverAresponse, 
                            null, 
                            ErrorRenderer.HttpHeaderCode.C417ExpectationFailed, 
                            true);
                        return;
                    }

                    firstResponse = new byte[0];
                    DirectHandle(client, clientConnectionAddress, clientConnectionPort, firstResponse);
                }
                catch (Exception e)
                {
                    client.Close(e.Message, e.StackTrace);
                }
            }
        }

        /// <summary>
        /// The is socks.
        /// </summary>
        /// <param name="firstresponse">
        /// The first response.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsSocks(byte[] firstresponse)
        {
            byte clientVersion = firstresponse[0];
            return (clientVersion == 5 || clientVersion == 4) && (clientVersion != 5 || firstresponse.Length >= 3) && (clientVersion != 4 || firstresponse.Length >= 8);
        }

        #endregion
    }
}