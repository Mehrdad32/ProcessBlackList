namespace ProcessBlacklist.Core;

public sealed class MonitorEngine(IProcessPlatform platform)
{
    private readonly SemaphoreSlim operationGate = new(1, 1);

    public async Task<ScanResult> ScanAsync(IEnumerable<BlacklistRule> rules, MonitorMode mode,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentException("Unknown monitoring mode.");
        var active = rules.Select(r => r.Validate()).Where(r => r.Enabled).ToArray();
        await operationGate.WaitAsync(cancellationToken);
        try
        {
            var processes = await platform.GetProcessesAsync(cancellationToken);
            var actions = new List<ProcessAction>();
            var seen = new HashSet<(int, long?)>();
            foreach (var process in processes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!seen.Add((process.Id, process.CreationTime))) continue;
                var rule = active.FirstOrDefault(r => r.Matches(process.Name));
                if (rule is null) continue;
                var reason = ProcessProtection.GetReason(process, platform.CurrentProcessId);
                TerminationResult result;
                if (reason is not null) result = new(ActionOutcome.Protected, reason);
                else if (process.CreationTime is not > 0 || process.IsCritical is null || string.IsNullOrWhiteSpace(process.ExecutablePath))
                    result = new(ActionOutcome.Unverified, process.InspectionError ?? "Process identity or critical status could not be verified.");
                else if (mode == MonitorMode.Preview) result = new(ActionOutcome.Preview, "Matches this rule; preview does not terminate processes.");
                else result = await TerminateCheckedAsync(process, cancellationToken);
                actions.Add(new(DateTimeOffset.UtcNow, process, rule.Id, result.Outcome, result.Message));
            }
            return new(processes, actions);
        }
        finally { operationGate.Release(); }
    }

    public async Task<ProcessAction> TerminateOneAsync(ProcessSnapshot process, CancellationToken cancellationToken = default)
    {
        await operationGate.WaitAsync(cancellationToken);
        try
        {
            var reason = ProcessProtection.GetReason(process, platform.CurrentProcessId);
            var result = reason is not null ? new TerminationResult(ActionOutcome.Protected, reason)
                : process.CreationTime is not > 0 || process.IsCritical is null || string.IsNullOrWhiteSpace(process.ExecutablePath)
                    ? new(ActionOutcome.Unverified, "Refresh before terminating; process identity could not be verified.")
                    : await TerminateCheckedAsync(process, cancellationToken);
            return new(DateTimeOffset.UtcNow, process, null, result.Outcome, result.Message);
        }
        finally { operationGate.Release(); }
    }

    private async Task<TerminationResult> TerminateCheckedAsync(ProcessSnapshot process, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try { return await platform.TerminateAsync(process, cancellationToken); }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (UnauthorizedAccessException ex) { return new(ActionOutcome.AccessDenied, ex.Message); }
        catch (Exception ex) { return new(ActionOutcome.Failed, ex.Message); }
    }
}
