// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TapTunnelModule.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The tap tunnel module.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.Network.TAP
{
    #region

    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Threading;

    using PeaRoxy.Windows.Network.TAP.Win32_WMI;

    using Common = PeaRoxy.Windows.Common;

    #endregion

    /// <summary>
    /// The tap tunnel.
    /// </summary>
    public static class TapTunnelModule
    {
        #region Fields

        /// <summary>
        /// The IP subnet.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        private static readonly IPAddress IpSubnet = IPAddress.Parse("255.255.255.0");

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TapTunnelModule"/> class.
        /// </summary>
        static TapTunnelModule()
        {
            AdapterAddressRange = IPAddress.Parse("10.0.0.0");
            DnsResolvingAddress = null;
            DnsResolvingAddress2 = null;
            AutoDnsResolvingAddress = IPAddress.Parse("8.8.8.8");
            SocksProxyEndPoint = new IPEndPoint(IPAddress.Loopback, 1080);
            ExceptionIPs = new IPAddress[0];
            TunnelName = "Tap Tunnel";
            AutoDnsResolving = false;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the adapter address range.
        /// </summary>
        public static IPAddress AdapterAddressRange { get; set; }

        /// <summary>
        /// Gets or sets the DNS resolving address.
        /// </summary>
        public static IPAddress DnsResolvingAddress { get; set; }

        /// <summary>
        /// Gets or sets the DNS resolving address 2.
        /// </summary>
        public static IPAddress DnsResolvingAddress2 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether auto DNS resolving.
        /// </summary>
        public static bool AutoDnsResolving { get; set; }

        /// <summary>
        /// Gets or sets the auto DNS resolving address.
        /// </summary>
        public static IPAddress AutoDnsResolvingAddress { get; set; }

        /// <summary>
        /// Gets or sets the exception i ps.
        /// </summary>
        public static IPAddress[] ExceptionIPs { get; set; }

        /// <summary>
        /// Gets or sets the socks proxy end point.
        /// </summary>
        public static IPEndPoint SocksProxyEndPoint { get; set; }

        /// <summary>
        /// Gets or sets the tunnel name.
        /// </summary>
        public static string TunnelName { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The flash DNS cache.
        /// </summary>
        public static void FlashDnsCache()
        {
            Common.CreateProcess("ipconfig", "/flushdns", true, false, false, true).WaitForExit();
        }

        /// <summary>
        /// The is running.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsRunning()
        {
            if (!Tun2Socks.IsTun2SocksRunning() || !Dns2Socks.IsDns2SocksRunning())
            {
                Tun2Socks.StopTun2Socks();
                Tun2Socks.CleanAllTun2Socks();
                Dns2Socks.StopDns2Socks();
                Dns2Socks.CleanAllDns2Socks();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Clean all tunnel processes
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static void CleanAllTunnelProcesses()
        {
            Tun2Socks.CleanAllTun2Socks();
            Dns2Socks.CleanAllDns2Socks();
        }

        /// <summary>
        /// The start tunnel.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool StartTunnel()
        {
            if (!CommonLibrary.Common.IsValidIpSubnet(IpSubnet))
            {
                return false;
            }

            AdapterAddressRange = CommonLibrary.Common.MergeIpIntoIpSubnet(
                AdapterAddressRange,
                IpSubnet,
                IPAddress.Parse("10.0.0.1"));
            IPAddress addressGateWay = CommonLibrary.Common.MergeIpIntoIpSubnet(
                AdapterAddressRange,
                IpSubnet,
                IPAddress.Parse("10.0.0.2"));
            NetworkAdapter net = TapAdapter.InstallAnAdapter(TunnelName);
            if (net == null)
            {
                return false;
            }

            if (
                !Tun2Socks.StartTun2Socks(
                    net,
                    AdapterAddressRange,
                    IpSubnet,
                    addressGateWay,
                    SocksProxyEndPoint))
            {
                return false;
            }

            NetworkAdapterConfiguration netconfig = net.GetNetworkConfiguration();
            if (netconfig == null)
            {
                return false;
            }

            int timeout = 30;
            timeout = timeout / 3;
            string dnsString = string.Empty;

            if (AutoDnsResolving && AutoDnsResolvingAddress != null)
            {
                Dns2Socks.StartDns2Socks(AutoDnsResolvingAddress, SocksProxyEndPoint);
                dnsString = SocksProxyEndPoint.Address.ToString();
            }
            else if (DnsResolvingAddress != null && DnsResolvingAddress2 != null)
            {
                dnsString = DnsResolvingAddress + "," + DnsResolvingAddress2;
            }
            else if (DnsResolvingAddress != null)
            {
                dnsString = DnsResolvingAddress.ToString();
            }
            else if (DnsResolvingAddress2 != null)
            {
                dnsString = DnsResolvingAddress2.ToString();
            }

            while (true)
            {
                if (netconfig.SetIpAddresses(AdapterAddressRange.ToString(), IpSubnet.ToString())
                    && netconfig.SetDnsSearchOrder(dnsString))
                {
                    break;
                }

                if (timeout == 0)
                {
                    return false;
                }

                timeout--;
                Thread.Sleep(3000);
            }

            IP4RouteTable.AddChangeRouteRule(
                IPAddress.Parse("0.0.0.0"),
                addressGateWay,
                2,
                IPAddress.Parse("0.0.0.0"),
                (int)net.InterfaceIndex);
            int lowestMetric = 1000;
            IP4RouteTable me = null;
            List<IP4RouteTable> internetRules = new List<IP4RouteTable>();
            foreach (IP4RouteTable r in IP4RouteTable.GetCurrentTable().ToArray())
            {
                if (r.Destination == "0.0.0.0")
                {
                    if (r.InterfaceIndex == net.InterfaceIndex)
                    {
                        me = r;
                    }
                    else
                    {
                        internetRules.Add(r);
                        if (lowestMetric > r.Metric1)
                        {
                            lowestMetric = r.Metric1;
                        }
                    }
                }
            }

            if (me == null)
            {
                return false;
            }

            foreach (IP4RouteTable ip4 in
                IP4RouteTable.GetCurrentTable()
                    .Where(
                        ip4 =>
                        ExceptionIPs.Any(
                            ip =>
                            ip4.Destination != null && ip4.NextHop != null && ip4.Mask != null
                            && ip4.Destination == ip.ToString() && ip4.Mask == "255.255.255.255"
                            && ip4.NextHop != "0.0.0.0")))
            {
                ip4.RemoveRouteRule();
            }

            int des = 0;
            if (me.Metric1 >= lowestMetric)
            {
                des = (me.Metric1 - lowestMetric) + 1;
            }

            foreach (IP4RouteTable r in internetRules)
            {
                if (des > 0)
                {
                    r.SetMetric1(r.Metric1 + des);
                }

                if (string.IsNullOrEmpty(r.NextHop))
                {
                    continue;
                }

                foreach (IPAddress ip in ExceptionIPs)
                {
                    // if (IP4RouteTable.GetBestInterfaceIndexForIP(ip) == net.InterfaceIndex)
                    IP4RouteTable.AddChangeRouteRule(ip, IPAddress.Parse(r.NextHop), 1, null, r.InterfaceIndex);
                }
            }

            FlashDnsCache();
            return true;
        }

        /// <summary>
        /// The stop tunnel.
        /// </summary>
        public static void StopTunnel()
        {
            Tun2Socks.StopTun2Socks();
            Tun2Socks.CleanAllTun2Socks();
            Dns2Socks.StopDns2Socks();
            Dns2Socks.CleanAllDns2Socks();
            foreach (
                IP4RouteTable ip4 in
                    IP4RouteTable.GetCurrentTable()
                        .Where(
                            ip4 =>
                            ExceptionIPs.Any(
                                ip =>
                                ip4.Destination != null && ip4.NextHop != null && ip4.Mask != null
                                && ip4.Destination == ip.ToString() && ip4.Mask == "255.255.255.255"
                                && ip4.NextHop != "0.0.0.0")))
            {
                ip4.RemoveRouteRule();
            }
        }

        #endregion
    }
}