// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionInfo.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The connection info.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Platform
{
    #region

    using System.Collections.Generic;
    using System.Net;

    #endregion

    /// <summary>
    ///     The connection info.
    /// </summary>
    public abstract class ConnectionInfo : ClassRegistry.PlatformDependentClassBaseType
    {
        #region Enums

        /// <summary>
        ///     The protocol.
        /// </summary>
        public enum Protocol
        {
            /// <summary>
            ///     The tcp.
            /// </summary>
            Tcp, 

            /// <summary>
            ///     The udp.
            /// </summary>
            Udp
        }

        /// <summary>
        ///     The status.
        /// </summary>
        public enum Status
        {
            /// <summary>
            ///     The tcp closed.
            /// </summary>
            TcpClosed = 1, 

            /// <summary>
            ///     The tcp listen.
            /// </summary>
            TcpListen = 2, 

            /// <summary>
            ///     The tcp syn sent.
            /// </summary>
            TcpSynSent = 3, 

            /// <summary>
            ///     The tcp syn received.
            /// </summary>
            TcpSynReceived = 4, 

            /// <summary>
            ///     The tcp established.
            /// </summary>
            TcpEstablished = 5, 

            /// <summary>
            ///     The tcp fin wait 1.
            /// </summary>
            TcpFinWait1 = 6, 

            /// <summary>
            ///     The tcp fin wait 2.
            /// </summary>
            TcpFinWait2 = 7, 

            /// <summary>
            ///     The tcp close wait.
            /// </summary>
            TcpCloseWait = 8, 

            /// <summary>
            ///     The tcp closing.
            /// </summary>
            TcpClosing = 9, 

            /// <summary>
            ///     The tcp last acknowledge.
            /// </summary>
            TcpLastAcknowledge = 10, 

            /// <summary>
            ///     The tcp time wait.
            /// </summary>
            TcpTimeWait = 11, 

            /// <summary>
            ///     The tcp delete tcb.
            /// </summary>
            TcpDeleteTcb = 12
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the local address.
        /// </summary>
        public IPEndPoint LocalAddress { get; protected set; }

        /// <summary>
        ///     Gets or sets the process id.
        /// </summary>
        public int ProcessId { get; protected set; }

        /// <summary>
        ///     Gets or sets the process name.
        /// </summary>
        public string ProcessName { get; protected set; }


        /// <summary>
        ///     Gets or sets the process path.
        /// </summary>
        public string ProcessPath { get; protected set; }

        /// <summary>
        ///     Gets or sets the protocol type.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        protected Protocol ProtocolType { get; set; }

        /// <summary>
        ///     Gets or sets the remote address.
        /// </summary>
        protected IPEndPoint RemoteAddress { get; set; }

        /// <summary>
        ///     Gets or sets the state.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        protected Status State { get; set; }

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
        public abstract ConnectionInfo GetTcpConnectionByLocalAddress(IPAddress ip, ushort port);

        /// <summary>
        ///     The get tcp connections.
        /// </summary>
        /// <returns>
        ///     The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public abstract List<ConnectionInfo> GetTcpConnections();

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
        public abstract ConnectionInfo GetUdpConnectionByLocalAddress(IPAddress ip, ushort port);

        /// <summary>
        ///     The get udp connections.
        /// </summary>
        /// <returns>
        ///     The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public abstract List<ConnectionInfo> GetUdpConnections();

        /// <summary>
        ///     The is supported.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public abstract bool IsSupported();

        #endregion
    }
}