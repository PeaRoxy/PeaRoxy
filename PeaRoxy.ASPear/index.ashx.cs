using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PeaRoxy.CommonLibrary;
using PeaRoxy.CoreProtocol;
namespace PeaRoxy.ASPear
{
    public class index : IHttpHandler
    {
        System.Collections.Generic.Dictionary<string, string> Config;
        System.Collections.ObjectModel.Collection<CommonLibrary.ConfigUser> Users;
        HttpContext context;
        public void ProcessRequest(HttpContext context)
        {
            Config = ConfigReader.GetSettings(HttpContext.Current.Server.MapPath("~/settings.ini"));
            Users = ConfigReader.GetUsers(HttpContext.Current.Server.MapPath("~/users.ini"));
            this.context = context;
            CoreProtocol.Cryptors.Cryptor cryptor = new CoreProtocol.Cryptors.Cryptor();
            CoreProtocol.Cryptors.Cryptor peerCryptor = new CoreProtocol.Cryptors.Cryptor();
            Common.EncryptionType encryptionType = Common.EncryptionType.None;
            byte[] encryptionSalt = new byte[4];
            byte[] requestInfo;
            byte[] encryptedHost = new byte[0];
            if (context.Request.Cookies.Count < 1)
                DoError("Request info is missing.");
            try
            {
                requestInfo = Convert.FromBase64String(context.Server.UrlDecode(context.Request.Cookies.Get(0).Value));
                Array.Copy(requestInfo, encryptionSalt, 4);
                encryptionType = (Common.EncryptionType)requestInfo[4];
                encryptedHost = new byte[requestInfo.Length - 5];
                Array.Copy(requestInfo, 5, encryptedHost, 0, requestInfo.Length - 5);
            }
            catch (Exception)
            {
                DoError("Request info is missing.");
            }

            switch (encryptionType)
            {
                case Common.EncryptionType.None:
                    if (Config["SupportedEncryptionTypes".ToLower()] != "0" && Config["SupportedEncryptionTypes".ToLower()] != "-1")
                        DoError("Unsupported encryption type.");
                    break;
                case Common.EncryptionType.SimpleXor:
                    if (Config["SupportedEncryptionTypes".ToLower()] != "2" && Config["SupportedEncryptionTypes".ToLower()] != "-1")
                        DoError("Unsupported encryption type.");
                    break;
                default:
                    DoError("Unsupported encryption type.");
                    break;
            }

            System.IO.Stream stream = context.Request.InputStream;
            byte[] requestBody = new byte[stream.Length];
            int rB = stream.Read(requestBody, 0, requestBody.Length);
            Array.Resize(ref requestBody, rB);

            if (requestBody.Length == 0)
                this.DoError("No request to handle.");

            byte[] encryptionKey = (byte[])encryptionSalt.Clone();
            if (this.Config["AuthMethod".ToLower()] == "1")
            {
                string userName = context.Request.ServerVariables["AUTH_USER"];
                string passWord = context.Request.ServerVariables["AUTH_PASSWORD"];
                if (userName != null && passWord != null && userName != string.Empty && passWord != string.Empty)
                {
                    bool isFound = false;
                    foreach (CommonLibrary.ConfigUser user in Users)
                    {
                        if (userName.ToLower() == user.Username.ToLower() && System.Text.Encoding.ASCII.GetBytes(passWord) == user.Hash)
                        {
                            encryptionKey = System.Text.Encoding.ASCII.GetBytes(user.Password);
                            isFound = true;
                            break;
                        }
                    }
                    if (!isFound)
                        DoError("Authentication failed.");
                }
                else
                    DoError("This server need authentication.");
            }

            switch (encryptionType)
            {
                case Common.EncryptionType.None:
                    peerCryptor = new CoreProtocol.Cryptors.Cryptor();
                    break;
                case Common.EncryptionType.SimpleXor:
                    peerCryptor = new CoreProtocol.Cryptors.SimpleXorCryptor(encryptionKey, false);
                    peerCryptor.SetSalt(encryptionSalt);
                    break;
                default:
                    DoError("Unsupported encryption type.");
                    break;
            }

            string Host = System.Text.Encoding.ASCII.GetString(peerCryptor.Decrypt(encryptedHost));
            byte[] Body = peerCryptor.Decrypt(requestBody);

            if (Config["EncryptionType".ToLower()] == "2")
            {
                if (encryptionType == Common.EncryptionType.SimpleXor)
                    cryptor = peerCryptor;
                else
                {
                    cryptor = new CoreProtocol.Cryptors.SimpleXorCryptor(encryptionKey, false);
                    cryptor.SetSalt(encryptionSalt);
                }
            }

            bool Https = Host.IndexOf("https", 0, Host.Length, StringComparison.CurrentCultureIgnoreCase) == 0;

            int protocolStartPoint = Host.IndexOf("://");

            if (protocolStartPoint != -1)
            {
                protocolStartPoint += 3;
                Host = Host.Substring(protocolStartPoint, Host.Length - protocolStartPoint);
            }

            ushort Port = 80;
            int portStartPoint = Host.IndexOf(":");
            if (portStartPoint != -1)
            {
                portStartPoint += 1;
                Port = ushort.Parse(Host.Substring(portStartPoint, Host.Length - portStartPoint));
                Host = Host.Substring(0, portStartPoint - 1);
            }

            System.IO.Stream connectionStream = null;
            System.Net.Sockets.Socket connectionSocket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            try
            {
                connectionSocket.Connect(Host, Port);
                if (!CommonLibrary.Common.IsSocketConnected(connectionSocket))
                    DoError("Connection failed.");

                connectionStream = new System.Net.Sockets.NetworkStream(connectionSocket);
                if (Https)
                {
                    connectionStream = new System.Net.Security.SslStream(connectionStream, false);
                    ((System.Net.Security.SslStream)connectionStream).AuthenticateAsClient(Host);
                }

                connectionStream.Write(Body, 0, Body.Length);
                connectionStream.Flush();

                context.Response.ContentType = "application/octet-stream";
                string cookieKey = context.Request.Cookies.Keys[0];
                context.Response.SetCookie(new HttpCookie(cookieKey, Config["EncryptionType".ToLower()]));
                context.Response.BufferOutput = false;
                context.Response.Buffer = false;
                context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                context.Response.Cache.SetNoStore();

                context.Response.Flush();
                int timeOut = int.Parse(Config["NoDataConnectionTimeOut".ToLower()]) * 1000;
                int sendPacketSize = int.Parse(Config["SendPacketSize".ToLower()]);
                while (CommonLibrary.Common.IsSocketConnected(connectionSocket) && timeOut > 0 && context.Response.IsClientConnected)
                {
                    if (connectionSocket.Available > 0)
                    {
                        byte[] data = new byte[sendPacketSize];
                        int readLen = connectionStream.Read(data, 0, data.Length);
                        if (readLen == 0)
                            break;
                        Array.Resize(ref data, readLen);
                        data = cryptor.Encrypt(data);
                        timeOut = int.Parse(Config["NoDataConnectionTimeOut".ToLower()]) * 1000;
                        context.Response.BinaryWrite(data);
                        context.Response.Flush();
                    }
                    else
                    {
                        timeOut -= 10;
                        System.Threading.Thread.Sleep(10);
                    }
                }
            }
            catch (Exception ex)
            {
                DoError(ex.Message);
            }
            if (CommonLibrary.Common.IsSocketConnected(connectionSocket))
            {
                connectionStream.Close();
                connectionSocket.Close();
            }
            context.Response.Close();
            context.Response.End();
        }

        private void DoError(string message)
        {
            try
            {
                if (Config["RedirectURL".ToLower()] != "0" && Config["RedirectURL".ToLower()] != string.Empty && context.Request.ServerVariables["HTTP_X_REQUESTED_WITH"] != "NOREDIRECT")
                    context.Response.Redirect(Config["RedirectURL".ToLower()]);
                else
                    context.Response.Write("Server Error: " + message);
                context.Response.End();
            }
            catch (Exception)
            {}
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}