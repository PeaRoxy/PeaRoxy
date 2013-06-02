using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace PeaRoxy.Platform
{
    public abstract class ConnectionInfo : ClassRegistry.PlatformDependentClassBaseType
    {
        public enum Protocol
        {
            TCP,
            UDP
        }
        public enum Status
        {
            TCP_Closed = 1,
            TCP_Listen = 2,
            TCP_SYNSent = 3,
            TCP_SYNReceived = 4,
            TCP_Established = 5,
            TCP_FinWait1 = 6,
            TCP_FinWait2 = 7,
            TCP_CloseWait = 8,
            TCP_Closing = 9,
            TCP_LastAcknowledge = 10,
            TCP_TimeWait = 11,
            TCP_DeleteTCB = 12
        }
        public Protocol ProtocolType { get; protected set; }
        public IPEndPoint LocalAddress { get; protected set; }
        public IPEndPoint RemoteAddress { get; protected set; }
        public int ProcessId { get; protected set; }
        public string ProcessString { get; protected set; }
        public Status State { get; protected set; }
        public abstract bool IsSupported();
        public abstract ConnectionInfo GetTcpConnectionByLocalAddress(IPAddress ip, ushort port);
        public abstract ConnectionInfo GetUdpConnectionByLocalAddress(IPAddress ip, ushort port);
        public abstract List<ConnectionInfo> GetTcpConnections();
        public abstract List<ConnectionInfo> GetUdpConnections();
    }
}
