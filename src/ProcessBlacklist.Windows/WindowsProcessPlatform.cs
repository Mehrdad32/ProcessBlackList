using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using ProcessBlacklist.Core;

namespace ProcessBlacklist.Windows;

public sealed class WindowsProcessPlatform : IProcessPlatform
{
    public int CurrentProcessId => Environment.ProcessId;

    public Task<IReadOnlyList<ProcessSnapshot>> GetProcessesAsync(CancellationToken cancellationToken) => Task.Run<IReadOnlyList<ProcessSnapshot>>(() =>
    {
        var results = new List<ProcessSnapshot>();
        var processes = Process.GetProcesses();
        try
        {
            foreach (var process in processes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int id;
                string name;
                try { id = process.Id; name = process.ProcessName; }
                catch (Exception ex) when (ex is InvalidOperationException or Win32Exception) { continue; }
                using var handle = NativeMethods.OpenProcess(NativeMethods.Query, false, id);
                try
                {
                    if (handle.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error());
                    var snapshot = ReadHandle(handle, id);
                    results.Add(snapshot);
                }
                catch (Win32Exception ex) { results.Add(new(id, name, null, null, null, ex.Message)); }
            }
        }
        finally { foreach (var process in processes) process.Dispose(); }
        return results.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ThenBy(p => p.Id).ToArray();
    }, cancellationToken);

    public Task<TerminationResult> TerminateAsync(ProcessSnapshot expected, CancellationToken cancellationToken) => Task.Run(() =>
    {
        cancellationToken.ThrowIfCancellationRequested();
        var reason = ProcessProtection.GetReason(expected, CurrentProcessId);
        if (reason is not null) return new TerminationResult(ActionOutcome.Protected, reason);
        if (expected.CreationTime is not > 0 || expected.IsCritical is null || string.IsNullOrWhiteSpace(expected.ExecutablePath))
            return new(ActionOutcome.Unverified, "Process identity is incomplete. Refresh before trying again.");

        // The identity check and termination share this handle. A recycled PID cannot redirect the write.
        using var handle = NativeMethods.OpenProcess(NativeMethods.Query | NativeMethods.Terminate | NativeMethods.Synchronize, false, expected.Id);
        if (handle.IsInvalid) return ErrorResult(Marshal.GetLastWin32Error());
        try
        {
            if (NativeMethods.WaitForSingleObject(handle, 0) == NativeMethods.ObjectSignaled)
                return new(ActionOutcome.GoneOrChanged, "The selected process has already exited.");
            var current = ReadHandle(handle, expected.Id);
            if (current.CreationTime != expected.CreationTime ||
                !string.Equals(current.Name, expected.Name, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(current.ExecutablePath, expected.ExecutablePath, StringComparison.OrdinalIgnoreCase))
                return new(ActionOutcome.GoneOrChanged, "Process identity changed. Nothing was terminated.");
            reason = ProcessProtection.GetReason(current, CurrentProcessId);
            if (reason is not null) return new(ActionOutcome.Protected, reason);
            cancellationToken.ThrowIfCancellationRequested();
            if (!NativeMethods.TerminateProcess(handle, 1))
            {
                var error = Marshal.GetLastWin32Error();
                if (NativeMethods.WaitForSingleObject(handle, 0) == NativeMethods.ObjectSignaled)
                    return new(ActionOutcome.GoneOrChanged, "The process exited before termination.");
                return ErrorResult(error);
            }
            return NativeMethods.WaitForSingleObject(handle, 1500) switch
            {
                NativeMethods.ObjectSignaled => new(ActionOutcome.Terminated, "Termination confirmed by Windows."),
                NativeMethods.Timeout => new(ActionOutcome.Pending, "Termination was requested; exit was not confirmed within 1.5 seconds."),
                _ => new(ActionOutcome.Failed, "Windows could not confirm process exit."),
            };
        }
        catch (Win32Exception ex) { return new(ActionOutcome.Unverified, "Unable to verify the current process: " + ex.Message); }
    }, cancellationToken);

    private static ProcessSnapshot ReadHandle(SafeProcessHandle handle, int id)
    {
        if (!NativeMethods.GetProcessTimes(handle, out var creation, out _, out _, out _))
            throw new Win32Exception(Marshal.GetLastWin32Error());
        var path = new StringBuilder(32768);
        var length = (uint)path.Capacity;
        if (!NativeMethods.QueryFullProcessImageName(handle, 0, path, ref length))
            throw new Win32Exception(Marshal.GetLastWin32Error());
        if (!NativeMethods.IsProcessCritical(handle, out var critical))
            throw new Win32Exception(Marshal.GetLastWin32Error());
        var fullPath = path.ToString();
        return new(id, Path.GetFileNameWithoutExtension(fullPath), creation.Value, fullPath, critical);
    }

    private static TerminationResult ErrorResult(int error) => error switch
    {
        5 => new(ActionOutcome.AccessDenied, "Windows denied access. If this is your process, try running as Administrator."),
        87 => new(ActionOutcome.GoneOrChanged, "The process is no longer available."),
        _ => new(ActionOutcome.Failed, new Win32Exception(error).Message),
    };
}
