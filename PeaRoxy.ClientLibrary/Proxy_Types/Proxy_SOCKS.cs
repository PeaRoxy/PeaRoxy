using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using PeaRoxy.ClientLibrary.Server_Types;

namespace PeaRoxy.ClientLibrary.Proxy_Types
{
    class Proxy_SOCKS
    {
        public static bool IsSOCKS(byte[] firstresponde)
        {
            byte client_version = firstresponde[0];
            if (client_version != 5 && client_version != 4)
                return false;
            else if (client_version == 5 && firstresponde.Length < 3)
                return false;
            else if (client_version == 4 && firstresponde.Length < 8)
                return false;
            return true;
        }
        public static void Handle(byte[] firstResponde, Proxy_Client client)
        {
            if (!client.Controller.IsSOCKS_Supported || !IsSOCKS(firstResponde))
            {
                client.Close();
                return;
            }

            // Responding with socks protocol
            byte client_version = firstResponde[0];
            if (client_version == 5)
            {
                byte[] client_supportedAuths = new byte[firstResponde[1]];
                Array.Copy(firstResponde, 2, client_supportedAuths, 0, firstResponde[1]);
                byte[] server_response = new byte[2];
                if (client_supportedAuths.Length == 0)
                {
                    client.Close("No auth method found.", null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
                    return;
                }
                server_response[0] = client_version;

                // -------------Selecting auth type
                byte server_selectedAuth = 255;
                foreach (byte item in client_supportedAuths)
                    if (item == 0)
                        server_selectedAuth = 0;
                server_response[1] = server_selectedAuth;
                client.Write(server_response);
                client.IsReceivingStarted = false;
                // -------------Doing auth
                if (server_selectedAuth == 255)
                {
                    client.Close("SOCKS Connection only accept clients with no authentication information.", null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
                    return;
                }

                // -------------Establishing connection
                byte[] responde = new byte[client.Client.ReceiveBufferSize];
                client.Client.ReceiveTimeout = client.NoDataTimeOut;
                client.Client.BeginReceive(responde, 0, responde.Length, System.Net.Sockets.SocketFlags.None, (AsyncCallback)delegate(IAsyncResult ar)
                {
                    try
                    {
                        int bytes = client.Client.EndReceive(ar);
                        Array.Resize(ref responde, bytes);

                        if (responde == null || responde.Length == 0)
                        {
                            client.Close("No request received. Connection Timeout.", null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
                            return;
                        }
                        if (responde[0] != client_version)
                        {
                            client.Close("Unknown SOCKS version, Expected " + client_version.ToString(), null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
                            return;
                        }

                        byte client_connectionType = responde[1];
                        byte client_addressType = responde[3];
                        string client_connectionAddress = null;
                        byte[] client_unformatedConnectionAddress;
                        ushort client_connectionPort = 0;
                        byte server_Aresponse = 0;
                        if (client_connectionType != 1)
                            server_Aresponse = 7;
                        switch (client_addressType)
                        {
                            case 1:
                                client_unformatedConnectionAddress = new byte[4];
                                Array.Copy(responde, 4, client_unformatedConnectionAddress, 0, 4);
                                client_connectionAddress = new IPAddress(client_unformatedConnectionAddress).ToString();
                                client_connectionPort = (ushort)((ushort)(responde[8] * 256) + (ushort)responde[9]);
                                break;
                            case 3:
                                client_unformatedConnectionAddress = new byte[responde[4]];
                                Array.Copy(responde, 5, client_unformatedConnectionAddress, 0, responde[4]);
                                client_connectionAddress = System.Text.Encoding.ASCII.GetString(client_unformatedConnectionAddress);
                                client_connectionPort = (ushort)((ushort)(responde[(5 + responde[4])] * 256) + (ushort)responde[(5 + responde[4] + 1)]);
                                break;
                            case 4:
                                client_unformatedConnectionAddress = new byte[16];
                                Array.Copy(responde, 4, client_unformatedConnectionAddress, 0, 16);
                                client_connectionAddress = new IPAddress(client_unformatedConnectionAddress).ToString();
                                client_connectionPort = (ushort)((ushort)(responde[20] * 256) + (ushort)responde[21]);
                                break;
                            default:
                                server_Aresponse = 8;
                                break;
                        }


                        server_response = new byte[3 + (responde.Length - 3)];
                        server_response[0] = client_version;
                        server_response[1] = server_Aresponse;
                        server_response[2] = 0;
                        server_response[3] = 3;
                        Array.Copy(responde, 3, server_response, 3, responde.Length - 3);
                        client.RequestAddress = "socks://" + client_connectionAddress + ":" + client_connectionPort.ToString();
                        client.Write(server_response);
                        client.IsReceivingStarted = false;
                        if (server_Aresponse != 0)
                        {
                            client.Close("Response Error, Code: " + server_Aresponse.ToString(), null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED, true);
                            return;
                        }
                        firstResponde = new byte[0];
                        DirectHandle(client, client_connectionAddress, client_connectionPort, firstResponde);
                    }
                    catch (Exception e)
                    {
                        client.Close(e.Message, e.StackTrace);
                    }
                }, responde);
            }
            else if (client_version == 4)
            {
                try
                {
                    byte client_connectionType = firstResponde[1];
                    string client_connectionAddress = null;
                    byte[] client_unformatedConnectionAddress;
                    ushort client_connectionPort = 0;
                    byte server_Aresponse = 90;
                    if (client_connectionType != 1)
                        server_Aresponse = 91;
                    client_connectionPort = (ushort)((ushort)(firstResponde[2] * 256) + (ushort)firstResponde[3]);
                    client_unformatedConnectionAddress = new byte[4];
                    Array.Copy(firstResponde, 4, client_unformatedConnectionAddress, 0, 4);
                    client_connectionAddress = new IPAddress(client_unformatedConnectionAddress).ToString();
                    if (client_connectionAddress.StartsWith("0.0.0.") && !client_connectionAddress.EndsWith(".0"))
                    {
                        int domainStart = 0, domainEnd = 0;
                        for (int i = 8; i < firstResponde.Length; i++)
                        {
                            if (firstResponde[i] == 0)
                                if (domainStart == 0)
                                    domainStart = i + 1;
                                else if (domainEnd == 0)
                                    domainEnd = i;
                        }
                        if (domainEnd != 0 && domainStart != 0)
                            client_connectionAddress = System.Text.Encoding.ASCII.GetString(firstResponde, domainStart, domainEnd - domainStart);
                        else
                            server_Aresponse = 91;
                    }
                    byte[] server_response = new byte[8];
                    server_response[0] = 0;
                    server_response[1] = server_Aresponse;
                    Array.Copy(firstResponde, 2, server_response, 2, 2); // PORT
                    Array.Copy(firstResponde, 4, server_response, 4, 4); // IP
                    client.RequestAddress = "socks://" + client_connectionAddress + ":" + client_connectionPort.ToString();
                    client.Write(server_response);
                    client.IsReceivingStarted = false;
                    if (server_Aresponse != 90)
                    {
                        client.Close("Response Error, Code: " + server_Aresponse.ToString(), null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED, true);
                        return;
                    }

                    firstResponde = new byte[0];
                    DirectHandle(client, client_connectionAddress, client_connectionPort, firstResponde);
                }
                catch (Exception e)
                {
                    client.Close(e.Message, e.StackTrace);
                }
            }
         }
        public static void DirectHandle(Proxy_Client client, string client_connectionAddress, ushort client_connectionPort, byte[] firstResponde)
        {
            if (client.IsSmartForwarderEnable && client.Controller.SmartPear.Forwarder_HTTPS_Enable && client.Controller.SmartPear.Forwarder_SOCKS_Enable) // Is Forwarder is Enable and Client need to be forwarded by NoServer
            {
                ServerType ac = null;
                ac = new Server_NoServer();
                ac.NoDataTimeout = client.Controller.ActiveServer.NoDataTimeout;
                if (client.Controller.SmartPear.Detector_Timeout_Enable) // If we have Timeout Detector then let change timeout
                {
                    ac.NoDataTimeout = client.Controller.SmartPear.Detector_Timeout;
                }
                client.Direct_DataSentCallback(firstResponde,true);
                ac.Establish(client_connectionAddress, client_connectionPort, client, firstResponde,
                (ServerType.DataCallbackDelegate)(delegate(ref byte[] data, ServerType thisactiveServer, Proxy_Client thisclient)
                {
                    return thisclient.Direct_DataSentCallback(data,true);
                }), (ServerType.DataCallbackDelegate)(delegate(ref byte[] data, ServerType thisactiveServer, Proxy_Client thisclient)
                {
                    return thisclient.Direct_DataReceivedCallback(ref data, thisactiveServer,true);
                }), (ServerType.ConnectionCallbackDelegate)(delegate(bool success, ServerType thisactiveServer, Proxy_Client thisclient)
                {
                    return thisclient.Direct_ConnectionStatusCallback(thisactiveServer, success,true);
                }));
            }
            else // Forwarder is Disable or Client is in proxy connection mode
            {
                ServerType ac = client.Controller.ActiveServer.Clone();
                if (client.Controller.SmartPear.Forwarder_HTTPS_Enable && client.Controller.SmartPear.Forwarder_SOCKS_Enable) // If we have forwarding enable then we need to send rcv callback
                {
                    ac.Establish(client_connectionAddress, client_connectionPort, client, firstResponde, null, (ServerType.DataCallbackDelegate)delegate(ref byte[] data, ServerType thisactiveServer, Proxy_Client thisclient)
                    {
                        return thisclient.Direct_DataReceivedCallback(ref data, thisactiveServer,true);
                    });
                }
                else // If we dont so SmartPear is disabled
                {
                    ac.Establish(client_connectionAddress, client_connectionPort, client, firstResponde);
                }
            }
        }
    }
}
