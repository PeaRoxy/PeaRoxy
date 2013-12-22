// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NetworkAdapterConfiguration.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The network adapter configuration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.Network.TAP.Win32_WMI
{
    #region

    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Management;
    using System.Reflection;

    #endregion

    /// <summary>
    /// The network adapter configuration.
    /// </summary>
    public class NetworkAdapterConfiguration
    {
        #region Fields

        /// <summary>
        /// The management source.
        /// </summary>
        private readonly ManagementObject managementSource;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkAdapterConfiguration"/> class.
        /// </summary>
        /// <param name="mo">
        /// The mo.
        /// </param>
        public NetworkAdapterConfiguration(ManagementObject mo)
        {
            this.managementSource = mo;
            this.RefreshProperties();
        }

        #endregion

        #region Public Properties
        // ReSharper disable UnusedMember.Global
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Gets a value indicating whether arp always source route.
        /// </summary>
        public bool ArpAlwaysSourceRoute { get; private set; }
        
        /// <summary>
        /// Gets a value indicating whether arp use ether snap.
        /// </summary>
        public bool ArpUseEtherSNAP { get; private set; }

        /// <summary>
        /// Gets the caption.
        /// </summary>
        public string Caption { get; private set; }

        /// <summary>
        /// Gets a value indicating whether DHCP enabled.
        /// </summary>
        public bool DHCPEnabled { get; private set; }

        /// <summary>
        /// Gets the DHCP server.
        /// </summary>
        public string DHCPServer { get; private set; }

        /// <summary>
        /// Gets the DNS domain.
        /// </summary>
        public string DNSDomain { get; private set; }

        /// <summary>
        /// Gets the DNS domain suffix search order.
        /// </summary>
        public string[] DNSDomainSuffixSearchOrder { get; private set; }

        /// <summary>
        /// Gets a value indicating whether DNS enabled for wins resolution.
        /// </summary>
        public bool DNSEnabledForWINSResolution { get; private set; }

        /// <summary>
        /// Gets the DNS host name.
        /// </summary>
        public string DNSHostName { get; private set; }

        /// <summary>
        /// Gets the DNS server search order.
        /// </summary>
        public string[] DNSServerSearchOrder { get; private set; }

        /// <summary>
        /// Gets the database path.
        /// </summary>
        public string DatabasePath { get; private set; }

        /// <summary>
        /// Gets the default IP gateway.
        /// </summary>
        public string[] DefaultIPGateway { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets a value indicating whether domain DNS registration enabled.
        /// </summary>
        public bool DomainDNSRegistrationEnabled { get; private set; }

        /// <summary>
        /// Gets the forward buffer memory.
        /// </summary>
        public uint ForwardBufferMemory { get; private set; }

        /// <summary>
        /// Gets a value indicating whether full DNS registration enabled.
        /// </summary>
        public bool FullDNSRegistrationEnabled { get; private set; }

        /// <summary>
        /// Gets the gateway cost metric.
        /// </summary>
        public ushort[] GatewayCostMetric { get; private set; }

        /// <summary>
        /// Gets the IP address.
        /// </summary>
        public string[] IPAddress { get; private set; }

        /// <summary>
        /// Gets the IP connection metric.
        /// </summary>
        public uint IPConnectionMetric { get; private set; }

        /// <summary>
        /// Gets a value indicating whether IP enabled.
        /// </summary>
        public bool IPEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether IP filter security enabled.
        /// </summary>
        public bool IPFilterSecurityEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether IP port security enabled.
        /// </summary>
        public bool IPPortSecurityEnabled { get; private set; }

        /// <summary>
        /// Gets the IP sec permit IP protocols.
        /// </summary>
        public string[] IPSecPermitIPProtocols { get; private set; }

        /// <summary>
        /// Gets the IP sec permit TCP ports.
        /// </summary>
        public string[] IPSecPermitTCPPorts { get; private set; }

        /// <summary>
        /// Gets the IP sec permit UDP ports.
        /// </summary>
        public string[] IPSecPermitUDPPorts { get; private set; }

        /// <summary>
        /// Gets the IP subnet.
        /// </summary>
        public string[] IPSubnet { get; private set; }

        /// <summary>
        /// Gets a value indicating whether IP use zero broadcast.
        /// </summary>
        public bool IPUseZeroBroadcast { get; private set; }

        /// <summary>
        /// Gets the IPX address.
        /// </summary>
        public string IPXAddress { get; private set; }

        /// <summary>
        /// Gets a value indicating whether IPX enabled.
        /// </summary>
        public bool IPXEnabled { get; private set; }

        /// <summary>
        /// Gets the IPX frame type.
        /// </summary>
        public uint[] IPXFrameType { get; private set; }

        /// <summary>
        /// Gets the IPX media type.
        /// </summary>
        public uint IPXMediaType { get; private set; }

        /// <summary>
        /// Gets the IPX network number.
        /// </summary>
        public string[] IPXNetworkNumber { get; private set; }

        /// <summary>
        /// Gets the IPX virtual net number.
        /// </summary>
        public string IPXVirtualNetNumber { get; private set; }

        /// <summary>
        /// Gets the index.
        /// </summary>
        public uint Index { get; private set; }

        /// <summary>
        /// Gets the interface index.
        /// </summary>
        public uint InterfaceIndex { get; private set; }

        /// <summary>
        /// Gets the keep alive interval.
        /// </summary>
        public uint KeepAliveInterval { get; private set; }

        /// <summary>
        /// Gets the keep alive time.
        /// </summary>
        public uint KeepAliveTime { get; private set; }

        /// <summary>
        /// Gets the mac address.
        /// </summary>
        public string MACAddress { get; private set; }

        /// <summary>
        /// Gets the MTU.
        /// </summary>
        public uint MTU { get; private set; }

        /// <summary>
        /// Gets the number forward packets.
        /// </summary>
        public uint NumForwardPackets { get; private set; }

        /// <summary>
        /// Gets a value indicating whether PMTUBH detect enabled.
        /// </summary>
        public bool PMTUBHDetectEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether PMTU discovery enabled.
        /// </summary>
        public bool PMTUDiscoveryEnabled { get; private set; }

        /// <summary>
        /// Gets the service name.
        /// </summary>
        public string ServiceName { get; private set; }

        /// <summary>
        /// Gets the TCP max connect retransmissions.
        /// </summary>
        public uint TcpMaxConnectRetransmissions { get; private set; }

        /// <summary>
        /// Gets the TCP max data retransmissions.
        /// </summary>
        public uint TcpMaxDataRetransmissions { get; private set; }

        /// <summary>
        /// Gets the TCP number connections.
        /// </summary>
        public uint TcpNumConnections { get; private set; }

        /// <summary>
        /// Gets a value indicating whether TCP use rfc1122 urgent pointer.
        /// </summary>
        public bool TcpUseRFC1122UrgentPointer { get; private set; }

        /// <summary>
        /// Gets the TCP window size.
        /// </summary>
        public ushort TcpWindowSize { get; private set; }

        /// <summary>
        /// Gets the TCPIP NetBios options.
        /// </summary>
        public uint TcpipNetbiosOptions { get; private set; }

        /// <summary>
        /// Gets a value indicating whether wins enable lm hosts lookup.
        /// </summary>
        public bool WINSEnableLMHostsLookup { get; private set; }

        /// <summary>
        /// Gets the wins host lookup file.
        /// </summary>
        public string WINSHostLookupFile { get; private set; }

        /// <summary>
        /// Gets the wins primary server.
        /// </summary>
        public string WINSPrimaryServer { get; private set; }

        /// <summary>
        /// Gets the wins scope id.
        /// </summary>
        public string WINSScopeID { get; private set; }

        /// <summary>
        /// Gets the wins secondary server.
        /// </summary>
        public string WINSSecondaryServer { get; private set; }
        
        /// <summary>
        /// Gets the setting id.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        public string settingID { get; private set; }

        // ReSharper restore UnusedAutoPropertyAccessor.Local
        // ReSharper restore UnusedMember.Global
        // ReSharper restore InconsistentNaming
        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get networks configuration.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static IEnumerable<NetworkAdapterConfiguration> GetNetworksConfiguration()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();

            return (from ManagementObject mo in moc select new NetworkAdapterConfiguration(mo)).ToList();
        }

        /// <summary>
        /// The enable DHCP.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void EnableDhcp()
        {
            this.managementSource.InvokeMethod("EnableDHCP", null, null);
            this.SetIpAddresses(new string[] { null }, null);
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
        /// The set DNS search order.
        /// </summary>
        /// <param name="dnsServers">
        /// The DNS servers.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool SetDnsSearchOrder(string dnsServers)
        {
            return this.SetDnsSearchOrder(dnsServers.Split(','));
        }

        /// <summary>
        /// The set DNS search order.
        /// </summary>
        /// <param name="dnsServers">
        /// The DNS servers.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool SetDnsSearchOrder(string[] dnsServers)
        {
            ManagementBaseObject newDns = this.managementSource.GetMethodParameters("SetDNSServerSearchOrder");
            newDns["DNSServerSearchOrder"] = dnsServers;
            ManagementBaseObject managementBaseObject = this.managementSource.InvokeMethod("SetDNSServerSearchOrder", newDns, null);
            bool res = managementBaseObject != null && ((uint)managementBaseObject["ReturnValue"]) == 0;
            this.RefreshProperties();
            return res;
        }

        /// <summary>
        /// The set IP addresses.
        /// </summary>
        /// <param name="ipAddresses">
        /// The IP addresses.
        /// </param>
        /// <param name="subnetMask">
        /// The subnet mask.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public bool SetIpAddresses(string ipAddresses, string subnetMask)
        {
            return this.SetIpAddresses(ipAddresses.Split(','), subnetMask);
        }

        /// <summary>
        /// The set IP addresses.
        /// </summary>
        /// <param name="ipAddresses">
        /// The IP addresses.
        /// </param>
        /// <param name="subnetMask">
        /// The subnet mask.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public bool SetIpAddresses(string[] ipAddresses, string subnetMask)
        {
            ManagementBaseObject newIp = this.managementSource.GetMethodParameters("EnableStatic");
            newIp["IPAddress"] = ipAddresses;
            newIp["SubnetMask"] = new[] { subnetMask };
            ManagementBaseObject managementBaseObject = this.managementSource.InvokeMethod("EnableStatic", newIp, null);
            bool res = managementBaseObject != null && ((uint)managementBaseObject["ReturnValue"]) == 0;
            this.RefreshProperties();
            return res;
        }

        #endregion
    }
}