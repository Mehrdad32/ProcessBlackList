using System.Globalization;
using ProcessBlacklist.Core;

var tests = new (string Name, Func<Task> Run)[]
{
    ("Exact name normalizes .EXE and whitespace, independently of culture", async () =>
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            var fake = new FakePlatform([FakePlatform.Target with { Name = "NOTEPAD" }]);
            var result = await new MonitorEngine(fake).ScanAsync([BlacklistRule.Create("  notepad.EXE  ", MatchMode.Exact)], MonitorMode.Preview);
            Require(result.Actions.Single().Outcome == ActionOutcome.Preview && fake.Writes.Count == 0);
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }),
    ("Exact name does not match a substring", async () =>
    {
        var fake = new FakePlatform([FakePlatform.Target with { Name = "phonepad" }]);
        var result = await new MonitorEngine(fake).ScanAsync([BlacklistRule.Create("pad", MatchMode.Exact)], MonitorMode.Terminate);
        Require(result.Actions.Count == 0 && fake.Writes.Count == 0);
    }),
    ("Contains matches names case-insensitively", async () =>
    {
        var fake = new FakePlatform([FakePlatform.Target, FakePlatform.Target with { Id = 102, Name = "PhonePad" }]);
        var result = await new MonitorEngine(fake).ScanAsync([BlacklistRule.Create("PAD", MatchMode.Contains)], MonitorMode.Preview);
        Require(result.Actions.Count == 2 && fake.Writes.Count == 0);
    }),
    ("Names with a second .exe suffix keep their identity", async () =>
    {
        var fake = new FakePlatform([FakePlatform.Target with { Name = "tool.exe" }]);
        var result = await new MonitorEngine(fake).ScanAsync([BlacklistRule.Create("tool.exe.exe", MatchMode.Exact)], MonitorMode.Preview);
        Require(result.Actions.Single().Outcome == ActionOutcome.Preview);
    }),
    ("Blank, all-matching, path and wildcard input is rejected", async () =>
    {
        foreach (var pattern in new[] { "", " ", ".EXE", ".", "..", "*.exe", "C:\\notepad.exe", "dir/tool", "a?b", "a\nb" })
            await Throws<ArgumentException>(() => Task.FromResult(BlacklistRule.Create(pattern, MatchMode.Contains)));
    }),
    ("Disabled rules cannot terminate", async () =>
    {
        var fake = new FakePlatform([FakePlatform.Target]);
        var result = await new MonitorEngine(fake).ScanAsync([BlacklistRule.Create("notepad", MatchMode.Exact) with { Enabled = false }], MonitorMode.Terminate);
        Require(result.Actions.Count == 0 && fake.Writes.Count == 0);
    }),
    ("Overlapping rules terminate once and first enabled rule wins", async () =>
    {
        var fake = new FakePlatform([FakePlatform.Target]);
        var first = BlacklistRule.Create("pad", MatchMode.Contains);
        var second = BlacklistRule.Create("notepad.exe", MatchMode.Exact);
        var result = await new MonitorEngine(fake).ScanAsync([first, second], MonitorMode.Terminate);
        Require(fake.Writes.Count == 1 && result.Actions.Single().RuleId == first.Id);
    }),
    ("Duplicate snapshots do not cause repeated termination", async () =>
    {
        var fake = new FakePlatform([FakePlatform.Target, FakePlatform.Target]);
        var result = await new MonitorEngine(fake).ScanAsync([BlacklistRule.Create("notepad", MatchMode.Exact)], MonitorMode.Terminate);
        Require(fake.Writes.Count == 1 && result.Actions.Count == 1);
    }),
    ("Preview never invokes termination", async () =>
    {
        var fake = new FakePlatform([FakePlatform.Target]);
        var result = await new MonitorEngine(fake).ScanAsync([BlacklistRule.Create("pad", MatchMode.Contains)], MonitorMode.Preview);
        Require(fake.Writes.Count == 0 && result.Actions.Single().Outcome == ActionOutcome.Preview);
    }),
    ("Self and reserved process IDs are protected", async () =>
    {
        var fake = new FakePlatform([FakePlatform.Target with { Id = 999 }, FakePlatform.Target with { Id = 4 }]);
        var result = await new MonitorEngine(fake).ScanAsync([BlacklistRule.Create("pad", MatchMode.Contains)], MonitorMode.Terminate);
        Require(fake.Writes.Count == 0 && result.Actions.All(a => a.Outcome == ActionOutcome.Protected));
    }),
    ("Critical names and native critical flags are protected", async () =>
    {
        var fake = new FakePlatform([FakePlatform.Target with { Name = "LSASS" }, FakePlatform.Target with { Id = 102, IsCritical = true }]);
        var result = await new MonitorEngine(fake).ScanAsync([BlacklistRule.Create("lsass", MatchMode.Exact), BlacklistRule.Create("pad", MatchMode.Contains)], MonitorMode.Terminate);
        Require(fake.Writes.Count == 0 && result.Actions.Count == 2 && result.Actions.All(a => a.Outcome == ActionOutcome.Protected));
    }),
    ("Incomplete identity or critical status prevents termination", async () =>
    {
        var fake = new FakePlatform([FakePlatform.Target with { CreationTime = null }, FakePlatform.Target with { Id = 102, IsCritical = null }, FakePlatform.Target with { Id = 103, ExecutablePath = null }, FakePlatform.Target with { Id = 104, CreationTime = 0 }]);
        var result = await new MonitorEngine(fake).ScanAsync([BlacklistRule.Create("notepad", MatchMode.Exact)], MonitorMode.Terminate);
        Require(fake.Writes.Count == 0 && result.Actions.Count == 4 && result.Actions.All(a => a.Outcome == ActionOutcome.Unverified));
    }),
    ("PID reuse or process exit is reported without a successful count", async () =>
    {
        var fake = new FakePlatform([FakePlatform.Target]) { NextResult = new(ActionOutcome.GoneOrChanged, "Identity changed") };
        var result = await new MonitorEngine(fake).ScanAsync([BlacklistRule.Create("notepad", MatchMode.Exact)], MonitorMode.Terminate);
        Require(result.Actions.Single().Outcome == ActionOutcome.GoneOrChanged);
    }),
    ("Access denied for one process does not stop others", async () =>
    {
        var fake = new FakePlatform([FakePlatform.Target, FakePlatform.Target with { Id = 102 }]) { DeniedId = 101 };
        var result = await new MonitorEngine(fake).ScanAsync([BlacklistRule.Create("notepad", MatchMode.Exact)], MonitorMode.Terminate);
        Require(result.Actions[0].Outcome == ActionOutcome.AccessDenied && result.Actions[1].Outcome == ActionOutcome.Terminated);
    }),
    ("Unexpected per-process failure is reported and scanning continues", async () =>
    {
        var fake = new FakePlatform([FakePlatform.Target, FakePlatform.Target with { Id = 102 }]) { FailedId = 101 };
        var result = await new MonitorEngine(fake).ScanAsync([BlacklistRule.Create("notepad", MatchMode.Exact)], MonitorMode.Terminate);
        Require(result.Actions[0].Outcome == ActionOutcome.Failed && result.Actions[1].Outcome == ActionOutcome.Terminated);
    }),
    ("Pending exit is not reported as confirmed termination", async () =>
    {
        var fake = new FakePlatform([FakePlatform.Target]) { NextResult = new(ActionOutcome.Pending, "Waiting") };
        var result = await new MonitorEngine(fake).ScanAsync([BlacklistRule.Create("notepad", MatchMode.Exact)], MonitorMode.Terminate);
        Require(result.Actions.Single().Outcome == ActionOutcome.Pending);
    }),
    ("Cancellation before a scan performs no writes", async () =>
    {
        var fake = new FakePlatform([FakePlatform.Target]);
        using var source = new CancellationTokenSource(); source.Cancel();
        await Throws<OperationCanceledException>(() => new MonitorEngine(fake).ScanAsync([BlacklistRule.Create("pad", MatchMode.Contains)], MonitorMode.Terminate, source.Token));
        Require(fake.Writes.Count == 0);
    }),
    ("Cancellation between processes prevents subsequent writes", async () =>
    {
        using var source = new CancellationTokenSource();
        var fake = new FakePlatform([FakePlatform.Target, FakePlatform.Target with { Id = 102 }]) { AfterWrite = () => source.Cancel() };
        await Throws<OperationCanceledException>(() => new MonitorEngine(fake).ScanAsync([BlacklistRule.Create("pad", MatchMode.Contains)], MonitorMode.Terminate, source.Token));
        Require(fake.Writes.Count == 1);
    }),
    ("Concurrent scans and manual actions are serialized", async () =>
    {
        var fake = new FakePlatform([FakePlatform.Target]) { DelayMilliseconds = 25 };
        var engine = new MonitorEngine(fake);
        await Task.WhenAll(engine.ScanAsync([BlacklistRule.Create("notepad", MatchMode.Exact)], MonitorMode.Terminate), engine.TerminateOneAsync(FakePlatform.Target));
        Require(fake.MaxConcurrentWrites == 1);
    }),
    ("A failed process enumeration releases the operation gate", async () =>
    {
        var fake = new FakePlatform([FakePlatform.Target]) { FailReadOnce = true };
        var engine = new MonitorEngine(fake);
        await Throws<IOException>(() => engine.ScanAsync([], MonitorMode.Preview));
        var result = await engine.ScanAsync([BlacklistRule.Create("notepad", MatchMode.Exact)], MonitorMode.Terminate);
        Require(result.Actions.Single().Outcome == ActionOutcome.Terminated);
    }),
    ("Manual termination applies the same protection policy", async () =>
    {
        var fake = new FakePlatform([]);
        var action = await new MonitorEngine(fake).TerminateOneAsync(FakePlatform.Target with { Id = 999 });
        Require(fake.Writes.Count == 0 && action.Outcome == ActionOutcome.Protected);
    }),
    ("Settings reject duplicate patterns, IDs and invalid intervals", async () =>
    {
        var rule = BlacklistRule.Create("notepad", MatchMode.Exact);
        foreach (var settings in new[] {
            new AppSettings { Rules = [rule, rule with { Id = Guid.NewGuid(), Pattern = "NOTEPAD" }] },
            new AppSettings { Rules = [rule, rule with { Pattern = "other" }] },
            new AppSettings { IntervalMilliseconds = 0 },
            new AppSettings { SchemaVersion = 2 },
            new AppSettings { Rules = [rule with { Mode = (MatchMode)99 }] },
        }) await Throws<ArgumentException>(() => Task.FromResult(settings.Validate()));
    }),
    ("Settings round trip preserves normalized rules and interval", async () =>
    {
        await WithFolder(async directory =>
        {
            var store = new JsonSettingsStore(Path.Combine(directory, "settings.json"));
            var rule = BlacklistRule.Create("NOTEPAD.EXE", MatchMode.Exact) with { Enabled = false };
            await store.SaveAsync(new() { Rules = [rule], IntervalMilliseconds = 2500 });
            var loaded = await store.LoadAsync();
            Require(loaded.IntervalMilliseconds == 2500 && loaded.Rules.Single() == rule && Directory.GetFiles(directory).Length == 1);
        });
    }),
    ("Corrupt settings are preserved rather than silently reset", async () =>
    {
        await WithFolder(async directory =>
        {
            var path = Path.Combine(directory, "settings.json");
            const string corrupt = "{broken";
            await File.WriteAllTextAsync(path, corrupt);
            await Throws<System.Text.Json.JsonException>(() => new JsonSettingsStore(path).LoadAsync());
            Require(await File.ReadAllTextAsync(path) == corrupt);
        });
    }),
    ("Invalid or cancelled saves preserve the previous file", async () =>
    {
        await WithFolder(async directory =>
        {
            var path = Path.Combine(directory, "settings.json");
            var store = new JsonSettingsStore(path);
            await store.SaveAsync(new() { Rules = [BlacklistRule.Create("notepad", MatchMode.Exact)] });
            var original = await File.ReadAllTextAsync(path);
            await Throws<ArgumentException>(() => store.SaveAsync(new() { IntervalMilliseconds = -1 }));
            using var source = new CancellationTokenSource(); source.Cancel();
            await Throws<OperationCanceledException>(() => store.SaveAsync(new(), source.Token));
            Require(await File.ReadAllTextAsync(path) == original && Directory.GetFiles(directory).Length == 1);
        });
    }),
    ("Unknown JSON fields and future schemas cannot silently downgrade", async () =>
    {
        await WithFolder(async directory =>
        {
            var path = Path.Combine(directory, "settings.json");
            await File.WriteAllTextAsync(path, "{\"SchemaVersion\":2}");
            await Throws<ArgumentException>(() => new JsonSettingsStore(path).LoadAsync());
            await File.WriteAllTextAsync(path, "{\"Unexpected\":true}");
            await Throws<System.Text.Json.JsonException>(() => new JsonSettingsStore(path).LoadAsync());
        });
    }),
    ("Missing settings return an empty stopped configuration", async () =>
    {
        await WithFolder(async directory =>
        {
            var loaded = await new JsonSettingsStore(Path.Combine(directory, "missing.json")).LoadAsync();
            Require(loaded.Rules.Count == 0 && loaded.IntervalMilliseconds == 1000 && Directory.GetFiles(directory).Length == 0);
        });
    }),
};

var failed = 0;
foreach (var test in tests)
{
    try { await test.Run(); Console.WriteLine("PASS " + test.Name); }
    catch (Exception ex) { failed++; Console.Error.WriteLine($"FAIL {test.Name}: {ex}"); }
}
Console.WriteLine($"{tests.Length - failed}/{tests.Length} Core scenarios passed.");
return failed == 0 ? 0 : 1;

static void Require(bool condition) { if (!condition) throw new Exception("Assertion failed."); }
static async Task Throws<T>(Func<Task> operation) where T : Exception
{
    try { await operation(); }
    catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}.");
}
static async Task WithFolder(Func<string, Task> test)
{
    var path = Path.Combine(Path.GetTempPath(), "ProcessBlacklistTests-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(path);
    try { await test(path); }
    finally { Directory.Delete(path, recursive: true); }
}

sealed class FakePlatform(IReadOnlyList<ProcessSnapshot> processes) : IProcessPlatform
{
    public static ProcessSnapshot Target => new(101, "notepad", 123456, "C:\\Windows\\notepad.exe", false);
    public int CurrentProcessId => 999;
    public List<ProcessSnapshot> Writes { get; } = [];
    public TerminationResult NextResult { get; init; } = new(ActionOutcome.Terminated, "Confirmed");
    public int? DeniedId { get; init; }
    public int? FailedId { get; init; }
    public int DelayMilliseconds { get; init; }
    public Action? AfterWrite { get; init; }
    public bool FailReadOnce { get; set; }
    public int MaxConcurrentWrites { get; private set; }
    private int activeWrites;
    public Task<IReadOnlyList<ProcessSnapshot>> GetProcessesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (FailReadOnce) { FailReadOnce = false; throw new IOException("Enumeration failed."); }
        return Task.FromResult(processes);
    }
    public async Task<TerminationResult> TerminateAsync(ProcessSnapshot expected, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        activeWrites++;
        MaxConcurrentWrites = Math.Max(MaxConcurrentWrites, activeWrites);
        try
        {
            Writes.Add(expected);
            if (DelayMilliseconds > 0) await Task.Delay(DelayMilliseconds, cancellationToken);
            if (expected.Id == DeniedId) throw new UnauthorizedAccessException("Access denied.");
            if (expected.Id == FailedId) throw new IOException("Unexpected platform failure.");
            AfterWrite?.Invoke();
            return NextResult;
        }
        finally { activeWrites--; }
    }
}
