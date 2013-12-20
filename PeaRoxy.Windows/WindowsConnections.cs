using System;
using System.Net;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PeaRoxy.Windows
{
    public class WindowsConnection : Platform.ConnectionInfo
    {
        [DllImport("iphlpapi.dll", SetLastError = false)]
        private extern static int AllocateAndGetTcpExTableFromStack(ref IntPtr pTable, bool bOrder, IntPtr heap, int zero, int flags);

        [DllImport("iphlpapi.dll", SetLastError = false)]
        private extern static int AllocateAndGetUdpExTableFromStack(ref IntPtr pTable, bool bOrder, IntPtr heap, int zero, int flags);

        [DllImport("kernel32", SetLastError = false)]
        private static extern IntPtr GetProcessHeap();

        [DllImport("iphlpapi.dll", SetLastError = false)]
        private static extern int GetExtendedTcpTable(byte[] pTcpTable, out int dwOutBufLen, bool sort,
            int ipVersion, TCP_TABLE_CLASS tblClass, int reserved);

        [DllImport("iphlpapi.dll", SetLastError = false)]
        private static extern int GetExtendedUdpTable(byte[] pUdpTable, out int dwOutBufLen, bool sort,
            int ipVersion, UDP_TABLE_CLASS tblClass, int reserved);

        private enum UDP_TABLE_CLASS
        {
            UDP_TABLE_BASIC,
            UDP_TABLE_OWNER_PID,
            UDP_TABLE_OWNER_MODULE
        }

        private enum TCP_TABLE_CLASS
        {
            TCP_TABLE_BASIC_LISTENER,
            TCP_TABLE_BASIC_CONNECTIONS,
            TCP_TABLE_BASIC_ALL,
            TCP_TABLE_OWNER_PID_LISTENER,
            TCP_TABLE_OWNER_PID_CONNECTIONS,
            TCP_TABLE_OWNER_PID_ALL,
            TCP_TABLE_OWNER_MODULE_LISTENER,
            TCP_TABLE_OWNER_MODULE_CONNECTIONS,
            TCP_TABLE_OWNER_MODULE_ALL,
        }

        private enum Result
        {
            NoError = 0
        }

        private static bool isV2 = false;
        private static bool isSupported = true;
        private static List<Platform.ConnectionInfo> tcpCache = new List<Platform.ConnectionInfo>();
        private static List<Platform.ConnectionInfo> udpCache = new List<Platform.ConnectionInfo>();

        internal WindowsConnection()
        {
        }

        public WindowsConnection(Status StateId, IPEndPoint LocalAddress, IPEndPoint RemoteAddress, int ProcessId)
        {
            this.ProtocolType = Protocol.Tcp;
            this.State = StateId;
            this.LocalAddress = LocalAddress;
            if (this.LocalAddress.Port != 0 && this.LocalAddress.Address.Equals(new IPAddress(0x0)))
                this.LocalAddress = new IPEndPoint(new IPAddress(0x0000007F), this.LocalAddress.Port);
            this.RemoteAddress = RemoteAddress;
            if (this.RemoteAddress.Port != 0 && this.RemoteAddress.Address.Equals(new IPAddress(0x0)))
                this.RemoteAddress = new IPEndPoint(new IPAddress(0x0000007F), this.RemoteAddress.Port);
            this.ProcessId = ProcessId;
            this.ProcessString = CommonFunctions.GetProcessNameByPId(this.ProcessId);
        }

        public WindowsConnection(IPEndPoint LocalAddress, int ProcessId)
        {
            this.ProtocolType = Protocol.Udp;
            this.LocalAddress = LocalAddress;
            if (this.LocalAddress.Port != 0 && this.LocalAddress.Address.Equals(new IPAddress(0x0)))
                this.LocalAddress = new IPEndPoint(new IPAddress(0x0000007F), this.LocalAddress.Port);
            this.ProcessId = ProcessId;
            this.ProcessString = CommonFunctions.GetProcessNameByPId(this.ProcessId);
        }

        public override bool IsSupported()
        {
            if (!isSupported)
                return false;
            try
            {
                GetTcpConnections();
                GetUdpConnections();
                return true;
            }
            catch (Exception) { }
            isSupported = false;
            return false;
        }

        public override Platform.ConnectionInfo GetTcpConnectionByLocalAddress(IPAddress ip, ushort port)
        {
            foreach (WindowsConnection con in tcpCache)
                if (con.LocalAddress.Address.Equals(ip) && con.LocalAddress.Port == port)
                    return con;
            tcpCache = GetTcpConnections();
            foreach (WindowsConnection con in tcpCache)
                if (con.LocalAddress.Address.Equals(ip) && con.LocalAddress.Port == port)
                    return con;
            return null;
        }

        public override Platform.ConnectionInfo GetUdpConnectionByLocalAddress(IPAddress ip, ushort port)
        {
            foreach (WindowsConnection con in udpCache)
                if (con.LocalAddress.Address.Equals(ip) && con.LocalAddress.Port == port)
                    return con;
            udpCache = GetUdpConnections();
            foreach (WindowsConnection con in udpCache)
                if (con.LocalAddress.Address.Equals(ip) && con.LocalAddress.Port == port)
                    return con;
            return null;
        }

        public override List<Platform.ConnectionInfo> GetTcpConnections()
        {
            if (isV2)
                return GetEXTcpConnectionsV2();
            else
                try
                {
                    return GetEXTcpConnectionsV1();
                }
                catch (Exception)
                {
                    isV2 = true;
                    return GetTcpConnections();
                }
        }

        public override List<Platform.ConnectionInfo> GetUdpConnections()
        {
            if (isV2)
                return GetEXUdpConnectionsV2();
            else
                try
                {
                    return GetEXUdpConnectionsV1();
                }
                catch (Exception)
                {
                    isV2 = true;
                    return GetUdpConnections();
                }
        }

        private static List<Platform.ConnectionInfo> GetEXTcpConnectionsV2()
        {
            List<Platform.ConnectionInfo> _list = new List<Platform.ConnectionInfo>();
            int AF_INET = 2;    // IP_v4
            int buffSize = 20000;
            bool firstRun = true;
            exec:
            byte[] buffer = new byte[buffSize];
            Result res = (Result)GetExtendedTcpTable(buffer, out buffSize, true, AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
            if (res != Result.NoError)
                if (firstRun)
                {
                    firstRun = false;
                    goto exec;
                }
                else
                    return _list;
            int NumEntries = Convert.ToInt32(buffer[0]);
            int nOffset = 4;
            for (int i = 0; i < NumEntries; i++)
            {
                nOffset += 4;
                _list.Add(new WindowsConnection(
                                        (Status)Convert.ToInt32(buffer[nOffset - 4]),
                                        CommonFunctions.Bytes2IPEndPoint(buffer, ref nOffset, false),
                                        CommonFunctions.Bytes2IPEndPoint(buffer, ref nOffset, true),
                                        CommonFunctions.Bytes2Int(buffer, ref nOffset)
                                        ));
            }
            return _list;
        }

        private static List<Platform.ConnectionInfo> GetEXUdpConnectionsV2()
        {
            List<Platform.ConnectionInfo> _list = new List<Platform.ConnectionInfo>();
            int AF_INET = 2;    // IP_v4
            int buffSize = 20000;
            bool firstRun = true;
            exec:
            byte[] buffer = new byte[buffSize];
            Result res = (Result)GetExtendedUdpTable(buffer, out buffSize, true, AF_INET, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID, 0);
            if (res != Result.NoError)
                if (firstRun)
                {
                    firstRun = false;
                    goto exec;
                }
                else
                    return _list;
            int NumEntries = Convert.ToInt32(buffer[0]);
            int nOffset = 4;
            for (int i = 0; i < NumEntries; i++)
                _list.Add(new WindowsConnection(
                                        CommonFunctions.Bytes2IPEndPoint(buffer, ref nOffset, false),
                                        CommonFunctions.Bytes2Int(buffer, ref nOffset)
                                        ));
            return _list;
        }

        private static List<Platform.ConnectionInfo> GetEXTcpConnectionsV1()
        {
            List<Platform.ConnectionInfo> _list = new List<Platform.ConnectionInfo>();
            int rowsize = 24;
            int BufferSize = 100000;
            IntPtr lpTable = Marshal.AllocHGlobal(BufferSize);
            Result res = (Result)AllocateAndGetTcpExTableFromStack(ref lpTable, true, GetProcessHeap(), 0, 2);
            if (res != Result.NoError)
                return _list;
            int NumEntries = (int)Marshal.ReadIntPtr(lpTable);
            lpTable = IntPtr.Zero;
            Marshal.FreeHGlobal(lpTable);
            BufferSize = (NumEntries * rowsize) + 4;
            lpTable = Marshal.AllocHGlobal(BufferSize);
            res = (Result)AllocateAndGetTcpExTableFromStack(ref lpTable, true, GetProcessHeap(), 0, 2);
            if (res != Result.NoError)
                return _list;
            IntPtr current = lpTable;
            NumEntries = (int)Marshal.ReadIntPtr(current);
            current = (IntPtr)((int)current + 4);
            for (int i = 0; i < NumEntries; i++)
            {
                Status stat = (Status)Marshal.ReadIntPtr(current);
                current = (IntPtr)((int)current + 4);
                IPEndPoint localAddress = new IPEndPoint((UInt32)Marshal.ReadIntPtr(current), (int)CommonFunctions.ConvertInt2PortNumber((UInt32)Marshal.ReadIntPtr((IntPtr)((int)current + 4))));
                current = (IntPtr)((int)current + 8);
                IPEndPoint remoteAddress = new IPEndPoint((UInt32)Marshal.ReadIntPtr(current), (((UInt32)Marshal.ReadIntPtr(current) != 0) ? (int)CommonFunctions.ConvertInt2PortNumber((UInt32)Marshal.ReadIntPtr((IntPtr)((int)current + 4))) : 0));
                current = (IntPtr)((int)current + 8);
                int PId = (int)Marshal.ReadIntPtr(current);
                current = (IntPtr)((int)current + 4);
                _list.Add(new WindowsConnection(
                                        stat,
                                        localAddress,
                                        remoteAddress,
                                        PId
                                        ));
            }
            Marshal.FreeHGlobal(lpTable);
            current = IntPtr.Zero;
            return _list;
        }

        private static List<Platform.ConnectionInfo> GetEXUdpConnectionsV1()
        {
            List<Platform.ConnectionInfo> _list = new List<Platform.ConnectionInfo>();
            int rowsize = 12;
            int BufferSize = 100000;
            IntPtr lpTable = Marshal.AllocHGlobal(BufferSize);
            Result res = (Result)AllocateAndGetUdpExTableFromStack(ref lpTable, true, GetProcessHeap(), 0, 2);
            if (res != Result.NoError)
                return _list;
            int NumEntries = (int)Marshal.ReadIntPtr(lpTable);
            lpTable = IntPtr.Zero;
            Marshal.FreeHGlobal(lpTable);
            BufferSize = (NumEntries * rowsize) + 4;
            lpTable = Marshal.AllocHGlobal(BufferSize);
            res = (Result)AllocateAndGetUdpExTableFromStack(ref lpTable, true, GetProcessHeap(), 0, 2);
            if (res != Result.NoError)
                return _list;
            IntPtr current = lpTable;
            NumEntries = (int)Marshal.ReadIntPtr(current);
            current = (IntPtr)((int)current + 4);
            for (int i = 0; i < NumEntries; i++)
            {
                IPEndPoint localAddress = new IPEndPoint((UInt32)Marshal.ReadIntPtr(current), (int)CommonFunctions.ConvertInt2PortNumber((UInt32)Marshal.ReadIntPtr((IntPtr)((int)current + 4))));
                current = (IntPtr)((int)current + 8);
                int PId = (int)Marshal.ReadIntPtr(current);
                current = (IntPtr)((int)current + 4);
                _list.Add(new WindowsConnection(
                                        localAddress,
                                        PId
                                        ));
            }
            Marshal.FreeHGlobal(lpTable);
            current = IntPtr.Zero;
            return _list;
        }

        private class CommonFunctions
        {
            private static Dictionary<int, Process> processCache = new Dictionary<int, Process>();
            [System.Diagnostics.DebuggerStepThrough]
            public static UInt16 ConvertInt2PortNumber(UInt32 dwPort)
            {
                byte[] b = new Byte[2];
                b[0] = byte.Parse((dwPort >> 8).ToString());
                b[1] = byte.Parse((dwPort & 0xFF).ToString());
                return BitConverter.ToUInt16(b, 0);
            }

            [System.Diagnostics.DebuggerStepThrough]
            public static int Bytes2Int(byte[] buffer, ref int nOffset)
            {
                int res = (((int)buffer[nOffset])) + (((int)buffer[nOffset + 1]) << 8) +
                    (((int)buffer[nOffset + 2]) << 16) + (((int)buffer[nOffset + 3]) << 24);
                nOffset += 4;
                return res;
            }

            [System.Diagnostics.DebuggerStepThrough]
            public static IPEndPoint Bytes2IPEndPoint(byte[] buffer, ref int nOffset, bool IsRemote)
            {
                //address
                Int64 m_Address = ((((buffer[nOffset + 3] << 0x18) | (buffer[nOffset + 2] << 0x10)) | (buffer[nOffset + 1] << 8)) | buffer[nOffset]) & ((long)0xffffffff);
                nOffset += 4;
                int m_Port = 0;
                m_Port = (IsRemote && (m_Address == 0)) ? 0 :
                            (((int)buffer[nOffset]) << 8) + (((int)buffer[nOffset + 1])) + (((int)buffer[nOffset + 2]) << 24) + (((int)buffer[nOffset + 3]) << 16);
                nOffset += 4;

                // store the remote endpoint
                IPEndPoint temp = new IPEndPoint(m_Address, m_Port);
                if (temp == null)
                    Debug.WriteLine("Parsed address is null. Addr=" + m_Address.ToString() + " Port=" + m_Port + " IsRemote=" + IsRemote.ToString());
                return temp;
            }

            [System.Diagnostics.DebuggerStepThrough]
            public static string GetProcessNameByPId(int PId)
            {
                try
                {
                    Process p = null;
                    if (processCache.ContainsKey(PId))
                        p = processCache[PId];
                    else
                    {
                        processCache.Clear();
                        Process[] pr = Process.GetProcesses();
                        foreach (Process process in pr)
                        {
                            processCache.Add(process.Id, process);
                            if (process.Id == PId)
                                p = process;
                        }
                    }

                    if (p != null)
                    {
                        return p.ProcessName;
                    }
                }
                catch (Exception) { }
                return string.Empty;
            }
        }
    }
}
