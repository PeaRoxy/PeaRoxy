using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace PeaRoxy.Windows.Network.Win32_WMI
{
    public class NetworkAdapter
    {
        public string AdapterType { get; private set; }
        public ushort AdapterTypeID { get; private set; }
        public bool AutoSense { get; private set; }
        public ushort Availability { get; private set; }
        public string Caption { get; private set; }
        public uint ConfigManagerErrorCode { get; private set; }
        public bool ConfigManagerUserConfig { get; private set; }
        public string CreationClassName { get; private set; }
        public string Description { get; private set; }
        public string DeviceID { get; private set; }
        public bool ErrorCleared { get; private set; }
        public string ErrorDescription { get; private set; }
        public string GUID { get; private set; }
        public uint Index { get; private set; }
        public bool Installed { get; private set; }
        public uint InterfaceIndex { get; private set; }
        public uint LastErrorCode { get; private set; }
        public string MACAddress { get; private set; }
        public string Manufacturer { get; private set; }
        public uint MaxNumberControlled { get; private set; }
        public ulong MaxSpeed { get; private set; }
        public string Name { get; private set; }
        public string NetConnectionID { get; private set; }
        public ushort NetConnectionStatus { get; private set; }
        public bool NetEnabled { get; private set; }
        public string[] NetworkAddresses { get; private set; }
        public string PermanentAddress { get; private set; }
        public bool PhysicalAdapter { get; private set; }
        public string PNPDeviceID { get; private set; }
        public ushort[] PowerManagementCapabilities { get; private set; }
        public bool PowerManagementSupported { get; private set; }
        public string ProductName { get; private set; }
        public string ServiceName { get; private set; }
        public ulong Speed { get; private set; }
        public string Status { get; private set; }
        public ushort StatusInfo { get; private set; }
        public string SystemCreationClassName { get; private set; }
        public string SystemName { get; private set; }
        private ManagementObject ManagementSource;
        private NetworkAdapterConfiguration netConfig;
        public static List<NetworkAdapter> GetNetworks()
        {
            List<NetworkAdapter> nas = new List<NetworkAdapter>();
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapter");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
                nas.Add(new NetworkAdapter(mo));
            return nas;
        }

        public static NetworkAdapter GetByName(string Name)
        {
            Name = Name.ToLower().Trim();
            foreach (NetworkAdapter na in NetworkAdapter.GetNetworks())
                if (na.NetConnectionID != null && na.NetConnectionID.ToLower().Trim() == Name)
                    return na;
            return null;
        }

        public static NetworkAdapter GetByServiceName(string ServiceName)
        {
            ServiceName = ServiceName.ToLower().Trim();
            foreach (NetworkAdapter na in NetworkAdapter.GetNetworks())
                if (na.ServiceName != null && na.ServiceName.ToLower().Trim() == ServiceName)
                    return na;
            return null;
        }

        public NetworkAdapter(ManagementObject mo)
        {
            ManagementSource = mo;
            foreach (System.Reflection.PropertyInfo p in this.GetType().GetProperties())
                try
                {
                    p.SetValue(this, ManagementSource[p.Name], null);
                }
                catch (Exception) { }
                
        }

        public void RefreshProperties()
        {
            ManagementSource.Get();
            foreach (System.Reflection.PropertyInfo p in this.GetType().GetProperties())
                try
                {
                    p.SetValue(this, ManagementSource[p.Name], null);
                }
                catch (Exception) { }
        }

        public void Win6_SetNetConnectionID(string name)
        {
            ManagementSource.SetPropertyValue("NetConnectionID", name);
            ManagementSource.Put();
            RefreshProperties();
        }

        public bool Enable()
        {
            return ((uint)ManagementSource.InvokeMethod("Enable", null, null)["ReturnValue"]) == 0;
        }

        public bool Disable()
        {
            return ((uint)ManagementSource.InvokeMethod("Disable", null, null)["ReturnValue"]) == 0;
        }

        public bool HardRenameAdapter(string NewName)
        {
            if (this.NetConnectionID == null || this.NetConnectionID == string.Empty)
                return false;
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo(
                "netsh",
                "interface set interface name=\"" + this.NetConnectionID + "\" newname=\"" + NewName + "\""
                );
            p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();
            p.WaitForExit();
            return p.StandardOutput.ReadLine() == string.Empty;
        }

        public bool RenameAdapter(string NewName)
        {
            if (this.NetConnectionID == NewName)
                return true;
            if (Environment.OSVersion.Version.Major >= 6)
            {
                this.Win6_SetNetConnectionID(NewName);
                this.RefreshProperties();
                if (this.NetConnectionID == NewName)
                    return true;
            }
            bool res = this.HardRenameAdapter(NewName);
            this.RefreshProperties();
            return (res || this.NetConnectionID == NewName);
        }

        public NetworkAdapterConfiguration GetNetworkConfiguration()
        {
            if (netConfig != null)
                return netConfig;
            foreach (NetworkAdapterConfiguration nac in NetworkAdapterConfiguration.GetNetworksConfiguration())
                if (nac.InterfaceIndex == this.InterfaceIndex)
                {
                    netConfig = nac;
                    return nac;
                }
            return null;
        }

        public List<IP4RouteTable> GetForwardRules()
        {
            List<IP4RouteTable> rules = new List<IP4RouteTable>();
            foreach (IP4RouteTable r in IP4RouteTable.GetCurrentTable())
                if (r.InterfaceIndex == this.InterfaceIndex)
                    rules.Add(r);
            return rules;
        }
    }
}
