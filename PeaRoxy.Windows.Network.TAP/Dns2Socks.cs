// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Dns2Socks.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The DNS 2 socks.
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

    #endregion

    /// <summary>
    /// The DNS 2 socks.
    /// </summary>
    public static class Dns2Socks
    {
        #region Static Fields

        /// <summary>
        /// The DNS socks process.
        /// </summary>
        private static Process dns2SocksProcess;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The clean all DNS socks.
        /// </summary>
        public static void CleanAllDns2Socks()
        {
            bool wasA = false;
            foreach (Process p in Process.GetProcesses().Where(p => p.ProcessName == "dns2socks"))
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
        /// The is DNS 2 socks running.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsDns2SocksRunning()
        {
            return dns2SocksProcess != null && dns2SocksProcess.HasExited == false;
        }

        /// <summary>
        /// The start DNS 2 socks.
        /// </summary>
        /// <param name="dnsIpAddress">
        /// The DNS IP address.
        /// </param>
        /// <param name="socksProxy">
        /// The socks proxy.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public static bool StartDns2Socks(IPAddress dnsIpAddress, IPEndPoint socksProxy)
        {
            CleanAllDns2Socks();
            dns2SocksProcess = Common.CreateProcess(
                "TAPDriver\\dns2socks.exe",
                string.Format("/q {0}:{1} {2} {3}", socksProxy.Address, socksProxy.Port, dnsIpAddress, socksProxy.Address), true, true);

            return true;
        }

        /// <summary>
        /// The stop DNS 2 socks.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool StopDns2Socks()
        {
            if (IsDns2SocksRunning())
            {
                dns2SocksProcess.Kill();
                dns2SocksProcess.WaitForExit();
                return true;
            }

            return false;
        }

        #endregion
    }
}