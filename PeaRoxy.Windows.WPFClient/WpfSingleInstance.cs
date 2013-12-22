// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WpfSingleInstance.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The WPF single instance.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient
{
    #region

    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Security.Principal;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    #endregion

    /// <summary>
    ///     The WPF single instance.
    /// </summary>
    public static class WpfSingleInstance
    {
        #region Static Fields

        /// <summary>
        ///     The auto exit application if startup deadlock.
        /// </summary>
        private static DispatcherTimer autoExitApplicationIfStartupDeadlock;

        #endregion

        #region Delegates

        /// <summary>
        ///     The second instance delegate.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        public delegate void SecondInstanceDelegate(string args);

        #endregion

        #region Public Events

        /// <summary>
        ///     The second instance callback.
        /// </summary>
        public static event SecondInstanceDelegate SecondInstanceCallback;

        #endregion

        #region Enums

        /// <summary>
        ///     The single instance modes.
        /// </summary>
        public enum SingleInstanceModes
        {
            /// <summary>
            ///     Do nothing.
            /// </summary>
            // ReSharper disable once UnusedMember.Global
            NotInited = 0, 

            /// <summary>
            ///     Every user can have own single instance.
            /// </summary>
            ForEveryUser, 
        }

        #endregion

        #region Methods

        /// <summary>
        /// Processing single instance.
        /// </summary>
        /// <param name="singleInstanceModes">
        /// The single instance modes
        /// </param>
        internal static void Make(SingleInstanceModes singleInstanceModes = SingleInstanceModes.ForEveryUser)
        {
            var appName = Application.Current.GetType().Assembly.ManifestModule.ScopeName;
            var keyUserName = string.Empty;
            if (singleInstanceModes == SingleInstanceModes.ForEveryUser)
            {
                var windowsIdentity = WindowsIdentity.GetCurrent();
                keyUserName = windowsIdentity != null && windowsIdentity.User != null
                                  ? windowsIdentity.User.ToString()
                                  : string.Empty;
            }

            // Be careful! Max 260 chars!
            var eventWaitHandleName = string.Format("{0}{1}", appName, keyUserName);
            MultiInstance.Default.LastArgs = string.Empty;
            MultiInstance.Default.Save();
            try
            {
                using (var eventWaitHandle = EventWaitHandle.OpenExisting(eventWaitHandleName))
                {
                    // It informs first instance about other startup attempting.
                    string[] args = Environment.GetCommandLineArgs();
                    if (args.Length > 1)
                    {
                        MultiInstance.Default.LastArgs = string.Join(" ", args, 1, args.Length - 1);
                        MultiInstance.Default.Save();
                    }

                    eventWaitHandle.Set();
                }

                // Let's terminate this posterior startup.
                // For that exit no interceptions.
                Environment.Exit(0);
            }
            catch
            {
                // It's first instance.

                // Register EventWaitHandle.
                using (var eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, eventWaitHandleName))
                {
                    ThreadPool.RegisterWaitForSingleObject(
                        eventWaitHandle, 
                        OtherInstanceAttemptedToStart, 
                        null, 
                        Timeout.Infinite, 
                        false);
                }

                RemoveApplicationsStartupDeadlockForStartupCrushedWindows();
            }
        }

        /// <summary>
        /// The other instance attempted to start.
        /// </summary>
        /// <param name="state">
        /// The state.
        /// </param>
        /// <param name="timedOut">
        /// The timed out.
        /// </param>
        private static void OtherInstanceAttemptedToStart(object state, bool timedOut)
        {
            RemoveApplicationsStartupDeadlockForStartupCrushedWindows();
            Application.Current.Dispatcher.BeginInvoke(
                new Action(
                    () =>
                        {
                            try
                            {
                                if (SecondInstanceCallback == null)
                                {
                                    return;
                                }

                                MultiInstance.Default.Reload();
                                string lastArgs = MultiInstance.Default.LastArgs;
                                MultiInstance.Default.LastArgs = string.Empty;
                                MultiInstance.Default.Save();
                                foreach (Delegate del in SecondInstanceCallback.GetInvocationList())
                                {
                                    if (del.Target == null || ((ISynchronizeInvoke)del.Target).InvokeRequired == false)
                                    {
                                        del.DynamicInvoke(new object[] { lastArgs });
                                    }
                                    else
                                    {
                                        ((ISynchronizeInvoke)del.Target).Invoke(del, new object[] { lastArgs });
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }));
        }

        /// <summary>
        ///     The remove applications startup deadlock for startup crushed windows.
        /// </summary>
        private static void RemoveApplicationsStartupDeadlockForStartupCrushedWindows()
        {
            Application.Current.Dispatcher.BeginInvoke(
                new Action(
                    () =>
                        {
                            autoExitApplicationIfStartupDeadlock = new DispatcherTimer(
                                TimeSpan.FromSeconds(6), 
                                DispatcherPriority.ApplicationIdle, 
                                (o, args) =>
                                    {
                                        if (
                                            Application.Current.Windows.Cast<Window>()
                                                .Count(window => !double.IsNaN(window.Left)) == 0)
                                        {
                                            // For that exit no interceptions.
                                            Environment.Exit(0);
                                        }
                                    }, 
                                Application.Current.Dispatcher);
                            autoExitApplicationIfStartupDeadlock.Start();
                        }), 
                DispatcherPriority.ApplicationIdle);
        }

        #endregion
    }
}