// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IP4RouteTable.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The i p 4 route table.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.Network.TAP.Win32_WMI
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Management;
    using System.Net;
    using System.Reflection;
    using System.Runtime.InteropServices;

    #endregion

    /// <summary>
    /// The IP4 route table.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class IP4RouteTable
    {
        #region Fields

        /// <summary>
        /// The management source.
        /// </summary>
        private readonly ManagementObject managementSource;

        /// <summary>
        /// The is legacy.
        /// </summary>
        private bool isLegacy;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IP4RouteTable"/> class.
        /// </summary>
        /// <param name="mo">
        /// The mo.
        /// </param>
        public IP4RouteTable(ManagementObject mo)
        {
            this.managementSource = mo;
            foreach (PropertyInfo p in this.GetType().GetProperties())
            {
                p.SetValue(this, this.managementSource[p.Name], null);
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the age.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        // ReSharper disable once UnusedMember.Global
        public uint Age { get; private set; }

        /// <summary>
        /// Gets the caption.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string Caption { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        // ReSharper disable once UnusedMember.Global
        public string Description { get; private set; }

        /// <summary>
        /// Gets the destination.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string Destination { get; private set; }

        /// <summary>
        /// Gets the information.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string Information { get; private set; }

        /// <summary>
        /// Gets the interface index.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int InterfaceIndex { get; private set; }

        /// <summary>
        /// Gets the mask.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string Mask { get; private set; }

        /// <summary>
        /// Gets the metric 1.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int Metric1 { get; private set; }

        /// <summary>
        /// Gets the metric 2.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        // ReSharper disable once UnusedMember.Global
        public int Metric2 { get; private set; }

        /// <summary>
        /// Gets the metric 3.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        // ReSharper disable once UnusedMember.Global
        public int Metric3 { get; private set; }

        /// <summary>
        /// Gets the metric 4.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        // ReSharper disable once UnusedMember.Global
        public int Metric4 { get; private set; }

        /// <summary>
        /// Gets the metric 5.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        // ReSharper disable once UnusedMember.Global
        public int Metric5 { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        // ReSharper disable once UnusedMember.Global
        public string Name { get; private set; }

        /// <summary>
        /// Gets the next hop.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string NextHop { get; private set; }

        /// <summary>
        /// Gets the protocol.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        // ReSharper disable once UnusedMember.Global
        public uint Protocol { get; private set; }

        /// <summary>
        /// Gets the status.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        // ReSharper disable once UnusedMember.Global
        public string Status { get; private set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        // ReSharper disable once UnusedMember.Global
        public uint Type { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add change route rule.
        /// </summary>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="gateWay">
        /// The gate way.
        /// </param>
        /// <param name="metric">
        /// The metric.
        /// </param>
        /// <param name="netMask">
        /// The net mask.
        /// </param>
        /// <param name="interfaceId">
        /// The interface id.
        /// </param>
        /// <param name="change">
        /// The change.
        /// </param>
        public static void AddChangeRouteRule(
            IPAddress destination, 
            IPAddress gateWay, 
            int metric, 
            IPAddress netMask = null, 
            int interfaceId = -1, 
            bool change = false)
        {
            Process p = Common.CreateProcess(
                "route",
                string.Format("{0} {1} {2}{3} " + "metric {4}{5}", change ? "CHANGE" : "ADD", destination, netMask != null ? "mask " + netMask + " " : string.Empty, gateWay, metric, (interfaceId > -1) ? " IF " + interfaceId : string.Empty));
            p.WaitForExit();
        }

        /// <summary>
        /// The get best interface for IP.
        /// </summary>
        /// <param name="ip">
        /// The IP.
        /// </param>
        /// <returns>
        /// The <see cref="NetworkAdapter"/>.
        /// </returns>
        // ReSharper disable once UnusedMember.Global
        public static NetworkAdapter GetBestInterfaceForIp(IPAddress ip)
        {
            int ad = GetBestInterfaceIndexForIp(ip);
            return ad == -1 ? null : NetworkAdapter.GetNetworks().FirstOrDefault(n => n.InterfaceIndex == ad);
        }

        /// <summary>
        /// The get best interface index for IP.
        /// </summary>
        /// <param name="ip">
        /// The IP.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int GetBestInterfaceIndexForIp(IPAddress ip)
        {
            uint interfaceNo;
#pragma warning disable 618
            int res = GetBestInterface((uint)ip.Address, out interfaceNo);
#pragma warning restore 618
            return res != 0 ? -1 : (int)interfaceNo;
        }

        /// <summary>
        /// The get current table.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static List<IP4RouteTable> GetCurrentTable()
        {
            ManagementClass mc = new ManagementClass("Win32_IP4RouteTable");
            ManagementObjectCollection moc = mc.GetInstances();

            return (from ManagementObject mo in moc select new IP4RouteTable(mo)).ToList();
        }

        /// <summary>
        /// The refresh properties.
        /// </summary>
        public void RefreshProperties()
        {
            this.managementSource.Get();
            foreach (PropertyInfo p in this.GetType().GetProperties())
            {
                p.SetValue(this, this.managementSource[p.Name], null);
            }
        }

        /// <summary>
        /// The remove route rule.
        /// </summary>
        public void RemoveRouteRule()
        {
            if (string.IsNullOrEmpty(this.Destination))
            {
                return;
            }

            string str = this.Destination;
            if (!string.IsNullOrEmpty(this.NextHop))
            {
                str += " " + this.NextHop;
            }

            if (!string.IsNullOrEmpty(this.Mask))
            {
                str = str.Replace(" ", " mask " + this.Mask + " ");
            }

            if (this.InterfaceIndex > 0)
            {
                str += " IF " + this.InterfaceIndex;
            }

            Process p = Common.CreateProcess("route", "DELETE " + str);
            p.WaitForExit();
        }

        /// <summary>
        /// The set metric 1.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        public void SetMetric1(int value)
        {
            if (!this.isLegacy)
            {
                try
                {
                    this.managementSource.SetPropertyValue("Metric1", value);
                    this.managementSource.Put();
                }
                catch (Exception)
                {
                    this.isLegacy = true;
                }
            }

            if (this.isLegacy)
            {
                AddChangeRouteRule(
                    IPAddress.Parse(this.Destination), 
                    IPAddress.Parse(this.NextHop), 
                    value, 
                    IPAddress.Parse(this.Mask), 
                    this.InterfaceIndex, 
                    true);
            }

            this.RefreshProperties();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get best interface.
        /// </summary>
        /// <param name="destAddr">
        /// The destination address.
        /// </param>
        /// <param name="bestIfIndex">
        /// The best interface index.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern int GetBestInterface(uint destAddr, out uint bestIfIndex);

        #endregion
    }
}