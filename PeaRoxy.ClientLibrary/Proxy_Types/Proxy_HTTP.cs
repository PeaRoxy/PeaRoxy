using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeaRoxy.ClientLibrary.Server_Types;

namespace PeaRoxy.ClientLibrary.Proxy_Types
{
    class Proxy_HTTP
    {
        public static bool IsHTTP(byte[] firstresponde, bool ignoreEnd = false)
        {
            string textData = System.Text.Encoding.ASCII.GetString(firstresponde).ToUpper();
            if (!ignoreEnd && textData.IndexOf("\r\n\r\n") == -1)
                return false;
            if (textData.IndexOf("GET ") != 0 && textData.IndexOf("POST ") != 0 && textData.IndexOf("HEAD ") != 0 && textData.IndexOf("PUT ") != 0
                        && textData.IndexOf("LOCK ") != 0 && textData.IndexOf("UNLOCK ") != 0 && textData.IndexOf("MOVE ") != 0 && textData.IndexOf("MKCOL ") != 0
                        && textData.IndexOf("PROPFIND ") != 0 && textData.IndexOf("PROPPATCH  ") != 0 && textData.IndexOf("DELETE ") != 0
                        && textData.IndexOf("TRACE ") != 0 && textData.IndexOf("OPTIONS ") != 0 && textData.IndexOf("PATCH ") != 0)
                return false;
            return true;
        }
        public static void Handle(byte[] firstResponde, Proxy_Client client, bool ignoreEnd = false)
        {
            if (!client.Controller.IsHTTP_Supported || client.Controller.Status == Proxy_Controller.ControllerStatus.Stopped || !IsHTTP(firstResponde, ignoreEnd))
            {
                client.Close();
                return;
            }

            string textData = System.Text.Encoding.ASCII.GetString(firstResponde);
            int pDest = textData.IndexOf("\r\n\r\n");
            if (pDest == -1)
                pDest = textData.Length;
            else
                pDest += 4;
            textData = textData.Substring(0, pDest);

            string client_connectionAddress = null;
            ushort client_connectionPort = 0;
            string[] headerlines = textData.Split('\n');
            string[] parts = headerlines[0].Split(' ');
            bool usedAsProxy = false;
            foreach (string line in headerlines)
            {
                if (line.Trim() == string.Empty)
                    break;
                string[] ls = line.Split(':');
                if (ls[0].Trim().ToUpper().IndexOf("PROXY-") != -1)
                {
                    usedAsProxy = true;
                    int startRemove = textData.IndexOf(line);
                    int countRemove = line.Trim().Length + 2;
                    textData = textData.Remove(startRemove, countRemove);
                }
                else if (ls[0].Trim().ToUpper().IndexOf("CONNECTION:") != -1)
                {
                    int startRemove = textData.IndexOf(line);
                    int countRemove = line.Trim().Length;
                    textData = textData.Remove(startRemove, countRemove);
                    textData.Insert(startRemove, "Connection: close");
                }
            }
            
            if (!usedAsProxy && (client.Controller.Status == Proxy_Controller.ControllerStatus.OnlyAutoConfig || client.Controller.Status == Proxy_Controller.ControllerStatus.Both) && !string.IsNullOrEmpty(client.Controller.AutoConfigPath) && parts[1].ToLower().Split('?')[0] == "/" + client.Controller.AutoConfigPath.ToLower())
            {
                client.Write(Proxy_HTTP.GenerateAutoConfigScript(client));
                client.Close();
            }
            else if (client.Controller.Status == Proxy_Controller.ControllerStatus.OnlyProxy || client.Controller.Status == Proxy_Controller.ControllerStatus.Both)
            {
                Uri url = new Uri(parts[1]);
                textData = textData.Replace(headerlines[0], headerlines[0].Replace(parts[1], url.PathAndQuery));
                client_connectionAddress = url.Host;
                client_connectionPort = (ushort)url.Port;
                byte[] headerData = System.Text.Encoding.ASCII.GetBytes(textData);
                client.RequestAddress = parts[1];
                if (headerData.Length > pDest)
                {
                    Array.Resize(ref firstResponde, firstResponde.Length + (headerData.Length - pDest));
                    Array.Copy(firstResponde, pDest, firstResponde, headerData.Length, firstResponde.Length - headerData.Length);
                }
                else if ((headerData.Length < pDest))
                {
                    Array.Copy(firstResponde, pDest, firstResponde, headerData.Length, firstResponde.Length - pDest);
                    Array.Resize(ref firstResponde, firstResponde.Length - (pDest - headerData.Length));
                }
                Array.Copy(headerData, 0, firstResponde, 0, headerData.Length);

                Proxy_HTTP.DirectHandle(client, client_connectionAddress, client_connectionPort, firstResponde);
            }
            else
            {
                client.RequestAddress = parts[1];
                client.Close("Currently we only serve AutoConfig files. Try restarting your browser.", null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
            }
        }
        public static void DirectHandle(Proxy_Client client, string client_connectionAddress, ushort client_connectionPort, byte[] firstResponde)
        {
            if (client.Controller.SmartPear.Forwarder_HTTP_Enable && client.IsSmartForwarderEnable)
            {
                ServerType ac = null;
                ac = new Server_NoServer();
                if (client.Controller.SmartPear.Detector_HTTP_Enable)
                {
                    ac.NoDataTimeout = client.Controller.ActiveServer.NoDataTimeout;
                    if (client.Controller.SmartPear.Detector_Timeout_Enable) // If we have Timeout Detector then let change timeout
                    {
                        ac.NoDataTimeout = client.Controller.SmartPear.Detector_Timeout;
                    }
                    client.HTTP_DataSentCallback(firstResponde);
                    ac.Establish(client_connectionAddress, client_connectionPort, client, firstResponde, (ServerType.DataCallbackDelegate)(delegate(ref byte[] data, ServerType thisactiveServer, Proxy_Client thisclient)
                    {
                        if (Proxy_HTTP.IsHTTP(data, true))
                        {
                            thisclient.Forwarder_Clean();
                            Proxy_HTTP.Handle(data, thisclient, true);
                            return false;
                        }
                        else
                            return thisclient.HTTP_DataSentCallback(data);
                    }), (ServerType.DataCallbackDelegate)(delegate(ref byte[] data, ServerType thisactiveServer, Proxy_Client thisclient)
                    {
                        return thisclient.HTTP_DataReceivedCallback(ref data, thisactiveServer);
                    }),
                    (ServerType.ConnectionCallbackDelegate)(delegate(bool success, ServerType thisactiveServer, Proxy_Client thisclient)
                    {
                        return thisclient.HTTP_ConnectionStatusCallback(thisactiveServer, success);
                    }));
                }
                else
                {
                    ac.NoDataTimeout = client.Controller.ActiveServer.NoDataTimeout;
                    if (client.Controller.SmartPear.Detector_Timeout_Enable) // If we have Timeout Detector then let change timeout
                    {
                        ac.NoDataTimeout = client.Controller.SmartPear.Detector_Timeout;
                    }
                    ac.Establish(client_connectionAddress, client_connectionPort, client, firstResponde, (ServerType.DataCallbackDelegate)(delegate(ref byte[] data, ServerType thisactiveServer, Proxy_Client thisclient)
                    {
                        if (Proxy_HTTP.IsHTTP(data, true))
                        {
                            Proxy_HTTP.Handle(data, thisclient, true);
                            return false;
                        }
                        else
                            return true;
                    }),null,
                    (ServerType.ConnectionCallbackDelegate)(delegate(bool success, ServerType thisactiveServer, Proxy_Client thisclient)
                    {
                        return thisclient.HTTP_ConnectionStatusCallback(thisactiveServer, success);
                    }));
                }
            }
            else
            {
                ServerType ac = client.Controller.ActiveServer.Clone();
                if (!client.IsSmartForwarderEnable && client.Controller.SmartPear.Forwarder_HTTP_Enable && client.Controller.SmartPear.Detector_HTTP_Enable)
                {
                    ac.Establish(client_connectionAddress, client_connectionPort, client, firstResponde, (ServerType.DataCallbackDelegate)(delegate(ref byte[] data, ServerType thisactiveServer, Proxy_Client thisclient)
                    {
                        if (Proxy_HTTP.IsHTTP(data, true))
                        {
                            if (client.Controller.SmartPear.Forwarder_HTTP_Enable && !client.IsSmartForwarderEnable)
                                thisclient.Forwarder_Clean();
                            Proxy_HTTP.Handle(data, thisclient, true);
                            return false;
                        }
                        else
                            return true;
                    }),
                    (ServerType.DataCallbackDelegate)delegate(ref byte[] data, ServerType thisactiveServer, Proxy_Client thisclient)
                    {
                        return thisclient.HTTP_DataReceivedCallback(ref data, thisactiveServer);
                    });
                }
                else
                {
                    ac.Establish(client_connectionAddress, client_connectionPort, client, firstResponde, (ServerType.DataCallbackDelegate)(delegate(ref byte[] data, ServerType thisactiveServer, Proxy_Client thisclient)
                    {
                        if (Proxy_HTTP.IsHTTP(data, true))
                        {
                            if (client.Controller.SmartPear.Forwarder_HTTP_Enable && !client.IsSmartForwarderEnable)
                                thisclient.Forwarder_Clean();
                            Proxy_HTTP.Handle(data, thisclient, true);
                            return false;
                        }
                        else
                            return true;
                    }));
                }
            }
        }
        private static byte[] GenerateAutoConfigScript(Proxy_Client client)
        {
            string ConnectionString = ((client.Controller.IP.Equals(System.Net.IPAddress.Any)) ? Environment.MachineName : client.Controller.IP.ToString()) + ":" + client.Controller.Port.ToString();
            string HTTPConnectionType = ((client.Controller.Status == Proxy_Controller.ControllerStatus.Both) ? ((client.Controller.IsHTTP_Supported) ? "PROXY " + ConnectionString + ";" : ((client.Controller.IsSOCKS_Supported) ? "SOCKS " + ConnectionString + ";" : string.Empty)) : string.Empty);
            string HTTPSConnectionType = ((client.Controller.Status == Proxy_Controller.ControllerStatus.Both) ? ((client.Controller.IsHTTPS_Supported) ? "PROXY " + ConnectionString + ";" : ((client.Controller.IsSOCKS_Supported) ? "SOCKS " + ConnectionString + ";" : string.Empty)) : string.Empty);
            string script = "function FindProxyForURL(url, host) {" + Environment.NewLine +
                            "   if (url.toLowerCase().indexOf('https://') === 0) {" + Environment.NewLine +
                            "       return '" + HTTPSConnectionType + "DIRECT';" + Environment.NewLine +
                            "   }" + Environment.NewLine +
                            "   return '" + HTTPConnectionType + "DIRECT';" + Environment.NewLine +
                            "}";
            string ctrl = "\r\n";
            string header = "HTTP/1.1 200 OK" + ctrl +
                            "Server: PeaRoxy Auto Config Script Generator" + ctrl +
                            "Content-Length: " + script.Length.ToString() + ctrl +
                            "Connection: close" + ctrl +
                            "Cache-Control: max-age=1, public" + ctrl +
                            "Content-Type: " + ((client.Controller.AutoConfigMime == Proxy_Controller.AutoConfigMimeType.Javascript) ? "application/x-javascript-config" : "application/x-ns-proxy-autoconfig") + ";" + ctrl + ctrl;
            return System.Text.Encoding.ASCII.GetBytes(header + script);
        }

    }
}
