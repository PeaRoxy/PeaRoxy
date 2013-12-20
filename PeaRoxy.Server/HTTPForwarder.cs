// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HttpForwarder.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The http forwarder.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Server
{
    #region

    using System;
    using System.Net.Sockets;
    using System.Threading;

    #endregion

    /// <summary>
    /// The http forwarder.
    /// </summary>
    internal class HttpForwarder
    {
        #region Fields

        /// <summary>
        /// The underlying client receive packet size.
        /// </summary>
        private int underlyingClientReceivePacketSize;

        /// <summary>
        /// The underlying client send packet size.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private int underlyingClientSendPacketSize;

        /// <summary>
        /// The write buffer.
        /// </summary>
        private byte[] writeBuffer = new byte[0];

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpForwarder"/> class.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="receivePacketSize">
        /// The receive packet size.
        /// </param>
        /// <param name="sendPacketSize">
        /// The send packet size.
        /// </param>
        public HttpForwarder(Socket client, int receivePacketSize = 8192, int sendPacketSize = 1024)
        {
            this.UnderlyingSocket = client;
            this.UnderlyingSocket.Blocking = false;
            this.underlyingClientReceivePacketSize = receivePacketSize;
            this.underlyingClientSendPacketSize = sendPacketSize;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether busy write.
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
        /// Gets the underlying socket.
        /// </summary>
        public Socket UnderlyingSocket { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The close.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="async">
        /// The async.
        /// </param>
        public void Close(string message = null, bool async = true)
        {
            try
            {
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
                        // Close request connection it-self
                        this.UnderlyingSocket.Close();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// The read.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>byte[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        public byte[] Read()
        {
            try
            {
                int i = 60; // Time out by second

                i = i * 100;
                while (i > 0)
                {
                    if (this.UnderlyingSocket.Available > 0)
                    {
                        byte[] buffer = new byte[this.underlyingClientReceivePacketSize];
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
                this.Close("F1. " + e.Message + "\r\n" + e.StackTrace);
            }

            return null;
        }

        /// <summary>
        /// The write.
        /// </summary>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        public void Write(byte[] bytes)
        {
            try
            {
                if (bytes != null)
                {
                    Array.Resize(ref this.writeBuffer, this.writeBuffer.Length + bytes.Length);
                    Array.Copy(bytes, 0, this.writeBuffer, this.writeBuffer.Length - bytes.Length, bytes.Length);
                }

                if (this.writeBuffer.Length <= 0 || !this.UnderlyingSocket.Poll(0, SelectMode.SelectWrite))
                    return;

                int bytesWritten = this.UnderlyingSocket.Send(this.writeBuffer, SocketFlags.None);
                Array.Copy(
                    this.writeBuffer, 
                    bytesWritten, 
                    this.writeBuffer, 
                    0, 
                    this.writeBuffer.Length - bytesWritten);
                Array.Resize(ref this.writeBuffer, this.writeBuffer.Length - bytesWritten);
            }
            catch (Exception e)
            {
                this.Close("F2. " + e.Message + "\r\n" + e.StackTrace);
            }
        }

        #endregion
    }
}