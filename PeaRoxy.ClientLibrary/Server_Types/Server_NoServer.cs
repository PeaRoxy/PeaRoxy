using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using PeaRoxy.CommonLibrary;
namespace PeaRoxy.ClientLibrary.Server_Types
{
    public class Server_NoServer : ServerType, IDisposable
    {
        private byte[] writeBuffer = new byte[0];
        private int CurrentTimeout = 0;
        private string Address = string.Empty;
        private ushort Port = 0;
        private bool IsServerExist = false;
        public override int NoDataTimeout { get; set; }
        public override Proxy_Client ParentClient { get; protected set; }
        public override bool IsDataSent { get; protected set; }
        public override bool IsDisconnected { get; protected set; }
        public override bool IsServerValid { get; protected set; }
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
        public Server_NoServer()
        {
            this.IsDisconnected = false;
            this.IsServerValid = false;
            this.NoDataTimeout = 60;
            this.IsDataSent = false;
        }
        public override ServerType Clone()
        {
            return new Server_NoServer() { NoDataTimeout = this.NoDataTimeout };
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
                System.Threading.ThreadStart st = delegate()
                {
                    try
                    {
                        if (this.IsServerExist == false)
                        {
                            this.IsServerExist = true;
                            if (this.GetType().Name != client.Controller.ActiveServer.GetType().Name && ConnectionStatusCallback != null && ConnectionStatusCallback.Invoke(false, this, ParentClient) == false)
                            {
                                ParentClient = null;
                                Close();
                            }
                            else
                                Close("Connection timeout. " + address + ":" + port, null, ErrorRenderer.HTTPHeaderCode.C_504_GATEWAY_TIMEOUT);
                        }
                    }
                    catch (Exception) { }
                };
                if (this.GetType().Name != client.Controller.ActiveServer.GetType().Name && ConnectionStatusCallback != null)
                    client.Controller.AddToScheduledTasks(st, Math.Min(this.NoDataTimeout, 30) * 1000);
                else
                    client.Controller.AddToScheduledTasks(st, 60 * 1000);
                UnderlyingSocket.BeginConnect(address, (int)port, (AsyncCallback)delegate(IAsyncResult ar)
                {
                    if (this.IsServerExist == false)
                    {
                        try
                        {
                            this.IsServerExist = true;
                            client.Controller.RemoveFromScheduledTasks(st);
                            UnderlyingSocket.EndConnect(ar);
                            if (this.GetType().Name != client.Controller.ActiveServer.GetType().Name && ConnectionStatusCallback != null && ConnectionStatusCallback.Invoke(true, this, ParentClient) == false)
                            {
                                ParentClient = null;
                                Close();
                                return;
                            }
                            UnderlyingSocket.Blocking = false;
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
                            if (this.GetType().Name != client.Controller.ActiveServer.GetType().Name && ConnectionStatusCallback != null && ConnectionStatusCallback.Invoke(false, this, ParentClient) == false)
                            {
                                ParentClient = null;
                                Close();
                            }
                            else
                                Close(ex.Message, ex.StackTrace, ErrorRenderer.HTTPHeaderCode.C_502_BAD_GATEWAY);
                        }
                    }
                }, null);
            }
            catch (Exception e)
            {
                if (e.TargetSite.Name == "Receive")
                    Close();
                else
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
                if (writeBuffer.Length > 0 && UnderlyingSocket.Poll(0,SelectMode.SelectWrite))
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
            return "NoServer Module";
        }

        void IDisposable.Dispose()
        {
            this.UnderlyingSocket.Dispose();
        }
    }
}
