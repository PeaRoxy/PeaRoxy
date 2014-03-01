// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Unmanaged.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The unmanaged code behind injected code.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.Network.Hook
{
    #region

    using System;
    using System.Collections;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Text;

    #endregion

    /// <summary>
    ///     The unmanaged code behind injected code.
    /// </summary>
    internal static class Unmanaged
    {
        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate ConnectStatus ConnectDelegate(IntPtr socket, ref SocketAddressIn address, int addressSize);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate SocketError GetAddressInfoDelegate(
            IntPtr nodeName,
            IntPtr serviceName,
            IntPtr hints,
            out IntPtr results);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate IntPtr GetHostByNameDelegate(string host);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate int SendDelegate(IntPtr socket, IntPtr buffer, int len, SocketFlags flags);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate SocketError WsaSendDelegate(
            IntPtr socket,
            IntPtr buffer,
            int len,
            out IntPtr numberOfBytesSent,
            SocketFlags flags,
            IntPtr overlapped,
            IntPtr completionRoutine);

        #endregion

        #region Enums

        public enum ConnectStatus
        {
            Error = -1,

            Ok = 0
        }

        #endregion

        #region Public Methods and Operators

        [DllImport("ws2_32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true,
            EntryPoint = "freeaddrinfo")]
        public static extern SocketError FreeAddressInfo(IntPtr addrIntPtr);

        [DllImport("ws2_32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true,
            EntryPoint = "FreeAddrInfoW")]
        public static extern SocketError FreeAddressInfoUni(IntPtr addrIntPtr);

        [DllImport("ws2_32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true,
            EntryPoint = "getaddrinfo")]
        public static extern SocketError GetAddressInfo(
            IntPtr nodeName,
            IntPtr serviceName,
            IntPtr hints,
            out IntPtr results);

        [DllImport("ws2_32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true,
            EntryPoint = "GetAddrInfoW")]
        public static extern SocketError GetAddressInfoUni(
            IntPtr nodeName,
            IntPtr serviceName,
            IntPtr hints,
            out IntPtr results);

        [DllImport("ws2_32.dll", CharSet = CharSet.Ansi, SetLastError = true, EntryPoint = "gethostbyname")]
        public static extern IntPtr GetHostByName(string host);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr windowHandle, StringBuilder text, int length);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr windowHandle);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr windowHandle);

        [DllImport("ws2_32.dll", SetLastError = true, EntryPoint = "send")]
        public static extern int Send(IntPtr socket, IntPtr buffer, int len, SocketFlags flags);

        [DllImport("ws2_32.dll", SetLastError = true, EntryPoint = "WSASend")]
        public static extern SocketError Send(
            IntPtr socket,
            IntPtr buffer,
            int len,
            out IntPtr numberOfBytesSent,
            SocketFlags flags,
            IntPtr overlapped,
            IntPtr completionRoutine);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetWindowText(IntPtr windowHandle, String text);

        [DllImport("ws2_32.dll", SetLastError = true, EntryPoint = "connect")]
        public static extern ConnectStatus WS2_Connect(IntPtr socket, ref SocketAddressIn address, int addressSize);

        [DllImport("ws2_32.dll", SetLastError = true, EntryPoint = "WSAGetLastError")]
        public static extern SocketError WSAGetLastError();

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string fileName);

        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr handle);

        #endregion

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct NativeIpHostEntry
        {
            private readonly IntPtr nameHandler;

            private readonly IntPtr aliasesHandler;

            private readonly short addrType;

            private readonly short length;

            private readonly IntPtr addrListHandler;

            public static IPHostEntry FromNative(IntPtr nativePointer)
            {
                NativeIpHostEntry hostent =
                    (NativeIpHostEntry)Marshal.PtrToStructure(nativePointer, typeof(NativeIpHostEntry));
                IPHostEntry entry = new IPHostEntry();
                if (hostent.nameHandler != IntPtr.Zero)
                {
                    entry.HostName = Marshal.PtrToStringAnsi(hostent.nameHandler);
                }

                ArrayList list = new ArrayList();
                IntPtr ptr = hostent.addrListHandler;
                nativePointer = Marshal.ReadIntPtr(ptr);
                while (nativePointer != IntPtr.Zero)
                {
                    uint newAddress = (uint)Marshal.ReadInt32(nativePointer);
                    list.Add(new IPAddress(newAddress));

                    ptr += IntPtr.Size;
                    nativePointer = Marshal.ReadIntPtr(ptr);
                }

                entry.AddressList = new IPAddress[list.Count];
                list.CopyTo(entry.AddressList, 0);
                list.Clear();
                ptr = hostent.aliasesHandler;
                nativePointer = Marshal.ReadIntPtr(ptr);
                while (nativePointer != IntPtr.Zero)
                {
                    string str = Marshal.PtrToStringAnsi(nativePointer);
                    if (str != null)
                    {
                        list.Add(str);
                    }

                    ptr += IntPtr.Size;
                    nativePointer = Marshal.ReadIntPtr(ptr);
                }

                entry.Aliases = new string[list.Count];
                list.CopyTo(entry.Aliases, 0);
                return entry;
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 16)]
        public struct SocketAddressIn
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            [FieldOffset(8)]
            internal readonly byte[] Padding;

            [FieldOffset(4)]
            internal AddressIn IPAddress;

            [FieldOffset(0)]
            private ushort family;

            [FieldOffset(2)]
            private short port;

            internal int Port
            {
                get
                {
                    return System.Net.IPAddress.NetworkToHostOrder(this.port);
                }
                set
                {
                    this.port = System.Net.IPAddress.HostToNetworkOrder((short)value);
                }
            }

            internal AddressFamily Family
            {
                get
                {
                    return (AddressFamily)this.family;
                }

                // ReSharper disable once UnusedMember.Global
                set
                {
                    this.family = (ushort)value;
                }
            }

            [StructLayout(LayoutKind.Explicit, Size = 4)]
            public struct AddressIn
            {
                [FieldOffset(0)]
                internal uint Int;

                [FieldOffset(0)]
                internal readonly byte Byte1;

                [FieldOffset(1)]
                internal readonly byte Byte2;

                [FieldOffset(2)]
                internal readonly byte Byte3;

                [FieldOffset(3)]
                internal readonly byte Byte4;

                public IPAddress IpAddress
                {
                    get
                    {
                        return new IPAddress(this.Int);
                    }
                    set
                    {
                        this.Int = BitConverter.ToUInt32(value.GetAddressBytes(), 0);
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WsaBuffer
        {
            public uint Length;

            public IntPtr Buffer;

            public static WsaBuffer FromString(string str)
            {
                byte[] connectBuf = Encoding.ASCII.GetBytes(str);
                GCHandle pinnedArray = GCHandle.Alloc(connectBuf, GCHandleType.Pinned);
                return new WsaBuffer { Buffer = pinnedArray.AddrOfPinnedObject(), Length = (uint)connectBuf.Length };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AddressInfo
        {
            internal AddressInfoHints Flags;

            internal AddressFamily Family;

            internal SocketType SocketType;

            internal ProtocolFamily Protocol;

            internal int AddressLen;

            internal IntPtr CanonName; // sbyte Array

            internal IntPtr Address; // byte Array

            internal IntPtr Next; // Next Element In AddressInfo Array

            [Flags]
            internal enum AddressInfoHints
            {
                None = 0,

                // ReSharper disable once UnusedMember.Global
                Passive = 0x01,

                // ReSharper disable once UnusedMember.Global
                Canonname = 0x02,

                // ReSharper disable once UnusedMember.Global
                Numerichost = 0x04,

                // ReSharper disable once UnusedMember.Global
                All = 0x0100,

                // ReSharper disable once UnusedMember.Global
                Addrconfig = 0x0400,

                // ReSharper disable once UnusedMember.Global
                V4Mapped = 0x0800,

                // ReSharper disable once UnusedMember.Global
                NonAuthoritative = 0x04000,

                // ReSharper disable once UnusedMember.Global
                Secure = 0x08000,

                // ReSharper disable once UnusedMember.Global
                ReturnPreferredNames = 0x010000,

                // ReSharper disable once UnusedMember.Global
                Fqdn = 0x00020000,

                // ReSharper disable once UnusedMember.Global
                Fileserver = 0x00040000,
            }
        }
    }
}