using System.Net.Sockets;
namespace PeaRoxy.ClientLibrary.Server_Types
{
    public abstract class ServerType
    {
        public delegate bool DataCallbackDelegate(ref byte[] data, ServerType activeServer, Proxy_Client client);
        public delegate bool ConnectionCallbackDelegate(bool success, ServerType activeServer, Proxy_Client client);
        public abstract void Establish(string address, ushort port, Proxy_Client client, byte[] headerData = null, DataCallbackDelegate sndCallback = null, DataCallbackDelegate rcvCallback = null, ConnectionCallbackDelegate connectionStatusCallback = null);
        public abstract ServerType Clone();
        public abstract void DoRoute();
        public abstract int NoDataTimeout { get; set; }
        public abstract string GetAddress();
        public abstract ushort GetPort();
        public abstract Proxy_Client ParentClient { get; protected set; }
        public abstract bool IsDataSent { get; protected set; }
        public abstract bool IsDisconnected { get; protected set; }
        public abstract bool IsServerValid { get; protected set; }
        public abstract Socket UnderlyingSocket { get; protected set; }
        public DataCallbackDelegate SndCallback;
        public DataCallbackDelegate RcvCallback;
        public ConnectionCallbackDelegate ConnectionStatusCallback;
    }
}
