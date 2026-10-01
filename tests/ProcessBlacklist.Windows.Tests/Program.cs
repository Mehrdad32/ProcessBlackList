using System.Diagnostics;
using System.Reflection;
using ProcessBlacklist.Core;
using ProcessBlacklist.Windows;

if (args.Contains("--child")) { Thread.Sleep(TimeSpan.FromMinutes(2)); return 0; }
var platform = new WindowsProcessPlatform();
var tests = new (string Name, Func<Task> Run)[]
{
    ("Native enumeration reads identity and critical status", async () =>
    {
        var processes = await platform.GetProcessesAsync(CancellationToken.None);
        var self = processes.Single(p => p.Id == Environment.ProcessId);
        Require(self.CreationTime is > 0 && self.ExecutablePath is not null && self.IsCritical == false);
    }),
    ("A mismatched creation time cannot terminate a reused PID", async () =>
    {
        using var child = StartChild();
        try
        {
            var snapshot = await ReadChild(child);
            var result = await platform.TerminateAsync(snapshot with { CreationTime = snapshot.CreationTime + 1 }, CancellationToken.None);
            Require(result.Outcome == ActionOutcome.GoneOrChanged && !child.HasExited);
        }
        finally { CleanupChild(child); }
    }),
    ("A mismatched image path prevents termination", async () =>
    {
        using var child = StartChild();
        try
        {
            var snapshot = await ReadChild(child);
            var result = await platform.TerminateAsync(snapshot with { ExecutablePath = "C:\\wrong.exe" }, CancellationToken.None);
            Require(result.Outcome == ActionOutcome.GoneOrChanged && !child.HasExited);
        }
        finally { CleanupChild(child); }
    }),
    ("Native termination confirms the exit of the owned test child", async () =>
    {
        using var child = StartChild();
        try
        {
            var snapshot = await ReadChild(child);
            var result = await platform.TerminateAsync(snapshot, CancellationToken.None);
            Require(result.Outcome == ActionOutcome.Terminated && child.WaitForExit(2000));
        }
        finally { CleanupChild(child); }
    }),
    ("Self protection also applies when called directly", async () =>
    {
        var self = (await platform.GetProcessesAsync(CancellationToken.None)).Single(p => p.Id == Environment.ProcessId);
        var result = await platform.TerminateAsync(self, CancellationToken.None);
        Require(result.Outcome == ActionOutcome.Protected);
    }),
    ("Incomplete identities cannot bypass the native guard", async () =>
    {
        using var child = StartChild();
        try
        {
            var snapshot = await ReadChild(child);
            var result = await platform.TerminateAsync(snapshot with { CreationTime = null }, CancellationToken.None);
            Require(result.Outcome == ActionOutcome.Unverified && !child.HasExited);
        }
        finally { CleanupChild(child); }
    }),
    ("Cancellation does not terminate the owned test child", async () =>
    {
        using var child = StartChild();
        try
        {
            var snapshot = await ReadChild(child);
            using var source = new CancellationTokenSource(); source.Cancel();
            try { await platform.TerminateAsync(snapshot, source.Token); throw new Exception("Expected cancellation."); }
            catch (OperationCanceledException) { }
            Require(!child.HasExited);
        }
        finally { CleanupChild(child); }
    }),
};
var failed = 0;
foreach (var test in tests)
{
    try { await test.Run(); Console.WriteLine("PASS " + test.Name); }
    catch (Exception ex) { failed++; Console.Error.WriteLine($"FAIL {test.Name}: {ex}"); }
}
Console.WriteLine($"{tests.Length - failed}/{tests.Length} native Windows scenarios passed.");
return failed == 0 ? 0 : 1;

static void Require(bool condition) { if (!condition) throw new Exception("Assertion failed."); }
static Process StartChild()
{
    var executable = Environment.ProcessPath ?? throw new Exception("Executable path unavailable.");
    var start = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true };
    if (Path.GetFileNameWithoutExtension(executable).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
        start.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
    start.ArgumentList.Add("--child");
    return Process.Start(start) ?? throw new Exception("Test child did not start.");
}
async Task<ProcessSnapshot> ReadChild(Process child)
{
    for (var attempt = 0; attempt < 30; attempt++)
    {
        var snapshot = (await platform.GetProcessesAsync(CancellationToken.None)).FirstOrDefault(p => p.Id == child.Id);
        if (snapshot is { CreationTime: > 0, ExecutablePath: not null, IsCritical: false }) return snapshot;
        await Task.Delay(50);
    }
    throw new Exception("Unable to inspect the owned test child.");
}
static void CleanupChild(Process child)
{
    if (!child.HasExited) { child.Kill(entireProcessTree: false); child.WaitForExit(2000); }
}
