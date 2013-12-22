// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Injector.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The injector.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.Network.Hook
{
    #region

    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Remoting;

    using EasyHook;

    #endregion

    /// <summary>
    /// The injector.
    /// </summary>
    internal static class Injector
    {
        #region Static Fields

        /// <summary>
        /// The channel name.
        /// </summary>
        private static string channelName;

        /// <summary>
        /// The is debug.
        /// </summary>
        private static bool isDebug;

        #endregion

        #region Methods

        /// <summary>
        /// The main.
        /// </summary>
        private static void Main()
        {
            IsDebugSetter();
            bool noGac = false;
            Process[] processes = Process.GetProcessesByName("seamonkey");
            Array.Reverse(processes);
            try
            {
                Config.Register(
                    "PeaRoxy",
                    isDebug ? "PeaRoxy.Windows.Network.Hook.exe" : Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName));
            }
            catch (ApplicationException)
            {
                Console.WriteLine("This is an administrative task! No admin privilege. Try to not use GAC");
                noGac = true;
            }

            RemoteHooking.IpcCreateServer<RemoteParent>(ref channelName, WellKnownObjectMode.SingleCall);
            foreach (Process p in processes)
            {
                try
                {
                    RemoteHooking.Inject(
                        p.Id, 
                        noGac ? InjectionOptions.DoNotRequireStrongName : InjectionOptions.Default, 
                        isDebug ? "PeaRoxy.Windows.Network.Hook.exe" : "PeaRoxy.Windows.Network.Hook_x86.exe", 
                        isDebug ? "PeaRoxy.Windows.Network.Hook.exe" : "PeaRoxy.Windows.Network.Hook_x64.exe", 
                        new object[] { channelName });
                }
                catch (Exception extInfo)
                {
                    Console.WriteLine("There was an error while connecting to target:\r\n{0}", extInfo);
                }
            }

            Console.ReadLine();
        }

        /// <summary>
        /// The is debug setter.
        /// </summary>
        [Conditional("DEBUG")]
        private static void IsDebugSetter()
        {
            isDebug = true;
        }

        #endregion
    }
}