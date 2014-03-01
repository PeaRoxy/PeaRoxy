namespace PeaRoxy.Windows.Network.Hook
{
    #region

    using System;

    #endregion

    internal class GuestSocketInfo
    {
        #region Public Properties

        public string HostName { get; set; }

        public bool NoSendYet { get; set; }

        public uint Port { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public IntPtr Socket { get; set; }

        #endregion
    }
}