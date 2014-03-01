// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tun2Socks.cs" company="PeaRoxy.com">
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
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Threading;

    using PeaRoxy.Windows.Network.TAP.Win32_WMI;

    #endregion

    /// <summary>
    /// The TUN2 socks.
    /// </summary>
    public static class Tun2Socks
    {
        #region Static Fields

        /// <summary>
        /// The TUN2 socks process.
        /// </summary>
        private static Process tun2SocksProcess;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The clean all TUN2 socks.
        /// </summary>
        public static void CleanAllTun2Socks()
        {
            bool wasA = false;
            foreach (Process p in Process.GetProcesses().Where(p => p.ProcessName == "tun2socks"))
            {
                try
                {
                    wasA = true;
                    p.Kill();
                    p.WaitForExit();
                }
                catch (Exception)
                {
                }
            }

            if (wasA)
            {
                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// The is TUN2 socks running.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsTun2SocksRunning()
        {
            return tun2SocksProcess != null && tun2SocksProcess.HasExited == false;
        }

        /// <summary>
        /// The start TUN2 socks.
        /// </summary>
        /// <param name="network">
        /// The network.
        /// </param>
        /// <param name="ipAddress">
        /// The IP address.
        /// </param>
        /// <param name="ipSubnet">
        /// The IP subnet.
        /// </param>
        /// <param name="ipServer">
        /// The IP server.
        /// </param>
        /// <param name="socksProxy">
        /// The socks proxy.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public static bool StartTun2Socks(
            NetworkAdapter network,
            IPAddress ipAddress,
            IPAddress ipSubnet,
            IPAddress ipServer,
            IPEndPoint socksProxy)
        {
            CleanAllTun2Socks();
            tun2SocksProcess = Common.CreateProcess(
                "TAPDriver\\tun2socks.exe",
                string.Format("--tundev \"{0}:{1}:{2}:{3}:{4}\" --netif-ipaddr {5} --netif-netmask {4} --socks-server-addr {6}:{7}", network.ServiceName, network.NetConnectionID, ipAddress, CommonLibrary.Common.MergeIpIntoIpSubnet(ipAddress, ipSubnet, IPAddress.Parse("10.0.0.0")), ipSubnet, ipServer, socksProxy.Address, socksProxy.Port), true, true);
            int timeout = 120;
            timeout = timeout * 10;
            while (!network.NetEnabled && // Vista+
                   network.NetConnectionStatus != 1 && // XP SP3+
                   network.NetConnectionStatus != 2 && // XP SP3+
                   network.NetConnectionStatus != 9)
            {
                // XP SP2-
                if (timeout == 0)
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    return StopTun2Socks() && false;
                }

                if (tun2SocksProcess.HasExited)
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    return StopTun2Socks() && false;
                }

                network.RefreshProperties();
                Thread.Sleep(100);
                timeout--;
            }

            return true;
        }

        /// <summary>
        /// The stop TUN2 socks.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool StopTun2Socks()
        {
            if (IsTun2SocksRunning())
            {
                tun2SocksProcess.Kill();
                tun2SocksProcess.WaitForExit();
                return true;
            }

            return false;
        }

        #endregion
    }
}