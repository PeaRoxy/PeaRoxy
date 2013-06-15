using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using PeaRoxy.CommonLibrary;
using System.Net.Security;
using PeaRoxy.ClientLibrary.Proxy_Types;
namespace PeaRoxy.ClientLibrary
{
    public class Proxy_Client
    {
        public enum eType
        {
            Unknown,
            TCP,
            UDP,
            DNS
        }

        public enum eStatus
        {
            Unknown,
            Connected,
            Waiting,
            Routing,
            Closing
        }
        private string requestAddress = string.Empty;
        private int CurrentTimeout = 0;
        private long last_rcvSpeedReaded = 0;
        private long last_sntSpeedReaded = 0;
        private long last_rcvBytes = 0;
        private long last_sntBytes = 0;
        private byte[] writeBuffer = new byte[0];
        private byte[] reqBuffer = new byte[0];
        private Platform.ConnectionInfo extendedInfo;
        internal byte[] SmartResponseBuffer = new byte[0];
        internal byte[] SmartRequestBuffer = new byte[0];
        public int SendPacketSize { get; set; }
        public int ReceivePacketSize { get; set; }
        public int NoDataTimeOut { get; set; }
        public string LastError { get; private set; }
        public bool IsDisconnected { get; private set; }
        public bool IsSendingStarted { get; internal set; }
        public bool IsReceivingStarted { get; internal set; }
        public bool IsSmartForwarderEnable { get; internal set; }
        public Socket Client { get; set; }
        public Proxy_Controller Controller { get; private set; }
        public long BytesSent { get; private set; }
        public long BytesReceived { get; private set; }
        public long AvgSendingSpeed
        {
            get
            {
                long BytesS = this.BytesSent - this.last_sntBytes;
                this.last_sntBytes = this.BytesSent;
                double timeE = (double)(Environment.TickCount - last_sntSpeedReaded) / (double)1000;
                if (timeE > 0)
                {
                    last_sntSpeedReaded = Environment.TickCount;
                    return (long)(BytesS / timeE);
                }
                return 0;
            }
        }
        public long AvgReceivingSpeed
        {
            get
            {
                long BytesR = this.BytesReceived - this.last_rcvBytes;
                this.last_rcvBytes = this.BytesReceived;
                double timeE = (double)(Environment.TickCount - last_rcvSpeedReaded) / (double)1000;
                if (timeE > 0)
                {
                    last_rcvSpeedReaded = Environment.TickCount;
                    return (long)(BytesR / timeE);
                }
                return 0;
            }
        }
        public eType Type { get; internal set; }
        public eStatus Status { get; internal set; }
        public string RequestAddress
        {
            get
            {
                return requestAddress;
            }
            set
            {
                requestAddress = value;
                this.IsSmartForwarderEnable = !this.IsNeedForwarding();
            }
        }
        public bool BusyWrite
        {
            get
            {
                if (writeBuffer.Length > 0)
                    this.Write(null);
                return (writeBuffer.Length > 0);
            }
        }
        public Proxy_Client(Socket client, Proxy_Controller parent, bool isDirectConnection = false)
        {
            last_sntSpeedReaded = last_rcvSpeedReaded = Environment.TickCount;
            last_sntBytes = last_rcvBytes = BytesReceived = BytesSent = 0;
            if (client != null && client.ProtocolType == ProtocolType.Tcp)
                this.Type = eType.TCP;
            else if (client != null && client.ProtocolType == ProtocolType.Udp)
                this.Type = eType.UDP;
            this.Status = eStatus.Connected;
            this.LastError = string.Empty;
            this.IsSmartForwarderEnable = false;
            this.NoDataTimeOut = 60;
            this.IsSmartForwarderEnable = parent.SmartPear.Forwarder_HTTP_Enable || parent.SmartPear.Forwarder_HTTPS_Enable || parent.SmartPear.Forwarder_SOCKS_Enable;
            this.SendPacketSize = 1024;
            this.ReceivePacketSize = 8192;
            this.IsDisconnected = false;
            this.Controller = parent;
            this.Client = client;
            if (this.Client != null)
                this.Client.Blocking = false;
            this.IsSendingStarted = isDirectConnection;
        }
        public Platform.ConnectionInfo GetExtendedInfo()
        {
            try
            {
                if (extendedInfo != null)
                    return extendedInfo;
                if (Client != null)
                    if (Platform.ClassRegistry.GetClass<Platform.ConnectionInfo>().IsSupported())
                        if (this.Type == eType.TCP)
                            extendedInfo = Platform.ClassRegistry.GetClass<Platform.ConnectionInfo>().GetTcpConnectionByLocalAddress(((IPEndPoint)Client.RemoteEndPoint).Address,
                                                                                                (ushort)((IPEndPoint)Client.RemoteEndPoint).Port);
                        else if (this.Type == eType.UDP)
                            extendedInfo = Platform.ClassRegistry.GetClass<Platform.ConnectionInfo>().GetUdpConnectionByLocalAddress(((IPEndPoint)Client.RemoteEndPoint).Address,
                                                                                                (ushort)((IPEndPoint)Client.RemoteEndPoint).Port);
            }
            catch (Exception) { }
            return null;
        }
        public void Accepting()
        {
            try
            {
                if (Common.IsSocketConnected(Client))
                {
                    if (!IsSendingStarted)
                    {
                        if (Client.Available > 0)
                        {
                            this.Status = eStatus.Waiting;
                            CurrentTimeout = 0;
                            byte[] client_data = Read();
                            if (client_data == null || client_data.Length == 0)
                            {
                                Close();
                                return;
                            }
                            Array.Resize(ref reqBuffer, reqBuffer.Length + client_data.Length);
                            Array.Copy(client_data, 0, reqBuffer, reqBuffer.Length - client_data.Length, client_data.Length);
                            IsSendingStarted = true;
                            if (this.Controller.Status == Proxy_Controller.ControllerStatus.Stopped)
                                Close();
                            else if (Proxy_HTTP.IsHTTP(reqBuffer))
                                Proxy_HTTP.Handle(reqBuffer, this);
                            else if (Proxy_HTTPS.IsHTTPS(reqBuffer))
                                Proxy_HTTPS.Handle(reqBuffer, this);
                            else if (Proxy_SOCKS.IsSOCKS(reqBuffer) && (this.Controller.Status == Proxy_Controller.ControllerStatus.OnlyProxy || this.Controller.Status == Proxy_Controller.ControllerStatus.Both))
                                if ((this.Controller.Status == Proxy_Controller.ControllerStatus.OnlyProxy || this.Controller.Status == Proxy_Controller.ControllerStatus.Both))
                                    Proxy_SOCKS.Handle(reqBuffer, this);
                                else
                                    Close();
                            else
                                IsSendingStarted = false;
                        }
                        else if (CurrentTimeout == Math.Min(this.NoDataTimeOut * 1000, 30000) || reqBuffer.Length >= 100)
                            if (reqBuffer.Length != 0)
                                Close("Unknown proxy connection", "Header: " + System.Text.Encoding.ASCII.GetString(reqBuffer), ErrorRenderer.HTTPHeaderCode.C_501_NOT_IMPLAMENTED);
                        CurrentTimeout++;
                    }
                }
                else
                {
                    Close();
                }
            }
            catch (Exception e)
            {
                Close(e.Message, e.StackTrace);
            }

        }
        public byte[] Read()
        {
            try
            {
                int i = 60; // Time out by second

                i = i * 100;
                while (i > 0)
                {
                    if (Client.Available > 0)
                    {
                        byte[] buffer = new byte[Client.ReceiveBufferSize];
                        int bytes = Client.Receive(buffer);
                        Array.Resize(ref buffer, bytes);
                        this.Status = eStatus.Routing;
                        this.DataSent(buffer.Length);
                        return buffer;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(10);
                        i--;
                    }
                }
            }
            catch (Exception)// e)
            {
                //Close(e.Message, e.StackTrace);
                Close();
            }
            return null;
        }

        public void Write(byte[] bytes, System.IO.Stream toStream = null)
        {
            try
            {
                IsReceivingStarted = true;
                if (toStream == null)
                {
                    if (bytes != null)
                    {
                        this.Status = eStatus.Routing;
                        Array.Resize(ref writeBuffer, writeBuffer.Length + bytes.Length);
                        Array.Copy(bytes, 0, writeBuffer, writeBuffer.Length - bytes.Length, bytes.Length);
                    }
                    if (writeBuffer.Length > 0 && Client.Poll(0, SelectMode.SelectWrite))
                    {
                        int bytesWritten = Client.Send(writeBuffer, SocketFlags.None);
                        this.DataReceived(writeBuffer.Length);
                        Array.Copy(writeBuffer, bytesWritten, writeBuffer, 0, writeBuffer.Length - bytesWritten);
                        Array.Resize(ref writeBuffer, writeBuffer.Length - bytesWritten);
                    }
                }
                else
                {
                    toStream.Write(bytes, 0, bytes.Length);
                    this.DataReceived(bytes.Length);
                }
            }
            catch (Exception e)
            {
                Close(e.Message, e.StackTrace);
            }
        }
        public void Close(string title = null,string message = null, ErrorRenderer.HTTPHeaderCode code = ErrorRenderer.HTTPHeaderCode.C_500_SERVER_ERROR, bool async = false, SslStream sslstream = null)
        {
            this.Status = eStatus.Closing;
            try
            {
                if (title != null)
                {
                    if (message == null)
                    {
                        message = "No more information.";
                    }
                    this.LastError = title + "\r\n" + message;
                }
                if (Client != null) // Testing or not
                {
                    if (this.LastError != string.Empty && title != null)
                        Proxy_Controller.LogIt(title);
                    if (title == null || !this.Controller.ErrorRenderer.RenderError(this, title, message, code, sslstream))
                    {
                        if (async)
                        {
                            byte[] db = new byte[0];
                            if (Client != null)
                                Client.BeginSend(db, 0, db.Length, SocketFlags.None, (AsyncCallback)delegate(IAsyncResult ar)
                                {
                                    try
                                    {
                                        Client.Close(); // Close request connection it-self
                                        Client.EndSend(ar);
                                    }
                                    catch (Exception) { }
                                }, null);
                        }
                        else
                        {
                            if (Client != null)
                                Client.Close();
                        }
                    }
                }
            }
            catch { }
            this.IsDisconnected = true;
            Controller.iDissconnected(this);
        }

        private void DataSent(int p)
        {
            this.BytesSent += p;
            if (this.Controller != null)
                Controller.BytesSent += p;
        }

        private void DataReceived(int p)
        {
            this.BytesReceived += p;
            if (this.Controller != null)
                Controller.BytesReceived += p;
        }
    }
}
