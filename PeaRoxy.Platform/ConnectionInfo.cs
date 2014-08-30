// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionInfo.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Platform
{
    using System.Collections.Generic;
    using System.Net;

    /// <summary>
    ///     This class is base class of all platform dependent classes about connections
    /// </summary>
    public abstract class ConnectionInfo : ClassRegistry.PlatformDependentClassBaseType
    {
        /// <summary>
        ///     The protocol types.
        /// </summary>
        public enum ProtocolTypes
        {
            Tcp,

            Udp
        }

        /// <summary>
        ///     The states.
        /// </summary>
        public enum States
        {
            TcpClosed = 1,

            TcpListen = 2,

            TcpSynSent = 3,

            TcpSynReceived = 4,

            TcpEstablished = 5,

            TcpFinWait1 = 6,

            TcpFinWait2 = 7,

            TcpCloseWait = 8,

            TcpClosing = 9,

            TcpLastAcknowledge = 10,

            TcpTimeWait = 11,

            TcpDeleteTcb = 12
        }

        /// <summary>
        ///     Gets or sets the local end point of the connection.
        /// </summary>
        public IPEndPoint LocalAddress { get; protected set; }

        /// <summary>
        ///     Gets or sets the PID of the process creating the connection.
        /// </summary>
        public int ProcessId { get; protected set; }

        /// <summary>
        ///     Gets or sets the creating process's name.
        /// </summary>
        public string ProcessName { get; protected set; }

        /// <summary>
        ///     Gets or sets the creating process's main executable file path.
        /// </summary>
        public string ProcessPath { get; protected set; }

        /// <summary>
        ///     Gets or sets the protocol type.
        /// </summary>
        public ProtocolTypes ProtocolType { get; set; }

        /// <summary>
        ///     Gets or sets the remote end point.
        /// </summary>
        public IPEndPoint RemoteAddress { get; set; }

        /// <summary>
        ///     Gets or sets the state of the connection.
        /// </summary>
        public States State { get; set; }

        /// <summary>
        ///     Give more information about a TCP connection to specific IP address and port.
        /// </summary>
        /// <param name="ip">
        ///     The local IP address of the connection.
        /// </param>
        /// <param name="port">
        ///     The local port number of the connection.
        /// </param>
        /// <returns>
        ///     The <see cref="ConnectionInfo" /> object contains the information about the connection.
        /// </returns>
        public abstract ConnectionInfo GetTcpConnectionByLocalAddress(IPAddress ip, ushort port);

        /// <summary>
        ///     Get a list of all TCP connections.
        /// </summary>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     of all connections.
        /// </returns>
        public abstract List<ConnectionInfo> GetTcpConnections();

        /// <summary>
        ///     Give more information about a UDP connection to specific IP address and port.
        /// </summary>
        /// <param name="ip">
        ///     The local IP address of the connection.
        /// </param>
        /// <param name="port">
        ///     The local port number of the connection.
        /// </param>
        /// <returns>
        ///     The <see cref="ConnectionInfo" /> object contains the information about the connection.
        /// </returns>
        public abstract ConnectionInfo GetUdpConnectionByLocalAddress(IPAddress ip, ushort port);

        /// <summary>
        ///     Get a list of all UDP connections.
        /// </summary>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     of all connections.
        /// </returns>
        public abstract List<ConnectionInfo> GetUdpConnections();

        /// <summary>
        ///     This method will shows if this functionality is supported in current environment/OS.
        /// </summary>
        /// <returns>
        ///     The result shows if we can use this class's in the current environment/OS.
        /// </returns>
        public abstract bool IsSupported();
    }
}