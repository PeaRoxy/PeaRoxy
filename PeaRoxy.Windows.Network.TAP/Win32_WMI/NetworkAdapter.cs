// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NetworkAdapter.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The network adapter.
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
    using System.Reflection;

    #endregion

    /// <summary>
    /// The network adapter.
    /// </summary>
    public class NetworkAdapter
    {
        #region Fields

        /// <summary>
        /// The management source.
        /// </summary>
        private readonly ManagementObject managementSource;

        /// <summary>
        /// The net config.
        /// </summary>
        private NetworkAdapterConfiguration netConfig;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkAdapter"/> class.
        /// </summary>
        /// <param name="mo">
        /// The mo.
        /// </param>
        public NetworkAdapter(ManagementObject mo)
        {
            this.managementSource = mo;
            foreach (PropertyInfo p in this.GetType().GetProperties())
            {
                try
                {
                    p.SetValue(this, this.managementSource[p.Name], null);
                }
                catch (Exception)
                {
                }
            }
        }

        #endregion

        #region Public Properties
        // ReSharper disable UnusedMember.Global
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Gets the adapter type.
        /// </summary>
        public string AdapterType { get; private set; }

        /// <summary>
        /// Gets the adapter type id.
        /// </summary>
        public ushort AdapterTypeID { get; private set; }

        /// <summary>
        /// Gets a value indicating whether auto sense.
        /// </summary>
        public bool AutoSense { get; private set; }

        /// <summary>
        /// Gets the availability.
        /// </summary>
        public ushort Availability { get; private set; }

        /// <summary>
        /// Gets the caption.
        /// </summary>
        public string Caption { get; private set; }

        /// <summary>
        /// Gets the config manager error code.
        /// </summary>
        public uint ConfigManagerErrorCode { get; private set; }

        /// <summary>
        /// Gets a value indicating whether config manager user config.
        /// </summary>
        public bool ConfigManagerUserConfig { get; private set; }

        /// <summary>
        /// Gets the creation class name.
        /// </summary>
        public string CreationClassName { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the device id.
        /// </summary>
        public string DeviceID { get; private set; }

        /// <summary>
        /// Gets a value indicating whether error cleared.
        /// </summary>
        public bool ErrorCleared { get; private set; }

        /// <summary>
        /// Gets the error description.
        /// </summary>
        public string ErrorDescription { get; private set; }

        /// <summary>
        /// Gets the GUID.
        /// </summary>
        public string GUID { get; private set; }

        /// <summary>
        /// Gets the index.
        /// </summary>
        public uint Index { get; private set; }

        /// <summary>
        /// Gets a value indicating whether installed.
        /// </summary>
        public bool Installed { get; private set; }

        /// <summary>
        /// Gets the interface index.
        /// </summary>
        public uint InterfaceIndex { get; private set; }

        /// <summary>
        /// Gets the last error code.
        /// </summary>
        public uint LastErrorCode { get; private set; }

        /// <summary>
        /// Gets the mac address.
        /// </summary>
        public string MACAddress { get; private set; }

        /// <summary>
        /// Gets the manufacturer.
        /// </summary>
        public string Manufacturer { get; private set; }

        /// <summary>
        /// Gets the max number controlled.
        /// </summary>
        public uint MaxNumberControlled { get; private set; }

        /// <summary>
        /// Gets the max speed.
        /// </summary>
        public ulong MaxSpeed { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the net connection id.
        /// </summary>
        public string NetConnectionID { get; private set; }

        /// <summary>
        /// Gets the net connection status.
        /// </summary>
        public ushort NetConnectionStatus { get; private set; }

        /// <summary>
        /// Gets a value indicating whether net enabled.
        /// </summary>
        public bool NetEnabled { get; private set; }

        /// <summary>
        /// Gets the network addresses.
        /// </summary>
        public string[] NetworkAddresses { get; private set; }

        /// <summary>
        /// Gets the PNP device id.
        /// </summary>
        public string PNPDeviceID { get; private set; }

        /// <summary>
        /// Gets the permanent address.
        /// </summary>
        public string PermanentAddress { get; private set; }

        /// <summary>
        /// Gets a value indicating whether physical adapter.
        /// </summary>
        public bool PhysicalAdapter { get; private set; }

        /// <summary>
        /// Gets the power management capabilities.
        /// </summary>
        public ushort[] PowerManagementCapabilities { get; private set; }

        /// <summary>
        /// Gets a value indicating whether power management supported.
        /// </summary>
        public bool PowerManagementSupported { get; private set; }

        /// <summary>
        /// Gets the product name.
        /// </summary>
        public string ProductName { get; private set; }

        /// <summary>
        /// Gets the service name.
        /// </summary>
        public string ServiceName { get; private set; }

        /// <summary>
        /// Gets the speed.
        /// </summary>
        public ulong Speed { get; private set; }

        /// <summary>
        /// Gets the status.
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// Gets the status info.
        /// </summary>
        public ushort StatusInfo { get; private set; }

        /// <summary>
        /// Gets the system creation class name.
        /// </summary>
        public string SystemCreationClassName { get; private set; }

        /// <summary>
        /// Gets the system name.
        /// </summary>
        public string SystemName { get; private set; }

        // ReSharper restore UnusedAutoPropertyAccessor.Local
        // ReSharper restore UnusedMember.Global
        // ReSharper restore InconsistentNaming
        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get by name.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="NetworkAdapter"/>.
        /// </returns>
        public static NetworkAdapter GetByName(string name)
        {
            name = name.ToLower().Trim();
            return GetNetworks().FirstOrDefault(na => na.NetConnectionID != null && na.NetConnectionID.ToLower().Trim() == name);
        }

        /// <summary>
        /// The get by service name.
        /// </summary>
        /// <param name="serviceName">
        /// The service name.
        /// </param>
        /// <returns>
        /// The <see cref="NetworkAdapter"/>.
        /// </returns>
        public static NetworkAdapter GetByServiceName(string serviceName)
        {
            serviceName = serviceName.ToLower().Trim();
            return GetNetworks().FirstOrDefault(na => na.ServiceName != null && na.ServiceName.ToLower().Trim() == serviceName);
        }

        /// <summary>
        /// The get networks.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        public static IEnumerable<NetworkAdapter> GetNetworks()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapter");
            ManagementObjectCollection moc = mc.GetInstances();
            return (from ManagementObject mo in moc select new NetworkAdapter(mo)).ToList();
        }

        /// <summary>
        /// The disable.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        // ReSharper disable once UnusedMember.Global
        public bool Disable()
        {
            ManagementBaseObject managementBaseObject = this.managementSource.InvokeMethod("Disable", null, null);
            return managementBaseObject != null && ((uint)managementBaseObject["ReturnValue"]) == 0;
        }

        /// <summary>
        /// The enable.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        // ReSharper disable once UnusedMember.Global
        public bool Enable()
        {
            ManagementBaseObject managementBaseObject = this.managementSource.InvokeMethod("Enable", null, null);
            return managementBaseObject != null && ((uint)managementBaseObject["ReturnValue"]) == 0;
        }

        /// <summary>
        /// The get forward rules.
        /// </summary>
        /// <returns>
        /// The <see>
        ///         <cref>List</cref>
        ///     </see>
        ///     .
        /// </returns>
        // ReSharper disable once UnusedMember.Global
        public List<IP4RouteTable> GetForwardRules()
        {
            return IP4RouteTable.GetCurrentTable().Where(r => r.InterfaceIndex == this.InterfaceIndex).ToList();
        }

        /// <summary>
        /// The get network configuration.
        /// </summary>
        /// <returns>
        /// The <see cref="NetworkAdapterConfiguration"/>.
        /// </returns>
        public NetworkAdapterConfiguration GetNetworkConfiguration()
        {
            if (this.netConfig != null)
            {
                return this.netConfig;
            }

            foreach (NetworkAdapterConfiguration nac in NetworkAdapterConfiguration.GetNetworksConfiguration().Where(nac => nac.InterfaceIndex == this.InterfaceIndex))
            {
                this.netConfig = nac;
                return nac;
            }

            return null;
        }

        /// <summary>
        /// The hard rename adapter.
        /// </summary>
        /// <param name="newName">
        /// The new name.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool HardRenameAdapter(string newName)
        {
            if (string.IsNullOrEmpty(this.NetConnectionID))
            {
                return false;
            }

            Process p = new Process
                            {
                                StartInfo =
                                    new ProcessStartInfo(
                                    "netsh",
                                    string.Format("interface set interface name=\"{0}\" newname=\"{1}\"", this.NetConnectionID, newName))
                                        {
                                            WindowStyle = ProcessWindowStyle.Hidden,
                                            RedirectStandardOutput = true,
                                            UseShellExecute = false
                                        }
                            };
            p.Start();
            p.WaitForExit();
            return p.StandardOutput.ReadLine() == string.Empty;
        }

        /// <summary>
        /// The refresh properties.
        /// </summary>
        public void RefreshProperties()
        {
            this.managementSource.Get();
            foreach (PropertyInfo p in this.GetType().GetProperties())
            {
                try
                {
                    p.SetValue(this, this.managementSource[p.Name], null);
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// The rename adapter.
        /// </summary>
        /// <param name="newName">
        /// The new name.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool RenameAdapter(string newName)
        {
            if (this.NetConnectionID == newName)
            {
                return true;
            }

            if (Environment.OSVersion.Version.Major >= 6)
            {
                this.Win6SetNetConnectionId(newName);
                this.RefreshProperties();
                if (this.NetConnectionID == newName)
                {
                    return true;
                }
            }

            bool res = this.HardRenameAdapter(newName);
            this.RefreshProperties();
            return res || this.NetConnectionID == newName;
        }

        /// <summary>
        /// The win 6_ set net connection id.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public void Win6SetNetConnectionId(string name)
        {
            this.managementSource.SetPropertyValue("NetConnectionID", name);
            this.managementSource.Put();
            this.RefreshProperties();
        }

        #endregion
    }
}