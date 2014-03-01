// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorRenderer.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The error renderer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary
{
    #region

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    using PeaRoxy.ClientLibrary.Properties;
    using PeaRoxy.CommonLibrary;
    using PeaRoxy.Platform;

    #endregion

    /// <summary>
    /// The error renderer.
    /// </summary>
    public class ErrorRenderer
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorRenderer"/> class.
        /// </summary>
        public ErrorRenderer()
        {
            this.OnPort443Direct = true;
            this.OnPort80Direct = true;
            this.Enable = true;

            if (ClassRegistry.GetClass<CertManager>().CreateAuthority("PeaRoxy Authority", "PeaRoxy.crt"))
                ClassRegistry.GetClass<CertManager>().RegisterAuthority("PeaRoxy Authority", "PeaRoxy.crt");
        }

        #endregion

        #region Enums

        /// <summary>
        /// The http header code.
        /// </summary>
        public enum HttpHeaderCode
        {
            /// <summary>
            /// 200 ok.
            /// </summary>
            C200Ok, 

            /// <summary>
            /// 500 server error.
            /// </summary>
            C500ServerError, 

            /// <summary>
            /// 501 not implemented.
            /// </summary>
            C501NotImplemented, 

            /// <summary>
            /// 502 bad gateway.
            /// </summary>
            C502BadGateway, 

            /// <summary>
            /// 504 gateway timeout.
            /// </summary>
            C504GatewayTimeout, 

            /// <summary>
            /// 417 expectation failed.
            /// </summary>
            C417ExpectationFailed, 
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether direct error rendering_ port 443.
        /// </summary>
        public bool OnPort443Direct { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether direct error rendering_ port 80.
        /// </summary>
        public bool OnPort80Direct { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether http error rendering is active
        /// </summary>
        public bool Enable { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get cert for domain.
        /// </summary>
        /// <param name="domainName">
        /// The domain name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
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
        /// The render error.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="code">
        /// The code.
        /// </param>
        /// <param name="sslStream">
        /// The SSL stream.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
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
                if (url.Scheme == "http")
                {
                    if (!this.Enable)
                    {
                        return false;
                    }
                }
                else if (url.Scheme == "https" || url.Scheme == "socks")
                {
                    if (
                        !((this.OnPort80Direct && url.Port == 80)
                          || (this.OnPort443Direct && url.Port == 443)))
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

                if (client.Controller.SmartPear.ForwarderSocksEnable
                    && client.Controller.SmartPear.ForwarderHttpsEnable)
                {
                    args.Add("P:SOCKS");
                }

                if (args.Count > 0)
                {
                    conString += " (";
                    foreach (string arg in args)
                    {
                        conString += " " + arg + "; ";
                    }

                    conString += ")";
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
                    default:
                        statusCode = "500 Internal Server Error";
                        break;
                }

                const string NewLineSep = "\r\n";
                string header = "HTTP/1.1 " + statusCode + NewLineSep + "Server: PeaRoxy Error Renderer" + NewLineSep
                                + "Content-Length: " + html.Length + NewLineSep + "Connection: close" + NewLineSep
                                + "Content-Type: text/html;" + NewLineSep + NewLineSep;
                byte[] db = Encoding.ASCII.GetBytes(header + html);

                if (!Common.IsSocketConnected(client.Client))
                {
                    return false;
                }

                if (https)
                {
                    string certAddress = url.DnsSafeHost;
                    if (!Common.IsIpAddress(certAddress))
                    {
                        certAddress = Common.GetNextLevelDomain(certAddress);
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

                    X509Certificate certificate = new X509Certificate2(certAddress, string.Empty);
                    if (sslStream == null)
                    {
                        client.Client.Blocking = true;
                        Stream stream = new NetworkStream(client.Client);
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
                                    client.Client.Close();
                                }
                                catch (Exception)
                                {
                                }
                            }, 
                        null);
                }
                else
                {
                    client.Client.BeginSend(
                        db, 
                        0, 
                        db.Length, 
                        SocketFlags.None, 
                        delegate(IAsyncResult ar)
                            {
                                try
                                {
                                    client.Client.EndSend(ar);
                                    client.Client.Close();
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

        #endregion
    }
}