// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServerType.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The server type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.ClientLibrary.ServerModules
{
    #region

    using System.Net.Sockets;

    #endregion

    /// <summary>s
    ///     The server abstract class for modules
    /// </summary>
    public abstract class ServerType
    {
        #region Delegates

        /// <summary>
        ///     The connection callback delegate.
        /// </summary>
        /// <param name="success">
        ///     The success.
        /// </param>
        /// <param name="activeServer">
        ///     The active server.
        /// </param>
        /// <param name="client">
        ///     The client.
        /// </param>
        /// <returns>
        /// Return a boolean indicating success of the process
        /// </returns>
        public delegate bool ConnectionCallbackDelegate(bool success, ServerType activeServer, ProxyClient client);

        /// <summary>
        ///     The data callback delegate.
        /// </summary>
        /// <param name="data">
        ///     The data.
        /// </param>
        /// <param name="activeServer">
        ///     The active server.
        /// </param>
        /// <param name="client">
        ///     The client.
        /// </param>
        /// <returns>
        /// Return a boolean indicating success of the process
        /// </returns>
        public delegate bool DataCallbackDelegate(ref byte[] data, ServerType activeServer, ProxyClient client);

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the connection status callback.
        /// </summary>
        public ConnectionCallbackDelegate ConnectionStatusCallback { get; protected internal set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is data sent.
        /// </summary>
        public abstract bool IsDataSent { get; protected set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is disconnected.
        /// </summary>
        public abstract bool IsDisconnected { get; protected set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is server valid.
        /// </summary>
        public abstract bool IsServerValid { get; protected set; }

        /// <summary>
        ///     Gets or sets the no data timeout.
        /// </summary>
        public abstract int NoDataTimeout { get; set; }

        /// <summary>
        ///     Gets or sets the parent client.
        /// </summary>
        public abstract ProxyClient ParentClient { get; protected set; }

        /// <summary>
        /// Gets or sets the receive data delegate.
        /// </summary>
        public DataCallbackDelegate ReceiveDataDelegate { get; protected internal set; }

        /// <summary>
        /// Gets or sets the send data delegate.
        /// </summary>
        public DataCallbackDelegate SendDataDelegate { get; protected internal set; }

        /// <summary>
        ///     Gets or sets the underlying socket.
        /// </summary>
        public abstract Socket UnderlyingSocket { get; protected set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The clone.
        /// </summary>
        /// <returns>
        ///     The <see cref="ServerType" />.
        /// </returns>
        public abstract ServerType Clone();

        /// <summary>
        ///     The do route.
        /// </summary>
        public abstract void DoRoute();

        /// <summary>
        /// The establish.
        /// </summary>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="headerData">
        /// The header data.
        /// </param>
        /// <param name="sendDataDelegate">
        /// The send callback.
        /// </param>
        /// <param name="receiveDataDelegate">
        /// The receive callback.
        /// </param>
        /// <param name="connectionStatusDelegate">
        /// The connection status callback.
        /// </param>
        public abstract void Establish(
            string address, 
            ushort port, 
            ProxyClient client, 
            byte[] headerData = null, 
            DataCallbackDelegate sendDataDelegate = null, 
            DataCallbackDelegate receiveDataDelegate = null, 
            ConnectionCallbackDelegate connectionStatusDelegate = null);

        /// <summary>
        ///     The get address.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public abstract string GetAddress();

        /// <summary>
        ///     The get port.
        /// </summary>
        /// <returns>
        ///     The <see cref="ushort" />.
        /// </returns>
        public abstract ushort GetPort();

        #endregion
    }
}