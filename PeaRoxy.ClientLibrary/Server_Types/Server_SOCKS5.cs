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
    public class Server_SOCKS5 : ServerType, IDisposable
    {
        private byte[] writeBuffer = new byte[0];
        private int CurrentTimeout;
        private string Address = string.Empty;
        private ushort Port = 0;
        public string ServerAddress;
        public ushort ServerPort;
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
        public Server_SOCKS5(string address, ushort port, string username = "", string password = "")
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("Invalid value.", "Server Address");

            this.IsServerValid = false;
            this.ServerAddress = address;
            this.ServerPort = port;
            this.Username = username;
            this.Password = password;
            this.NoDataTimeout = 60;
            this.IsDataSent = false;
            this.IsDisconnected = false;
        }

        public override ServerType Clone()
        {
            return new Server_SOCKS5(ServerAddress, ServerPort, Username, Password) { NoDataTimeout = this.NoDataTimeout };
        }

        public override void Establish(string address, ushort port, Proxy_Client client, byte[] headerData = null, DataCallbackDelegate sndCallback = null, DataCallbackDelegate rcvCallback = null, ConnectionCallbackDelegate connectionStatusCallback = null)
        {
            try
            {
                byte version = 5;
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
                        UnderlyingSocket.Blocking = false;
                        byte[] client_request = new byte[0];
                        byte[] server_response;
                        if (Username.Trim() != string.Empty && Password != string.Empty)
                        {
                            Array.Resize(ref client_request, 4);
                            client_request[0] = version; // version
                            client_request[1] = 2; // 2 auth method
                            client_request[2] = 0; // No auth
                            client_request[3] = 2; // User and pass
                        }
                        else
                        {
                            Array.Resize(ref client_request, 3);
                            client_request[0] = version; // version
                            client_request[1] = 1; // 1 auth method
                            client_request[2] = 0; // No auth
                        }
                        this.Write(client_request, false);
                        server_response = this.Read();
                        if (server_response == null || server_response.Length < 2)
                        {
                            Close("Connection timeout.", null, ErrorRenderer.HTTPHeaderCode.C_504_GATEWAY_TIMEOUT);
                            return;
                        }
                        if (server_response[0] != version)
                        {
                            Close("Unsupported version of proxy.", null, ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                            return;
                        }
                        if ((server_response[1] != 0 && server_response[1] != 2) || (server_response[1] == 2 && !(Username.Trim() != string.Empty && Password != string.Empty)))
                        {
                            Close("Unsupported authentication method.", null, ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                            return;
                        }

                        if (server_response[1] == 2 && Username.Trim() != string.Empty && Password != string.Empty)
                        {
                            byte[] username = System.Text.Encoding.ASCII.GetBytes(Username.Trim());
                            byte[] password = System.Text.Encoding.ASCII.GetBytes(Password);
                            if (username.Length > byte.MaxValue)
                            {
                                Close("Username is to long.", null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
                                return;
                            }
                            Array.Resize(ref client_request, username.Length + password.Length + 3);
                            client_request[0] = 1; // Auth version
                            client_request[1] = (byte)username.Length;
                            client_request[username.Length + 2] = (byte)password.Length;
                            Array.Copy(username, 0, client_request, 2, username.Length);
                            Array.Copy(password, 0, client_request, username.Length + 3, password.Length);
                            this.Write(client_request, false);
                            server_response = this.Read();
                            if (server_response == null || server_response.Length < 2)
                            {
                                Close("Connection timeout.", null, ErrorRenderer.HTTPHeaderCode.C_504_GATEWAY_TIMEOUT);
                                return;
                            }
                            if (server_response[0] != 1)
                            {
                                Close("Unsupported version of proxy's user/pass authentication method.", null, ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                                return;
                            }
                            if (server_response[1] != 0)
                            {
                                Close("Authentication failed.", null, ErrorRenderer.HTTPHeaderCode.C_417_EXPECTATION_FAILED);
                                return;
                            }
                        }

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
                        client_request[1] = 1;
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

                        this.Write(client_request, false);
                        server_response = this.Read();
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
                        if (server_response[1] != 0)
                        {
                            switch (server_response[1])
                            {
                                case 1:
                                    Close("Connection failed, Error Code: " + server_response[1].ToString(), null, ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                                    break;
                                case 2:
                                    Close("SOCKS Error Message: General failure.", null, ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                                    break;
                                case 3:
                                    Close("SOCKS Error Message: Connection not allowed by rule set.", null, ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                                    break;
                                case 4:
                                    Close("SOCKS Error Message: Network unreachable.", null, ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                                    break;
                                case 5:
                                    Close("SOCKS Error Message: Connection refused by destination host.", null, ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                                    break;
                                case 6:
                                    Close("SOCKS Error Message: TTL expired.", null, ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                                    break;
                                case 7:
                                    Close("SOCKS Error Message: Command not supported / protocol error.", null, ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                                    break;
                                case 8:
                                    Close("SOCKS Error Message: Address type not supported.", null, ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                                    break;
                                default:
                                    Close("Connection failed, Error Code: " + server_response[1].ToString(), null, ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                                    break;
                            }
                            return;
                        }
                        this.IsServerValid = true;
                        if (headerData != null && Common.IsSocketConnected(UnderlyingSocket))
                        {
                            this.Write(headerData);
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
                if ((ParentClient.BusyWrite || this.BusyWrite || (Common.IsSocketConnected(UnderlyingSocket) && Common.IsSocketConnected(ParentClient.Client))) && CurrentTimeout > 0)
                {
                    if (ParentClient.IsSmartForwarderEnable && ParentClient.SmartResponseBuffer.Length > 0 && (CurrentTimeout <= this.NoDataTimeout * 500 || CurrentTimeout <= ((this.NoDataTimeout - ParentClient.Controller.SmartPear.Detector_HTTP_ResponseBufferTimeout) * 1000)))
                    {
                        CurrentTimeout = this.NoDataTimeout * 1000;
                        ParentClient.IsSmartForwarderEnable = false;
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
                            {
                                ParentClient.Write(buffer);
                            }
                        }
                    }
                    if (!this.BusyWrite && ParentClient.Client.Available > 0)
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
                                this.Write(buffer);
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

        private void Write(byte[] bytes, bool async = true)
        {
            try
            {
                if (bytes != null)
                {
                    Array.Resize(ref writeBuffer, writeBuffer.Length + bytes.Length);
                    Array.Copy(bytes, 0, writeBuffer, writeBuffer.Length - bytes.Length, bytes.Length);
                }
                if (writeBuffer.Length > 0 && UnderlyingSocket.Poll(0, SelectMode.SelectWrite))
                {
                    int bytesWritten = UnderlyingSocket.Send(writeBuffer, SocketFlags.None);
                    Array.Copy(writeBuffer, bytesWritten, writeBuffer, 0, writeBuffer.Length - bytesWritten);
                    Array.Resize(ref writeBuffer, writeBuffer.Length - bytesWritten);
                }
                if (!async)
                {
                    int i = 60; // Time out by second
                    i = i * 100;
                    while (i > 0 && writeBuffer.Length > 0)
                    {
                        int wbl = writeBuffer.Length;
                        Write(null);
                        if (writeBuffer.Length == wbl)
                        {
                            System.Threading.Thread.Sleep(10);
                            i--;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Close(e.Message, e.StackTrace);
            }
        }

        private byte[] Read()
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
            return "SOCKS5 Module " + this.ServerAddress + ":" + this.ServerPort + ((this.Username != string.Empty) ? " Using USN/PSW" : " Open");
        }

        public void Dispose()
        {
            if (this.UnderlyingSocket != null)
                this.UnderlyingSocket.Dispose();
        }
    }
}
