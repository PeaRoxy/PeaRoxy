using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace PeaRoxy.Windows.Network.Win32_WMI
{
    public class NetworkAdapterConfiguration
    {
        public bool ArpAlwaysSourceRoute { get; private set; }
        public bool ArpUseEtherSNAP { get; private set; }
        public string Caption { get; private set; }
        public string DatabasePath { get; private set; }
        public string[] DefaultIPGateway { get; private set; }
        public string Description { get; private set; }
        public bool DHCPEnabled { get; private set; }
        public string DHCPServer { get; private set; }
        public string DNSDomain { get; private set; }
        public string[] DNSDomainSuffixSearchOrder { get; private set; }
        public bool DNSEnabledForWINSResolution { get; private set; }
        public string DNSHostName { get; private set; }
        public string[] DNSServerSearchOrder { get; private set; }
        public bool DomainDNSRegistrationEnabled { get; private set; }
        public uint ForwardBufferMemory { get; private set; }
        public bool FullDNSRegistrationEnabled { get; private set; }
        public ushort[] GatewayCostMetric { get; private set; }
        public uint Index { get; private set; }
        public uint InterfaceIndex { get; private set; }
        public string[] IPAddress { get; private set; }
        public uint IPConnectionMetric { get; private set; }
        public bool IPEnabled { get; private set; }
        public bool IPFilterSecurityEnabled { get; private set; }
        public bool IPPortSecurityEnabled { get; private set; }
        public string[] IPSecPermitIPProtocols { get; private set; }
        public string[] IPSecPermitTCPPorts { get; private set; }
        public string[] IPSecPermitUDPPorts { get; private set; }
        public string[] IPSubnet { get; private set; }
        public bool IPUseZeroBroadcast { get; private set; }
        public string IPXAddress { get; private set; }
        public bool IPXEnabled { get; private set; }
        public uint[] IPXFrameType { get; private set; }
        public uint IPXMediaType { get; private set; }
        public string[] IPXNetworkNumber { get; private set; }
        public string IPXVirtualNetNumber { get; private set; }
        public uint KeepAliveInterval { get; private set; }
        public uint KeepAliveTime { get; private set; }
        public string MACAddress { get; private set; }
        public uint MTU { get; private set; }
        public uint NumForwardPackets { get; private set; }
        public bool PMTUBHDetectEnabled { get; private set; }
        public bool PMTUDiscoveryEnabled { get; private set; }
        public string ServiceName { get; private set; }
        public string settingID { get; private set; }
        public uint TcpipNetbiosOptions { get; private set; }
        public uint TcpMaxConnectRetransmissions { get; private set; }
        public uint TcpMaxDataRetransmissions { get; private set; }
        public uint TcpNumConnections { get; private set; }
        public bool TcpUseRFC1122UrgentPointer { get; private set; }
        public ushort TcpWindowSize { get; private set; }
        public bool WINSEnableLMHostsLookup { get; private set; }
        public string WINSHostLookupFile { get; private set; }
        public string WINSPrimaryServer { get; private set; }
        public string WINSScopeID { get; private set; }
        public string WINSSecondaryServer { get; private set; }
        private ManagementObject ManagementSource;

        public static List<NetworkAdapterConfiguration> GetNetworksConfiguration()
        {
            List<NetworkAdapterConfiguration> nas = new List<NetworkAdapterConfiguration>();
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
                nas.Add(new NetworkAdapterConfiguration(mo));
            return nas;
        }

        public NetworkAdapterConfiguration(ManagementObject mo)
        {
            ManagementSource = mo;
            RefreshProperties();
        }

        public void RefreshProperties()
        {
            ManagementSource.Get();
            foreach (System.Reflection.PropertyInfo p in this.GetType().GetProperties())
                p.SetValue(this, ManagementSource[p.Name], null);
        }

        public bool SetIPAddresses(string ipAddresses, string subnetMask)
        {
            return SetIPAddresses(ipAddresses.Split(','), subnetMask);
        }

        public bool SetIPAddresses(string[] ipAddresses, string subnetMask)
        {
            ManagementBaseObject newIP = ManagementSource.GetMethodParameters("EnableStatic");
            newIP["IPAddress"] = ipAddresses;
            newIP["SubnetMask"] = new string[] { subnetMask };
            bool res  = ((uint)ManagementSource.InvokeMethod("EnableStatic", newIP, null)["ReturnValue"]) == 0;
            RefreshProperties();
            return res;
        }

        public bool SetDnsSearchOrder(string dnsServers)
        {
            return SetDnsSearchOrder(dnsServers.Split(','));
        }

        public bool SetDnsSearchOrder(string[] dnsServers)
        {
            ManagementBaseObject newDNS = ManagementSource.GetMethodParameters("SetDNSServerSearchOrder");
            newDNS["DNSServerSearchOrder"] = dnsServers;
            bool res = ((uint)ManagementSource.InvokeMethod("SetDNSServerSearchOrder", newDNS, null)["ReturnValue"]) == 0;
            RefreshProperties();
            return res;
        }
        public void EnableDHCP()
        {
            ManagementSource.InvokeMethod("EnableDHCP", null, null);
            SetIPAddresses(new string[] { null }, null);
        }
    }
}
