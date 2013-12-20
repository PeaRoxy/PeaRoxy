// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Common.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The common.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows
{
    #region

    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    #endregion

    /// <summary>
    /// The common.
    /// </summary>
    public static class Common
    {
        #region Enums

        /// <summary>
        /// The exit windows.
        /// </summary>
        [Flags]
        private enum ExitWindows : uint
        {
            // ONE of the following five:
            /// <summary>
            /// The log off.
            /// </summary>
            LogOff = 0x00, 

            /// <summary>
            /// The shut down.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            ShutDown = 0x01, 

            /// <summary>
            /// The reboot.
            /// </summary>
            Reboot = 0x02, 

            /// <summary>
            /// The power off.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            PowerOff = 0x08, 

            /// <summary>
            /// The restart apps.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            RestartApps = 0x40, 

            // plus AT MOST ONE of the following two:
            /// <summary>
            /// The force.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            Force = 0x04, 

            /// <summary>
            /// The force if hung.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            ForceIfHung = 0x10, 
        }

        /// <summary>
        /// The shutdown reason.
        /// </summary>
        [Flags]
        private enum ShutdownReason : uint
        {
            /// <summary>
            /// The major application.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MajorApplication = 0x00040000, 

            /// <summary>
            /// The major hardware.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MajorHardware = 0x00010000, 

            /// <summary>
            /// The major legacy api.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MajorLegacyApi = 0x00070000, 

            /// <summary>
            /// The major operating system.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MajorOperatingSystem = 0x00020000, 

            /// <summary>
            /// The major other.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MajorOther = 0x00000000, 

            /// <summary>
            /// The major power.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MajorPower = 0x00060000, 

            /// <summary>
            /// The major software.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MajorSoftware = 0x00030000, 

            /// <summary>
            /// The major system.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MajorSystem = 0x00050000, 

            /// <summary>
            /// The minor blue screen.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorBlueScreen = 0x0000000F, 

            /// <summary>
            /// The minor cord unplugged.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorCordUnplugged = 0x0000000b, 

            /// <summary>
            /// The minor disk.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorDisk = 0x00000007, 

            /// <summary>
            /// The minor environment.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorEnvironment = 0x0000000c, 

            /// <summary>
            /// The minor hardware driver.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorHardwareDriver = 0x0000000d, 

            /// <summary>
            /// The minor hotfix.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorHotfix = 0x00000011, 

            /// <summary>
            /// The minor hung.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorHung = 0x00000005, 

            /// <summary>
            /// The minor installation.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorInstallation = 0x00000002, 

            /// <summary>
            /// The minor maintenance.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorMaintenance = 0x00000001, 

            /// <summary>
            /// The minor mmc.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorMmc = 0x00000019, 

            /// <summary>
            /// The minor network connectivity.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorNetworkConnectivity = 0x00000014, 

            /// <summary>
            /// The minor network card.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorNetworkCard = 0x00000009, 

            /// <summary>
            /// The minor other.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorOther = 0x00000000, 

            /// <summary>
            /// The minor other driver.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorOtherDriver = 0x0000000e, 

            /// <summary>
            /// The minor power supply.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorPowerSupply = 0x0000000a, 

            /// <summary>
            /// The minor processor.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorProcessor = 0x00000008, 

            /// <summary>
            /// The minor reconfig.
            /// </summary>
            MinorReconfig = 0x00000004, 

            /// <summary>
            /// The minor security.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorSecurity = 0x00000013, 

            /// <summary>
            /// The minor security fix.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorSecurityFix = 0x00000012, 

            /// <summary>
            /// The minor security fix uninstall.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorSecurityFixUninstall = 0x00000018, 

            /// <summary>
            /// The minor service pack.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorServicePack = 0x00000010, 

            /// <summary>
            /// The minor service pack uninstall.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorServicePackUninstall = 0x00000016, 

            /// <summary>
            /// The minor term srv.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorTermSrv = 0x00000020, 

            /// <summary>
            /// The minor unstable.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorUnstable = 0x00000006, 

            /// <summary>
            /// The minor upgrade.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorUpgrade = 0x00000003, 

            /// <summary>
            /// The minor wmi.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            MinorWmi = 0x00000015, 

            /// <summary>
            /// The flag user defined.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            FlagUserDefined = 0x40000000, 

            /// <summary>
            /// The flag planned.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            FlagPlanned = 0x80000000
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create process.
        /// </summary>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <param name="isHidden">
        /// The is hidden.
        /// </param>
        /// <param name="isOutputNeeded">
        /// The is output needed.
        /// </param>
        /// <returns>
        /// The <see cref="Process"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        public static Process CreateProcess(
            string address, 
            string args, 
            bool isHidden = true, 
            bool isOutputNeeded = false)
        {
            Process process = new Process
                                  {
                                      StartInfo =
                                          new ProcessStartInfo(address, args)
                                              {
                                                  WindowStyle = (isHidden) ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
                                              }
                                  };
            if (isOutputNeeded)
            {
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
            }

            return process.Start() ? process : null;
        }

        /// <summary>
        /// The log off user.
        /// </summary>
        public static void LogOffUser()
        {
            ExitWindowsEx(ExitWindows.LogOff, ShutdownReason.MinorReconfig);
        }

        /// <summary>
        /// The restart windows.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static void RestartWindows()
        {
            ExitWindowsEx(ExitWindows.Reboot, ShutdownReason.MinorReconfig);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The exit windows ex.
        /// </summary>
        /// <param name="uFlags">
        /// The u flags.
        /// </param>
        /// <param name="dwReason">
        /// The dw reason.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ExitWindowsEx(ExitWindows uFlags, ShutdownReason dwReason);

        #endregion
    }
}