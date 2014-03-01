// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Guest.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The injected code.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.Network.Hook
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    using EasyHook;

    #endregion

    /// <summary>
    ///     The injected code.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class Guest : IEntryPoint
    {
        #region Constants

        private const string TextToAdd = " [ PeaRoxy: Active ]";

        #endregion

        #region Fields

        private readonly Regex dnsGrabbeRegex;

        private readonly Dictionary<string, string> dnsReverseDictionary = new Dictionary<string, string>();

        private readonly Dictionary<IntPtr, GuestSocketInfo> dnsSocketDictionary =
            new Dictionary<IntPtr, GuestSocketInfo>();

        private readonly byte fakeIpSupLevel = 240;

        private readonly List<LocalHook> hooks = new List<LocalHook>();

        private readonly Host.Remote hostConnector;

        private readonly int hostProcessId;

        private readonly bool isDebug;

        private readonly List<IntPtr> windows = new List<IntPtr>();

        private uint lastFakeIp = uint.MaxValue;

        #endregion

        #region Constructors and Destructors

        // ReSharper disable once UnusedParameter.Local
        public Guest(RemoteHooking.IContext inContext, string inChannelName)
        {
            try
            {
                this.hostConnector = RemoteHooking.IpcConnectClient<Host.Remote>(inChannelName);
                this.hostProcessId = Process.GetCurrentProcess().Id;
                this.isDebug = this.hostConnector.IsDebugEnable();
                this.fakeIpSupLevel = this.hostConnector.GetFakeIpSupLevel();
                string dnsGrabbePattern = this.hostConnector.GetDnsGrabberPattern();
                if (!string.IsNullOrEmpty(dnsGrabbePattern))
                {
                    this.dnsGrabbeRegex = new Regex(dnsGrabbePattern);
                }
            }
            catch (Exception e)
            {
                this.ReportInternalException(e.ToString());
            }
        }

        #endregion

        #region Public Methods and Operators

        public LocalHook AddHook(string libName, string entryPoint, Delegate inNewProc)
        {
            LocalHook lh = LocalHook.Create(LocalHook.GetProcAddress(libName, entryPoint), inNewProc, this);
            lh.ThreadACL.SetExclusiveACL(new[] { 0 });
            return lh;
        }

        public void ChangeAllWindowsCaptionToDeActive()
        {
            if (TextToAdd != string.Empty)
            {
                foreach (IntPtr window in this.windows)
                {
                    try
                    {
                        if (window == IntPtr.Zero || !Unmanaged.IsWindow(window))
                        {
                            continue;
                        }

                        int length = Unmanaged.GetWindowTextLength(window);
                        StringBuilder sb = new StringBuilder(length + 1);
                        Unmanaged.GetWindowText(window, sb, sb.Capacity);
                        string title = sb.ToString();
                        if (title != string.Empty && title.Contains(TextToAdd))
                        {
                            Unmanaged.SetWindowText(window, title.Replace(TextToAdd, ""));
                        }
                    }
                    catch (Exception e)
                    {
                        this.ReportInternalException(e.ToString());
                    }
                }
            }
        }

        public void ChangeMainWindowCaptionToActive()
        {
            try
            {
                if (TextToAdd != string.Empty)
                {
                    Process currentProcess = Process.GetCurrentProcess();
                    currentProcess.WaitForInputIdle();
                    IntPtr mainWindowHandler = currentProcess.MainWindowHandle;
                    if (mainWindowHandler == IntPtr.Zero || currentProcess.MainWindowTitle == string.Empty
                        || currentProcess.MainWindowTitle.Contains(TextToAdd))
                    {
                        return;
                    }
                    bool success = Unmanaged.SetWindowText(
                        mainWindowHandler,
                        currentProcess.MainWindowTitle + TextToAdd);
                    if (success && !this.windows.Contains(mainWindowHandler))
                    {
                        this.windows.Add(mainWindowHandler);
                    }
                }
            }
            catch (Exception e)
            {
                this.ReportInternalException(e.ToString());
            }
        }

        public void Run(RemoteHooking.IContext inContext, string inChannelName)
        {
            try
            {
                try
                {
                    IntPtr wsa2Library = Unmanaged.LoadLibrary("ws2_32.dll");
                    this.hooks.Add(this.AddHook("ws2_32.dll", "connect", new Unmanaged.ConnectDelegate(this.Connect)));
                    this.hooks.Add(this.AddHook("ws2_32.dll", "send", new Unmanaged.SendDelegate(this.Send)));
                    this.hooks.Add(this.AddHook("ws2_32.dll", "WSASend", new Unmanaged.WsaSendDelegate(this.WsaSend)));
                    this.hooks.Add(
                        this.AddHook(
                            "ws2_32.dll",
                            "gethostbyname",
                            new Unmanaged.GetHostByNameDelegate(this.GetHostByName)));
                    this.hooks.Add(
                        this.AddHook(
                            "ws2_32.dll",
                            "GetAddrInfoW",
                            (Unmanaged.GetAddressInfoDelegate)
                            ((IntPtr a, IntPtr b, IntPtr c, out IntPtr d) => this.GetAddressInfo(a, b, c, out d, true))));
                    this.hooks.Add(
                        this.AddHook(
                            "ws2_32.dll",
                            "getaddrinfo",
                            (Unmanaged.GetAddressInfoDelegate)
                            ((IntPtr a, IntPtr b, IntPtr c, out IntPtr d) => this.GetAddressInfo(a, b, c, out d, false))));
                    if (!wsa2Library.Equals(IntPtr.Zero))
                    {
                        Unmanaged.FreeLibrary(wsa2Library);
                    }
                }
                catch (Exception e)
                {
                    this.ReportMessage(e.ToString());
                    this.ReportMessage("Failed to hook");
                    return;
                }

                this.ReportMessage("Hooked successfully");
                while (true)
                {
                    Thread.Sleep(100);
                    this.ChangeMainWindowCaptionToActive();
                    this.hostConnector.Ping(this.hostProcessId);
                }
            }
            catch (Exception e)
            {
                this.ReportInternalException(e.ToString());
            }
            foreach (LocalHook hook in hooks)
            {
                hook.Dispose();
            }
            hooks.Clear();
            this.ChangeAllWindowsCaptionToDeActive();
        }

        #endregion

        #region Methods

        private Unmanaged.ConnectStatus Connect(IntPtr socket, ref Unmanaged.SocketAddressIn address, int addressSize)
        {
            try
            {
                IPEndPoint proxyEndPoint = this.hostConnector.GetProxyEndPoint(this.hostProcessId);

                bool isActive = address.Family == AddressFamily.InterNetwork && proxyEndPoint != null
                                && !address.IPAddress.IpAddress.Equals(proxyEndPoint.Address)
                                && !address.IPAddress.IpAddress.Equals(IPAddress.Loopback);
                if (isActive)
                {
                    string internetIpAddress = address.IPAddress.IpAddress.ToString();
                    string hostName = (this.dnsReverseDictionary.ContainsKey(internetIpAddress)
                                           ? this.dnsReverseDictionary[internetIpAddress]
                                           : internetIpAddress);
                    GuestSocketInfo socketInfo = new GuestSocketInfo
                                                     {
                                                         HostName = hostName,
                                                         NoSendYet = true,
                                                         Port = (uint)address.Port,
                                                         Socket = socket
                                                     };
                    if (!this.dnsSocketDictionary.ContainsKey(socket))
                    {
                        this.dnsSocketDictionary.Add(socket, socketInfo);
                    }
                    else
                    {
                        this.dnsSocketDictionary[socket] = socketInfo;
                    }

                    address.IPAddress.IpAddress = proxyEndPoint.Address;
                    address.Port = proxyEndPoint.Port;

                    this.ReportMessage("Connect: " + socketInfo.HostName);
                }
            }
            catch (Exception e)
            {
                this.ReportMessage(e.ToString());
            }

            Unmanaged.ConnectStatus returnValue = Unmanaged.WS2_Connect(socket, ref address, addressSize);
            if (returnValue == Unmanaged.ConnectStatus.Error
                && (Unmanaged.WSAGetLastError() == SocketError.WouldBlock
                    || Unmanaged.WSAGetLastError() == SocketError.Success))
            {
                // Non blocking mode
                returnValue = Unmanaged.ConnectStatus.Ok;
            }

            return returnValue;
        }

        private IPAddress GenerateInternalIpAddress()
        {
            if (this.lastFakeIp >= (this.fakeIpSupLevel * 0x01000000U) + 0x0000FFFFU)
            {
                this.lastFakeIp = this.fakeIpSupLevel * 0x01000000U;
            }

            this.lastFakeIp += 1;
            return
                new IPAddress(
                    (this.lastFakeIp & 0x000000FFU) << 24 | (this.lastFakeIp & 0x0000FF00U) << 8
                    | (this.lastFakeIp & 0x00FF0000U) >> 8 | (this.lastFakeIp & 0xFF000000U) >> 24);
        }

        private SocketError GetAddressInfo(
            IntPtr nodeName,
            IntPtr serviceName,
            IntPtr hints,
            out IntPtr results,
            bool isUnicode)
        {
            SocketError returnValue = isUnicode
                                          ? Unmanaged.GetAddressInfoUni(nodeName, serviceName, hints, out results)
                                          : Unmanaged.GetAddressInfo(nodeName, serviceName, hints, out results);

            try
            {
                string hostname = isUnicode ? Marshal.PtrToStringUni(nodeName) : Marshal.PtrToStringAnsi(nodeName);
                List<IPAddress> internetIpAddresses = new List<IPAddress>();
                if (returnValue == SocketError.Success || returnValue == SocketError.HostNotFound)
                {
                    if (returnValue == SocketError.Success)
                    {
                        IntPtr handle = new IntPtr(results.ToInt64());
                        do
                        {
                            Unmanaged.AddressInfo addressInfo =
                                (Unmanaged.AddressInfo)Marshal.PtrToStructure(handle, typeof(Unmanaged.AddressInfo));

                            if (addressInfo.Family == AddressFamily.InterNetwork && addressInfo.AddressLen >= 8)
                            {
                                Unmanaged.SocketAddressIn socketAddress =
                                    (Unmanaged.SocketAddressIn)
                                    Marshal.PtrToStructure(addressInfo.Address, typeof(Unmanaged.SocketAddressIn));

                                if (this.dnsGrabbeRegex != null
                                    && this.dnsGrabbeRegex.IsMatch(socketAddress.IPAddress.IpAddress.ToString()))
                                {
                                    internetIpAddresses.Clear();
                                    break;
                                }
                                internetIpAddresses.Add(socketAddress.IPAddress.IpAddress);
                            }
                            handle = addressInfo.Next;
                        }
                        while (handle != IntPtr.Zero);
                    }

                    if (internetIpAddresses.Count == 0)
                    {
                        internetIpAddresses.Add(this.GenerateInternalIpAddress());
                        if (isUnicode)
                        {
                            if (!results.Equals(IntPtr.Zero))
                            {
                                Unmanaged.FreeAddressInfoUni(results);
                            }
                            returnValue =
                                Unmanaged.GetAddressInfoUni(
                                    Marshal.StringToHGlobalUni(internetIpAddresses[0].ToString()),
                                    serviceName,
                                    hints,
                                    out results);
                        }
                        else
                        {
                            if (!results.Equals(IntPtr.Zero))
                            {
                                Unmanaged.FreeAddressInfo(results);
                            }
                            returnValue =
                                Unmanaged.GetAddressInfo(
                                    Marshal.StringToHGlobalAnsi(internetIpAddresses[0].ToString()),
                                    serviceName,
                                    hints,
                                    out results);
                        }
                    }
                    if (internetIpAddresses.Count > 0)
                    {
                        foreach (IPAddress internetIpAddress in internetIpAddresses)
                        {
                            if (this.dnsReverseDictionary.ContainsKey(internetIpAddress.ToString()))
                            {
                                this.dnsReverseDictionary[internetIpAddress.ToString()] = hostname;
                            }
                            else
                            {
                                this.dnsReverseDictionary.Add(internetIpAddress.ToString(), hostname);
                            }

                            this.ReportMessage("GetAddressInfo: " + hostname + " -> " + internetIpAddress);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.ReportMessage(e.ToString());
            }

            return returnValue;
        }

        private IntPtr GetHostByName(string host)
        {
            IntPtr returnValue = Unmanaged.GetHostByName(host);
            try
            {
                List<IPAddress> internetIpAddresses = new List<IPAddress>();
                if (returnValue != IntPtr.Zero)
                {
                    IPHostEntry results = Unmanaged.NativeIpHostEntry.FromNative(returnValue);
                    foreach (IPAddress ipAddress in results.AddressList)
                    {
                        if (this.dnsGrabbeRegex != null && this.dnsGrabbeRegex.IsMatch(ipAddress.ToString()))
                        {
                            internetIpAddresses.Clear();
                            break;
                        }
                        internetIpAddresses.Add(ipAddress);
                    }
                }
                if (internetIpAddresses.Count == 0)
                {
                    internetIpAddresses.Add(this.GenerateInternalIpAddress());
                    returnValue = Unmanaged.GetHostByName(internetIpAddresses[0].ToString());
                }
                if (internetIpAddresses.Count > 0)
                {
                    foreach (IPAddress internetIpAddress in internetIpAddresses)
                    {
                        if (this.dnsReverseDictionary.ContainsKey(internetIpAddress.ToString()))
                        {
                            this.dnsReverseDictionary[internetIpAddress.ToString()] = host;
                        }
                        else
                        {
                            this.dnsReverseDictionary.Add(internetIpAddress.ToString(), host);
                        }
                        this.ReportMessage("GetHostByName: " + host + " -> " + internetIpAddress);
                    }
                }
            }
            catch (Exception e)
            {
                this.ReportMessage(e.ToString());
            }

            return returnValue;
        }

        private void ReportInternalException(string p)
        {
            try
            {
                if (!this.isDebug)
                {
                    return;
                }

                File.AppendAllText(
                    Path.Combine(Path.GetTempPath(), "PeaRoxy.Windows.Network.Hook-" + hostProcessId + "-ErrorLog.log"),
                    string.Format("{0}{1}{2}{1}", new String('-', 30), Environment.NewLine, p));
            }
            catch
            {
            }
        }

        private void ReportMessage(string str)
        {
            try
            {
                if (this.hostConnector != null)
                {
                    this.hostConnector.ReportMessage(this.hostProcessId, str);
                }
            }
            catch (Exception e)
            {
                this.ReportInternalException(e.ToString());
            }
        }

        private int Send(IntPtr socket, IntPtr buffer, int len, SocketFlags flags)
        {
            try
            {
                if (this.dnsSocketDictionary.ContainsKey(socket) && this.dnsSocketDictionary[socket].NoSendYet)
                {
                    lock (this.dnsSocketDictionary)
                    {
                        if (this.dnsSocketDictionary[socket].NoSendYet)
                        {
                            this.dnsSocketDictionary[socket].NoSendYet = false;
                            byte[] connectBuf =
                                Encoding.ASCII.GetBytes(
                                    "FASTCONNECT " + this.dnsSocketDictionary[socket].HostName + ":"
                                    + this.dnsSocketDictionary[socket].Port + "\r\n\r\n");
                            GCHandle pinnedArray = GCHandle.Alloc(connectBuf, GCHandleType.Pinned);

                            Unmanaged.Send(
                                socket,
                                pinnedArray.AddrOfPinnedObject(),
                                connectBuf.Length,
                                SocketFlags.None);
                            pinnedArray.Free();
                            this.ReportMessage("Send: " + len);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.ReportMessage(e.ToString());
            }
            return Unmanaged.Send(socket, buffer, len, flags);
        }

        private SocketError WsaSend(
            IntPtr socket,
            IntPtr buffer,
            int len,
            out IntPtr numberOfBytesSent,
            SocketFlags flags,
            IntPtr overlapped,
            IntPtr completionRoutine)
        {
            try
            {
                if (this.dnsSocketDictionary.ContainsKey(socket) && this.dnsSocketDictionary[socket].NoSendYet)
                {
                    lock (this.dnsSocketDictionary)
                    {
                        if (this.dnsSocketDictionary[socket].NoSendYet)
                        {
                            this.dnsSocketDictionary[socket].NoSendYet = false;
                            Unmanaged.WsaBuffer sendBuffer =
                                Unmanaged.WsaBuffer.FromString(
                                    "FASTCONNECT " + this.dnsSocketDictionary[socket].HostName + ":"
                                    + this.dnsSocketDictionary[socket].Port + "\r\n\r\n");

                            IntPtr bufferPointer = Marshal.AllocHGlobal(Marshal.SizeOf(sendBuffer));
                            Marshal.StructureToPtr(sendBuffer, bufferPointer, true);

                            IntPtr o;
                            SocketError error = Unmanaged.Send(
                                socket,
                                bufferPointer,
                                1,
                                out o,
                                SocketFlags.None,
                                IntPtr.Zero,
                                IntPtr.Zero);

                            this.ReportMessage("WSASend: " + len + " - Result: " + error);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.ReportMessage(e.ToString());
            }
            return Unmanaged.Send(socket, buffer, len, out numberOfBytesSent, flags, overlapped, completionRoutine);
        }

        #endregion
    }
}