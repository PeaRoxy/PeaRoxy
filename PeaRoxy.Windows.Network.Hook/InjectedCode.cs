// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InjectedCode.cs" company="PeaRoxy.com">
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
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    using EasyHook;

    #endregion

    /// <summary>
    /// The injected code.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class InjectedCode : IEntryPoint
    {
        #region Fields

        /// <summary>
        /// The DNS reverse array.
        /// </summary>
        private readonly Dictionary<string, string> dnsReverseArray = new Dictionary<string, string>();

        /// <summary>
        /// The parent.
        /// </summary>
        private readonly RemoteParent parent;

        /// <summary>
        /// The WS2 get address info unicode handler.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private LocalHook ws2GetAddressInfoUniHandler;

        /// <summary>
        /// The WS2 get address info handler.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private LocalHook ws2GetAddressInfoHandler;

        /// <summary>
        /// The WS2 connect handler.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private LocalHook ws2ConnectHandler;

        /// <summary>
        /// The WS2 get host by name handler.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private LocalHook ws2GetHostByNameHandler;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectedCode"/> class.
        /// </summary>
        /// <param name="inContext">
        /// The in context.
        /// </param>
        /// <param name="inChannelName">
        /// The in channel name.
        /// </param>
        // ReSharper disable once UnusedParameter.Local
        public InjectedCode(RemoteHooking.IContext inContext, string inChannelName)
        {
            try
            {
                this.parent = RemoteHooking.IpcConnectClient<RemoteParent>(inChannelName);
                this.parent.Ping();
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add hook.
        /// </summary>
        /// <param name="libName">
        /// The lib name.
        /// </param>
        /// <param name="entryPoint">
        /// The entry point.
        /// </param>
        /// <param name="inNewProc">
        /// The in new procedure.
        /// </param>
        /// <returns>
        /// The <see cref="LocalHook"/>.
        /// </returns>
        public LocalHook AddHook(string libName, string entryPoint, Delegate inNewProc)
        {
            try
            {
                LocalHook lh = LocalHook.Create(LocalHook.GetProcAddress(libName, entryPoint), inNewProc, this);
                lh.ThreadACL.SetExclusiveACL(new[] { 0 });
                return lh;
            }
            catch (Exception e)
            {
                this.parent.ReportException(e);
            }

            return null;
        }

        /// <summary>
        /// The run.
        /// </summary>
        /// <param name="inContext">
        /// The in context.
        /// </param>
        /// <param name="inChannelName">
        /// The in channel name.
        /// </param>
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedParameter.Global
        public void Run(RemoteHooking.IContext inContext, string inChannelName)
        {
            this.ws2ConnectHandler = this.AddHook(
                "ws2_32.dll", 
                "connect", 
                new Unmanaged.ConnectDelegate(this.Ws2Connect));

            this.ws2GetHostByNameHandler = this.AddHook(
                "ws2_32.dll", 
                "gethostbyname", 
                new Unmanaged.GetHostByNameDelegate(this.Ws2GetHostByName));
            this.ws2GetAddressInfoUniHandler = this.AddHook(
                "ws2_32.dll",
                "GetAddrInfoW",
                (Unmanaged.GetAddressInfoDelegate)((IntPtr a, IntPtr b, IntPtr c, out IntPtr d) => this.Ws2GetAddressInfo(a, b, c, out d, true)));
            this.ws2GetAddressInfoHandler = this.AddHook(
                "ws2_32.dll",
                "getaddrinfo",
                (Unmanaged.GetAddressInfoDelegate)((IntPtr a, IntPtr b, IntPtr c, out IntPtr d) => this.Ws2GetAddressInfo(a, b, c, out d, false)));
            try
            {
                this.parent.IsInstalled(inContext.HostPID);
                while (true)
                {
                    Thread.Sleep(100);
                    this.parent.Ping();
                }
            }
            catch
            {
                // NET Remoting will raise an exception if host is unreachable
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The w s 2_ connect.
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
        /// The <see cref="Unmanaged.ConnectStatus"/>.
        /// </returns>
        private Unmanaged.ConnectStatus Ws2Connect(
            IntPtr socket, 
            ref Unmanaged.SocketAddressIn address, 
            int addressSize)
        {
            if (this.parent != null)
            {
                this.parent.LogToScreen("WS2_Connect: " + address.Family);
            }

            string hostNameAndPort = string.Empty;
            try
            {
                if (address.Family == AddressFamily.InterNetwork)
                {
                    string internetIpAddress = string.Format(
                        "{0}.{1}.{2}.{3}",
                        address.IPAddress.Byte1,
                        address.IPAddress.Byte2,
                        address.IPAddress.Byte3,
                        address.IPAddress.Byte4);
                    hostNameAndPort = (this.dnsReverseArray.ContainsKey(internetIpAddress)
                                           ? this.dnsReverseArray[internetIpAddress]
                                           : internetIpAddress) + ":" + address.Port;

                    address.IPAddress.Byte1 = 127;
                    address.IPAddress.Byte2 = 0;
                    address.IPAddress.Byte3 = 0;
                    address.IPAddress.Byte4 = 1;
                    address.Port = 1080;

                    if (this.parent != null)
                    {
                        this.parent.LogToScreen("WS2_Connect: " + hostNameAndPort);
                    }
                }
            }
            catch (Exception e)
            {
                if (this.parent != null) this.parent.ReportException(e);
            }

            Unmanaged.ConnectStatus returnValue = Unmanaged.WS2_Connect(socket, ref address, addressSize);
            if (returnValue == Unmanaged.ConnectStatus.Error
                && (Unmanaged.WS2_GetLastError() == SocketError.WouldBlock
                    || Unmanaged.WS2_GetLastError() == SocketError.Success))
            {
                // Non blocking mode
                returnValue = Unmanaged.ConnectStatus.Ok;
            }

            try
            {
                if (address.Family == AddressFamily.InterNetwork)
                {
                    byte[] connectBuf = Encoding.ASCII.GetBytes("FASTCONNECT " + hostNameAndPort + "\r\n\r\n");
                    GCHandle pinnedArray = GCHandle.Alloc(connectBuf, GCHandleType.Pinned);
                    Unmanaged.WS2_Send(socket, pinnedArray.AddrOfPinnedObject(), connectBuf.Length, SocketFlags.None);
                    pinnedArray.Free();
                }
            }
            catch (Exception e)
            {
                if (this.parent != null) this.parent.ReportException(e);
            }

            return returnValue;
        }

        /// <summary>
        /// The w s 2_ get address info.
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
        /// <param name="isUnicode">
        /// The is unicode.
        /// </param>
        /// <returns>
        /// The <see cref="SocketError"/>.
        /// </returns>
        private SocketError Ws2GetAddressInfo(
            IntPtr nodeName, 
            IntPtr serviceName, 
            IntPtr hints, 
            out IntPtr results, 
            bool isUnicode)
        {
            SocketError returnValue = isUnicode
                                          ? Unmanaged.WS2_GetAddressInfoUni(nodeName, serviceName, hints, out results)
                                          : Unmanaged.WS2_GetAddressInfo(nodeName, serviceName, hints, out results);

            try
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
                            string internetIpAddress = string.Format(
                                "{0}.{1}.{2}.{3}",
                                Marshal.ReadByte(addressInfo.Address + 4),
                                Marshal.ReadByte(addressInfo.Address + 5),
                                Marshal.ReadByte(addressInfo.Address + 6),
                                Marshal.ReadByte(addressInfo.Address + 7));
                            string hostname = isUnicode
                                                  ? Marshal.PtrToStringUni(nodeName)
                                                  : Marshal.PtrToStringAnsi(nodeName);
                            if (this.dnsReverseArray.ContainsKey(internetIpAddress))
                            {
                                this.dnsReverseArray[internetIpAddress] = hostname;
                            }
                            else
                            {
                                this.dnsReverseArray.Add(internetIpAddress, hostname);
                            }

                            if (this.parent != null)
                            {
                                this.parent.LogToScreen("WS2_GetAddressInfo: " + hostname + " -> " + internetIpAddress);
                            }
                        }

                        handle = addressInfo.Next;
                    }
                    while (handle != IntPtr.Zero);
                }
            }
            catch (Exception e)
            {
                if (this.parent != null) this.parent.ReportException(e);
            }

            return returnValue;
        }

        /// <summary>
        /// The w s 2_ get host by name.
        /// </summary>
        /// <param name="host">
        /// The host.
        /// </param>
        /// <returns>
        /// The <see cref="IntPtr"/>.
        /// </returns>
        private IntPtr Ws2GetHostByName(string host)
        {
            IntPtr returnValue = Unmanaged.WS2_GetHostByName(host);
            try
            {
                if (returnValue != IntPtr.Zero)
                {
                    IPHostEntry results = Unmanaged.NativeIpHostEntry.FromNative(returnValue);
                    foreach (IPAddress ip in results.AddressList)
                    {
                        if (this.dnsReverseArray.ContainsKey(ip.ToString()))
                        {
                            this.dnsReverseArray[ip.ToString()] = results.HostName;
                        }
                        else
                        {
                            this.dnsReverseArray.Add(ip.ToString(), results.HostName);
                        }

                        if (this.parent != null)
                        {
                            this.parent.LogToScreen("WS2_GetHostByName: " + results.HostName + " -> " + ip);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (this.parent != null) this.parent.ReportException(e);
            }

            return returnValue;
        }

        #endregion
    }
}