// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Unmanaged.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The unmanaged.
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

    #endregion

    /// <summary>
    ///     The unmanaged.
    /// </summary>
    internal static class Unmanaged
    {
        #region Delegates

        /// <summary>
        ///     The connect_ delegate.
        /// </summary>
        /// <param name="socket">
        ///     The socket.
        /// </param>
        /// <param name="address">
        ///     The address.
        /// </param>
        /// <param name="addressSize">
        ///     The address size.
        /// </param>
        /// <returns>
        ///     Status of connection
        /// </returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate ConnectStatus ConnectDelegate(IntPtr socket, ref SocketAddressIn address, int addressSize);

        /// <summary>
        ///     The get address info_ delegate.
        /// </summary>
        /// <param name="nodeName">
        ///     The node name.
        /// </param>
        /// <param name="serviceName">
        ///     The service name.
        /// </param>
        /// <param name="hints">
        ///     The hints.
        /// </param>
        /// <param name="results">
        ///     The results.
        /// </param>
        /// <returns>
        ///     Status of socket
        /// </returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate SocketError GetAddressInfoDelegate(
            IntPtr nodeName, 
            IntPtr serviceName, 
            IntPtr hints, 
            out IntPtr results);

        /// <summary>
        ///     The get host by name delegate.
        /// </summary>
        /// <param name="host">
        ///     The host.
        /// </param>
        /// <returns>
        ///     IP handler
        /// </returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        public delegate IntPtr GetHostByNameDelegate(string host);

        #endregion

        #region Enums

        /// <summary>
        ///     The connect status.
        /// </summary>
        public enum ConnectStatus
        {
            /// <summary>
            ///     The error.
            /// </summary>
            Error = -1, 

            /// <summary>
            ///     The ok.
            /// </summary>
            Ok = 0
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The WS2 connect.
        /// </summary>
        /// <param name="socket">
        /// The socket.
        /// </param>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <param name="addressSize">
        /// The address size.
        /// </param>
        /// <returns>
        /// The <see cref="ConnectStatus"/>.
        /// </returns>
        [DllImport("ws2_32.dll", SetLastError = true, EntryPoint = "connect")]
        public static extern ConnectStatus WS2_Connect(IntPtr socket, ref SocketAddressIn address, int addressSize);

        /// <summary>
        /// The WS2 get address info.
        /// </summary>
        /// <param name="nodeName">
        /// The node name.
        /// </param>
        /// <param name="serviceName">
        /// The service name.
        /// </param>
        /// <param name="hints">
        /// The hints.
        /// </param>
        /// <param name="results">
        /// The results.
        /// </param>
        /// <returns>
        /// The <see cref="SocketError"/>.
        /// </returns>
        [DllImport("ws2_32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true, 
            EntryPoint = "getaddrinfo")]
        public static extern SocketError WS2_GetAddressInfo(
            IntPtr nodeName, 
            IntPtr serviceName, 
            IntPtr hints, 
            out IntPtr results);

        /// <summary>
        /// The WS2 get address info unicode.
        /// </summary>
        /// <param name="nodeName">
        /// The node name.
        /// </param>
        /// <param name="serviceName">
        /// The service name.
        /// </param>
        /// <param name="hints">
        /// The hints.
        /// </param>
        /// <param name="results">
        /// The results.
        /// </param>
        /// <returns>
        /// The <see cref="SocketError"/>.
        /// </returns>
        [DllImport("ws2_32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true, 
            EntryPoint = "GetAddrInfoW")]
        public static extern SocketError WS2_GetAddressInfoUni(
            IntPtr nodeName, 
            IntPtr serviceName, 
            IntPtr hints, 
            out IntPtr results);

        /// <summary>
        /// The WS2 get host by name.
        /// </summary>
        /// <param name="host">
        /// The host.
        /// </param>
        /// <returns>
        /// The <see cref="IntPtr"/>.
        /// </returns>
        [DllImport("ws2_32.dll", CharSet = CharSet.Ansi, SetLastError = true, EntryPoint = "gethostbyname")]
        public static extern IntPtr WS2_GetHostByName(string host);

        /// <summary>
        ///     The WS2 get last error.
        /// </summary>
        /// <returns>
        ///     The <see cref="SocketError" />.
        /// </returns>
        [DllImport("ws2_32.dll", SetLastError = true, EntryPoint = "WSAGetLastError")]
        public static extern SocketError WS2_GetLastError();

        /// <summary>
        /// The WS2 send.
        /// </summary>
        /// <param name="socket">
        /// The socket.
        /// </param>
        /// <param name="buffer">
        /// The buffer.
        /// </param>
        /// <param name="len">
        /// The len.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [DllImport("Ws2_32.dll", SetLastError = true, EntryPoint = "send")]
        public static extern int WS2_Send(IntPtr socket, IntPtr buffer, int len, SocketFlags flags);

        #endregion

        /// <summary>
        ///     The native IP host entry.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct NativeIpHostEntry
        {
            /// <summary>
            ///     The name.
            /// </summary>
            private readonly IntPtr nameHandler;

            /// <summary>
            ///     The aliases.
            /// </summary>
            private readonly IntPtr aliasesHandler;

            /// <summary>
            ///     The address type.
            /// </summary>
            private readonly short addrType;

            /// <summary>
            ///     The length.
            /// </summary>
            private readonly short length;

            /// <summary>
            ///     The address list handler.
            /// </summary>
            private readonly IntPtr addrListHandler;

            /// <summary>
            /// The from native.
            /// </summary>
            /// <param name="nativePointer">
            /// The native pointer.
            /// </param>
            /// <returns>
            /// The <see cref="IPHostEntry"/>.
            /// </returns>
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
                    int newAddress = Marshal.ReadInt32(nativePointer);
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

        /// <summary>
        ///     The socket address_ in.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = 16)]
        public struct SocketAddressIn
        {
            /// <summary>
            ///     The padding.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal readonly byte[] Padding;

            /// <summary>
            ///     The IP address.
            /// </summary>
            internal AddressIn IPAddress;

            /// <summary>
            ///     The family.
            /// </summary>
            private ushort family;

            /// <summary>
            ///     The port.
            /// </summary>
            private short port;

            /// <summary>
            ///     Gets or sets the port.
            /// </summary>
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

            /// <summary>
            ///     Gets or sets the family.
            /// </summary>
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

            /// <summary>
            ///     The address in.
            /// </summary>
            [StructLayout(LayoutKind.Explicit, Size = 4)]
            public struct AddressIn
            {
                /// <summary>
                ///     The integer.
                /// </summary>
                [FieldOffset(0)]
                internal readonly uint Int;

                /// <summary>
                ///     The byte 1.
                /// </summary>
                [FieldOffset(0)]
                internal byte Byte1;

                /// <summary>
                ///     The byte 2.
                /// </summary>
                [FieldOffset(1)]
                internal byte Byte2;

                /// <summary>
                ///     The byte 3.
                /// </summary>
                [FieldOffset(2)]
                internal byte Byte3;

                /// <summary>
                ///     The byte 4.
                /// </summary>
                [FieldOffset(3)]
                internal byte Byte4;
            }
        }

        /// <summary>
        ///     The address info.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct AddressInfo
        {
            /// <summary>
            ///     The flags.
            /// </summary>
            internal readonly AddressInfoHints Flags;

            /// <summary>
            ///     The family.
            /// </summary>
            internal readonly AddressFamily Family;

            /// <summary>
            ///     The socket type.
            /// </summary>
            internal readonly SocketType SocketType;

            /// <summary>
            ///     The protocol.
            /// </summary>
            internal readonly ProtocolFamily Protocol;

            /// <summary>
            ///     The address len.
            /// </summary>
            internal readonly int AddressLen;

            /// <summary>
            ///     The canon name.
            /// </summary>
            internal IntPtr CanonName; // sbyte Array

            /// <summary>
            ///     The address.
            /// </summary>
            internal IntPtr Address; // byte Array

            /// <summary>
            ///     The next.
            /// </summary>
            internal IntPtr Next; // Next Element In AddressInfo Array

            /// <summary>
            ///     The address info hints.
            /// </summary>
            [Flags]
            internal enum AddressInfoHints
            {
                /// <summary>
                ///     The canon name.
                /// </summary>
                // ReSharper disable once UnusedMember.Global
                AiCanonname = 2, 

                /// <summary>
                ///     The FQDN.
                /// </summary>
                // ReSharper disable once UnusedMember.Global
                AiFqdn = 0x20000, 

                /// <summary>
                ///     The numeric host.
                /// </summary>
                // ReSharper disable once UnusedMember.Global
                AiNumerichost = 4, 

                /// <summary>
                ///     The passive.
                /// </summary>
                // ReSharper disable once UnusedMember.Global
                AiPassive = 1
            }
        }
    }
}