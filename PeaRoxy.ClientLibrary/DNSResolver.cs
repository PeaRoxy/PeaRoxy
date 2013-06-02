using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace PeaRoxy.ClientLibrary
{
    public class DNSResolver : IDisposable
    {
        private Socket DNSTCPListenerSocket;
        private Socket DNSUDPListenerSocket;
        private Proxy_Controller Parent;
        private IPEndPoint ipLocalDNS;
        public bool DNSResolver_Supported { get; set; }
        public bool DNSResolver_UDPSupported { get; set; }
        public IPAddress DNSResolver_ServerIP { get; set; }
        public DNSResolver(Proxy_Controller parent)
        {
            this.Parent = parent;
            this.DNSResolver_ServerIP = IPAddress.Parse("8.8.4.4");
            this.DNSResolver_Supported = true;
            this.DNSResolver_UDPSupported = true;
        }

        public void Start()
        {
            if (DNSResolver_Supported)
            {
                this.ipLocalDNS = new IPEndPoint(Parent.IP, 53);
                this.DNSTCPListenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.DNSTCPListenerSocket.Bind(ipLocalDNS);
                this.DNSTCPListenerSocket.Listen(256);
            }
        }

        public void Stop()
        {
            try
            {
                if (DNSResolver_Supported)
                {
                    this.DNSTCPListenerSocket.Close();
                    if (DNSResolver_UDPSupported && DNSUDPListenerSocket != null)
                    {
                        this.DNSUDPListenerSocket.Close();
                        this.DNSUDPListenerSocket = null;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public void Accepting()
        {
            try
            {
                if (DNSResolver_Supported && this.DNSTCPListenerSocket.Poll(0, SelectMode.SelectRead))
                {
                    Proxy_Client c = new Proxy_Client(this.DNSTCPListenerSocket.Accept(), Parent, true) { ReceivePacketSize = Parent.ReceivePacketSize, SendPacketSize = Parent.SendPacketSize, NoDataTimeOut = 10 };
                    c.Type = Proxy_Client.eType.DNS;
                    lock (Parent.ConnectedClients)
                        Parent.ConnectedClients.Add(c);
                    Parent.ActiveServer.Clone().Establish(this.DNSResolver_ServerIP.ToString(), 53, c);
                }
                if (DNSResolver_Supported && DNSResolver_UDPSupported)
                {
                    if (DNSUDPListenerSocket == null)
                    {
                        this.DNSUDPListenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        this.DNSUDPListenerSocket.EnableBroadcast = true;
                        this.DNSUDPListenerSocket.Bind(ipLocalDNS);
                    }
                    try
                    {
                        if (this.DNSUDPListenerSocket.Poll(0, SelectMode.SelectRead))
                        {
                            byte[] globalBuffer = new byte[500];
                            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                            int i = DNSUDPListenerSocket.ReceiveFrom(globalBuffer, ref remoteEP);
                            byte[] buffer = new byte[i + 2];
                            Array.Copy(globalBuffer, 0, buffer, 2, i);
                            new System.Threading.Thread(delegate()
                            {
                                try
                                {
                                    buffer[0] = (byte)Math.Floor((double)i / 256);
                                    buffer[1] = (byte)(i % 256);
                                    IPAddress ip = Parent.IP; // Connecting to our self on same port, But TCP
                                    if (ip.Equals(IPAddress.Any))
                                        ip = IPAddress.Loopback;
                                    Socket TCPConnector = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                    TCPConnector.Connect(ip, 53);
                                    TCPConnector.Send(buffer);
                                    buffer = new byte[TCPConnector.ReceiveBufferSize];
                                    i = TCPConnector.Receive(buffer);
                                    if (i != 0)
                                    {
                                        int neededBytes = (buffer[0] * 256) + buffer[1] + 2;
                                        Array.Resize(ref buffer, Math.Max(i, neededBytes));
                                        if (i < neededBytes)
                                        {
                                            int timeout = 2000;
                                            int recieved = i;
                                            while (recieved < neededBytes && timeout > 0 && CommonLibrary.Common.IsSocketConnected(TCPConnector))
                                            {
                                                i = TCPConnector.Receive(buffer, recieved, buffer.Length - recieved, SocketFlags.None);
                                                recieved += i;
                                                if (i == 0)
                                                    break;
                                                timeout--;
                                                System.Threading.Thread.Sleep(1);
                                            }
                                        }
                                    }
                                    TCPConnector.Close();
                                    DNSUDPListenerSocket.SendTo(buffer, 2, buffer.Length - 2, SocketFlags.None, remoteEP);
                                }
                                catch (Exception e)
                                {
                                    Proxy_Controller.LogIt("DNS Resolver UDP Error: " + e.Message);
                                }
                            }) { IsBackground = true }.Start();
                        }
                    }
                    catch (Exception)
                    {
                        DNSUDPListenerSocket.Close();
                        DNSUDPListenerSocket = null;
                    }
                }
            }
            catch (Exception)
            {
                //Stat.LogIt("DNS Resolver Error: " + e.Message);
            }
        }

        public void Dispose()
        {
            this.DNSTCPListenerSocket.Dispose();
            this.DNSUDPListenerSocket.Dispose();
        }
    }
}
