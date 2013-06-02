using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IO;
using System.Net.Sockets;

namespace PeaRoxy.ClientLibrary
{
    public class ErrorRenderer
    {
        public enum HTTPHeaderCode
        {
            C_200_OK,
            C_500_SERVER_ERROR,
            C_501_NOT_IMPLAMENTED,
            C_502_BAD_GATEWAY,
            C_504_GATEWAY_TIMEOUT,
            C_417_EXPECTATION_FAILED,
        }

        public bool HTTPErrorRendering { get; set; }
        public bool DirectErrorRendering_Port80 { get; set; }
        public bool DirectErrorRendering_Port443 { get; set; }
        public ErrorRenderer()
        {
            DirectErrorRendering_Port443 = true;
            DirectErrorRendering_Port80 = true;
            HTTPErrorRendering = true;

            if (Platform.ClassRegistry.GetClass<Platform.CertManager>().CreateAuthority("PeaRoxy Authority", "HTTPSCerts\\PeaRoxy.crt"))
            {
                Platform.ClassRegistry.GetClass<Platform.CertManager>().RegisterAuthority("PeaRoxy Authority", "HTTPSCerts\\PeaRoxy.crt");
            }
        }

        public static string GetCertForDomain(string domainName)
        {
            domainName = domainName.ToLower().Trim();
            string md5 = CommonLibrary.Common.MD5(domainName);
            if (Platform.ClassRegistry.GetClass<Platform.CertManager>().CreateCert(domainName, "HTTPSCerts\\PeaRoxy.crt", "HTTPSCerts\\" + md5 + ".crt"))
            {
                return "HTTPSCerts\\" + md5 + ".crt";
            }
            return null;
        }

        public bool RenderError(Proxy_Client client, string title, string message, ErrorRenderer.HTTPHeaderCode code = ErrorRenderer.HTTPHeaderCode.C_500_SERVER_ERROR, SslStream sslStream = null)
        {
            try
            {
                if (client.RequestAddress == string.Empty)
                    return false;
                Uri url;
                if (!Uri.TryCreate(client.RequestAddress, UriKind.Absolute, out url))
                    return false;
                bool https = false;
                if (url.Scheme == "http")
                {
                    if (!HTTPErrorRendering)
                        return false;
                }
                else if (url.Scheme == "https" || url.Scheme == "socks")
                {
                    if (!((DirectErrorRendering_Port80 && url.Port == 80) || (DirectErrorRendering_Port443 && url.Port == 443)))
                        return false;
                    https = url.Port == 443;
                }
                else
                    return false;
                if (client.IsReceivingStarted || !client.IsSendingStarted)
                    return false;
                string HTML = PeaRoxy.ClientLibrary.Properties.Resources.HTTPErrorTemplate;
                HTML = HTML.Replace("%VERSION", System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
                HTML = HTML.Replace("%TITLE", title);
                HTML = HTML.Replace("%MESSAGE", CommonLibrary.Common.ConvertToHtmlEntities(message));
                string conString = "Not Connected";
                if (client.Controller.ActiveServer != null)
                    conString = client.Controller.ActiveServer.ToString();
                List<string> l_Arg = new List<string>();
                if (client.Controller.SmartPear.Forwarder_HTTP_Enable)
                    l_Arg.Add("P:HTTP");
                if (client.Controller.SmartPear.Forwarder_HTTPS_Enable)
                    l_Arg.Add("P:HTTPS");
                if (client.Controller.SmartPear.Forwarder_SOCKS_Enable && client.Controller.SmartPear.Forwarder_HTTPS_Enable)
                    l_Arg.Add("P:SOCKS");
                if (l_Arg.Count > 0)
                {
                    conString += " (";
                    foreach (string arg in l_Arg)
                        conString += " " + arg + "; ";
                    conString += ")";
                }
                HTML = HTML.Replace("%CONSTRING", conString);

                string statusCode = string.Empty;
                switch (code)
                {
                    case HTTPHeaderCode.C_200_OK:
                        statusCode = "200 OK";
                        break;
                    case HTTPHeaderCode.C_501_NOT_IMPLAMENTED:
                        statusCode = "501 Not Implemented";
                        break;
                    case HTTPHeaderCode.C_502_BAD_GATEWAY:
                        statusCode = "502 Bad Gateway";
                        break;
                    case HTTPHeaderCode.C_504_GATEWAY_TIMEOUT:
                        statusCode = "504 Gateway Timeout";
                        break;
                    case HTTPHeaderCode.C_417_EXPECTATION_FAILED:
                        statusCode = "417 Expectation Failed";
                        break;
                    default:
                        statusCode = "500 Internal Server Error";
                        break;
                }

                string ctrl = "\r\n";
                string header = "HTTP/1.1 " + statusCode + ctrl +
                                "Server: PeaRoxy Error Renderer" + ctrl +
                                "Content-Length: " + HTML.Length.ToString() + ctrl +
                                "Connection: close" + ctrl +
                                "Content-Type: text/html;" + ctrl + ctrl;
                byte[] db = System.Text.Encoding.ASCII.GetBytes(header + HTML);

                if (!CommonLibrary.Common.IsSocketConnected(client.Client))
                    return false;

                if (https)
                {
                    string certAddress = url.DnsSafeHost;
                    if (!CommonLibrary.Common.IsIPAddress(certAddress))
                    {
                        certAddress = CommonLibrary.Common.GetNextLevelDomain(certAddress);
                        if (certAddress == null || certAddress == string.Empty)
                            return false;
                        //if (certAddress != url.DnsSafeHost)
                        //    certAddress = "*." + certAddress;
                    }
                    certAddress = GetCertForDomain(certAddress);
                    if (certAddress == null || certAddress == string.Empty)
                        return false;
                    X509Certificate certificate = new X509Certificate2(certAddress, "");
                    if (sslStream == null)
                    {
                        client.Client.Blocking = true;
                        Stream stream = new NetworkStream(client.Client);
                        sslStream = new SslStream(stream);
                        sslStream.ReadTimeout = 30 * 1000; // 30 Sec
                        sslStream.WriteTimeout = 30 * 1000; // 30 Sec
                        sslStream.AuthenticateAsServer(certificate);
                    }
                    sslStream.BeginWrite(db, 0, db.Length, (AsyncCallback)delegate(IAsyncResult ar)
                    {
                        try
                        {
                            sslStream.EndWrite(ar);
                            sslStream.Flush();
                            sslStream.Close();
                            client.Client.Close();
                        }
                        catch (Exception) { }
                    }, null);
                }
                else
                {
                    client.Client.BeginSend(db, 0, db.Length, System.Net.Sockets.SocketFlags.None, (AsyncCallback)delegate(IAsyncResult ar)
                    {
                        try
                        {
                            client.Client.EndSend(ar);
                            client.Client.Close();
                        }
                        catch (Exception) { }
                    }, null);
                }
                return true;
            }
            catch (Exception e)
            {
                Proxy_Controller.LogIt(e.Message + " - " + e.StackTrace);
            }
            return false;
        }
    }
}
