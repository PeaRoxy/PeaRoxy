using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using EasyHook;

namespace PeaRoxy.Windows.Network.Hook
{
    public class InjectedCode : EasyHook.IEntryPoint
    {
        RemoteParent Parent;
        LocalHook   WS2_Connect_Handler,
                    WS2_GetHostByName_Handler,
                    WA2_GetAddressInfo_Handler,
                    WA2_GetAddressInfoUni_Handler;
        System.Collections.Generic.Dictionary<string, string> DNSReverseArray = new Dictionary<string, string>();
        public InjectedCode(RemoteHooking.IContext InContext, String InChannelName)
        {
            try
            {
                Parent = RemoteHooking.IpcConnectClient<RemoteParent>(InChannelName);
                Parent.Ping();
            }
            catch (Exception) { }
        }

        public LocalHook AddHook(string LibName, string EntryPoint, Delegate InNewProc)
        {
            try
            {
                LocalHook lh = LocalHook.Create(LocalHook.GetProcAddress(LibName, EntryPoint), InNewProc, this);
                lh.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                return lh;
            }
            catch (Exception e) { Parent.ReportException(e); }
            return null;
        }

        public void Run(RemoteHooking.IContext InContext, String InChannelName)
        {
            WS2_Connect_Handler = AddHook("ws2_32.dll", "connect", new Unmanaged.Connect_Delegate(WS2_Connect));

            WS2_GetHostByName_Handler = AddHook("ws2_32.dll", "gethostbyname", new Unmanaged.GetHostByName_Delegate(WS2_GetHostByName));
            WA2_GetAddressInfoUni_Handler = AddHook("ws2_32.dll", "GetAddrInfoW", 
                (Unmanaged.GetAddressInfo_Delegate)delegate(IntPtr a, IntPtr b, IntPtr c, out IntPtr d)
                {
                    return WS2_GetAddressInfo(a, b, c, out d, true);
                });
            WA2_GetAddressInfo_Handler = AddHook("ws2_32.dll", "getaddrinfo",
                (Unmanaged.GetAddressInfo_Delegate)delegate(IntPtr a, IntPtr b, IntPtr c, out IntPtr d)
                {
                    return WS2_GetAddressInfo(a, b, c, out d, false);
                });
            try
            {
                Parent.IsInstalled(InContext.HostPID);
                while (true)
                {
                    Thread.Sleep(100);
                    Parent.Ping();
                }
            }
            catch
            {
                // NET Remoting will raise an exception if host is unreachable
            }
        }

        private System.Net.Sockets.SocketError WS2_GetAddressInfo(IntPtr NodeName, IntPtr ServiceName, IntPtr Hints, out IntPtr Results, bool IsUnicode)
        {
            System.Net.Sockets.SocketError returnValue = System.Net.Sockets.SocketError.SocketError;
            if (IsUnicode)
                returnValue = Unmanaged.WA2_GetAddressInfoUni(NodeName, ServiceName, Hints, out Results);
            else
                returnValue = Unmanaged.WA2_GetAddressInfo(NodeName, ServiceName, Hints, out Results);
            try
            {
                if (returnValue == System.Net.Sockets.SocketError.Success)
                {
                    IntPtr _handle = new IntPtr(Results.ToInt64());
                    do
                    {
                        Unmanaged.AddressInfo aInfo = (Unmanaged.AddressInfo)Marshal.PtrToStructure(_handle, typeof(Unmanaged.AddressInfo));
                        if (aInfo.Family == System.Net.Sockets.AddressFamily.InterNetwork && aInfo.Addrlen >= 8)
                        {
                            string IPAddress =  Marshal.ReadByte(aInfo.Address + 4) + "." +
                                                Marshal.ReadByte(aInfo.Address + 5) + "." +
                                                Marshal.ReadByte(aInfo.Address + 6) + "." +
                                                Marshal.ReadByte(aInfo.Address + 7);
                            string Hostname = IsUnicode ? Marshal.PtrToStringUni(NodeName) : Marshal.PtrToStringAnsi(NodeName);
                            if (DNSReverseArray.ContainsKey(IPAddress))
                                DNSReverseArray[IPAddress] = Hostname;
                            else
                                DNSReverseArray.Add(IPAddress, Hostname);
                            if (Parent != null)
                                Parent.LogToScreen("WS2_GetAddressInfo: " + Hostname + " -> " + IPAddress);
                        }
                        _handle = aInfo.Next;
                    } while (_handle != IntPtr.Zero);
                }
            }
            catch (Exception e) { Parent.ReportException(e); }
            return returnValue;
        }

        private IntPtr WS2_GetHostByName(string Host)
        {
            IntPtr returnValue = Unmanaged.WA2_GetHostByName(Host);
            try
            {
                if (returnValue != IntPtr.Zero)
                {
                    System.Net.IPHostEntry Results = Unmanaged.NativeIPHostEntry.FromNative(returnValue);
                    foreach (System.Net.IPAddress IP in Results.AddressList)
                    {
                        if (DNSReverseArray.ContainsKey(IP.ToString()))
                            DNSReverseArray[IP.ToString()] = Results.HostName;
                        else
                            DNSReverseArray.Add(IP.ToString(), Results.HostName);
                        if (Parent != null)
                            Parent.LogToScreen("WS2_GetHostByName: " + Results.HostName + " -> " + IP.ToString());
                    }
                }
            }
            catch (Exception e) { Parent.ReportException(e); }
            return returnValue;
        }

        private Unmanaged.ConnectStatus WS2_Connect(IntPtr Socket, ref Unmanaged.SocketAddress_In Address, int AddressSize)
        {
            if (Parent != null)
                Parent.LogToScreen("WS2_Connect: " + Address.Family.ToString());
            string HostNameAndPort = "";
            try
            {
                if (Address.Family == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    string IPAddress =  Address.IPAddress.Byte1 + "." +
                                        Address.IPAddress.Byte2 + "." +
                                        Address.IPAddress.Byte3 + "." +
                                        Address.IPAddress.Byte4;
                    HostNameAndPort = (DNSReverseArray.ContainsKey(IPAddress) ? DNSReverseArray[IPAddress] : IPAddress) + ":" + Address.Port;

                    Address.IPAddress.Byte1 = 127;
                    Address.IPAddress.Byte2 = 0;
                    Address.IPAddress.Byte3 = 0;
                    Address.IPAddress.Byte4 = 1;
                    Address.Port = 1080;

                    if (Parent != null)
                        Parent.LogToScreen("WS2_Connect: " + HostNameAndPort);
                }
            }
            catch (Exception e) { Parent.ReportException(e); }

            Unmanaged.ConnectStatus returnValue = Unmanaged.WS2_Connect(Socket, ref Address, AddressSize);
            if (returnValue == Unmanaged.ConnectStatus.Error &&
               (Unmanaged.WS2_GetLastError() == System.Net.Sockets.SocketError.WouldBlock ||
                   Unmanaged.WS2_GetLastError() == System.Net.Sockets.SocketError.Success)) // Non blocking mode
                returnValue = Unmanaged.ConnectStatus.Ok;

            try
            {
                if (Address.Family == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    byte[] connectBuf = System.Text.Encoding.ASCII.GetBytes("FASTCONNECT " + HostNameAndPort + "\r\n\r\n");
                    GCHandle pinnedArray = GCHandle.Alloc(connectBuf, GCHandleType.Pinned);
                    Unmanaged.WS2_Send(Socket, pinnedArray.AddrOfPinnedObject(), connectBuf.Length, System.Net.Sockets.SocketFlags.None);
                    pinnedArray.Free();
                }
            }
            catch (Exception e) { Parent.ReportException(e); }

            return returnValue;
        }

    }
}
