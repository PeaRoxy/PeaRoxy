// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorRenderer.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    using PeaRoxy.ClientLibrary.Properties;
    using PeaRoxy.CommonLibrary;
    using PeaRoxy.Platform;

    /// <summary>
    ///     The error renderer class is responsible for printing the error messages to the output
    /// </summary>
    public class ErrorRenderer
    {
        /// <summary>
        ///     The HTTP header codes
        /// </summary>
        public enum HttpHeaderCode
        {
            /// <summary>
            ///     200 OK.
            /// </summary>
            C200Ok,

            /// <summary>
            ///     404 Not Found.
            /// </summary>
            C404NotFound,

            /// <summary>
            ///     500 Server Error.
            /// </summary>
            C500ServerError,

            /// <summary>
            ///     501 Not Implemented.
            /// </summary>
            C501NotImplemented,

            /// <summary>
            ///     502 Bad Gateway.
            /// </summary>
            C502BadGateway,

            /// <summary>
            ///     504 Gateway Timeout.
            /// </summary>
            C504GatewayTimeout,

            /// <summary>
            ///     417 Expectation Failed.
            /// </summary>
            C417ExpectationFailed,
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ErrorRenderer" /> class.
        /// </summary>
        public ErrorRenderer()
        {
            this.EnableOnPort443 = true;
            this.EnableOnPort80 = true;
            this.EnableOnHttp = true;

            if (ClassRegistry.GetClass<CertManager>().CreateAuthority("PeaRoxy Authority", "PeaRoxy.crt"))
            {
                ClassRegistry.GetClass<CertManager>().RegisterAuthority("PeaRoxy Authority", "PeaRoxy.crt");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether error rendering should response to the direct requests on port 443
        /// </summary>
        public bool EnableOnPort443 { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether error rendering should response to the direct requests on port 80
        /// </summary>
        public bool EnableOnPort80 { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether error rendering should response to the HTTP requests
        /// </summary>
        public bool EnableOnHttp { get; set; }

        /// <summary>
        ///     Generating or locating a certification for the specified domain name
        /// </summary>
        /// <param name="domainName">
        ///     The domain name.
        /// </param>
        /// <returns>
        ///     File path of the certification file or NULL if any error occurred.
        /// </returns>
        public static string GetCertForDomain(string domainName)
        {
            domainName = domainName.ToLower().Trim();
            string md5 = Common.Md5(domainName);
            if (ClassRegistry.GetClass<CertManager>()
                .CreateCert(domainName, "PeaRoxy.crt", Path.Combine("Cache", md5 + ".crt")))
            {
                return Path.Combine("HTTPSCerts", "Cache", md5 + ".crt");
            }

            return null;
        }

        /// <summary>
        ///     Render the error message on the specified Proxy Client
        /// </summary>
        /// <param name="client">
        ///     The client.
        /// </param>
        /// <param name="title">
        ///     The title.
        /// </param>
        /// <param name="message">
        ///     The message.
        /// </param>
        /// <param name="code">
        ///     The HTTP code.
        /// </param>
        /// <param name="sslStream">
        ///     The SSL stream if any.
        /// </param>
        /// <returns>
        ///     <see cref="bool" /> showing if the process ended successfully.
        /// </returns>
        public bool RenderError(
            ProxyClient client,
            string title,
            string message,
            HttpHeaderCode code = HttpHeaderCode.C500ServerError,
            SslStream sslStream = null)
        {
            try
            {
                if (client.RequestAddress == string.Empty)
                {
                    return false;
                }

                Uri url;
                if (!Uri.TryCreate(client.RequestAddress, UriKind.Absolute, out url))
                {
                    return false;
                }

                bool https = false;
                if (url.Scheme.Equals("HTTP", StringComparison.OrdinalIgnoreCase))
                {
                    if (!this.EnableOnHttp)
                    {
                        return false;
                    }
                }
                else if (url.Scheme.Equals("HTTPS", StringComparison.OrdinalIgnoreCase)
                         || url.Scheme.Equals("SOCKS", StringComparison.OrdinalIgnoreCase))
                {
                    if (!((this.EnableOnPort80 && url.Port == 80) || (this.EnableOnPort443 && url.Port == 443)))
                    {
                        return false;
                    }

                    https = url.Port == 443;
                }
                else
                {
                    return false;
                }

                if (client.IsReceivingStarted || !client.IsSendingStarted)
                {
                    return false;
                }

                string html = Resources.HTTPErrorTemplate;
                html = html.Replace("%VERSION", Assembly.GetEntryAssembly().GetName().Version.ToString());
                html = html.Replace("%TITLE", title);
                html = html.Replace("%MESSAGE", Common.ConvertToHtmlEntities(message));
                string conString = "Not Connected";
                if (client.Controller.ActiveServer != null)
                {
                    conString = client.Controller.ActiveServer.ToString();
                }

                List<string> args = new List<string>();
                if (client.Controller.SmartPear.ForwarderHttpEnable)
                {
                    args.Add("P:HTTP");
                }

                if (client.Controller.SmartPear.ForwarderHttpsEnable)
                {
                    args.Add("P:HTTPS");
                }

                if (client.Controller.SmartPear.ForwarderSocksEnable && client.Controller.SmartPear.ForwarderHttpsEnable)
                {
                    args.Add("P:SOCKS");
                }

                if (args.Count > 0)
                {
                    conString = args.Aggregate(conString + " (", (current, arg) => current + (" " + arg + "; ")) + ")";
                }

                html = html.Replace("%CONSTRING", conString);

                string statusCode;
                switch (code)
                {
                    case HttpHeaderCode.C200Ok:
                        statusCode = "200 OK";
                        break;
                    case HttpHeaderCode.C501NotImplemented:
                        statusCode = "501 Not Implemented";
                        break;
                    case HttpHeaderCode.C502BadGateway:
                        statusCode = "502 Bad Gateway";
                        break;
                    case HttpHeaderCode.C504GatewayTimeout:
                        statusCode = "504 Gateway Timeout";
                        break;
                    case HttpHeaderCode.C417ExpectationFailed:
                        statusCode = "417 Expectation Failed";
                        break;
                    case HttpHeaderCode.C404NotFound:
                        statusCode = "404 Not Found";
                        break;
                    default:
                        statusCode = "500 Internal Server Error";
                        break;
                }

                const string NewLineSep = "\r\n";
                string header = "HTTP/1.1 " + statusCode + NewLineSep + "Server: PeaRoxy Error Renderer" + NewLineSep
                                + "Content-Length: " + html.Length + NewLineSep + "Connection: close" + NewLineSep
                                + "Content-Type: text/html;" + NewLineSep + NewLineSep;
                byte[] db = Encoding.ASCII.GetBytes(header + html);

                if (!Common.IsSocketConnected(client.UnderlyingSocket))
                {
                    return false;
                }

                if (https)
                {
                    string certAddress = url.DnsSafeHost;
                    if (!Common.IsIpAddress(certAddress))
                    {
                        if (string.IsNullOrEmpty(certAddress))
                        {
                            return false;
                        }
                    }

                    certAddress = GetCertForDomain(certAddress);
                    if (string.IsNullOrEmpty(certAddress))
                    {
                        return false;
                    }

                    try
                    {
                        X509Certificate certificate = new X509Certificate2(certAddress, string.Empty);
                        if (sslStream == null)
                        {
                            client.UnderlyingSocket.Blocking = true;
                            Stream stream = new NetworkStream(client.UnderlyingSocket);
                            sslStream = new SslStream(stream) { ReadTimeout = 30 * 1000, WriteTimeout = 30 * 1000 };
                            sslStream.AuthenticateAsServer(certificate);
                        }

                        sslStream.BeginWrite(
                            db,
                            0,
                            db.Length,
                            delegate(IAsyncResult ar)
                                {
                                    try
                                    {
                                        sslStream.EndWrite(ar);
                                        sslStream.Flush();
                                        sslStream.Close();
                                        client.UnderlyingSocket.Close();
                                    }
                                    catch (Exception)
                                    {
                                    }
                                },
                            null);
                    }
                    catch
                    {
                    }
                }
                else
                {
                    client.UnderlyingSocket.BeginSend(
                        db,
                        0,
                        db.Length,
                        SocketFlags.None,
                        delegate(IAsyncResult ar)
                            {
                                try
                                {
                                    client.UnderlyingSocket.EndSend(ar);
                                    client.UnderlyingSocket.Close();
                                }
                                catch (Exception)
                                {
                                }
                            },
                        null);
                }

                return true;
            }
            catch (Exception e)
            {
                ProxyController.LogIt(e.Message + " - " + e.StackTrace);
            }

            return false;
        }
    }
}