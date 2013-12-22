// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TapAdapter.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The tap tunnel.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.Network.TAP
{
    #region

    using System;
    using System.Diagnostics.CodeAnalysis;

    using PeaRoxy.Windows.Network.TAP.Win32_WMI;

    #endregion

    /// <summary>
    /// The tap adapter.
    /// </summary>
    public static class TapAdapter
    {
        #region Constants

        /// <summary>
        /// The driver service name.
        /// </summary>
        public const string DriverServiceName = "tap0901";

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The install an adapter.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="NetworkAdapter"/>.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public static NetworkAdapter InstallAnAdapter(string name)
        {
            NetworkAdapter net = NetworkAdapter.GetByServiceName(DriverServiceName);
            if (net == null)
            {
                if (NetworkAdapter.GetByName(name) != null)
                {
                    return null;
                }

                string osBit = Environment.Is64BitOperatingSystem ? "x64" : "x86";
                Common.CreateProcess(
                    "TAPDriver\\" + osBit + "\\tapinstall.exe",
                    "install \"TAPDriver\\" + osBit + "\\OemWin2k.inf\" " + DriverServiceName).WaitForExit();
            }

            net = NetworkAdapter.GetByServiceName(DriverServiceName);
            if (net == null)
            {
                return null;
            }

            if (!net.RenameAdapter(name))
            {
                return null;
            }

            return net;
        }

        /// <summary>
        /// The remove all adapters.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable once UnusedMember.Global
        public static void RemoveAllAdapters()
        {
            string osBit = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            Common.CreateProcess("TAPDriver\\" + osBit + "\\tapinstall.exe", "remove " + DriverServiceName)
                .WaitForExit();
        }

        #endregion
    }
}