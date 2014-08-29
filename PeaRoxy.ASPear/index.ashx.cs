// --------------------------------------------------------------------------------------------------------------------
// <copyright file="index.ashx.cs" company="PeaRoxy">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The index.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ASPear
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Web;

    using PeaRoxy.CommonLibrary;
    using PeaRoxy.CoreProtocol.Cryptors;

    #endregion

    /// <summary>
    /// The index.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    // ReSharper disable once InconsistentNaming
    public class index : IHttpHandler
    {
        #region Fields

        /// <summary>
        /// The config.
        /// </summary>
        private Dictionary<string, string> config;

        /// <summary>
        /// The users.
        /// </summary>
        private Collection<ConfigUser> users;

        /// <summary>
        /// The webContext.
        /// </summary>
        private HttpContext context;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether is reusable.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The process request.
        /// </summary>
        /// <param name="webContext">
        /// The webContext.
        /// </param>
        public void ProcessRequest(HttpContext webContext)
        {
            this.config = ConfigReader.GetSettings(HttpContext.Current.Server.MapPath("~/settings.ini"));
            this.users = ConfigReader.GetUsers(HttpContext.Current.Server.MapPath("~/users.ini"));
            this.context = webContext;
            Cryptor cryptor = new Cryptor();
            Cryptor peerCryptor;
            Common.EncryptionTypes encryptionType;
            byte[] encryptionSalt = new byte[4];
            byte[] encryptedHost;
            if (webContext.Request.Cookies.Count < 1)
            {
                this.DoError("Request info is missing.");
                return;
            }

            try
            {
                HttpCookie httpCookie = webContext.Request.Cookies.Get(0);
                if (httpCookie == null)
                {
                    this.DoError("Request info is missing.");
                    return;
                }

                string base64 = webContext.Server.UrlDecode(httpCookie.Value);
                if (base64 == null)
                {
                    this.DoError("Request info is invalid.");
                    return;
                }

                byte[] requestInfo = Convert.FromBase64String(base64);
                Array.Copy(requestInfo, encryptionSalt, 4);
                encryptionType = (Common.EncryptionTypes)requestInfo[4];
                encryptedHost = new byte[requestInfo.Length - 5];
                Array.Copy(requestInfo, 5, encryptedHost, 0, requestInfo.Length - 5);
            }
            catch (Exception)
            {
                this.DoError("Request info is missing.");
                return;
            }

            switch (encryptionType)
            {
                case Common.EncryptionTypes.None:
                    if (this.config["SupportedEncryptionTypes".ToLower()] != "0"
                        && this.config["SupportedEncryptionTypes".ToLower()] != "-1")
                    {
                        this.DoError("Unsupported encryption type.");
                        return;
                    }

                    break;
                case Common.EncryptionTypes.SimpleXor:
                    if (this.config["SupportedEncryptionTypes".ToLower()] != "2"
                        && this.config["SupportedEncryptionTypes".ToLower()] != "-1")
                    {
                        this.DoError("Unsupported encryption type.");
                        return;
                    }

                    break;
                default:
                    this.DoError("Unsupported encryption type.");
                    return;
            }

            Stream stream = webContext.Request.InputStream;
            byte[] requestBody = new byte[stream.Length];
            int rB = stream.Read(requestBody, 0, requestBody.Length);
            Array.Resize(ref requestBody, rB);

            if (requestBody.Length == 0)
            {
                this.DoError("No request to handle.");
                return;
            }

            byte[] encryptionKey = (byte[])encryptionSalt.Clone();
            if (this.config["AuthMethod".ToLower()] == "1")
            {
                string userName = webContext.Request.ServerVariables["AUTH_USER"];
                string passWord = webContext.Request.ServerVariables["AUTH_PASSWORD"];
                if (userName != null && passWord != null && userName != string.Empty && passWord != string.Empty)
                {
                    bool isFound = false;
                    foreach (
                        ConfigUser user in
                            this.users.Where(
                                user =>
                                string.Equals(userName, user.Username, StringComparison.CurrentCultureIgnoreCase)
                                && Encoding.ASCII.GetBytes(passWord) == user.Hash))
                    {
                        encryptionKey = Encoding.ASCII.GetBytes(user.Password);
                        isFound = true;
                        break;
                    }

                    if (!isFound)
                    {
                        this.DoError("Authentication failed.");
                        return;
                    }
                }
                else
                {
                    this.DoError("This server need authentication.");
                    return;
                }
            }

            switch (encryptionType)
            {
                case Common.EncryptionTypes.None:
                    peerCryptor = new Cryptor();
                    break;
                case Common.EncryptionTypes.SimpleXor:
                    peerCryptor = new SimpleXorCryptor(encryptionKey, false);
                    peerCryptor.SetSalt(encryptionSalt);
                    break;
                default:
                    this.DoError("Unsupported encryption type.");
                    return;
            }

            string host = Encoding.ASCII.GetString(peerCryptor.Decrypt(encryptedHost));
            byte[] body = peerCryptor.Decrypt(requestBody);

            if (this.config["EncryptionType".ToLower()] == "2")
            {
                if (encryptionType == Common.EncryptionTypes.SimpleXor)
                {
                    cryptor = peerCryptor;
                }
                else
                {
                    cryptor = new SimpleXorCryptor(encryptionKey, false);
                    cryptor.SetSalt(encryptionSalt);
                }
            }

            bool https = host.IndexOf("https", 0, host.Length, StringComparison.OrdinalIgnoreCase) == 0;

            int protocolStartPoint = host.IndexOf("://", StringComparison.Ordinal);

            if (protocolStartPoint != -1)
            {
                protocolStartPoint += 3;
                host = host.Substring(protocolStartPoint, host.Length - protocolStartPoint);
            }

            ushort port = 80;
            int portStartPoint = host.IndexOf(":", StringComparison.Ordinal);
            if (portStartPoint != -1)
            {
                portStartPoint += 1;
                port = ushort.Parse(host.Substring(portStartPoint, host.Length - portStartPoint));
                host = host.Substring(0, portStartPoint - 1);
            }

            Stream connectionStream;
            Socket connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                connectionSocket.Connect(host, port);
                if (!Common.IsSocketConnected(connectionSocket))
                {
                    this.DoError("Connection failed.");
                    return;
                }

                connectionStream = new NetworkStream(connectionSocket);
                if (https)
                {
                    connectionStream = new SslStream(connectionStream, false);
                    ((SslStream)connectionStream).AuthenticateAsClient(host);
                }

                connectionStream.Write(body, 0, body.Length);
                connectionStream.Flush();

                webContext.Response.ContentType = "application/octet-stream";
                string cookieKey = webContext.Request.Cookies.Keys[0];
                webContext.Response.SetCookie(new HttpCookie(cookieKey, this.config["EncryptionType".ToLower()]));
                webContext.Response.BufferOutput = false;
                webContext.Response.Buffer = false;
                webContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                webContext.Response.Cache.SetNoStore();

                webContext.Response.Flush();
                int timeOut = int.Parse(this.config["NoDataConnectionTimeOut".ToLower()]) * 1000;
                int sendPacketSize = int.Parse(this.config["SendPacketSize".ToLower()]);
                while (Common.IsSocketConnected(connectionSocket) && timeOut > 0 && webContext.Response.IsClientConnected)
                {
                    if (connectionSocket.Available > 0)
                    {
                        byte[] data = new byte[sendPacketSize];
                        int readLen = connectionStream.Read(data, 0, data.Length);
                        if (readLen == 0)
                        {
                            break;
                        }

                        Array.Resize(ref data, readLen);
                        data = cryptor.Encrypt(data);
                        timeOut = int.Parse(this.config["NoDataConnectionTimeOut".ToLower()]) * 1000;
                        webContext.Response.BinaryWrite(data);
                        webContext.Response.Flush();
                    }
                    else
                    {
                        timeOut -= 10;
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception ex)
            {
                this.DoError(ex.Message);
                return;
            }

            if (Common.IsSocketConnected(connectionSocket))
            {
                connectionStream.Close();
                connectionSocket.Close();
            }

            webContext.Response.Close();
            webContext.Response.End();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The do error.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        private void DoError(string message)
        {
            try
            {
                if (this.config["RedirectURL".ToLower()] != "0" && this.config["RedirectURL".ToLower()] != string.Empty
                    && this.context.Request.ServerVariables["HTTP_X_REQUESTED_WITH"] != "NOREDIRECT")
                {
                    this.context.Response.Redirect(this.config["RedirectURL".ToLower()]);
                }
                else
                {
                    this.context.Response.Write("Server Error: " + message);
                }

                this.context.Response.End();
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}