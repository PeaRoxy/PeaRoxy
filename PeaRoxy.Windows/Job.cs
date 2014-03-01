// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowsConnections.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// Based on the code of Alexander Yezutov and Matt Howells
// Reference: http://stackoverflow.com/questions/6266820/
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows
{
    #region

    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    #endregion

    public class Job : IDisposable
    {
        #region Fields

        private IntPtr handle;

        private bool isDisposed;

        #endregion

        #region Constructors and Destructors

        public Job()
        {
            this.handle = CreateJobObject(IntPtr.Zero, null);
        }

        #endregion

        #region Public Methods and Operators

        public bool AddProcess(Process process)
        {
            return AssignProcessToJobObject(this.handle, process.Handle);
        }

        public bool AddProcess(int processId)
        {
            return this.AddProcess(Process.GetProcessById(processId));
        }

        public void Close()
        {
            CloseHandle(this.handle);
            this.handle = IntPtr.Zero;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SetBasicLimitInformation(JobObjectBasicLimitInformation limit)
        {
            JobObjectExtendedLimitInformation extendedInfo = new JobObjectExtendedLimitInformation
                                                                 {
                                                                     BasicLimitInformation
                                                                         = limit
                                                                 };
            this.SetExtendedLimitInformation(extendedInfo);
        }

        public void SetExtendedLimitInformation(JobObjectExtendedLimitInformation limit)
        {
            int length = Marshal.SizeOf(typeof(JobObjectExtendedLimitInformation));
            IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(limit, extendedInfoPtr, false);
            this.SetInformation<JobObjectExtendedLimitInformation>(JobObjectInfoType.ExtendedLimitInformation,  extendedInfoPtr);
        }

        public void SetInformation<T>(JobObjectInfoType infoType, IntPtr info)
        {
            SetInformation(infoType, info, Marshal.SizeOf(typeof(T)));
        }

        public void SetInformation(JobObjectInfoType infoType, IntPtr info, int infoSize)
        {
            if (!SetInformationJobObject(this.handle, infoType, info, (uint)infoSize))
            {
                throw new Exception(
                    string.Format("Unable to set information.  Error: {0}", Marshal.GetLastWin32Error()));
            }
        }

        #endregion

        #region Methods

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateJobObject(IntPtr a, string lpName);

        [DllImport("kernel32.dll")]
        private static extern bool SetInformationJobObject(
            IntPtr hJob,
            JobObjectInfoType infoType,
            IntPtr lpJobObjectInfo,
            UInt32 cbJobObjectInfoLength);

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.Close();
            this.isDisposed = true;
        }

        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IoCounters
    {
        public UInt64 ReadOperationCount;

        public UInt64 WriteOperationCount;

        public UInt64 OtherOperationCount;

        public UInt64 ReadTransferCount;

        public UInt64 WriteTransferCount;

        public UInt64 OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JobObjectBasicLimitInformation
    {
        public Int64 PerProcessUserTimeLimit;

        public Int64 PerJobUserTimeLimit;

        public LimitFlags Limits;

        public UIntPtr MinimumWorkingSetSize;

        public UIntPtr MaximumWorkingSetSize;

        public UInt32 ActiveProcessLimit;

        public UIntPtr Affinity;

        public UInt32 PriorityClass;

        public UInt32 SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SecurityAttributes
    {
        public UInt32 Length;

        public IntPtr SecurityDescriptor;

        public Int32 InheritHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JobObjectExtendedLimitInformation
    {
        public JobObjectBasicLimitInformation BasicLimitInformation;

        public IoCounters IoInfo;

        public UIntPtr ProcessMemoryLimit;

        public UIntPtr JobMemoryLimit;

        public UIntPtr PeakProcessMemoryUsed;

        public UIntPtr PeakJobMemoryUsed;
    }

    public enum JobObjectInfoType
    {
        AssociateCompletionPortInformation = 7,

        BasicLimitInformation = 2,

        BasicUiRestrictions = 4,

        EndOfJobTimeInformation = 6,

        ExtendedLimitInformation = 9,

        SecurityLimitInformation = 5,

        GroupInformation = 11
    }

    public enum LimitFlags : uint
    {
        JobObjectLimitActiveProcess = 0x00000008,

        JobObjectLimitAffinity = 0x00000010,

        JobObjectLimitBreakawayOk = 0x00000800,

        JobObjectLimitDieOnUnhandledException = 0x00000400,

        JobObjectLimitJobMemory = 0x00000200,

        JobObjectLimitJobTime = 0x00000004,

        JobObjectLimitKillOnJobClose = 0x00002000,

        JobObjectLimitPreserveJobTime = 0x00000040,

        JobObjectLimitPriorityClass = 0x00000020,

        JobObjectLimitProcessMemory = 0x00000100,

        JobObjectLimitProcessTime = 0x00000002,

        JobObjectLimitSchedulingClass = 0x00000080,

        JobObjectLimitSilentBreakawayOk = 0x00001000,

        JobObjectLimitSubsetAffinity = 0x00004000,

        JobObjectLimitWorkingset = 0x00000001,
    }
}