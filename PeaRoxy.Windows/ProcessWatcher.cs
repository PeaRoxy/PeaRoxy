// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessWatcher.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The process watcher.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows
{
    #region

    using System;
    using System.Management;

    #endregion

    /// <summary>
    /// The process watcher.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    internal class ProcessWatcher
    {
        #region Methods

        /// <summary>
        /// The process ended.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ProcessEnded(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject targetInstance = (ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value;
            string processName = targetInstance.Properties["Name"].Value.ToString();
            Console.WriteLine("{0} process ended", processName);
        }

        /// <summary>
        /// The process started.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ProcessStarted(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject targetInstance = (ManagementBaseObject)e.NewEvent.Properties["TargetInstance"].Value;
            string processName = targetInstance.Properties["Name"].Value.ToString();
            Console.WriteLine("{0} process started", processName);
        }

        /// <summary>
        /// The watch for process end.
        /// </summary>
        /// <param name="processName">
        /// The process name.
        /// </param>
        /// <returns>
        /// The <see cref="ManagementEventWatcher"/>.
        /// </returns>
        // ReSharper disable once UnusedMember.Local
        private ManagementEventWatcher WatchForProcessEnd(string processName)
        {
            string queryString = "SELECT TargetInstance" + "  FROM __InstanceDeletionEvent " + "WITHIN  10 "
                                 + " WHERE TargetInstance ISA 'Win32_Process' " + "   AND TargetInstance.Name = '"
                                 + processName + "'";

            // The dot in the scope means use the current machine
            const string Scope = @"\\.\root\CIMV2";

            // Create a watcher and listen for events
            ManagementEventWatcher watcher = new ManagementEventWatcher(Scope, queryString);
            watcher.EventArrived += this.ProcessEnded;
            watcher.Start();
            return watcher;
        }

        /// <summary>
        /// The watch for process start.
        /// </summary>
        /// <param name="processName">
        /// The process name.
        /// </param>
        /// <returns>
        /// The <see cref="ManagementEventWatcher"/>.
        /// </returns>
        // ReSharper disable once UnusedMember.Local
        private ManagementEventWatcher WatchForProcessStart(string processName)
        {
            string queryString = "SELECT TargetInstance" + "  FROM __InstanceCreationEvent " + "WITHIN  10 "
                                 + " WHERE TargetInstance ISA 'Win32_Process' " + "   AND TargetInstance.Name = '"
                                 + processName + "'";

            // The dot in the scope means use the current machine
            const string Scope = @"\\.\root\CIMV2";

            // Create a watcher and listen for events
            ManagementEventWatcher watcher = new ManagementEventWatcher(Scope, queryString);
            watcher.EventArrived += this.ProcessStarted;
            watcher.Start();
            return watcher;
        }

        #endregion
    }
}