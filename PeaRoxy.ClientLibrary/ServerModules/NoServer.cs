// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NoServer.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary.ServerModules
{
    using System;
    using System.Net.Sockets;
    using System.Threading;

    using global::PeaRoxy.CommonLibrary;

    /// <summary>
    ///     The Direct Connection Module
    /// </summary>
    public sealed class NoServer : ServerType, IDisposable
    {
        private int currentTimeout;

        private bool isServerExist;

        private string requestedAddress = string.Empty;

        private ushort requestedPort;

        private byte[] writeBuffer = new byte[0];

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoServer" /> class.
        /// </summary>
        public NoServer()
        {
            this.IsClosed = false;
            this.IsServerValid = false;
            this.NoDataTimeout = 60;
            this.IsDataSent = false;
        }

        /// <summary>
        ///     Gets a value indicating whether we are busy writing to the server.
        /// </summary>
        public bool BusyWrite
        {
            get
            {
                if (this.writeBuffer.Length > 0)
                {
                    this.Write(null);
                }

                return this.writeBuffer.Length > 0;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether we had sent some data to the server
        /// </summary>
        public override bool IsDataSent { get; protected set; }

        /// <summary>
        ///     Gets or sets a value indicating whether we had a close request on the underlying connection
        /// </summary>
        public override bool IsClosed { get; protected set; }

        /// <summary>
        ///     Gets or sets a value indicating whether server exists and is valid.
        /// </summary>
        public override bool IsServerValid { get; protected set; }

        /// <summary>
        ///     Gets or sets the number of seconds after last data transmission to close the connection
        /// </summary>
        public override int NoDataTimeout { get; set; }

        /// <summary>
        ///     Gets the parent client.
        /// </summary>
        public override ProxyClient ParentClient { get; protected set; }

        /// <summary>
        ///     Gets the underlying socket to the server.
        /// </summary>
        public override Socket UnderlyingSocket { get; protected set; }

        void IDisposable.Dispose()
        {
            this.UnderlyingSocket.Dispose();
        }

        /// <summary>
        ///     The clone method is to make another copy of this class with exactly same settings
        /// </summary>
        /// <returns>
        ///     The <see cref="ServerType" />.
        /// </returns>
        public override ServerType Clone()
        {
            return new NoServer { NoDataTimeout = this.NoDataTimeout };
        }

        /// <summary>
        ///     The method to handle the routing process, should call repeatedly
        /// </summary>
        public override void Route()
        {
            try
            {
                if ((this.ParentClient.BusyWrite || this.BusyWrite
                     || (Common.IsSocketConnected(this.UnderlyingSocket) && this.ParentClient.UnderlyingSocket != null
                         && Common.IsSocketConnected(this.ParentClient.UnderlyingSocket))) && this.currentTimeout > 0)
                {
                    if (this.ParentClient.IsSmartForwarderEnable && this.ParentClient.SmartResponseBuffer.Length > 0
                        && (this.currentTimeout <= this.NoDataTimeout * 500
                            || this.currentTimeout
                            <= ((this.NoDataTimeout
                                 - this.ParentClient.Controller.SmartPear.DetectorHttpResponseBufferTimeout) * 1000)))
                    {
                        this.currentTimeout = this.NoDataTimeout * 1000;
                        this.ParentClient.IsSmartForwarderEnable = false;
                        this.ParentClient.Write(this.ParentClient.SmartResponseBuffer);
                    }

                    if (!this.ParentClient.BusyWrite && this.UnderlyingSocket.Available > 0)
                    {
                        this.IsDataSent = true;
                        this.currentTimeout = this.NoDataTimeout * 1000;
                        byte[] buffer = this.Read();
                        if (buffer != null && buffer.Length > 0)
                        {
                            if (this.ReceiveDataDelegate != null
                                && this.ReceiveDataDelegate.Invoke(ref buffer, this, this.ParentClient) == false)
                            {
                                this.ParentClient = null;
                                this.Close();
                                return;
                            }

                            if (buffer.Length > 0)
                            {
                                this.ParentClient.Write(buffer);
                            }
                        }
                    }

                    if (!this.BusyWrite && this.ParentClient.UnderlyingSocket.Available > 0)
                    {
                        this.IsDataSent = true;
                        this.currentTimeout = this.NoDataTimeout * 1000;
                        byte[] buffer = this.ParentClient.Read();
                        if (buffer != null && buffer.Length > 0)
                        {
                            if (this.SendDataDelegate != null
                                && this.SendDataDelegate.Invoke(ref buffer, this, this.ParentClient) == false)
                            {
                                this.ParentClient = null;
                                this.Close();
                                return;
                            }

                            if (buffer.Length > 0)
                            {
                                this.Write(buffer);
                            }
                        }
                    }

                    this.currentTimeout--;
                }
                else
                {
                    if (this.IsDataSent == false && this.ConnectionStatusCallback != null
                        && this.ConnectionStatusCallback.Invoke(false, this, this.ParentClient) == false)
                    {
                        this.ParentClient = null;
                    }

                    this.Close(false);
                }
            }
            catch (Exception e)
            {
                if (e.TargetSite.Name == "Receive")
                {
                    this.Close();
                }
                else
                {
                    this.Close(e.Message, e.StackTrace);
                }
            }
        }

        /// <summary>
        ///     The establish method is used to connect to the server and try to request access to the specific address.
        /// </summary>
        /// <param name="address">
        ///     The requested address.
        /// </param>
        /// <param name="port">
        ///     The requested port.
        /// </param>
        /// <param name="client">
        ///     The client to connect with.
        /// </param>
        /// <param name="headerData">
        ///     The data to send after connecting.
        /// </param>
        /// <param name="sendDataDelegate">
        ///     The send callback.
        /// </param>
        /// <param name="receiveDataDelegate">
        ///     The receive callback.
        /// </param>
        /// <param name="connectionStatusCallback">
        ///     The connection status changed callback.
        /// </param>
        public override void Establish(
            string address,
            ushort port,
            ProxyClient client,
            byte[] headerData = null,
            DataCallbackDelegate sendDataDelegate = null,
            DataCallbackDelegate receiveDataDelegate = null,
            ConnectionCallbackDelegate connectionStatusCallback = null)
        {
            try
            {
                this.requestedAddress = address;
                this.requestedPort = port;
                this.ParentClient = client;
                this.SendDataDelegate = sendDataDelegate;
                this.ReceiveDataDelegate = receiveDataDelegate;
                this.ConnectionStatusCallback = connectionStatusCallback;
                this.UnderlyingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ThreadStart st = delegate
                    {
                        try
                        {
                            if (this.isServerExist == false)
                            {
                                this.isServerExist = true;
                                if (this.GetType().Name != client.Controller.ActiveServer.GetType().Name
                                    && this.ConnectionStatusCallback != null
                                    && this.ConnectionStatusCallback.Invoke(false, this, this.ParentClient) == false)
                                {
                                    this.ParentClient = null;
                                    this.Close();
                                }
                                else
                                {
                                    this.Close(
                                        "Connection timeout. " + this.requestedAddress + ":" + this.requestedPort,
                                        null,
                                        ErrorRenderer.HttpHeaderCode.C504GatewayTimeout);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    };
                if (this.GetType().Name != client.Controller.ActiveServer.GetType().Name
                    && this.ConnectionStatusCallback != null)
                {
                    client.Controller.AddTasksToQueue(st, Math.Min(this.NoDataTimeout, 30) * 1000);
                }
                else
                {
                    client.Controller.AddTasksToQueue(st, 60 * 1000);
                }

                this.UnderlyingSocket.BeginConnect(
                    this.requestedAddress,
                    this.requestedPort,
                    delegate(IAsyncResult ar)
                        {
                            if (this.isServerExist == false)
                            {
                                try
                                {
                                    this.isServerExist = true;
                                    client.Controller.RemoveTaskFromQueue(st);
                                    this.UnderlyingSocket.EndConnect(ar);
                                    if (this.GetType().Name != client.Controller.ActiveServer.GetType().Name
                                        && this.ConnectionStatusCallback != null
                                        && this.ConnectionStatusCallback.Invoke(true, this, this.ParentClient) == false)
                                    {
                                        this.ParentClient = null;
                                        this.Close();
                                        return;
                                    }

                                    this.UnderlyingSocket.Blocking = false;
                                    this.IsServerValid = true;
                                    if (headerData != null && Common.IsSocketConnected(this.UnderlyingSocket))
                                    {
                                        this.Write(headerData);
                                    }

                                    this.currentTimeout = this.NoDataTimeout * 1000;
                                    client.Controller.ClientMoveToRouting(this);
                                }
                                catch (Exception ex)
                                {
                                    if (this.GetType().Name != client.Controller.ActiveServer.GetType().Name
                                        && this.ConnectionStatusCallback != null
                                        && this.ConnectionStatusCallback.Invoke(false, this, this.ParentClient) == false)
                                    {
                                        this.ParentClient = null;
                                        this.Close();
                                    }
                                    else
                                    {
                                        this.Close(
                                            ex.Message,
                                            ex.StackTrace,
                                            ErrorRenderer.HttpHeaderCode.C502BadGateway);
                                    }
                                }
                            }
                        },
                    null);
            }
            catch (Exception e)
            {
                if (e.TargetSite.Name == "Receive")
                {
                    this.Close();
                }
                else
                {
                    this.Close(e.Message, e.StackTrace);
                }
            }
        }

        /// <summary>
        ///     Get the latest requested address
        /// </summary>
        /// <returns>
        ///     The requested address as <see cref="string" />.
        /// </returns>
        public override string GetRequestedAddress()
        {
            return this.requestedAddress;
        }

        /// <summary>
        ///     Get the latest requested port number
        /// </summary>
        /// <returns>
        ///     The requested port number as <see cref="ushort" />.
        /// </returns>
        public override ushort GetRequestedPort()
        {
            return this.requestedPort;
        }

        public override string ToString()
        {
            return "NoServer Module";
        }

        private void Close(bool async)
        {
            this.Close(null, null, ErrorRenderer.HttpHeaderCode.C500ServerError, async);
        }

        private void Close(
            string title = null,
            string message = null,
            ErrorRenderer.HttpHeaderCode code = ErrorRenderer.HttpHeaderCode.C500ServerError,
            bool async = false)
        {
            try
            {
                if (this.ParentClient != null)
                {
                    this.ParentClient.Close(title, message, code, async);
                }

                this.IsClosed = true;

                if (async)
                {
                    byte[] db = new byte[0];
                    if (this.UnderlyingSocket != null)
                    {
                        this.UnderlyingSocket.BeginSend(
                            db,
                            0,
                            db.Length,
                            SocketFlags.None,
                            delegate(IAsyncResult ar)
                                {
                                    try
                                    {
                                        this.UnderlyingSocket.Close(); // Close request connection it-self
                                        this.UnderlyingSocket.EndSend(ar);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                },
                            null);
                    }
                }
                else
                {
                    if (this.UnderlyingSocket != null)
                    {
                        this.UnderlyingSocket.Close();
                    }
                }
            }
            catch
            {
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
                    if (this.UnderlyingSocket.Available > 0)
                    {
                        byte[] buffer = new byte[this.ParentClient.ReceivePacketSize];
                        int bytes = this.UnderlyingSocket.Receive(buffer);
                        Array.Resize(ref buffer, bytes);
                        return buffer;
                    }

                    Thread.Sleep(10);
                    i--;
                }
            }
            catch (Exception e)
            {
                this.Close(e.Message, e.StackTrace);
            }

            return null;
        }

        private void Write(byte[] bytes)
        {
            try
            {
                if (bytes != null)
                {
                    Array.Resize(ref this.writeBuffer, this.writeBuffer.Length + bytes.Length);
                    Array.Copy(bytes, 0, this.writeBuffer, this.writeBuffer.Length - bytes.Length, bytes.Length);
                }

                if (this.writeBuffer.Length > 0 && this.UnderlyingSocket.Poll(0, SelectMode.SelectWrite))
                {
                    int bytesWritten = this.UnderlyingSocket.Send(this.writeBuffer, SocketFlags.None);
                    Array.Copy(
                        this.writeBuffer,
                        bytesWritten,
                        this.writeBuffer,
                        0,
                        this.writeBuffer.Length - bytesWritten);
                    Array.Resize(ref this.writeBuffer, this.writeBuffer.Length - bytesWritten);
                }
            }
            catch (Exception e)
            {
                this.Close(e.Message, e.StackTrace);
            }
        }
    }
}