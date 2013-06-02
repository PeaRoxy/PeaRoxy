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
    public class Server_HTTPS : ServerType, IDisposable
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
        public Server_HTTPS(string address, ushort port, string username = "", string password = "")
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
            return new Server_HTTPS(ServerAddress, ServerPort, Username, Password) { NoDataTimeout = this.NoDataTimeout };
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
                UnderlyingSocket.BeginConnect(ServerAddress, (int)ServerPort, (AsyncCallback)delegate(IAsyncResult ar)
                {
                    try
                    {
                        UnderlyingSocket.EndConnect(ar);
                        UnderlyingSocket.Blocking = false;
                        this.ParentClient.Controller.FailAttempts = 0;

                        string HTTPSHeader = "CONNECT " + address + ":" + port.ToString() + " HTTP/1.1" + "\r\n";
                        if (this.Username != string.Empty)
                            HTTPSHeader += "Proxy-Authorization: Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(this.Username + ":" + this.Password)) + "\r\n";
                        HTTPSHeader += "\r\n";
                        bool firstTry = true;

                        sendHeader:
                        this.Write(System.Text.Encoding.ASCII.GetBytes(HTTPSHeader));
                        CurrentTimeout = this.NoDataTimeout * 1000;
                        while (UnderlyingSocket.Available <= 0)
                        {
                            if ((client.Client != null && !Common.IsSocketConnected(client.Client)) || !Common.IsSocketConnected(UnderlyingSocket))
                            {
                                this.Close();
                                return;
                            }
                            if (!this.BusyWrite)
                            {
                                CurrentTimeout--;
                                if (CurrentTimeout == 0)
                                {
                                    this.Close("No response, Timeout.", null, ErrorRenderer.HTTPHeaderCode.C_504_GATEWAY_TIMEOUT);
                                    return;
                                }
                            }
                            System.Threading.Thread.Sleep(1);
                        }
                        byte[] responde = this.Read();
                        string response = System.Text.Encoding.ASCII.GetString(responde).ToLower();
                        if (firstTry == true && this.Username != string.Empty && (response.StartsWith("HTTP/1.1 407".ToLower()) || response.StartsWith("HTTP/1.0 407".ToLower())))
                        {
                            firstTry = false;
                            goto sendHeader;
                        }
                        else if (response.StartsWith("HTTP/1.1 200".ToLower()) || response.StartsWith("HTTP/1.0 200".ToLower()))
                        {
                            this.IsServerValid = true;
                            if (headerData != null && Common.IsSocketConnected(UnderlyingSocket))
                            {
                                this.Write(headerData);
                            }
                            CurrentTimeout = this.NoDataTimeout * 1000;
                            client.Controller.iMoveToRouting(this);
                        }
                        else
                        {
                            this.Close("Connection failed", "response: " + response.Substring(0, Math.Min(response.Length, 50)), ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                            return;
                        }
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

        private void Write(byte[] bytes)
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
            return "HTTPS Module " + this.ServerAddress + ":" + this.ServerPort + ((this.Username != string.Empty) ? " Using USN/PSW" : " Open");
        }

        public void Dispose()
        {
            this.UnderlyingSocket.Dispose();
        }
    }
}
