// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowsConnections.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace PeaRoxy.Windows
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Runtime.InteropServices;

    using PeaRoxy.Platform;

    #endregion

    /// <summary>
    /// The windows connection.
    /// </summary>
    public class WindowsConnection : ConnectionInfo
    {
        #region Static Fields

        /// <summary>
        /// The is supported.
        /// </summary>
        private static bool isSupported = true;

        /// <summary>
        /// The is v 2.
        /// </summary>
        private static bool isV2;

        /// <summary>
        /// The tcp cache.
        /// </summary>
        private static List<ConnectionInfo> tcpCache = new List<ConnectionInfo>();

        /// <summary>
        /// The udp cache.
        /// </summary>
        private static List<ConnectionInfo> udpCache = new List<ConnectionInfo>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsConnection"/> class.
        /// </summary>
        /// <param name="stateId">
        /// The state id.
        /// </param>
        /// <param name="localAddress">
        /// The local address.
        /// </param>
        /// <param name="remoteAddress">
        /// The remote address.
        /// </param>
        /// <param name="processId">
        /// The process id.
        /// </param>
        public WindowsConnection(Status stateId, IPEndPoint localAddress, IPEndPoint remoteAddress, int processId)
        {
            this.ProtocolType = Protocol.Tcp;
            this.State = stateId;
            this.LocalAddress = localAddress;
            if (this.LocalAddress.Port != 0 && this.LocalAddress.Address.Equals(new IPAddress(0x0)))
            {
                this.LocalAddress = new IPEndPoint(new IPAddress(0x0000007F), this.LocalAddress.Port);
            }

            this.RemoteAddress = remoteAddress;
            if (this.RemoteAddress.Port != 0 && this.RemoteAddress.Address.Equals(new IPAddress(0x0)))
            {
                this.RemoteAddress = new IPEndPoint(new IPAddress(0x0000007F), this.RemoteAddress.Port);
            }

            this.ProcessId = processId;
            Win32Process process = Win32Process.GetProcessByPidWithCache(this.ProcessId);
            this.ProcessName = process.Name;
            this.ProcessPath = process.ExecutablePath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsConnection"/> class.
        /// </summary>
        /// <param name="localAddress">
        /// The local address.
        /// </param>
        /// <param name="processId">
        /// The process id.
        /// </param>
        public WindowsConnection(IPEndPoint localAddress, int processId)
        {
            this.ProtocolType = Protocol.Udp;
            this.LocalAddress = localAddress;
            if (this.LocalAddress.Port != 0 && this.LocalAddress.Address.Equals(new IPAddress(0x0)))
            {
                this.LocalAddress = new IPEndPoint(new IPAddress(0x0000007F), this.LocalAddress.Port);
            }

            this.ProcessId = processId;
            Win32Process process = Win32Process.GetProcessByPidWithCache(this.ProcessId);
            this.ProcessName = process.Name;
            this.ProcessPath = process.ExecutablePath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsConnection"/> class.
        /// </summary>
        internal WindowsConnection()
        {
        }

        #endregion

        #region Enums

        /// <summary>
        /// The result.
        /// </summary>
        private enum Result
        {
            /// <summary>
            /// The no error.
            /// </summary>
            NoError = 0
        }

        /// <summary>
        /// The tcp table class
        /// </summary>
        private enum TcpTableClass
        {
            /// <summary>
            /// The tcp table basic listener.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            TcpTableBasicListener, 

            /// <summary>
            /// The tc p_ tabl e_ basi c_ connections.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            TcpTableBasicConnections, 

            /// <summary>
            /// The tc p_ tabl e_ basi c_ all.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            TcpTableBasicAll, 

            /// <summary>
            /// The tc p_ tabl e_ owne r_ pi d_ listener.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            TcpTableOwnerPidListener, 

            /// <summary>
            /// The tc p_ tabl e_ owne r_ pi d_ connections.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            TcpTableOwnerPidConnections, 

            /// <summary>
            /// The tc p_ tabl e_ owne r_ pi d_ all.
            /// </summary>
            TcpTableOwnerPidAll, 

            /// <summary>
            /// The tc p_ tabl e_ owne r_ modul e_ listener.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            TcpTableOwnerModuleListener, 

            /// <summary>
            /// The tc p_ tabl e_ owne r_ modul e_ connections.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            TcpTableOwnerModuleConnections, 

            /// <summary>
            /// The tc p_ tabl e_ owne r_ modul e_ all.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            TcpTableOwnerModuleAll, 
        }

        /// <summary>
        /// The ud p_ tabl e_ class.
        /// </summary>
        private enum UdpTableClass
        {
            /// <summary>
            /// The ud p_ tabl e_ basic.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            UdpTableBasic, 

            /// <summary>
            /// The ud p_ tabl e_ owne r_ pid.
            /// </summary>
            UdpTableOwnerPid, 

            /// <summary>
            /// The ud p_ tabl e_ owne r_ module.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            UdpTableOwnerModule
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get tcp connection by local address.
        /// </summary>
        /// <param name="ip">
        /// The ip.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <returns>
        /// The <see cref="ConnectionInfo"/>.
        /// </returns>
        public override ConnectionInfo GetTcpConnectionByLocalAddress(IPAddress ip, ushort port)
        {
            foreach (var con in tcpCache.Cast<WindowsConnection>().Where(con => con.LocalAddress.Address.Equals(ip) && con.LocalAddress.Port == port))
            {
                return con;
            }

            tcpCache = this.GetTcpConnections();
            return tcpCache.Cast<WindowsConnection>().FirstOrDefault(con => con.LocalAddress.Address.Equals(ip) && con.LocalAddress.Port == port);
        }

        /// <summary>
        /// The get tcp connections.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public override List<ConnectionInfo> GetTcpConnections()
        {
            if (isV2)
            {
                return GetExTcpConnectionsV2();
            }

            try
            {
                return GetExTcpConnectionsV1();
            }
            catch (Exception)
            {
                isV2 = true;
                return this.GetTcpConnections();
            }
        }

        /// <summary>
        /// The get udp connection by local address.
        /// </summary>
        /// <param name="ip">
        /// The ip.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <returns>
        /// The <see cref="ConnectionInfo"/>.
        /// </returns>
        public override ConnectionInfo GetUdpConnectionByLocalAddress(IPAddress ip, ushort port)
        {
            foreach (var connectionInfo in udpCache)
            {
                var con = (WindowsConnection)connectionInfo;
                if (con.LocalAddress.Address.Equals(ip) && con.LocalAddress.Port == port)
                {
                    return con;
                }
            }

            udpCache = this.GetUdpConnections();
            return udpCache.Cast<WindowsConnection>().FirstOrDefault(con => con.LocalAddress.Address.Equals(ip) && con.LocalAddress.Port == port);
        }

        /// <summary>
        /// The get udp connections.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public override List<ConnectionInfo> GetUdpConnections()
        {
            if (isV2)
            {
                return GetExUdpConnectionsV2();
            }

            try
            {
                return GetExUdpConnectionsV1();
            }
            catch (Exception)
            {
                isV2 = true;
                return this.GetUdpConnections();
            }
        }

        /// <summary>
        /// The is supported.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool IsSupported()
        {
            if (!isSupported)
            {
                return false;
            }

            try
            {
                this.GetTcpConnections();
                this.GetUdpConnections();
                return true;
            }
            catch (Exception)
            {
            }

            isSupported = false;
            return false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The allocate and get tcp ex table from stack.
        /// </summary>
        /// <param name="pTable">
        /// The p table.
        /// </param>
        /// <param name="bOrder">
        /// The b order.
        /// </param>
        /// <param name="heap">
        /// The heap.
        /// </param>
        /// <param name="zero">
        /// The zero.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [DllImport("iphlpapi.dll", SetLastError = false)]
        private static extern int AllocateAndGetTcpExTableFromStack(
            ref IntPtr pTable, 
            bool bOrder, 
            IntPtr heap, 
            int zero, 
            int flags);

        /// <summary>
        /// The allocate and get udp ex table from stack.
        /// </summary>
        /// <param name="pTable">
        /// The p table.
        /// </param>
        /// <param name="bOrder">
        /// The b order.
        /// </param>
        /// <param name="heap">
        /// The heap.
        /// </param>
        /// <param name="zero">
        /// The zero.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [DllImport("iphlpapi.dll", SetLastError = false)]
        private static extern int AllocateAndGetUdpExTableFromStack(
            ref IntPtr pTable, 
            bool bOrder, 
            IntPtr heap, 
            int zero, 
            int flags);

        /// <summary>
        /// The get ex tcp connections v 1.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        private static List<ConnectionInfo> GetExTcpConnectionsV1()
        {
            List<ConnectionInfo> list = new List<ConnectionInfo>();
            const int RowSize = 24;
            int bufferSize = 100000;
            IntPtr lpTable = Marshal.AllocHGlobal(bufferSize);
            Result res = (Result)AllocateAndGetTcpExTableFromStack(ref lpTable, true, GetProcessHeap(), 0, 2);
            if (res != Result.NoError)
            {
                return list;
            }

            int numEntries = (int)Marshal.ReadIntPtr(lpTable);
            lpTable = IntPtr.Zero;
            Marshal.FreeHGlobal(lpTable);
            bufferSize = (numEntries * RowSize) + 4;
            lpTable = Marshal.AllocHGlobal(bufferSize);
            res = (Result)AllocateAndGetTcpExTableFromStack(ref lpTable, true, GetProcessHeap(), 0, 2);
            if (res != Result.NoError)
            {
                return list;
            }

            IntPtr current = lpTable;
            numEntries = (int)Marshal.ReadIntPtr(current);
            current = (IntPtr)((int)current + 4);
            for (int i = 0; i < numEntries; i++)
            {
                Status stat = (Status)Marshal.ReadIntPtr(current);
                current = (IntPtr)((int)current + 4);
                IPEndPoint localAddress = new IPEndPoint(
                    (UInt32)Marshal.ReadIntPtr(current), 
                    CommonFunctions.ConvertInt2PortNumber((UInt32)Marshal.ReadIntPtr((IntPtr)((int)current + 4))));
                current = (IntPtr)((int)current + 8);
                IPEndPoint remoteAddress = new IPEndPoint(
                    (UInt32)Marshal.ReadIntPtr(current), 
                    ((UInt32)Marshal.ReadIntPtr(current) != 0)
                         ? CommonFunctions.ConvertInt2PortNumber((UInt32)Marshal.ReadIntPtr((IntPtr)((int)current + 4)))
                         : 0);
                current = (IntPtr)((int)current + 8);
                int pId = (int)Marshal.ReadIntPtr(current);
                current = (IntPtr)((int)current + 4);
                list.Add(new WindowsConnection(stat, localAddress, remoteAddress, pId));
            }

            Marshal.FreeHGlobal(lpTable);
            return list;
        }

        /// <summary>
        /// The get ex tcp connections v 2.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        private static List<ConnectionInfo> GetExTcpConnectionsV2()
        {
            List<ConnectionInfo> list = new List<ConnectionInfo>();
            const int AfInet = 2; // IP_v4
            int buffSize = 20000;
            bool firstRun = true;
            exec:
            byte[] buffer = new byte[buffSize];
            Result res =
                (Result)
                GetExtendedTcpTable(buffer, out buffSize, true, AfInet, TcpTableClass.TcpTableOwnerPidAll, 0);
            if (res != Result.NoError)
            {
                if (!firstRun)
                {
                    return list;
                }

                firstRun = false;
                goto exec;
            }

            int numEntries = Convert.ToInt32(buffer[0]);
            int nOffset = 4;
            for (int i = 0; i < numEntries; i++)
            {
                nOffset += 4;
                list.Add(
                    new WindowsConnection(
                        (Status)Convert.ToInt32(buffer[nOffset - 4]), 
                        CommonFunctions.Bytes2IpEndPoint(buffer, ref nOffset, false), 
                        CommonFunctions.Bytes2IpEndPoint(buffer, ref nOffset, true), 
                        CommonFunctions.Bytes2Int(buffer, ref nOffset)));
            }

            return list;
        }

        /// <summary>
        /// The get ex udp connections v 1.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        private static List<ConnectionInfo> GetExUdpConnectionsV1()
        {
            List<ConnectionInfo> list = new List<ConnectionInfo>();
            const int RowSize = 12;
            int bufferSize = 100000;
            IntPtr lpTable = Marshal.AllocHGlobal(bufferSize);
            Result res = (Result)AllocateAndGetUdpExTableFromStack(ref lpTable, true, GetProcessHeap(), 0, 2);
            if (res != Result.NoError)
            {
                return list;
            }

            int numEntries = (int)Marshal.ReadIntPtr(lpTable);
            lpTable = IntPtr.Zero;
            Marshal.FreeHGlobal(lpTable);
            bufferSize = (numEntries * RowSize) + 4;
            lpTable = Marshal.AllocHGlobal(bufferSize);
            res = (Result)AllocateAndGetUdpExTableFromStack(ref lpTable, true, GetProcessHeap(), 0, 2);
            if (res != Result.NoError)
            {
                return list;
            }

            IntPtr current = lpTable;
            numEntries = (int)Marshal.ReadIntPtr(current);
            current = (IntPtr)((int)current + 4);
            for (int i = 0; i < numEntries; i++)
            {
                IPEndPoint localAddress = new IPEndPoint(
                    (UInt32)Marshal.ReadIntPtr(current), 
                    CommonFunctions.ConvertInt2PortNumber((UInt32)Marshal.ReadIntPtr((IntPtr)((int)current + 4))));
                current = (IntPtr)((int)current + 8);
                int pId = (int)Marshal.ReadIntPtr(current);
                current = (IntPtr)((int)current + 4);
                list.Add(new WindowsConnection(localAddress, pId));
            }

            Marshal.FreeHGlobal(lpTable);
            return list;
        }

        /// <summary>
        /// The get ex udp connections v 2.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        private static List<ConnectionInfo> GetExUdpConnectionsV2()
        {
            List<ConnectionInfo> list = new List<ConnectionInfo>();
            const int AfInet = 2; // IP_v4
            int buffSize = 20000;
            bool firstRun = true;
            exec:
            byte[] buffer = new byte[buffSize];
            Result res =
                (Result)GetExtendedUdpTable(buffer, out buffSize, true, AfInet, UdpTableClass.UdpTableOwnerPid, 0);
            if (res != Result.NoError)
            {
                if (!firstRun) return list;
                
                firstRun = false;
                goto exec;
            }

            int numEntries = Convert.ToInt32(buffer[0]);
            int nOffset = 4;
            for (int i = 0; i < numEntries; i++)
            {
                list.Add(
                    new WindowsConnection(
                        CommonFunctions.Bytes2IpEndPoint(buffer, ref nOffset, false), 
                        CommonFunctions.Bytes2Int(buffer, ref nOffset)));
            }

            return list;
        }

        /// <summary>
        /// The get extended tcp table.
        /// </summary>
        /// <param name="pTcpTable">
        /// The p tcp table.
        /// </param>
        /// <param name="dwOutBufLen">
        /// The dw out buf len.
        /// </param>
        /// <param name="sort">
        /// The sort.
        /// </param>
        /// <param name="ipVersion">
        /// The ip version.
        /// </param>
        /// <param name="tblClass">
        /// The tbl class.
        /// </param>
        /// <param name="reserved">
        /// The reserved.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [DllImport("iphlpapi.dll", SetLastError = false)]
        private static extern int GetExtendedTcpTable(
            byte[] pTcpTable, 
            out int dwOutBufLen, 
            bool sort, 
            int ipVersion, 
            TcpTableClass tblClass, 
            int reserved);

        /// <summary>
        /// The get extended udp table.
        /// </summary>
        /// <param name="pUdpTable">
        /// The p udp table.
        /// </param>
        /// <param name="dwOutBufLen">
        /// The dw out buf len.
        /// </param>
        /// <param name="sort">
        /// The sort.
        /// </param>
        /// <param name="ipVersion">
        /// The ip version.
        /// </param>
        /// <param name="tblClass">
        /// The tbl class.
        /// </param>
        /// <param name="reserved">
        /// The reserved.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [DllImport("iphlpapi.dll", SetLastError = false)]
        private static extern int GetExtendedUdpTable(
            byte[] pUdpTable, 
            out int dwOutBufLen, 
            bool sort, 
            int ipVersion, 
            UdpTableClass tblClass, 
            int reserved);

        /// <summary>
        /// The get process heap.
        /// </summary>
        /// <returns>
        /// The <see cref="IntPtr"/>.
        /// </returns>
        [DllImport("kernel32", SetLastError = false)]
        private static extern IntPtr GetProcessHeap();

        #endregion

        /// <summary>
        /// The common functions.
        /// </summary>
        private static class CommonFunctions
        {
            #region Public Methods and Operators

            /// <summary>
            /// The bytes 2 ip end point.
            /// </summary>
            /// <param name="buffer">
            /// The buffer.
            /// </param>
            /// <param name="nOffset">
            /// The n offset.
            /// </param>
            /// <param name="isRemote">
            /// The is remote.
            /// </param>
            /// <returns>
            /// The <see cref="IPEndPoint"/>.
            /// </returns>
            [DebuggerStepThrough]
            public static IPEndPoint Bytes2IpEndPoint(byte[] buffer, ref int nOffset, bool isRemote)
            {
                // address
                long mAddress = ((((buffer[nOffset + 3] << 0x18) | (buffer[nOffset + 2] << 0x10))
                                    | (buffer[nOffset + 1] << 8)) | buffer[nOffset]) & 0xffffffff;
                nOffset += 4;
                int mPort = (isRemote && (mAddress == 0))
                                ? 0
                                : (buffer[nOffset] << 8) + buffer[nOffset + 1] + (buffer[nOffset + 2] << 24)
                                  + (buffer[nOffset + 3] << 16);
                nOffset += 4;

                // store the remote endpoint
                try
                {
                    return new IPEndPoint(mAddress, mPort);
                }
                catch (Exception)
                {
                    Debug.WriteLine(
                         "Parsed address is null. Addr=" + mAddress + " Port=" + mPort + " IsRemote="
                         + isRemote);
                }

                return null;
            }

            /// <summary>
            /// The bytes 2 int.
            /// </summary>
            /// <param name="buffer">
            /// The buffer.
            /// </param>
            /// <param name="nOffset">
            /// The n offset.
            /// </param>
            /// <returns>
            /// The <see cref="int"/>.
            /// </returns>
            [DebuggerStepThrough]
            public static int Bytes2Int(byte[] buffer, ref int nOffset)
            {
                int res = buffer[nOffset] + (buffer[nOffset + 1] << 8) + (buffer[nOffset + 2] << 16)
                          + (buffer[nOffset + 3] << 24);
                nOffset += 4;
                return res;
            }

            /// <summary>
            /// The convert int 2 port number.
            /// </summary>
            /// <param name="dwPort">
            /// The dw port.
            /// </param>
            /// <returns>
            /// The <see cref="ushort"/>.
            /// </returns>
            [DebuggerStepThrough]
            public static ushort ConvertInt2PortNumber(uint dwPort)
            {
                byte[] b = new byte[2];
                b[0] = byte.Parse((dwPort >> 8).ToString(CultureInfo.InvariantCulture));
                b[1] = byte.Parse((dwPort & 0xFF).ToString(CultureInfo.InvariantCulture));
                return BitConverter.ToUInt16(b, 0);
            }



            #endregion
        }
    }
}