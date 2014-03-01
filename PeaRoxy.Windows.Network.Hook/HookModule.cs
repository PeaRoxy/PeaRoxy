// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HookModule.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.Network.Hook
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Threading;

    #endregion

    public static class HookModule
    {
        #region Static Fields

        private static Process hookProcess;

        #endregion

        #region Public Methods and Operators

        public static void CleanAllHookProcesses()
        {
            bool wasA = false;
            foreach (Process p in Process.GetProcesses().Where(p => p.ProcessName == "PeaRoxy.Windows.Network.Hook"))
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

        public static bool IsHookProcessRunning()
        {
            return hookProcess != null && hookProcess.HasExited == false;
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        public static bool StartHookProcess(
            IEnumerable<string> applicationsToInject,
            IPEndPoint fastProxyAddress,
            string dnsGrabberPattern = "",
            bool isDebug = false)
        {
            CleanAllHookProcesses();
            hookProcess = Common.CreateProcess(
                "PeaRoxy.Windows.Network.Hook.exe",
                string.Format(
                    "-p {0}:{1} -a {2} {3} {4}",
                    fastProxyAddress.Address.Equals(IPAddress.Any) ? IPAddress.Loopback : fastProxyAddress.Address,
                    fastProxyAddress.Port,
                    string.Join("|", applicationsToInject.Select(x => x.Trim()).ToArray()),
                    isDebug ? "-d" : "",
                    string.IsNullOrWhiteSpace(dnsGrabberPattern) ? "" : "-i \"" + dnsGrabberPattern + "\""),
                !isDebug,
                true);

            return hookProcess != null;
        }

        public static bool StopHookProcess()
        {
            if (IsHookProcessRunning())
            {
                hookProcess.Kill();
                hookProcess.WaitForExit();
                return true;
            }

            return false;
        }

        #endregion
    }
}