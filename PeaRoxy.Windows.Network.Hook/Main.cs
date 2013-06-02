using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using EasyHook;

namespace PeaRoxy.Windows.Network.Hook
{
    public class Main : EasyHook.IEntryPoint
    {
        private static TEST.HookInterface Interface;
        LocalHook connectHook, errorHook, error1Hook;
        private static Dictionary<int, int> globalError = new Dictionary<int,int>();
        public Main(RemoteHooking.IContext InContext, String InChannelName)
        {
            Interface = RemoteHooking.IpcConnectClient<TEST.HookInterface>(InChannelName);
            Interface.Ping();
        }

        public void Run(RemoteHooking.IContext InContext, String InChannelName)
        {
            try
            {
                connectHook = LocalHook.Create(LocalHook.GetProcAddress("ws2_32.dll", "connect"), new Dconnect(connect_Hooked), this);
                connectHook.ThreadACL.SetExclusiveACL(new Int32[1]);
                errorHook = LocalHook.Create(LocalHook.GetProcAddress("ws2_32.dll", "WSAGetLastError"), new DWSAGetLastError(WSAGetLastError_Hooked), this);
                errorHook.ThreadACL.SetExclusiveACL(new Int32[1]);
                error1Hook = LocalHook.Create(LocalHook.GetProcAddress("ws2_32.dll", "WSASetLastError"), new DWSASetLastError(WSASetLastError_Hooked), this);
                error1Hook.ThreadACL.SetExclusiveACL(new Int32[1]);
                Interface.IsInstalled(RemoteHooking.GetCurrentProcessId());
            }
            catch (Exception e)
            {
                Interface.ReportException(e);
                return;
            }
            try
            {
                while (true)
                {
                    Thread.Sleep(500);
                    Interface.Ping();
                }
            }
            catch
            {
                // NET Remoting will raise an exception if host is unreachable
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        public struct sockaddr_in
        {
            internal short sin_family;
            internal ushort sin_port;
            internal in_addr sin_addr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal byte[] sin_zero;
            [StructLayout(LayoutKind.Explicit, Size = 4)]
            public struct in_addr
            {
                [FieldOffset(0)]
                internal byte s_b1;
                [FieldOffset(1)]
                internal byte s_b2;
                [FieldOffset(2)]
                internal byte s_b3;
                [FieldOffset(3)]
                internal byte s_b4;

                [FieldOffset(0)]
                internal ushort s_w1;
                [FieldOffset(2)]
                internal ushort s_w2;

                [FieldOffset(0)]
                internal uint S_addr;
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        delegate int Dconnect(IntPtr socket, ref sockaddr_in address, int addressSize);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        delegate int DWSAGetLastError();

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        delegate void DWSASetLastError(int e); 

        [DllImport("ws2_32.dll", SetLastError = true)]
        static extern int connect(IntPtr socket, ref sockaddr_in address, int addressSize);

        [DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern Int32 WSAGetLastError();

        [DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern void WSASetLastError(int e);

        static void WSASetLastError_Hooked(int e)
        {
            globalError[RemoteHooking.GetCurrentThreadId()] = e;
        }

        static int WSAGetLastError_Hooked()
        {
            if (globalError.ContainsKey(RemoteHooking.GetCurrentThreadId()))
            {
                if (Interface != null)
                    Interface.LogToScreen("Error: " + globalError[RemoteHooking.GetCurrentThreadId()].ToString());
                return globalError[RemoteHooking.GetCurrentThreadId()];
            }
            return 0;
        }

        static int connect_Hooked(IntPtr socket, ref sockaddr_in address, int addressSize)
        {
            int ret = connect(socket, ref address, addressSize);
            globalError[RemoteHooking.GetCurrentThreadId()] = ret * -10035;
            if (Interface != null)
                Interface.LogToScreen(ret.ToString() + " - " + globalError[RemoteHooking.GetCurrentThreadId()].ToString() + " - " + address.sin_addr.s_b1.ToString() + "." + address.sin_addr.s_b2.ToString() + "." + address.sin_addr.s_b3.ToString() + "." + address.sin_addr.s_b4.ToString());
            return ret;
        }
    }
}
