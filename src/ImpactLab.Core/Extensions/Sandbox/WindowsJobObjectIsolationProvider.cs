using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ImpactLab.Core.Extensions.Sandbox;

public sealed class WindowsJobObjectIsolationProvider : IExtensionIsolationProvider
{
    public string Name => "windows-job-object";
    public bool IsAvailable => OperatingSystem.IsWindows();

    public IExtensionIsolationLease Attach(Process process, ExtensionSandboxPolicy policy)
    {
        if (!IsAvailable)
            throw new PlatformNotSupportedException();
        var job = CreateJobObject(IntPtr.Zero, null);
        if (job == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        try
        {
            var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
            info.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;
            if (policy.MaxWorkingSetBytes > 0)
            {
                info.BasicLimitInformation.LimitFlags |= JOB_OBJECT_LIMIT_PROCESS_MEMORY;
                info.ProcessMemoryLimit = (UIntPtr)policy.MaxWorkingSetBytes;
            }
            var length = Marshal.SizeOf<JOBOBJECT_EXTENDED_LIMIT_INFORMATION>();
            var ptr = Marshal.AllocHGlobal(length);
            try
            {
                Marshal.StructureToPtr(info, ptr, false);
                if (
                    !SetInformationJobObject(
                        job,
                        JobObjectExtendedLimitInformation,
                        ptr,
                        (uint)length
                    )
                )
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            if (!AssignProcessToJobObject(job, process.Handle))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return new Lease(job, policy);
        }
        catch
        {
            CloseHandle(job);
            throw;
        }
    }

    private sealed class Lease : IExtensionIsolationLease
    {
        private IntPtr _job;

        public Lease(IntPtr job, ExtensionSandboxPolicy p)
        {
            _job = job;
            Diagnostics =
            [
                "Windows Job Object assigned.",
                p.KillOnHostExit
                    ? "Kill-on-job-close enabled."
                    : "Host policy requested no kill-on-exit; Job Object still owns process lifetime in v16.",
                p.MaxWorkingSetBytes > 0
                    ? $"Process memory limit: {p.MaxWorkingSetBytes} bytes."
                    : "No process memory limit configured.",
                "Restricted token/AppContainer is not claimed; authenticated IPC and Job Object are the implemented v16 boundaries.",
            ];
        }

        public string Provider => "windows-job-object";
        public IReadOnlyList<string> Diagnostics { get; }

        public void Dispose()
        {
            var h = Interlocked.Exchange(ref _job, IntPtr.Zero);
            if (h != IntPtr.Zero)
                CloseHandle(h);
        }
    }

    private const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x00002000,
        JOB_OBJECT_LIMIT_PROCESS_MEMORY = 0x00000100;
    private const int JobObjectExtendedLimitInformation = 9;

    [StructLayout(LayoutKind.Sequential)]
    private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {
        public long PerProcessUserTimeLimit,
            PerJobUserTimeLimit;
        public uint LimitFlags;
        public UIntPtr MinimumWorkingSetSize,
            MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public UIntPtr Affinity;
        public uint PriorityClass,
            SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IO_COUNTERS
    {
        public ulong ReadOperationCount,
            WriteOperationCount,
            OtherOperationCount,
            ReadTransferCount,
            WriteTransferCount,
            OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        public IO_COUNTERS IoInfo;
        public UIntPtr ProcessMemoryLimit,
            JobMemoryLimit,
            PeakProcessMemoryUsed,
            PeakJobMemoryUsed;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string? lpName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetInformationJobObject(
        IntPtr hJob,
        int infoType,
        IntPtr lpJobObjectInfo,
        uint cbJobObjectInfoLength
    );

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr h);
}
