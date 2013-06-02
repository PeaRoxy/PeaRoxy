using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeaRoxy.ClientLibrary.Server_Types;

namespace PeaRoxy.ClientLibrary.Proxy_Types
{
    class Proxy_HTTPS
    {
        public static bool IsHTTPS(byte[] firstresponde)
        {
            string textData = System.Text.Encoding.ASCII.GetString(firstresponde);
            if (textData.IndexOf("\r\n\r\n") == -1)
                return false;
            if (textData.ToUpper().IndexOf("CONNECT ") != 0)
                return false;
            return true;
        }
        public static void Handle(byte[] firstResponde, Proxy_Client client)
        {
            if (!client.Controller.IsHTTPS_Supported || !IsHTTPS(firstResponde))
            {
                client.Close();
                return;
            }
            string textData = System.Text.Encoding.ASCII.GetString(firstResponde);
            // Getting Information From HTTPS Header
            string client_connectionAddress = null;
            ushort client_connectionPort = 0;
            string[] parts = textData.Split('\n')[0].Split(' ');
            Uri url = new Uri("https://" + parts[1]);
            client_connectionAddress = url.Host;
            client_connectionPort = (ushort)url.Port;
            byte[] server_response = System.Text.Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
            client.RequestAddress = "https://" + client_connectionAddress + ":" + client_connectionPort.ToString();
            client.Write(server_response);
            client.IsReceivingStarted = false;
            if (client.Controller.Status == Proxy_Controller.ControllerStatus.OnlyProxy || client.Controller.Status == Proxy_Controller.ControllerStatus.Both)
            {
                firstResponde = new byte[0];
                DirectHandle(client, client_connectionAddress, client_connectionPort, firstResponde);
            }
            else
            {
                client.Close("Currently we only serve AutoConfig files. Try restarting your browser.", null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
            }
        }
        public static void DirectHandle(Proxy_Client client, string client_connectionAddress, ushort client_connectionPort, byte[] firstResponde)
        {
            if (client.IsSmartForwarderEnable && client.Controller.SmartPear.Forwarder_HTTPS_Enable) // Is Forwarder is Enable and Client need to be forwarded by NoServer
            {
                ServerType ac = null;
                ac = new Server_NoServer();
                ac.NoDataTimeout = client.Controller.ActiveServer.NoDataTimeout;
                if (client.Controller.SmartPear.Detector_Timeout_Enable) // If we have Timeout Detector then let change timeout
                {
                    ac.NoDataTimeout = client.Controller.SmartPear.Detector_Timeout;
                }
                client.Direct_DataSentCallback(firstResponde,false);
                ac.Establish(client_connectionAddress, client_connectionPort, client, firstResponde,
                (ServerType.DataCallbackDelegate)(delegate(ref byte[] data, ServerType thisactiveServer, Proxy_Client thisclient)
                {
                    return thisclient.Direct_DataSentCallback(data,false);
                }),(ServerType.DataCallbackDelegate)(delegate(ref byte[] data, ServerType thisactiveServer, Proxy_Client thisclient)
                {
                    return thisclient.Direct_DataReceivedCallback(ref data, thisactiveServer,false);
                }), (ServerType.ConnectionCallbackDelegate)(delegate(bool success,ServerType thisactiveServer, Proxy_Client thisclient)
                {
                    return thisclient.Direct_ConnectionStatusCallback(thisactiveServer, success,false);
                }));
            }
            else // Forwarder is Disable or Client is in proxy connection mode
            {
                ServerType ac = client.Controller.ActiveServer.Clone();
                if (client.Controller.SmartPear.Forwarder_HTTPS_Enable) // If we have forwarding enable then we need to send rcv callback
                {
                    ac.Establish(client_connectionAddress, client_connectionPort, client, firstResponde, null, (ServerType.DataCallbackDelegate)delegate(ref byte[] data, ServerType thisactiveServer, Proxy_Client thisclient)
                    {
                        return thisclient.Direct_DataReceivedCallback(ref data, thisactiveServer,false);
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
