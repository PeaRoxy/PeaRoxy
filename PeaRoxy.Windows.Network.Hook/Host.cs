// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Host.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The injector class and host holder
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.Network.Hook
{
    #region

    using System;
    using System.Diagnostics;
    using System.Diagnostics.Eventing.Reader;
    using System.Net;

    using EasyHook;

    #endregion

    internal class Host
    {
        #region Fields

        private StatusEnum status;

        #endregion

        #region Constructors and Destructors

        public Host(Process process)
        {
            this.status = StatusEnum.Idle;
            this.UnderlyingProcess = process;
        }

        #endregion

        #region Enums

        public enum StatusEnum
        {
            Idle,

            Injecting,

            Injected,

            Aborted,

            Error,
        }

        #endregion

        #region Public Properties

        public int LastPing { get; private set; }

        public StatusEnum Status
        {
            get
            {
                if ((Environment.TickCount - this.LastPing > 20000 && this.status != StatusEnum.Idle)
                    || this.UnderlyingProcess.HasExited)
                {
                    this.status = StatusEnum.Aborted;
                }

                return this.status;
            }
            private set
            {
                this.status = value;
            }
        }

        public Process UnderlyingProcess { get; private set; }

        #endregion

        #region Public Methods and Operators

        public void Inject()
        {
            this.status = StatusEnum.Injecting;
            this.LastPing = Environment.TickCount;
            try
            {
                RemoteHooking.Inject(
                    this.UnderlyingProcess.Id,
                    "PeaRoxy.Windows.Network.Hook.exe",
                    "PeaRoxy.Windows.Network.Hook.exe",
                    Controller.ChannelName);
            }
            catch (Exception e)
            {
                Console.WriteLine(this.UnderlyingProcess.Id + " - " + e.Message);
                this.status = StatusEnum.Error;
            }
        }

        #endregion

        // ReSharper disable once ClassNeverInstantiated.Global
        public class Remote : MarshalByRefObject
        {
            #region Public Methods and Operators

            public string GetDnsGrabberPattern()
            {
                return Controller.DnsGrabberPattern;
            }

            public byte GetFakeIpSupLevel()
            {
                return Controller.FakeIpSupLevel;
            }

            public IPEndPoint GetProxyEndPoint(int pid)
            {
                return Controller.ProxyEndPoint;
            }

            public bool IsDebugEnable()
            {
                return Controller.IsDebug;
            }

            public void Ping(int pid)
            {
                this.GetHostObject(pid).Status = StatusEnum.Injected;
                this.GetHostObject(pid).LastPing = Environment.TickCount;
            }

            public void ReportMessage(int pid, string str)
            {
                Controller.DebugPrint(pid + " - " + str);
            }

            #endregion

            #region Methods

            private Host GetHostObject(int pid)
            {
                return Controller.Hosts.ContainsKey(pid) ? Controller.Hosts[pid] : null;
            }

            #endregion
        }
    }
}