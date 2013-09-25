using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using SocketError = System.Net.Sockets.SocketError;
using SocketFlags = System.Net.Sockets.SocketFlags;
namespace PeaRoxy.Windows.Network.Hook
{
    class Unmanaged
    {
        public enum ConnectStatus : int
        {
            Error = -1,
            Ok = 0
        }

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        public struct SocketAddress_In
        {
            private ushort _Family;
            private short _Port;
            internal AddressIn IPAddress;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal byte[] Padding;
            [StructLayout(LayoutKind.Explicit, Size = 4)]
            public struct AddressIn
            {
                [FieldOffset(0)]
                internal byte Byte1;
                [FieldOffset(1)]
                internal byte Byte2;
                [FieldOffset(2)]
                internal byte Byte3;
                [FieldOffset(3)]
                internal byte Byte4;
                [FieldOffset(0)]
                internal uint Int;
            }
            internal int Port
            {
                get { return System.Net.IPAddress.NetworkToHostOrder(_Port); }
                set { _Port = System.Net.IPAddress.HostToNetworkOrder((short)value); }
            }
            internal AddressFamily Family
            {
                get { return (AddressFamily)_Family; }
                set { _Family = (ushort)value; }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct NativeIPHostEntry
        {
            private IntPtr h_name;
            private IntPtr h_aliases;
            private short h_addrtype;
            private short h_length;
            private IntPtr h_addr_list;
            public static IPHostEntry FromNative(IntPtr nativePointer)
            {
                NativeIPHostEntry hostent = (NativeIPHostEntry)Marshal.PtrToStructure(nativePointer, typeof(NativeIPHostEntry));
                IPHostEntry entry = new IPHostEntry();
                if (hostent.h_name != IntPtr.Zero)
                    entry.HostName = Marshal.PtrToStringAnsi(hostent.h_name);
                ArrayList list = new ArrayList();
                IntPtr ptr = hostent.h_addr_list;
                nativePointer = Marshal.ReadIntPtr(ptr);
                while (nativePointer != IntPtr.Zero)
                {
                    int newAddress = Marshal.ReadInt32(nativePointer);
                    list.Add(new IPAddress(newAddress));
                    ptr += IntPtr.Size;
                    nativePointer = Marshal.ReadIntPtr(ptr);
                }
                entry.AddressList = new IPAddress[list.Count];
                list.CopyTo(entry.AddressList, 0);
                list.Clear();
                ptr = hostent.h_aliases;
                nativePointer = Marshal.ReadIntPtr(ptr);
                while (nativePointer != IntPtr.Zero)
                {
                    string str = Marshal.PtrToStringAnsi(nativePointer);
                    list.Add(str);
                    ptr += IntPtr.Size;
                    nativePointer = Marshal.ReadIntPtr(ptr);
                }
                entry.Aliases = new string[list.Count];
                list.CopyTo(entry.Aliases, 0);
                return entry;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AddressInfo
        {
            internal AddressInfoHints Flags;
            internal AddressFamily Family;
            internal SocketType SocketType;
            internal ProtocolFamily Protocol;
            internal int Addrlen;
            internal IntPtr CanonName; // sbyte Array
            internal IntPtr Address; // byte Array
            internal IntPtr Next; // Next Element In AddressInfo Array
            [Flags]
            internal enum AddressInfoHints
            {
                AI_CANONNAME = 2,
                AI_FQDN = 0x20000,
                AI_NUMERICHOST = 4,
                AI_PASSIVE = 1
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate ConnectStatus Connect_Delegate(IntPtr Socket, ref SocketAddress_In Address, int AddressSize);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate IntPtr GetHostByName_Delegate(string Host);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate SocketError GetAddressInfo_Delegate(IntPtr NodeName, IntPtr ServiceName, IntPtr Hints, out IntPtr Results);

        [DllImport("ws2_32.dll", SetLastError = true, EntryPoint = "connect")]
        public static extern ConnectStatus WS2_Connect(IntPtr Socket, ref SocketAddress_In Address, int AddressSize);

        [DllImport("Ws2_32.dll", SetLastError = true, EntryPoint = "send")]
        public static extern int WS2_Send(IntPtr Socket, IntPtr Buffer, int Len, SocketFlags Flags);

        [DllImport("ws2_32.dll", SetLastError = true, EntryPoint = "WSAGetLastError")]
        public static extern SocketError WS2_GetLastError();

        [DllImport("ws2_32.dll", CharSet = CharSet.Ansi, SetLastError = true, EntryPoint = "gethostbyname")]
        public static extern IntPtr WA2_GetHostByName(string Host);

        [DllImport("ws2_32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true, EntryPoint = "GetAddrInfoW")]
        public static extern SocketError WA2_GetAddressInfoUni(IntPtr NodeName, IntPtr ServiceName, IntPtr Hints, out IntPtr Results);

        [DllImport("ws2_32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true, EntryPoint = "getaddrinfo")]
        public static extern SocketError WA2_GetAddressInfo(IntPtr NodeName, IntPtr ServiceName, IntPtr Hints, out IntPtr Results);
    }
}
