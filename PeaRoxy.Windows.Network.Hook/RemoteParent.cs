// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteParent.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The remote parent.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.Network.Hook
{
    #region

    using System;

    #endregion

    /// <summary>
    /// The remote parent.
    /// </summary>
    public abstract class RemoteParent : MarshalByRefObject
    {
        #region Public Methods and Operators

        /// <summary>
        /// The is installed.
        /// </summary>
        /// <param name="clientPid">
        /// The client process id.
        /// </param>
        public void IsInstalled(int clientPid)
        {
            Console.WriteLine("FileMon has been installed in target {0}.\n", clientPid);
        }

        /// <summary>
        /// The log to screen.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void LogToScreen(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// The ping.
        /// </summary>
        public void Ping()
        {
        }

        /// <summary>
        /// The report exception.
        /// </summary>
        /// <param name="inInfo">
        /// The in info.
        /// </param>
        public void ReportException(Exception inInfo)
        {
            Console.WriteLine("The target process has reported an error:\n" + inInfo);
        }

        #endregion
    }
}