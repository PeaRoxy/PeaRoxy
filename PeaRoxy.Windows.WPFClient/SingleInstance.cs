    using System;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    namespace SingleInstance
    {
        public static class WpfSingleInstance
        {
            public delegate void SecondInstanceDelegate(string args);
            public static event SecondInstanceDelegate SecondInstanceCallback;
            /// <summary>
            /// Processing single instance in <see cref="SingleInstanceModes"/> <see cref="SingleInstanceModes.ForEveryUser"/> mode.
            /// </summary>
            internal static void Make()
            {
                Make(SingleInstanceModes.ForEveryUser);
            }

            /// <summary>
            /// Processing single instance.
            /// </summary>
            /// <param name="singleInstanceModes"></param>
            internal static void Make(SingleInstanceModes singleInstanceModes)
            {
                var appName = Application.Current.GetType().Assembly.ManifestModule.ScopeName;
                var keyUserName = string.Empty;
                if (singleInstanceModes == SingleInstanceModes.ForEveryUser)
                {
                    var windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
                    keyUserName = windowsIdentity != null ? windowsIdentity.User.ToString() : String.Empty;
                }
                // Be careful! Max 260 chars!
                var eventWaitHandleName = string.Format(
                    "{0}{1}",
                    appName,
                    keyUserName
                    );
                PeaRoxy.Windows.WPFClient.MultiInstance.Default.LastArgs = "";
                PeaRoxy.Windows.WPFClient.MultiInstance.Default.Save();
                try
                {
                    using (var eventWaitHandle = EventWaitHandle.OpenExisting(eventWaitHandleName))
                    {
                        // It informs first instance about other startup attempting.
                        string[] args = Environment.GetCommandLineArgs();
                        if (args != null && args.Length > 1)
                        {
                            PeaRoxy.Windows.WPFClient.MultiInstance.Default.LastArgs = string.Join(" ", args, 1, args.Length - 1);
                            PeaRoxy.Windows.WPFClient.MultiInstance.Default.Save();
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
                        ThreadPool.RegisterWaitForSingleObject(eventWaitHandle, OtherInstanceAttemptedToStart, null, Timeout.Infinite, false);
                    }

                    RemoveApplicationsStartupDeadlockForStartupCrushedWindows();
                }
            }

            private static void OtherInstanceAttemptedToStart(Object state, Boolean timedOut)
            {
                RemoveApplicationsStartupDeadlockForStartupCrushedWindows();
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (SecondInstanceCallback == null)
                            return;
                        PeaRoxy.Windows.WPFClient.MultiInstance.Default.Reload();
                        string lastArgs = PeaRoxy.Windows.WPFClient.MultiInstance.Default.LastArgs;
                        PeaRoxy.Windows.WPFClient.MultiInstance.Default.LastArgs = "";
                        PeaRoxy.Windows.WPFClient.MultiInstance.Default.Save();
                        foreach (System.Delegate del in SecondInstanceCallback.GetInvocationList())
                        {
                            if (del.Target == null || ((System.ComponentModel.ISynchronizeInvoke)del.Target).InvokeRequired == false)
                                del.DynamicInvoke(new object[] { lastArgs });
                            else
                                ((System.ComponentModel.ISynchronizeInvoke)del.Target).Invoke(del, new object[] { lastArgs });
                        }
                    }
                    catch { }
                }));
            }

            internal static DispatcherTimer AutoExitAplicationIfStartupDeadlock;

            public static void RemoveApplicationsStartupDeadlockForStartupCrushedWindows()
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    AutoExitAplicationIfStartupDeadlock =
                        new DispatcherTimer(
                            TimeSpan.FromSeconds(6),
                            DispatcherPriority.ApplicationIdle,
                            (o, args) =>
                            {
                                if (Application.Current.Windows.Cast<Window>().Count(window => !Double.IsNaN(window.Left)) == 0)
                                {
                                    // For that exit no interceptions.
                                    Environment.Exit(0);
                                }
                            },
                            Application.Current.Dispatcher
                        );
                }),
                    DispatcherPriority.ApplicationIdle
                    );
            }
        }

        public enum SingleInstanceModes
        {
            /// <summary>
            /// Do nothing.
            /// </summary>
            NotInited = 0,

            /// <summary>
            /// Every user can have own single instance.
            /// </summary>
            ForEveryUser,
        }
    }