using Microsoft.Win32.SafeHandles;
using System.Collections.Concurrent;
using System.Diagnostics;
using Windows.Win32;
using Windows.Win32.System.JobObjects;
using Windows.Win32.System.Threading;

namespace Nickvision.Desktop.Helpers;

public static partial class WindowsProcessHelpers
{
    private const int MaxJobProcessIds = 256;

    private static readonly ConcurrentDictionary<int, SafeFileHandle> _jobObjects;

    static WindowsProcessHelpers()
    {
        _jobObjects = new ConcurrentDictionary<int, SafeFileHandle>();
    }

    public static unsafe void SetAsParentProcess(Process p)
    {
        var pid = p.Id;
#pragma warning disable CA1416
        var job = PInvoke.CreateJobObject(null, (string?)null);
        var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            BasicLimitInformation = new JOBOBJECT_BASIC_LIMIT_INFORMATION
            {
                LimitFlags = JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE | JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION
            }
        };
        PInvoke.SetInformationJobObject(job, JOBOBJECTINFOCLASS.JobObjectExtendedLimitInformation, &info, (uint)sizeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
        PInvoke.AssignProcessToJobObject(job, new SafeFileHandle(p.Handle, ownsHandle: false));
#pragma warning restore CA1416
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
#pragma warning disable CA1416
                    using var handle = PInvoke.OpenThread_SafeHandle(THREAD_ACCESS_RIGHTS.THREAD_SUSPEND_RESUME, false, (uint)thread.Id);
                    if (handle.IsInvalid)
                    {
                        continue;
                    }
                    PInvoke.SuspendThread(handle);
#pragma warning restore CA1416
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
#pragma warning disable CA1416
                    using var handle = PInvoke.OpenThread_SafeHandle(THREAD_ACCESS_RIGHTS.THREAD_SUSPEND_RESUME, false, (uint)thread.Id);
                    if (handle.IsInvalid)
                    {
                        continue;
                    }
                    PInvoke.ResumeThread(handle);
#pragma warning restore CA1416
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
        var bufferSize = 2 * sizeof(uint) + MaxJobProcessIds * sizeof(nuint);
        var buffer = new byte[bufferSize];
        fixed (byte* pBuffer = buffer)
        {
#pragma warning disable CA1416
            if (!PInvoke.QueryInformationJobObject(job, JOBOBJECTINFOCLASS.JobObjectBasicProcessIdList, pBuffer, (uint)bufferSize, null))
#pragma warning restore CA1416
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
