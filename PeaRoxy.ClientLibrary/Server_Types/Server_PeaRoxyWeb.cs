using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using PeaRoxy.CommonLibrary;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Net.Security;
using PeaRoxy.ClientLibrary.Proxy_Types;
namespace PeaRoxy.ClientLibrary.Server_Types
{
    public class Server_PeaRoxyWeb : ServerType, IDisposable
    {
        private static RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();
        private byte[] writeBuffer = new byte[0];
        private int CurrentTimeout;
        private string Address = string.Empty;
        private ushort Port = 0;
        private bool isHTTPS = false;
        private bool isDoneForRouting = false;
        private Common.Encryption_Type encryptionType = Common.Encryption_Type.None;
        private Common.Encryption_Type serverEncryptionType = Common.Encryption_Type.None;
        private byte[] encryptionSaltBytes = new byte[4];
        private byte[] encryptionKey;
        private PeaRoxy.CoreProtocol.Cryptors.Cryptor Cryptor = new PeaRoxy.CoreProtocol.Cryptors.Cryptor();
        private PeaRoxy.CoreProtocol.Cryptors.Cryptor peerCryptor = new PeaRoxy.CoreProtocol.Cryptors.Cryptor();
        private int readedBytes = 0;
        private int contentBytes = -1;
        private SslStream clientsslStream = null;
        public Uri ServerURI;
        public string ServerAddress;
        public string Username;
        public string Password;
        public override bool IsServerValid { get; protected set; }
        public override int NoDataTimeout { get; set; }
        public override Proxy_Client ParentClient { get; protected set; }
        public override bool IsDataSent { get; protected set; }
        public override bool IsDisconnected { get; protected set; }
        public override Socket UnderlyingSocket { get; protected set; }
        public bool BusyWrite
        {
            get
            {
                if (writeBuffer.Length > 0)
                    this.Write(null);
                return (writeBuffer.Length > 0);
            }
        }

        public Server_PeaRoxyWeb(string address, string username = "", string password = "", Common.Encryption_Type encType = Common.Encryption_Type.None, bool addressChecked = false)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("Invalid value.", "Server Address");

            if (encType != Common.Encryption_Type.SimpleXOR && encType != Common.Encryption_Type.None)
                throw new ArgumentException("Invalid value.", "Encryption Type");


            this.IsServerValid = false;
            this.ServerAddress = address;
            this.ServerURI = new Uri(address);

            if (!addressChecked)
            {
                HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create(this.ServerURI);
                wreq.AllowAutoRedirect = true;
                wreq.Proxy = null;
                wreq.Method = "HEAD";
                wreq.Headers.Add("X-Requested-With", "NOREDIRECT");
                HttpWebResponse wres = (HttpWebResponse)wreq.GetResponse();
                wres.Close();
                if (wres.StatusCode != HttpStatusCode.OK)
                {
                    throw new ArgumentException("Server responded with status code " + wres.StatusCode.ToString(), "Server Address");
                }
                this.ServerURI = wres.ResponseUri;
                this.ServerAddress = this.ServerURI.ToString();
            }

            this.Username = username;
            this.Password = password;
            this.encryptionType = encType;
            this.NoDataTimeout = 60;
            this.IsDataSent = false;
            this.IsDisconnected = false;
        }

        public override ServerType Clone()
        {
            return new Server_PeaRoxyWeb(ServerAddress, Username, Password, encryptionType, true) { NoDataTimeout = this.NoDataTimeout };
        }

        public override void Establish(string address, ushort port, Proxy_Client client, byte[] headerData = null, DataCallbackDelegate sndCallback = null, DataCallbackDelegate rcvCallback = null, ConnectionCallbackDelegate connectionStatusCallback = null)
        {
            try
            {
                this.Address = address;
                this.Port = port;
                ParentClient = client;
                this.SndCallback = sndCallback;
                this.RcvCallback = rcvCallback;
                this.ConnectionStatusCallback = connectionStatusCallback;
                UnderlyingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                UnderlyingSocket.BeginConnect(ServerURI.Host, ServerURI.Port, (AsyncCallback)delegate(IAsyncResult ar)
                {
                    try
                    {
                        UnderlyingSocket.EndConnect(ar);
                        UnderlyingSocket.Blocking = false;
                        this.ParentClient.Controller.FailAttempts = 0;
                        bool sucsess = false;

                        sucsess = CheckConnectionTypeWithClient(ref headerData);
                        if (!sucsess) return;

                        sucsess = this.SendRequestToServer(ref headerData);
                        if (!sucsess) return;

                        sucsess = this.ReadServerResponce(ref headerData);
                        if (!sucsess) return;
                        ParentClient.Controller.iMoveToRouting(this);

                        sucsess = this.ReadHeaderOfActualResponce(ref headerData);
                        if (!sucsess) return;

                        WriteToClient(headerData);
                        isDoneForRouting = true;
                        CurrentTimeout = this.NoDataTimeout * 1000;
                    }
                    catch (Exception ex)
                    {
                        this.ParentClient.Controller.FailAttempts++;
                        Close(ex.Message, ex.StackTrace, ErrorRenderer.HTTPHeaderCode.C_500_SERVER_ERROR);
                    }
                }, null);
            }
            catch (Exception e)
            {
                Close(e.Message, e.StackTrace);
            }
        }

        private bool CheckConnectionTypeWithClient(ref byte[] bytes)
        {
            if (bytes.Length == 0) // IS HTTPS
            {
                try
                {
                    Uri url;
                    if (!Uri.TryCreate(ParentClient.RequestAddress, UriKind.Absolute, out url))
                    {
                        this.Close("Unrecognizable address.", null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
                        return false;
                    }

                    string certAddress = url.DnsSafeHost;
                    if (!CommonLibrary.Common.IsIPAddress(certAddress))
                    {
                        certAddress = CommonLibrary.Common.GetNextLevelDomain(certAddress);
                        if (certAddress == null || certAddress == string.Empty)
                        {
                            this.Close("Domain name is not acceptable for HTTPS connection.", url.ToString(), ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
                            return false;
                        }
                        //if (certAddress != url.DnsSafeHost)
                        //      certAddress = "*." + certAddress;
                    }

                    certAddress = ErrorRenderer.GetCertForDomain(certAddress);
                    if (certAddress == null || certAddress == string.Empty)
                    {
                        this.Close("No certificate available or failed to generate one.", certAddress, ErrorRenderer.HTTPHeaderCode.C_500_SERVER_ERROR);
                        return false;
                    }
                    X509Certificate certificate = new X509Certificate2(certAddress, "");
                    System.Net.Sockets.TcpClient tc = new System.Net.Sockets.TcpClient();
                    ParentClient.Client.Blocking = true;
                    Stream stream = new NetworkStream(ParentClient.Client);
                    clientsslStream = new SslStream(stream);
                    clientsslStream.ReadTimeout = 30 * 1000; // 30 Sec
                    clientsslStream.WriteTimeout = 30 * 1000; // 30 Sec
                    clientsslStream.AuthenticateAsServer(certificate);
                    byte[] zeroByteBuffer = new byte[0];
                    bytes = new byte[16384];
                    string headerString = "";
                    bool headerRec = false;
                    int arraySize = 0;
                    do
                    {
                        if (arraySize >= bytes.Length)
                        {
                            this.Close("No header after 16KB of data.", null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
                            return false;
                        }
                        int readCount = clientsslStream.Read(bytes, arraySize, bytes.Length - arraySize);
                        if (readCount > 0)
                            arraySize += readCount;
                        else
                        {
                            this.Close("Connection closed by client.", null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
                            return false;
                        }
                        headerString += System.Text.Encoding.ASCII.GetString(bytes, arraySize - readCount, readCount);
                        if (headerString.IndexOf("\r\n\r\n") != -1)
                            headerRec = true;
                    } while (!headerRec);
                    Array.Resize(ref bytes, arraySize);
                    if (Proxy_HTTP.IsHTTP(bytes))
                        isHTTPS = true;
                    else
                    {
                        this.Close("Connection header is not HTTP.", null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
                        return false;
                    }
                }
                catch (Exception)
                {
                    this.Close();
                    return false;
                }
            }
            else if (!Proxy_HTTP.IsHTTP(bytes))
            {
                this.Close("PeaRoxy supports only HTTPS and HTTP connections currently.", null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
                return false;
            }
            return true;
        }

        private bool SendRequestToServer(ref byte[] bytes)
        {
            //--------------------------- Read request length
            int requestContentLength = -1;
            bool needContentLength = false;
            string textData = System.Text.Encoding.ASCII.GetString(bytes);
            int pDest = textData.IndexOf("\r\n\r\n");
            if (pDest != -1)
                textData = textData.Substring(0, pDest + 4);
            int start = textData.IndexOf("Content-Length:", StringComparison.InvariantCultureIgnoreCase);
            if (start != -1)
            {
                start += "Content-Length:".Length;
                int count = textData.IndexOf("\r\n", start) - start;
                requestContentLength = int.Parse(textData.Substring(start, count).Trim());
            }

            if (textData.StartsWith("POST", StringComparison.InvariantCultureIgnoreCase))
            {
                if (requestContentLength == -1)
                {
                    needContentLength = false;
                    requestContentLength = 0;
                }
                else
                    needContentLength = true;
            }
            else
            {
                if (requestContentLength == -1)
                    requestContentLength = 0;
                needContentLength = true;
            }

            if (needContentLength)
                requestContentLength += System.Text.Encoding.ASCII.GetByteCount(textData);

            //--------------------------- Appending Request Header
            string HTTPHeader = "POST " + ServerURI.PathAndQuery + " HTTP/1.1" + "\r\n";
            HTTPHeader += "Host: " + ServerURI.Host + "\r\n";
            HTTPHeader += "Content-Type: text/plain" + "\r\n";
            if (needContentLength)
                HTTPHeader += "Content-Length: " + requestContentLength.ToString() + "\r\n";
            else
            {
                HTTPHeader += "TE: chunked" + "\r\n";
                HTTPHeader += "Transfer-Encoding: chunked" + "\r\n";
            }
            HTTPHeader += "Connection: close" + "\r\n";

            rnd.GetNonZeroBytes(encryptionSaltBytes);
            encryptionKey = encryptionSaltBytes;

            if (this.Username != string.Empty)
            {
                HTTPHeader += "Authorization: Basic " + Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(this.Username + ":" + Common.MD5(this.Password))) + "\r\n";
                encryptionKey = System.Text.Encoding.ASCII.GetBytes(this.Password);
            }

            switch (encryptionType)
            {
                case Common.Encryption_Type.SimpleXOR:
                    Cryptor = new CoreProtocol.Cryptors.SimpleXORCryptor(encryptionKey, false);
                    break;
            }

            Cryptor.SetSalt(encryptionSaltBytes);
            byte[] hostBytes = System.Text.Encoding.ASCII.GetBytes((isHTTPS ? "https://" : "http://") + this.Address + ":" + this.Port.ToString());
            hostBytes = Cryptor.Encrypt(hostBytes);
            byte[] encryptedOutput = new byte[hostBytes.Length + 5];
            Array.Copy(hostBytes, 0, encryptedOutput, 5, hostBytes.Length);
            Array.Copy(encryptionSaltBytes, 0, encryptedOutput, 0, encryptionSaltBytes.Length);
            encryptedOutput[4] = (byte)encryptionType;
            string cookieValue = System.IO.Path.GetRandomFileName().Replace(".", "") +
                "=" + Uri.EscapeDataString(Convert.ToBase64String(encryptedOutput)) + "; ";
            HTTPHeader += "Cookie: " + cookieValue + "\r\n";
            HTTPHeader += "\r\n";

            //--------------------------- Write Header and request
            this.Write(System.Text.Encoding.ASCII.GetBytes(HTTPHeader), false);
            this.Write(bytes);

            int readed = bytes.Length;
            int timeout = this.NoDataTimeout * 1000;
            while ((readed < requestContentLength || !needContentLength) && timeout > 0)
            {
                if (this.BusyWrite)
                {
                    timeout--;
                    System.Threading.Thread.Sleep(1);
                    continue;
                }
                byte[] buffer = new byte[8192];
                int readCount = 0;
                if (isHTTPS)
                {
                    readCount = clientsslStream.Read(buffer, 0, Math.Min(buffer.Length, requestContentLength - readed));
                    Array.Resize(ref buffer, readCount);
                }
                else
                {
                    if (ParentClient.Client.Available > 0)
                    {
                        timeout = this.NoDataTimeout * 1000;
                        buffer = ParentClient.Read();
                        readCount = buffer.Length;
                    }
                    else
                    {
                        timeout--;
                        System.Threading.Thread.Sleep(1);
                        continue;
                    }
                }
                if (readCount == 0)
                {
                    this.Close("Connection closed by client.", null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
                    return false;
                }
                readed += readCount;
                this.Write(buffer);
                
                if (!needContentLength && buffer.Length >= 5 && PeaRoxy.CommonLibrary.Common.GetFirstBytePatternIndex(buffer, System.Text.Encoding.ASCII.GetBytes("0\r\n\r\n"), buffer.Length - 5) != -1)
                    break;
            }
            IsDataSent = true;
            return true;
        }

        private bool ReadServerResponce(ref byte[] bytes)
        {
            CurrentTimeout = 60000; //this.NoDataTimeout * 1000; // let make it 60sec
            string responseHeader = "";
            bytes = new byte[0];
            while (responseHeader.IndexOf("\r\n\r\n") == -1)
            {
                if ((ParentClient.Client != null && !Common.IsSocketConnected(ParentClient.Client)) || !Common.IsSocketConnected(UnderlyingSocket))
                {
                    this.Close();
                    return false;
                }
                if (UnderlyingSocket.Available > 0)
                {
                    CurrentTimeout = this.NoDataTimeout * 1000;
                    byte[] response = this.Read(false);
                    if (response == null || response.Length <= 0)
                    {
                        this.Close("No response from server, Read Failure.", null, ErrorRenderer.HTTPHeaderCode.C_504_GATEWAY_TIMEOUT);
                        return false;
                    }
                    Array.Resize(ref bytes, bytes.Length + response.Length);
                    Array.Copy(response, 0, bytes, bytes.Length - response.Length, response.Length);
                    responseHeader += System.Text.Encoding.ASCII.GetString(response);
                }
                else
                {
                    if (!this.BusyWrite)
                    {
                        CurrentTimeout--;
                        if (CurrentTimeout == 0)
                        {
                            this.Close("No response from server, Timeout.", null, ErrorRenderer.HTTPHeaderCode.C_504_GATEWAY_TIMEOUT);
                            return false;
                        }
                    }
                    System.Threading.Thread.Sleep(1);
                }
            }
            int endOfHeaderIndex = responseHeader.IndexOf("\r\n\r\n") + 4;
            responseHeader = responseHeader.Substring(0, endOfHeaderIndex);
            if (!responseHeader.StartsWith("HTTP/1.1 200", StringComparison.InvariantCultureIgnoreCase) && !responseHeader.StartsWith("HTTP/1.0 200", StringComparison.InvariantCultureIgnoreCase))
            {
                this.Close("Bad server response. Is address of server ", responseHeader.Substring(0, Math.Min(responseHeader.Length, responseHeader.IndexOf("\r"))), ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                return false;
            }
            int headerLenght = System.Text.Encoding.ASCII.GetByteCount(responseHeader);
            Array.Copy(bytes, headerLenght, bytes, 0, bytes.Length - headerLenght);
            Array.Resize(ref bytes, bytes.Length - headerLenght);

            //--------------------------- We have header. Reading cookies for getting encryption type
            int indexOfCookies = responseHeader.IndexOf("\r\nSet-Cookie: ", StringComparison.InvariantCultureIgnoreCase);
            if (indexOfCookies != -1)
            {
                int startOfCookies = responseHeader.IndexOf("=", indexOfCookies, StringComparison.InvariantCultureIgnoreCase) + 1;
                int endOfCookies = responseHeader.IndexOf(";", indexOfCookies, StringComparison.InvariantCultureIgnoreCase);
                serverEncryptionType = (Common.Encryption_Type)int.Parse(responseHeader.Substring(startOfCookies, endOfCookies - startOfCookies));
            }

            if (serverEncryptionType == encryptionType)
                peerCryptor = Cryptor;
            else
                switch (serverEncryptionType)
                {
                    case Common.Encryption_Type.None:
                        // Do Nothing. It is OK
                        break;
                    case Common.Encryption_Type.SimpleXOR:
                        peerCryptor = new CoreProtocol.Cryptors.SimpleXORCryptor(encryptionKey, false);
                        break;
                    default:
                        this.Close("Unsupported encryption method used by server.", null, ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                        return false;
                }
            bytes = peerCryptor.Decrypt(bytes);
            return true;
        }

        private bool ReadHeaderOfActualResponce(ref byte[] bytes)
        {
            //--------------------------- Everything is Done. But let wait until we read header of actual response
            string Header = System.Text.Encoding.ASCII.GetString(bytes);
            bool erChecked = false;
            CurrentTimeout = this.NoDataTimeout * 1000;
            while (Header.IndexOf("\r\n\r\n") == -1)
            {
                if (!erChecked && Header != string.Empty)
                {
                    erChecked = true;
                    if (Header.StartsWith("Server Error:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.Close(Header, null, ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                        return false;
                    }
                }
                if ((ParentClient.Client != null && !Common.IsSocketConnected(ParentClient.Client)) || !Common.IsSocketConnected(UnderlyingSocket))
                {
                    this.Close();
                    return false;
                }
                if (UnderlyingSocket.Available > 0)
                {
                    CurrentTimeout = this.NoDataTimeout * 1000;
                    byte[] response = this.Read(false);
                    if (response == null || response.Length <= 0)
                    {
                        this.Close("No response from server, Read Failure.", null, ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                        return false;
                    }
                    Array.Resize(ref bytes, bytes.Length + response.Length);
                    Array.Copy(response, 0, bytes, bytes.Length - response.Length, response.Length);
                    Header += System.Text.Encoding.ASCII.GetString(response);
                }
                else
                {
                    CurrentTimeout--;
                    if (CurrentTimeout == 0)
                    {
                        this.Close("No response from server, Timeout.", null, ErrorRenderer.HTTPHeaderCode.C_504_GATEWAY_TIMEOUT);
                        return false;
                    }
                    System.Threading.Thread.Sleep(1);
                }
            }
            this.IsServerValid = true;
            int endOfHeaderIndex = Header.IndexOf("\r\n\r\n") + 4;
            Header = Header.Substring(0, endOfHeaderIndex);
            int headerLenght = System.Text.Encoding.ASCII.GetByteCount(Header);
            Array.Copy(bytes, headerLenght, bytes, 0, bytes.Length - headerLenght);
            Array.Resize(ref bytes, bytes.Length - headerLenght);

            //--------------------------- It is better to add connection close to response header. Of course it may not solve our keep alive problem so we keep
            // content length of response too so we can close it later.
            int start = Header.IndexOf("Content-Length:", StringComparison.InvariantCultureIgnoreCase);
            if (start != -1)
            {
                start += "Content-Length:".Length;
                int count = Header.IndexOf("\r\n", start) - start;
                contentBytes = int.Parse(Header.Substring(start, count).Trim());
            }

            int indexOfConnectionType = Header.IndexOf("\r\nConnection: ", StringComparison.InvariantCultureIgnoreCase);
            if (indexOfConnectionType != -1)
            {
                int countUntilEndOfLine = Header.IndexOf("\r\n", indexOfConnectionType + 2) - indexOfConnectionType;
                Header = Header.Remove(indexOfConnectionType, countUntilEndOfLine);
            }
            Header = Header.Insert(Header.Length - 2, "Connection: close\r\nProxy-Connection: close\r\n");

            byte[] headerBytes = System.Text.Encoding.ASCII.GetBytes(Header);
            if (contentBytes != -1)
                contentBytes += headerBytes.Length;
            Array.Resize(ref headerBytes, headerBytes.Length + bytes.Length);
            Array.Copy(bytes, 0, headerBytes, headerBytes.Length - bytes.Length, bytes.Length);
            bytes = headerBytes;
            return true;
        }

        private void WriteToClient(byte[] bytes)
        {
            if (isHTTPS && clientsslStream != null)
                clientsslStream.Write(bytes);
            else
                ParentClient.Write(bytes);

            if (contentBytes != -1)
            {
                readedBytes += bytes.Length;
                if (readedBytes >= contentBytes)
                    UnderlyingSocket.Close();
            }
            else if (bytes.Length >= 5 && PeaRoxy.CommonLibrary.Common.GetFirstBytePatternIndex(bytes, System.Text.Encoding.ASCII.GetBytes("0\r\n\r\n"), bytes.Length - 5) != -1)
                UnderlyingSocket.Close();
        }

        public override void DoRoute()
        {
            try
            {
                if (!isDoneForRouting)
                    return;
                if ((ParentClient.BusyWrite || this.BusyWrite || (Common.IsSocketConnected(UnderlyingSocket) && Common.IsSocketConnected(ParentClient.Client))) && CurrentTimeout > 0)
                {
                    if (ParentClient.IsSmartForwarderEnable && ParentClient.SmartResponseBuffer.Length > 0 && (CurrentTimeout <= this.NoDataTimeout * 500 || CurrentTimeout <= ((this.NoDataTimeout - ParentClient.Controller.SmartPear.Detector_HTTP_ResponseBufferTimeout) * 1000)))
                    {
                        CurrentTimeout = this.NoDataTimeout * 1000;
                        ParentClient.IsSmartForwarderEnable = false;
                        if (isHTTPS && clientsslStream != null)
                            clientsslStream.Write(ParentClient.SmartResponseBuffer);
                        else
                            ParentClient.Write(ParentClient.SmartResponseBuffer);
                    }

                    if (!ParentClient.BusyWrite && UnderlyingSocket.Available > 0)
                    {
                        IsDataSent = true;
                        CurrentTimeout = this.NoDataTimeout * 1000;
                        byte[] buffer = this.Read();
                        if (buffer != null && buffer.Length > 0)
                        {
                            if (RcvCallback != null && RcvCallback.Invoke(ref buffer, this, ParentClient) == false)
                            {
                                ParentClient = null;
                                Close();
                                return;
                            }

                            if (buffer.Length > 0)
                                WriteToClient(buffer);
                        }
                    }
                    if (!this.BusyWrite && ParentClient.Client.Available > 0)
                    {
                        if (isHTTPS)
                            Close();
                        byte[] buffer = ParentClient.Read();
                        if (buffer != null && buffer.Length > 0)
                        {
                            if (SndCallback != null && SndCallback.Invoke(ref buffer, this, ParentClient) == false)
                            {
                                ParentClient = null;
                                Close();
                                return;
                            }
                            Close(false);
                        }
                    }
                    CurrentTimeout--;
                }
                else
                {
                    if (IsDataSent == false && ConnectionStatusCallback != null && ConnectionStatusCallback.Invoke(false, this, ParentClient) == false)
                        ParentClient = null;
                    Close(false);
                }
            }
            catch (Exception) // e)
            {
                //if (e.TargetSite.Name == "Receive")
                //    Close();
                //else
                //    Close(e.Message, e.StackTrace);
            }
        }

        private void Write(byte[] bytes,bool encryption = true)
        {
            try
            {
                if (bytes != null)
                {
                    if (encryption)
                        bytes = Cryptor.Encrypt(bytes);

                    Array.Resize(ref writeBuffer, writeBuffer.Length + bytes.Length);
                    Array.Copy(bytes, 0, writeBuffer, writeBuffer.Length - bytes.Length, bytes.Length);
                }
                if (writeBuffer.Length > 0 && UnderlyingSocket.Poll(0, SelectMode.SelectWrite))
                {
                    int bytesWritten = UnderlyingSocket.Send(writeBuffer, SocketFlags.None);
                    Array.Copy(writeBuffer, bytesWritten, writeBuffer, 0, writeBuffer.Length - bytesWritten);
                    Array.Resize(ref writeBuffer, writeBuffer.Length - bytesWritten);
                }
            }
            catch (Exception e)
            {
                Close(e.Message, e.StackTrace);
            }
        }

        private byte[] Read(bool encryption = true)
        {
            try
            {
                int i = 60; // Time out by second

                i = i * 100;
                while (i > 0)
                {
                    if (UnderlyingSocket.Available > 0)
                    {
                        byte[] buffer = new byte[ParentClient.ReceivePacketSize];
                        int bytes = UnderlyingSocket.Receive(buffer);
                        Array.Resize(ref buffer, bytes);

                        if (encryption)
                            buffer = peerCryptor.Decrypt(buffer);

                        return buffer;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(10);
                        i--;
                    }
                }
            }
            catch (Exception e)
            {
                Close(e.Message, e.StackTrace);
            }
            return null;
        }

        private void Close(bool async)
        {
            this.Close(null, null, ErrorRenderer.HTTPHeaderCode.C_500_SERVER_ERROR, async);
        }

        private void Close(string title = null, string message = null, ErrorRenderer.HTTPHeaderCode code = ErrorRenderer.HTTPHeaderCode.C_500_SERVER_ERROR, bool async = false)
        {
            try
            {
                if (ParentClient != null)
                    if (isHTTPS && clientsslStream != null)
                        ParentClient.Close(title, message, code, async, clientsslStream);
                    else
                        ParentClient.Close(title, message, code, async);

                this.IsDisconnected = true;

                if (async)
                {
                    byte[] db = new byte[0];
                    if (UnderlyingSocket != null)
                        UnderlyingSocket.BeginSend(db, 0, db.Length, SocketFlags.None, (AsyncCallback)delegate(IAsyncResult ar)
                        {
                            try
                            {
                                UnderlyingSocket.Close(); // Close request connection it-self
                                UnderlyingSocket.EndSend(ar);
                            }
                            catch (Exception) { }
                        }, null);
                }
                else
                {
                    if (UnderlyingSocket != null)
                        UnderlyingSocket.Close();
                }
            }
            catch { }
        }

        public override string GetAddress()
        {
            return Address;
        }

        public override ushort GetPort()
        {
            return Port;
        }

        public override string ToString()
        {
            return "PeaRoxyWeb Module " + this.ServerAddress + ((this.Username != string.Empty) ? " Using USN/PSW" : " Open");
        }

        public void Dispose()
        {
            this.UnderlyingSocket.Dispose();
            if (this.clientsslStream != null)
                this.clientsslStream.Dispose();
        }
    }
}