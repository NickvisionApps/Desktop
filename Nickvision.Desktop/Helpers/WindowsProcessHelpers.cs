using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Nickvision.Desktop.Helpers;

public static partial class WindowsProcessHelpers
{
    private const uint THREAD_SUSPEND_RESUME = 0x0002u;
    private const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000u;
    private const uint JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION = 0x0400u;
    private const int JobObjectExtendedLimitInformation = 9;
    private const int JobObjectBasicProcessIdList = 3;
    private const int MaxJobProcessIds = 256;

    [StructLayout(LayoutKind.Sequential)]
    private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public uint LimitFlags;
        public nuint MinimumWorkingSetSize;
        public nuint MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public nuint Affinity;
        public uint PriorityClass;
        public uint SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IO_COUNTERS
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        public IO_COUNTERS IoInfo;
        public nuint ProcessMemoryLimit;
        public nuint JobMemoryLimit;
        public nuint PeakProcessMemoryUsed;
        public nuint PeakJobMemoryUsed;
    }

    private sealed class SafeJobHandle : SafeHandle
    {
        public SafeJobHandle(nint handle) : base(nint.Zero, true)
        {
            SetHandle(handle);
        }

        public override bool IsInvalid => handle == nint.Zero;

        protected override bool ReleaseHandle() => CloseHandle(handle);
    }

    private sealed class SafeThreadHandle : SafeHandle
    {
        public SafeThreadHandle(nint handle) : base(nint.Zero, true)
        {
            SetHandle(handle);
        }

        public override bool IsInvalid => handle == nint.Zero;

        protected override bool ReleaseHandle() => CloseHandle(handle);
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial nint CreateJobObject(nint lpJobAttributes, nint lpName);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool SetInformationJobObject(nint hJob, int JobObjectInformationClass, void* lpJobObjectInformation, uint cbJobObjectInformationLength);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AssignProcessToJobObject(nint hJob, nint hProcess);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial nint OpenThread(uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwThreadId);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial uint SuspendThread(nint hThread);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial uint ResumeThread(nint hThread);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool QueryInformationJobObject(nint hJob, int JobObjectInformationClass, void* lpJobObjectInformation, uint cbJobObjectInformationLength, uint* lpReturnLength);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(nint hObject);

    private static readonly ConcurrentDictionary<int, SafeJobHandle> _jobObjects;

    static WindowsProcessHelpers()
    {
        _jobObjects = new ConcurrentDictionary<int, SafeJobHandle>();
    }

    public static unsafe void SetAsParentProcess(Process p)
    {
        var pid = p.Id;
        var jobHandle = CreateJobObject(nint.Zero, nint.Zero);
        var job = new SafeJobHandle(jobHandle);
        var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            BasicLimitInformation = new JOBOBJECT_BASIC_LIMIT_INFORMATION
            {
                LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE | JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION
            }
        };
        SetInformationJobObject(job.DangerousGetHandle(), JobObjectExtendedLimitInformation, &info, (uint)sizeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
        AssignProcessToJobObject(job.DangerousGetHandle(), p.Handle);
        _jobObjects[pid] = job;
        p.Exited += (_, e) =>
        {
            job.Dispose();
            _jobObjects.TryRemove(pid, out var _);
        };
    }

    public static void Suspend(Process p)
    {
        foreach (var id in GetJobProcessIds(p))
        {
            try
            {
                using var proc = Process.GetProcessById((int)(uint)id);
                foreach (ProcessThread thread in proc.Threads)
                {
                    using var handle = new SafeThreadHandle(OpenThread(THREAD_SUSPEND_RESUME, false, (uint)thread.Id));
                    if (handle.IsInvalid)
                    {
                        continue;
                    }
                    SuspendThread(handle.DangerousGetHandle());
                }
            }
            catch { }
        }
    }

    public static void Resume(Process p)
    {
        foreach (var id in GetJobProcessIds(p))
        {
            try
            {
                using var proc = Process.GetProcessById((int)(uint)id);
                foreach (ProcessThread thread in proc.Threads)
                {
                    using var handle = new SafeThreadHandle(OpenThread(THREAD_SUSPEND_RESUME, false, (uint)thread.Id));
                    if (handle.IsInvalid)
                    {
                        continue;
                    }
                    ResumeThread(handle.DangerousGetHandle());
                }
            }
            catch { }
        }
    }

    private static unsafe nuint[] GetJobProcessIds(Process p)
    {
        if (!_jobObjects.TryGetValue(p.Id, out var job))
        {
            return [];
        }
        // Buffer layout: DWORD NumberOfAssignedProcesses + DWORD NumberOfProcessIdsInList + ULONG_PTR ProcessIdList[MaxJobProcessIds]
        var bufferSize = 2 * sizeof(uint) + MaxJobProcessIds * sizeof(nuint);
        var buffer = new byte[bufferSize];
        fixed (byte* pBuffer = buffer)
        {
            if (!QueryInformationJobObject(job.DangerousGetHandle(), JobObjectBasicProcessIdList, pBuffer, (uint)bufferSize, null))
            {
                return [];
            }
            var count = *(uint*)(pBuffer + sizeof(uint));
            var result = new nuint[count];
            var ids = (nuint*)(pBuffer + 2 * sizeof(uint));
            for (var i = 0; i < count; i++)
            {
                result[i] = ids[i];
            }
            return result;
        }
    }
}
