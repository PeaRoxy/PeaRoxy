// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Controller.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The controller
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.Network.Hook
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels.Ipc;
    using System.Threading;

    using EasyHook;

    #endregion

    internal static class Controller
    {
        #region Static Fields

        private static readonly Dictionary<int, Host> InternalHosts = new Dictionary<int, Host>();

        private static readonly List<string> InternalTargetProcesses = new List<string>();

        // ReSharper disable once NotAccessedField.Local
        private static IpcServerChannel channel;

        #endregion

        #region Public Properties

        public static string ChannelName { get; private set; }

        public static InjectionOptions GlobalOptions { get; private set; }

        public static Dictionary<int, Host> Hosts
        {
            get
            {
                return InternalHosts;
            }
        }

        public static bool IsDebug { get; set; }

        public static IPEndPoint ProxyEndPoint { get; set; }

        public static List<string> TargetProcesses
        {
            get
            {
                return InternalTargetProcesses;
            }
        }

        public static string DnsGrabberPattern { get; set; }

        public static byte FakeIpSupLevel { get; set; }

        #endregion

        #region Public Methods and Operators

        [Conditional("DEBUG")]
        public static void DebugInit()
        {
            IsDebug = true;
        }

        public static void DebugPrint(string str)
        {
            if (IsDebug)
            {
                Console.WriteLine(str);
            }
        }

        public static void Start()
        {
            DebugInit();
            string channelName = null;
            channel = RemoteHooking.IpcCreateServer<Host.Remote>(ref channelName, WellKnownObjectMode.SingleCall);
            ChannelName = channelName;
            if (ProxyEndPoint == null)
            {
                throw new Exception("Missing proxy end point");
            }

            try
            {
                Config.Register("PeaRoxy", "PeaRoxy.Windows.Network.Hook.exe");
            }
            catch (ApplicationException)
            {
                Console.WriteLine("This is an administrative task! No admin privilege.");
                Console.ReadLine();
                Environment.Exit(0);
            }

            while (true)
            {
                try
                {
                    Process[] processes = Process.GetProcesses();
                    foreach (Process process in processes)
                    {
                        if (InternalTargetProcesses.Contains(process.ProcessName.ToLower().Trim()))
                        {
                            if (!InternalHosts.ContainsKey(process.Id))
                            {
                                InternalHosts.Add(process.Id, new Host(process));
                            }
                        }
                    }

                    foreach (KeyValuePair<int, Host> injectedHost in new Dictionary<int, Host>(InternalHosts))
                    {
                        switch (injectedHost.Value.Status)
                        {
                            case Host.StatusEnum.Idle:
                                injectedHost.Value.Inject();
                                break;
                            case Host.StatusEnum.Aborted:
                                InternalHosts.Remove(injectedHost.Key);
                                break;
                        }
                    }
                    Thread.Sleep(100);
                }
                catch (Exception e)
                {
                    DebugPrint(e.ToString());
                }
            }
        }

        #endregion
    }
}