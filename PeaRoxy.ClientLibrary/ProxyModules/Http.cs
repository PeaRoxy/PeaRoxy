// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Http.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The proxy_ http.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary.ProxyModules
{
    #region

    using System;
    using System.Net;
    using System.Security.Policy;
    using System.Text;

    using PeaRoxy.ClientLibrary.ServerModules;

    #endregion

    /// <summary>
    /// The http proxy module
    /// </summary>
    internal class Http
    {
        #region Public Methods and Operators

        /// <summary>
        /// The direct handle.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="clientConnectionAddress">
        /// The client_connection address.
        /// </param>
        /// <param name="clientConnectionPort">
        /// The client_connection port.
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
            if (client.Controller.SmartPear.ForwarderHttpEnable && client.IsSmartForwarderEnable)
            {
                ServerType ac = new NoServer();
                if (client.Controller.SmartPear.DetectorHttpEnable)
                {
                    ac.NoDataTimeout = client.Controller.ActiveServer.NoDataTimeout;
                    if (client.Controller.SmartPear.DetectorTimeoutEnable)
                    {
                        // If we have Timeout Detector then let change timeout
                        ac.NoDataTimeout = client.Controller.SmartPear.DetectorTimeout;
                    }

                    client.HttpDataSentCallback(firstResponse);
                    ac.Establish(
                        clientConnectionAddress, 
                        clientConnectionPort, 
                        client, 
                        firstResponse, 
                        delegate(ref byte[] data, ServerType thisactiveServer, ProxyClient thisclient)
                            {
                                if (IsHttp(data, true))
                                {
                                    thisclient.ForwarderClean();
                                    Handle(data, thisclient, true);
                                    return false;
                                }

                                return thisclient.HttpDataSentCallback(data);
                            }, 
                        delegate(ref byte[] data, ServerType thisactiveServer, ProxyClient thisclient)
                            {
                                return thisclient.HttpDataReceivedCallback(ref data, thisactiveServer);
                            }, 
                        delegate(bool success, ServerType thisactiveServer, ProxyClient thisclient)
                            {
                                return thisclient.HttpConnectionStatusCallback(thisactiveServer, success);
                            });
                }
                else
                {
                    ac.NoDataTimeout = client.Controller.ActiveServer.NoDataTimeout;
                    if (client.Controller.SmartPear.DetectorTimeoutEnable)
                    {
                        // If we have Timeout Detector then let change timeout
                        ac.NoDataTimeout = client.Controller.SmartPear.DetectorTimeout;
                    }

                    client.HttpDataSentCallback(firstResponse);
                    ac.Establish(
                        clientConnectionAddress, 
                        clientConnectionPort, 
                        client, 
                        firstResponse, 
                        delegate(ref byte[] data, ServerType thisactiveServer, ProxyClient thisclient)
                            {
                                if (IsHttp(data, true))
                                {
                                    Handle(data, thisclient, true);
                                    return false;
                                }

                                return thisclient.HttpDataSentCallback(data);
                            }, 
                        delegate(ref byte[] data, ServerType thisactiveServer, ProxyClient thisclient)
                            {
                                return thisclient.HttpDataReceivedCallback(ref data, thisactiveServer);
                            }, 
                        delegate(bool success, ServerType thisactiveServer, ProxyClient thisclient)
                            {
                                return thisclient.HttpConnectionStatusCallback(thisactiveServer, success);
                            });
                }
            }
            else
            {
                ServerType ac = client.Controller.ActiveServer.Clone();
                if (!client.IsSmartForwarderEnable && client.Controller.SmartPear.ForwarderHttpEnable
                    && (client.Controller.SmartPear.DetectorHttpEnable || client.Controller.SmartPear.DetectorDnsGrabberEnable))
                {
                    ac.Establish(
                        clientConnectionAddress, 
                        clientConnectionPort, 
                        client, 
                        firstResponse, 
                        delegate(ref byte[] data, ServerType thisactiveServer, ProxyClient thisclient)
                            {
                                if (IsHttp(data, true))
                                {
                                    if (client.Controller.SmartPear.ForwarderHttpEnable
                                        && !client.IsSmartForwarderEnable)
                                    {
                                        thisclient.ForwarderClean();
                                    }

                                    Handle(data, thisclient, true);
                                    return false;
                                }

                                return true;
                            }, 
                        delegate(ref byte[] data, ServerType thisactiveServer, ProxyClient thisclient)
                            {
                                return thisclient.HttpDataReceivedCallback(ref data, thisactiveServer);
                            });
                }
                else
                {
                    ac.Establish(
                        clientConnectionAddress, 
                        clientConnectionPort, 
                        client, 
                        firstResponse, 
                        delegate(ref byte[] data, ServerType thisactiveServer, ProxyClient thisclient)
                            {
                                if (IsHttp(data, true))
                                {
                                    if (client.Controller.SmartPear.ForwarderHttpEnable
                                        && !client.IsSmartForwarderEnable)
                                    {
                                        thisclient.ForwarderClean();
                                    }

                                    Handle(data, thisclient, true);
                                    return false;
                                }

                                return true;
                            });
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
        /// <param name="ignoreEnd">
        /// The ignore end.
        /// </param>
        public static void Handle(byte[] firstResponse, ProxyClient client, bool ignoreEnd = false)
        {
            if (!client.Controller.IsHttpSupported
                || client.Controller.Status == ProxyController.ControllerStatus.None
                || !IsHttp(firstResponse, ignoreEnd))
            {
                client.Close();
                return;
            }

            string textData = Encoding.ASCII.GetString(firstResponse);
            int headerLocation = textData.IndexOf("\r\n\r\n", StringComparison.OrdinalIgnoreCase);
            if (headerLocation == -1)
            {
                headerLocation = textData.Length;
            }
            else
            {
                headerLocation += 4;
            }

            textData = textData.Substring(0, headerLocation);

            string[] headerlines = textData.Split('\n');
            string[] parts = headerlines[0].Split(' ');
            bool usedAsProxy = false;
            string host = string.Empty;
            foreach (string line in headerlines)
            {
                if (line.Trim() == string.Empty)
                {
                    break;
                }

                string[] ls = line.Split(':');
                if (ls[0].Trim().ToUpper().IndexOf("PROXY-", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    usedAsProxy = true;
                    int startRemove = textData.IndexOf(line, StringComparison.OrdinalIgnoreCase);
                    int countRemove = line.Trim().Length + 2;
                    textData = textData.Remove(startRemove, countRemove);
                }
                else if (ls[0].Trim().ToUpper().IndexOf("CONNECTION", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    int startRemove = textData.IndexOf(line, StringComparison.OrdinalIgnoreCase);
                    int countRemove = line.Trim().Length;
                    textData = textData.Remove(startRemove, countRemove);
                    textData = textData.Insert(startRemove, "Connection: close");
                }
                else if (ls[0].Trim().ToUpper().IndexOf("HOST", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    host = ls[1].Trim();
                }
            }

            if (!usedAsProxy
                && client.Controller.Status.HasFlag(ProxyController.ControllerStatus.AutoConfig)
                && !string.IsNullOrEmpty(client.Controller.AutoConfigPath)
                && parts[1].ToLower().Split('?')[0] == "/" + client.Controller.AutoConfigPath.ToLower())
            {
                client.Write(GenerateAutoConfigScript(client));
                client.Close();
            }
            else if (client.Controller.Status.HasFlag(ProxyController.ControllerStatus.Proxy))
            {
                if (!Uri.IsWellFormedUriString(parts[1], UriKind.Absolute) && host != string.Empty
                    && Uri.IsWellFormedUriString("http://" + host + parts[1], UriKind.Absolute))
                {
                    parts[1] = "http://" + host + parts[1];
                }

                if (!Uri.IsWellFormedUriString(parts[1], UriKind.Absolute)
                    && Uri.IsWellFormedUriString("http://" + parts[1], UriKind.Absolute))
                {
                    parts[1] = "http://" + parts[1];
                }

                Uri url = new Uri(parts[1]);
                textData = textData.Replace(headerlines[0], headerlines[0].Replace(parts[1], url.PathAndQuery));
                string clientConnectionAddress = url.Host;
                ushort clientConnectionPort = (ushort)url.Port;
                byte[] headerData = Encoding.ASCII.GetBytes(textData);
                client.RequestAddress = parts[1];
                if (headerData.Length > headerLocation)
                {
                    Array.Resize(ref firstResponse, firstResponse.Length + (headerData.Length - headerLocation));
                    Array.Copy(
                        firstResponse, 
                        headerLocation, 
                        firstResponse, 
                        headerData.Length, 
                        firstResponse.Length - headerData.Length);
                }
                else if (headerData.Length < headerLocation)
                {
                    Array.Copy(firstResponse, headerLocation, firstResponse, headerData.Length, firstResponse.Length - headerLocation);
                    Array.Resize(ref firstResponse, firstResponse.Length - (headerLocation - headerData.Length));
                }

                Array.Copy(headerData, 0, firstResponse, 0, headerData.Length);

                DirectHandle(client, clientConnectionAddress, clientConnectionPort, firstResponse);
            }
            else
            {
                client.RequestAddress = parts[1];
                client.Close(
                    "Currently we only serve AutoConfig files. Try restarting your browser.", 
                    null, 
                    ErrorRenderer.HttpHeaderCode.C417ExpectationFailed);
            }
        }

        /// <summary>
        /// The is http.
        /// </summary>
        /// <param name="firstresponse">
        /// The first response.
        /// </param>
        /// <param name="ignoreEnd">
        /// The ignore end.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsHttp(byte[] firstresponse, bool ignoreEnd = false)
        {
            string textData = Encoding.ASCII.GetString(firstresponse).ToUpper();
            if (!ignoreEnd && textData.IndexOf("\r\n\r\n", StringComparison.OrdinalIgnoreCase) == -1)
            {
                return false;
            }

            return textData.IndexOf("GET ", StringComparison.Ordinal) == 0 || 
                    textData.IndexOf("POST ", StringComparison.Ordinal) == 0 || 
                    textData.IndexOf("HEAD ", StringComparison.Ordinal) == 0 || 
                    textData.IndexOf("PUT ", StringComparison.Ordinal) == 0 || 
                    textData.IndexOf("LOCK ", StringComparison.Ordinal) == 0 || 
                    textData.IndexOf("UNLOCK ", StringComparison.Ordinal) == 0 || 
                    textData.IndexOf("MOVE ", StringComparison.Ordinal) == 0 || 
                    textData.IndexOf("MKCOL ", StringComparison.Ordinal) == 0 || 
                    textData.IndexOf("PROPFIND ", StringComparison.Ordinal) == 0 ||
                    textData.IndexOf("PROPPATCH  ", StringComparison.Ordinal) == 0 || 
                    textData.IndexOf("DELETE ", StringComparison.Ordinal) == 0 || 
                    textData.IndexOf("TRACE ", StringComparison.Ordinal) == 0 || 
                    textData.IndexOf("OPTIONS ", StringComparison.Ordinal) == 0 || 
                    textData.IndexOf("PATCH ", StringComparison.Ordinal) == 0;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The generate auto config script.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>byte[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        private static byte[] GenerateAutoConfigScript(ProxyClient client)
        {
            string connectionString = (client.Controller.Ip.Equals(IPAddress.Any)
                                           ? IPAddress.Loopback.ToString()
                                           : client.Controller.Ip.ToString()) + ":" + client.Controller.Port;
            string httpConnectionType = (client.Controller.Status.HasFlag(ProxyController.ControllerStatus.Proxy))
                                             ? (client.Controller.IsHttpSupported
                                                    ? "PROXY " + connectionString + ";"
                                                    : (client.Controller.IsSocksSupported
                                                           ? "SOCKS " + connectionString + ";"
                                                           : string.Empty))
                                             : string.Empty;
            string httpsConnectionType = (client.Controller.Status.HasFlag(ProxyController.ControllerStatus.Proxy))
                                              ? (client.Controller.IsHttpsSupported
                                                     ? "PROXY " + connectionString + ";"
                                                     : (client.Controller.IsSocksSupported
                                                            ? "SOCKS " + connectionString + ";"
                                                            : string.Empty))
                                              : string.Empty;
            const string NewLineSep = "\r\n";
            string script =
                string.Format(
                    "function FindProxyForURL(url, host) {{if (url.toLowerCase().indexOf('https://') === 0) {{return '{0}DIRECT';}}return '{1}DIRECT';}}",
                    httpsConnectionType,
                    httpConnectionType);

            string header =
                string.Format(
                    "HTTP/1.1 200 OK{0}Server: PeaRoxy Auto Config Script Generator{0}Content-Length: {1}{0}Connection: close{0}Cache-Control: max-age=1, public{0}Content-Type: {2};{3}{3}",
                    NewLineSep,
                    script.Length,
                    (client.Controller.AutoConfigMime == ProxyController.AutoConfigMimeType.Javascript) ? "application/x-javascript-config" : "application/x-ns-proxy-autoconfig", 
                    NewLineSep);
            return Encoding.ASCII.GetBytes(header + script);
        }

        #endregion
    }
}