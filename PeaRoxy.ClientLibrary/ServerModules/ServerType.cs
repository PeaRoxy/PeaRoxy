// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServerType.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary.ServerModules
{
    using System.Net.Sockets;

    /// <summary>
    ///     The server abstract class for modules
    /// </summary>
    public abstract class ServerType
    {
        /// <summary>
        ///     The connection callback delegate.
        /// </summary>
        /// <param name="success">
        ///     A boolean indicating success of the process
        /// </param>
        /// <param name="activeServer">
        ///     The current active server
        /// </param>
        /// <param name="client">
        ///     The related client
        /// </param>
        /// <returns>
        ///     Return a boolean indicating success of the process
        /// </returns>
        public delegate bool ConnectionCallbackDelegate(bool success, ServerType activeServer, ProxyClient client);

        /// <summary>
        ///     The data callback delegate.
        /// </summary>
        /// <param name="data">
        ///     The related data in bytes
        /// </param>
        /// <param name="activeServer">
        ///     The current active server
        /// </param>
        /// <param name="client">
        ///     The related client
        /// </param>
        /// <returns>
        ///     Return a boolean indicating success of the process
        /// </returns>
        public delegate bool DataCallbackDelegate(ref byte[] data, ServerType activeServer, ProxyClient client);

        /// <summary>
        ///     Gets or sets the connection status callback.
        /// </summary>
        public ConnectionCallbackDelegate ConnectionStatusCallback { get; protected internal set; }

        /// <summary>
        ///     Gets or sets a value indicating whether we had sent some data to the server
        /// </summary>
        public abstract bool IsDataSent { get; protected set; }

        /// <summary>
        ///     Gets or sets a value indicating whether we had a close request on the underlying connection
        /// </summary>
        public abstract bool IsClosed { get; protected set; }

        /// <summary>
        ///     Gets or sets a value indicating whether server exists and is valid.
        /// </summary>
        public abstract bool IsServerValid { get; protected set; }

        /// <summary>
        ///     Gets or sets the number of seconds after last data transmission to close the connection
        /// </summary>
        public abstract int NoDataTimeout { get; set; }

        /// <summary>
        ///     Gets the parent client.
        /// </summary>
        public abstract ProxyClient ParentClient { get; protected set; }

        /// <summary>
        ///     Gets or sets the data received delegate.
        /// </summary>
        public DataCallbackDelegate ReceiveDataDelegate { get; protected internal set; }

        /// <summary>
        ///     Gets or sets the data sent delegate.
        /// </summary>
        public DataCallbackDelegate SendDataDelegate { get; protected internal set; }

        /// <summary>
        ///     Gets the underlying socket to the server.
        /// </summary>
        public abstract Socket UnderlyingSocket { get; protected set; }

        /// <summary>
        ///     The clone method is to make another copy of this class with exactly same settings
        /// </summary>
        /// <returns>
        ///     The <see cref="ServerType" />.
        /// </returns>
        public abstract ServerType Clone();

        /// <summary>
        ///     The method to handle the routing process, should call repeatedly
        /// </summary>
        public abstract void Route();

        /// <summary>
        ///     The establish method is used to connect to the server and try to request access to the specific address.
        /// </summary>
        /// <param name="address">
        ///     The requested address.
        /// </param>
        /// <param name="port">
        ///     The requested port.
        /// </param>
        /// <param name="client">
        ///     The client to connect with.
        /// </param>
        /// <param name="headerData">
        ///     The data to send after connecting.
        /// </param>
        /// <param name="sendDataDelegate">
        ///     The send callback.
        /// </param>
        /// <param name="receiveDataDelegate">
        ///     The receive callback.
        /// </param>
        /// <param name="connectionStatusCallback">
        ///     The connection status changed callback.
        /// </param>
        public abstract void Establish(
            string address,
            ushort port,
            ProxyClient client,
            byte[] headerData = null,
            DataCallbackDelegate sendDataDelegate = null,
            DataCallbackDelegate receiveDataDelegate = null,
            ConnectionCallbackDelegate connectionStatusCallback = null);

        /// <summary>
        ///     Get the latest requested address
        /// </summary>
        /// <returns>
        ///     The requested address as <see cref="string" />.
        /// </returns>
        public abstract string GetRequestedAddress();

        /// <summary>
        ///     Get the latest requested port number
        /// </summary>
        /// <returns>
        ///     The requested port number as <see cref="ushort" />.
        /// </returns>
        public abstract ushort GetRequestedPort();
    }
}