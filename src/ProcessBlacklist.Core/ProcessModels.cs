namespace ProcessBlacklist.Core;

// Creation time is the native Windows FILETIME, not a rounded display timestamp.
public sealed record ProcessSnapshot(int Id, string Name, long? CreationTime, string? ExecutablePath,
    bool? IsCritical = null, string? InspectionError = null);

public enum MonitorMode { Preview, Terminate }
public enum ActionOutcome { Preview, Terminated, Protected, Unverified, GoneOrChanged, AccessDenied, Pending, Failed }
public sealed record TerminationResult(ActionOutcome Outcome, string Message);
public sealed record ProcessAction(DateTimeOffset Time, ProcessSnapshot Process, Guid? RuleId,
    ActionOutcome Outcome, string Message);
public sealed record ScanResult(IReadOnlyList<ProcessSnapshot> Processes, IReadOnlyList<ProcessAction> Actions);

public interface IProcessPlatform
{
    int CurrentProcessId { get; }
    Task<IReadOnlyList<ProcessSnapshot>> GetProcessesAsync(CancellationToken cancellationToken);

    // Implementations must re-check identity/critical status and terminate using the SAME native handle.
    Task<TerminationResult> TerminateAsync(ProcessSnapshot expected, CancellationToken cancellationToken);
}

public static class ProcessProtection
{
    private static readonly HashSet<string> ProtectedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Idle", "System", "Registry", "Secure System", "Memory Compression",
        "smss", "csrss", "wininit", "services", "lsass", "winlogon",
    };

    public static string? GetReason(ProcessSnapshot process, int currentProcessId)
    {
        if (process.Id <= 4) return "Reserved Windows process.";
        if (process.Id == currentProcessId) return "ProcessBlacklist cannot terminate itself.";
        if (ProtectedNames.Contains(process.Name)) return "Protected Windows process name.";
        if (process.IsCritical == true) return "Windows reports this process as critical.";
        return null;
    }
}
