using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace PeaRoxy.Server
{
    class HTTPForwarder
    {
        private byte[] writeBuffer = new byte[0];
        private int UnderlyingClientReceivePacketSize;
        private int UnderlyingClientSendPacketSize;
        public Socket UnderlyingSocket { get; private set; }
        public bool BusyWrite
        {
            get
            {
                if (writeBuffer.Length > 0)
                    this.Write(null);
                return (writeBuffer.Length > 0);
            }
        }
        public HTTPForwarder(
                Socket client,
                int ReceivePacketSize = 8192,
                int SendPacketSize = 1024
            )
        {
            this.UnderlyingSocket = client;
            this.UnderlyingSocket.Blocking = false;
            this.UnderlyingClientReceivePacketSize = ReceivePacketSize;
            this.UnderlyingClientSendPacketSize = SendPacketSize;
        }

        public void Write(byte[] bytes)
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
                Close("F2. " + e.Message + "\r\n" + e.StackTrace);
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
                    if (UnderlyingSocket.Available > 0)
                    {
                        byte[] buffer = new byte[this.UnderlyingClientReceivePacketSize];
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
                Close("F1. " + e.Message + "\r\n" + e.StackTrace);
            }
            return null;
        }

        public void Close(string message = null, bool async = true)
        {
            try
            {
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
                    if (UnderlyingSocket != null) // Close request connection it-self
                        UnderlyingSocket.Close();
                }
            }
            catch (Exception) { }
        }
    }
}
