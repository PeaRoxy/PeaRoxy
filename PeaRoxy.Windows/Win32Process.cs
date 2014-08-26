namespace PeaRoxy.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Management;
    using System.Reflection;

    public class Win32Process
    {
        private static readonly Dictionary<int, Win32Process> ProcessCache = new Dictionary<int, Win32Process>();

        private readonly ManagementObject managementSource;

        public Win32Process(ManagementObject mo)
        {
            this.managementSource = mo;
            foreach (PropertyInfo p in this.GetType().GetProperties())
            {
                p.SetValue(this, this.managementSource[p.Name], null);
            }
        }

        public string Caption { get; set; }

        public string CommandLine { get; set; }

        public string Description { get; set; }

        public string ExecutablePath { get; set; }

        public string Name { get; set; }

        public UInt32 PageFileUsage { get; set; }

        public UInt32 ParentProcessId { get; set; }

        public UInt32 Priority { get; set; }

        public UInt32 ProcessId { get; set; }

        public UInt32 ThreadCount { get; set; }

        public static IEnumerable<Win32Process> GetProcesses()
        {
            ManagementClass mc = new ManagementClass("Win32_Process");
            ManagementObjectCollection moc = mc.GetInstances();

            return (from ManagementObject mo in moc select new Win32Process(mo)).ToList();
        }

        [DebuggerStepThrough]
        public static Win32Process GetProcessByPidWithCache(int pId)
        {
            try
            {
                if (ProcessCache.ContainsKey(pId))
                {
                    return ProcessCache[pId];
                }
                ProcessCache.Clear();
                IEnumerable<Win32Process> pr = GetProcesses();
                foreach (Win32Process process in pr)
                {
                    ProcessCache.Add((int)process.ProcessId, process);
                    if (process.ProcessId == pId)
                    {
                        return process;
                    }
                }
            }
            catch (Exception)
            {
            }

            return null;
        }
    }
}