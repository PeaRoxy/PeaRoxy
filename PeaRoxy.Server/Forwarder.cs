// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Forwarder.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Server
{
    using System;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    ///     The Forwarder object is responsible for connecting the client and the forwarding server to each other
    /// </summary>
    internal class Forwarder
    {
        private readonly int receiveBufferSize;

        private byte[] writeBuffer = new byte[0];

        /// <summary>
        ///     Initializes a new instance of the <see cref="Forwarder" /> class.
        /// </summary>
        /// <param name="client">
        ///     The client's socket.
        /// </param>
        /// <param name="receiveBufferSize">
        ///     The max receive packet size.
        /// </param>
        public Forwarder(Socket client, int receiveBufferSize = 8192)
        {
            this.UnderlyingSocket = client;
            this.UnderlyingSocket.Blocking = false;
            this.receiveBufferSize = receiveBufferSize;
        }

        /// <summary>
        ///     Gets a value indicating whether we are busy writing.
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
        ///     Gets the underlying socket.
        /// </summary>
        public Socket UnderlyingSocket { get; private set; }

        /// <summary>
        ///     The close method is used to close the connection to the other end
        /// </summary>
        /// <param name="async">
        ///     Indicating if the closing process should treat the client as an asynchronous client
        /// </param>
        public void Close(bool async = true)
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
        ///     The read method to read the data from the other end
        /// </summary>
        /// <returns>
        ///     Received data in form of
        ///     <see>
        ///         <cref>byte[]</cref>
        ///     </see>
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
                        byte[] buffer = new byte[this.receiveBufferSize];
                        int bytes = this.UnderlyingSocket.Receive(buffer);
                        Array.Resize(ref buffer, bytes);
                        return buffer;
                    }

                    Thread.Sleep(10);
                    i--;
                }
            }
            catch
            {
                this.Close();
            }

            return null;
        }

        /// <summary>
        ///     The write method to write the data to the other end.
        /// </summary>
        /// <param name="bytes">
        ///     The data to write.
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
                {
                    return;
                }

                int bytesWritten = this.UnderlyingSocket.Send(this.writeBuffer, SocketFlags.None);
                Array.Copy(this.writeBuffer, bytesWritten, this.writeBuffer, 0, this.writeBuffer.Length - bytesWritten);
                Array.Resize(ref this.writeBuffer, this.writeBuffer.Length - bytesWritten);
            }
            catch
            {
                this.Close();
            }
        }
    }
}