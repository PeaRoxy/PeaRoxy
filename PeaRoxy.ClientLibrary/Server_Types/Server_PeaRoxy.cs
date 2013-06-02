using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using PeaRoxy.CommonLibrary;
namespace PeaRoxy.ClientLibrary.Server_Types
{
    public class Server_PeaRoxy : ServerType, IDisposable
    {
        internal PeaRoxy.CoreProtocol.PeaRoxyProtocol Protocol;
        private int CurrentTimeout;
        private string Address = string.Empty;
        private ushort Port = 0;
        private Common.Compression_Type compressionType = Common.Compression_Type.None;
        private Common.Encryption_Type encryptionType = Common.Encryption_Type.None;
        public string ServerAddress;
        public string ServerDomain;
        public ushort ServerPort;
        public string Username;
        public string Password;
        public override bool IsServerValid { get; protected set; }
        public override int NoDataTimeout { get; set; }
        public override Proxy_Client ParentClient { get; protected set; }
        public override bool IsDataSent { get; protected set; }
        public override bool IsDisconnected { get; protected set; }
        public override Socket UnderlyingSocket { get; protected set; }
        public Server_PeaRoxy(string address, ushort port, string domain, string username = "", string password = "", Common.Encryption_Type encType = Common.Encryption_Type.None, Common.Compression_Type comType = Common.Compression_Type.None)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("Invalid value.", "Server Address");

            this.ServerDomain = domain.ToLower().Trim();
            if (this.ServerDomain == string.Empty)
                ServerDomain = "~";
            this.IsServerValid = false;
            this.ServerAddress = address;
            this.ServerPort = port;
            this.Username = username;
            this.Password = password;
            this.encryptionType = encType;
            this.compressionType = comType;
            this.NoDataTimeout = 60;
            this.IsDataSent = false;
            this.IsDisconnected = false;
        }

        public override ServerType Clone()
        {
            return new Server_PeaRoxy(ServerAddress, ServerPort, ServerDomain, Username, Password, encryptionType, compressionType) { NoDataTimeout = this.NoDataTimeout };
        }

        public override void Establish(string address, ushort port, Proxy_Client client, byte[] headerData = null, DataCallbackDelegate sndCallback = null, DataCallbackDelegate rcvCallback = null, ConnectionCallbackDelegate connectionStatusCallback = null)
        {
            try
            {
                byte version = 1;
                this.Address = address;
                this.Port = port;
                ParentClient = client;
                this.SndCallback = sndCallback;
                this.RcvCallback = rcvCallback;
                this.ConnectionStatusCallback = connectionStatusCallback;
                UnderlyingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                UnderlyingSocket.BeginConnect(ServerAddress, (int)ServerPort, (AsyncCallback)delegate(IAsyncResult ar)
                {
                    try
                    {
                        UnderlyingSocket.EndConnect(ar);
                        this.ParentClient.Controller.FailAttempts = 0;
                        PeaRoxy.CoreProtocol.HTTPForger.SendRequest(UnderlyingSocket, this.ServerDomain);
                        UnderlyingSocket.Blocking = false;
                        Protocol = new CoreProtocol.PeaRoxyProtocol(UnderlyingSocket, encryptionType, compressionType) { ReceivePacketSize = ParentClient.ReceivePacketSize, SendPacketSize = ParentClient.SendPacketSize, CloseCallback = this.Close };
                        byte[] client_request_pre;
                        if (Username.Trim() != string.Empty && Password != string.Empty)
                        {
                            byte[] username = System.Text.Encoding.ASCII.GetBytes(Username.Trim());
                            byte[] password = System.Text.Encoding.ASCII.GetBytes(Password);
                            password = MD5.Create().ComputeHash(password);
                            if (username.Length > 255)
                            {
                                Close("Username is to long.", null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
                                return;
                            }
                            client_request_pre = new byte[3 + username.Length + password.Length];
                            client_request_pre[0] = 1; // Password Auth type
                            client_request_pre[1] = (byte)username.Length;
                            client_request_pre[username.Length + 2] = (byte)password.Length;

                            Array.Copy(username, 0, client_request_pre, 2, username.Length);
                            Array.Copy(password, 0, client_request_pre, username.Length + 3, password.Length);
                            Protocol.EncryptionKey = System.Text.Encoding.ASCII.GetBytes(Password);
                        }
                        else
                        {
                            client_request_pre = new byte[1];
                            client_request_pre[0] = 0; // No Auth
                        }

                        byte[] client_request;
                        byte client_addressType = 0;
                        IPAddress client_ip;
                        byte[] client_addressBytes;
                        if (IPAddress.TryParse(address, out client_ip))
                        {
                            client_addressBytes = client_ip.GetAddressBytes();
                            if (client_addressBytes.Length == 16)
                                client_addressType = 4;
                            else if (client_addressBytes.Length == 4)
                                client_addressType = 1;
                            else
                            {
                                Close("Unknown IP Type.", null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
                                return;
                            }
                        }
                        else
                        {
                            client_addressType = 3;
                            client_addressBytes = System.Text.Encoding.ASCII.GetBytes(address);
                        }

                        client_request = new byte[6 + client_addressBytes.Length + ((client_addressType == 3) ? 1 : 0)];
                        client_request[0] = version;
                        client_request[1] = 0;
                        client_request[2] = 0;
                        client_request[3] = client_addressType;
                        if (client_addressType == 3)
                        {
                            if (client_addressBytes.Length > 255)
                            {
                                Close("Hostname is too long.", null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
                                return;
                            }
                            client_request[4] = (byte)client_addressBytes.Length;
                        }
                        Array.Copy(client_addressBytes, 0, client_request, 4 + ((client_addressType == 3) ? 1 : 0), client_addressBytes.Length);
                        client_request[client_request.GetUpperBound(0) - 1] = (byte)Math.Floor((double)port / (double)256);
                        client_request[client_request.GetUpperBound(0)] = (byte)(port % 256);
                        Array.Resize(ref client_request_pre, client_request.Length + client_request_pre.Length);
                        Array.Copy(client_request, 0, client_request_pre, client_request_pre.Length - client_request.Length, client_request.Length);
                        Protocol.Write(client_request_pre, false);
                        CoreProtocol.HTTPForger Forger = new CoreProtocol.HTTPForger(UnderlyingSocket);
                        if (!Forger.ReceiveResponse())
                        {
                            Close("HTTPForger failed to validate server response.", null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
                            return;
                        }
                        byte[] server_response = Protocol.Read();
                        if (server_response == null)
                        {
                            Close("Connection timeout.", null, ErrorRenderer.HTTPHeaderCode.C_504_GATEWAY_TIMEOUT);
                            return;
                        }
                        if (server_response[0] != client_request[0])
                        {
                            Close("Server version is different from what we expect. Server's version: " + server_response[0].ToString() + ", Expected: " + client_request[0].ToString(), null, ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                            return;
                        }
                        if (server_response[1] == 99)
                        {
                            Close("Connection failed, Error Code: Auth Failed.", null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
                            return;
                        }
                        else if (server_response[1] != 0)
                        {
                            Close("Connection failed, Error Code: " + server_response[1].ToString(), null, ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                            return;
                        }
                        this.IsServerValid = true;
                        if (headerData != null && Common.IsSocketConnected(UnderlyingSocket))
                        {
                            Protocol.Write(headerData, true);
                        }
                        CurrentTimeout = this.NoDataTimeout * 1000;
                        client.Controller.iMoveToRouting(this);
                    }
                    catch (Exception ex)
                    {
                        this.ParentClient.Controller.FailAttempts++;
                        Close(ex.Message, ex.StackTrace, ErrorRenderer.HTTPHeaderCode.C_504_GATEWAY_TIMEOUT);
                    }
                }, null);
            }
            catch (Exception e)
            {
                Close(e.Message, e.StackTrace);
            }
        }

        public override void DoRoute()
        {
            try
            {
                if ((ParentClient.BusyWrite || Protocol.BusyWrite || (Common.IsSocketConnected(UnderlyingSocket) && Common.IsSocketConnected(ParentClient.Client))) && CurrentTimeout > 0)
                {
                    if (ParentClient.IsSmartForwarderEnable && ParentClient.SmartResponseBuffer.Length > 0 && (CurrentTimeout <= this.NoDataTimeout * 500 || CurrentTimeout <= ((this.NoDataTimeout - ParentClient.Controller.SmartPear.Detector_HTTP_ResponseBufferTimeout) * 1000)) && ParentClient.SmartResponseBuffer.Length > 0)
                    {
                        CurrentTimeout = this.NoDataTimeout * 1000;
                        ParentClient.IsSmartForwarderEnable = false;
                        ParentClient.Write(ParentClient.SmartResponseBuffer);
                    }

                    if (!ParentClient.BusyWrite && Protocol.isDataAvailable())
                    {
                        IsDataSent = true;
                        CurrentTimeout = this.NoDataTimeout * 1000;
                        byte[] buffer = Protocol.Read();
                        if (buffer != null && buffer.Length > 0)
                        {
                            if (RcvCallback != null && RcvCallback.Invoke(ref buffer, this, ParentClient) == false)
                            {
                                ParentClient = null;
                                Close();
                                return;
                            }

                            if (buffer.Length > 0)
                            {
                                ParentClient.Write(buffer);
                            }
                        }
                    }
                    if (!Protocol.BusyWrite && ParentClient.Client.Available > 0 )
                    {
                        IsDataSent = true;
                        CurrentTimeout = this.NoDataTimeout * 1000;
                        byte[] buffer = ParentClient.Read();
                        if (buffer != null && buffer.Length > 0)
                        {
                            if (SndCallback != null && SndCallback.Invoke(ref buffer, this, ParentClient) == false)
                            {
                                ParentClient = null;
                                Close();
                                return;
                            }

                            if (buffer.Length > 0)
                            {
                                Protocol.Write(buffer, true);
                            }
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
            catch (Exception e)
            {
                if (e.TargetSite.Name == "Receive")
                    Close();
                else
                    Close(e.Message, e.StackTrace);
            }
        }

        private void Close(string title, bool async)
        {
            this.Close(title, null, ErrorRenderer.HTTPHeaderCode.C_500_SERVER_ERROR, async);
        }

        private void Close(bool async)
        {
            this.Close(null, null, ErrorRenderer.HTTPHeaderCode.C_500_SERVER_ERROR, async);
        }

        private void Close(string title = null, string message = null, ErrorRenderer.HTTPHeaderCode code = ErrorRenderer.HTTPHeaderCode.C_500_SERVER_ERROR, bool async = false)
        {
            try
            {
                this.IsDisconnected = true;
                if (Protocol != null)
                    Protocol.Close(null, async);

                if (ParentClient != null)
                    ParentClient.Close(title, message, code, async);
            } catch { }
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
            return "PeaRoxy Module " + this.ServerAddress + ":" + this.ServerPort + ((this.Username != string.Empty) ? " Using USN/PSW" : " Open");
        }

        public void Dispose()
        {
            this.UnderlyingSocket.Close();
        }
    }
}
