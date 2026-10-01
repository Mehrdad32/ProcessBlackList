using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ProcessBlacklist.App;
using ProcessBlacklist.Core;

namespace ProcessBlacklist.Ui.Smoke;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        var output = Path.GetFullPath(args.FirstOrDefault() ?? "ui-preview");
        Directory.CreateDirectory(output);
        var platform = new FakePlatform();
        var store = new MemoryStore();
        using var main = new MainForm(platform, store, output);
        Exception? failure = null;
        main.Shown += async (_, _) =>
        {
            try
            {
                var start = Find<Button>(main, "StartMonitoring");
                var stop = Find<Button>(main, "StopMonitoring");
                var ruleGrid = Find<DataGridView>(main, "RuleGrid");
                var processGrid = Find<DataGridView>(main, "ProcessGrid");
                var pattern = Find<TextBox>(main, "RulePattern");
                var status = Find<Label>(main, "OperationStatus");
                await Until(() => processGrid.Rows.Count == 3 && Find<Button>(main, "RefreshProcesses").Enabled);
                Require(!Find<CheckBox>(main, "TerminateMode").Checked && !stop.Enabled && !start.Enabled, "Startup is not stopped/preview.");
                pattern.Text = "*.exe";
                Require(!Find<Button>(main, "AddRule").Enabled, "Wildcard input is accepted.");
                pattern.Text = "notepad.EXE";
                Find<Button>(main, "AddRule").PerformClick();
                await Until(() => ruleGrid.Rows.Count == 1);
                Require(store.Settings.Rules.Single().Pattern == "notepad" && start.Enabled, "Rule was not normalized and persisted.");
                start.PerformClick();
                await Until(() => status.Text.Contains("Preview · 1 matches"));
                Require(platform.Writes == 0 && !Find<Button>(main, "AddRule").Enabled, "Preview terminated a process or allowed rule edits.");
                stop.PerformClick();
                await Until(() => start.Enabled && !stop.Enabled);
                Find<Button>(main, "ToggleRule").PerformClick();
                await Until(() => !store.Settings.Rules.Single().Enabled);
                Require(!start.Enabled, "Disabled rule still enables monitoring.");
                Find<Button>(main, "ToggleRule").PerformClick();
                await Until(() => store.Settings.Rules.Single().Enabled);
                store.FailSave = true;
                pattern.Text = "phonepad.exe";
                Find<Button>(main, "AddRule").PerformClick();
                await Until(() => status.Text.Contains("Simulated save failure"));
                Require(ruleGrid.Rows.Count == 1 && store.Settings.Rules.Count == 1, "A failed save changed active rules.");
                store.FailSave = false;
                pattern.Clear();

                processGrid.CurrentCell = processGrid.Rows[0].Cells[0];
                var previous = (ProcessSnapshot)processGrid.CurrentRow!.Tag!;
                platform.Processes = platform.Processes.Where(p => p.Id != previous.Id).ToArray();
                Find<Button>(main, "RefreshProcesses").PerformClick();
                await Until(() => processGrid.Rows.Count == 2);
                Require(processGrid.CurrentCell is null && !Find<Button>(main, "EndSelected").Enabled, "Removed selection was replaced by another process.");
                platform.Processes = FakePlatform.Initial;
                Find<Button>(main, "RefreshProcesses").PerformClick();
                await Until(() => processGrid.Rows.Count == 3);
                main.ClientSize = new Size(960, 680);
                Capture(main, Path.Combine(output, "main.png"));
                main.ClientSize = new Size(820, 560);
                Capture(main, Path.Combine(output, "main-compact.png"));
                main.ClientSize = new Size(960, 680);
                main.Font = new Font("Segoe UI", 12F);
                main.ClientSize = new Size(960, 680);
                Capture(main, Path.Combine(output, "main-large-text.png"));
                main.Font = new Font("Segoe UI", 9.5F);
                main.ClientSize = new Size(960, 680);
                Find<TabControl>(main, "MainTabs").SelectedIndex = 1;
                Capture(main, Path.Combine(output, "activity.png"));
                Console.WriteLine("PASS UI smoke: stopped preview startup, rule validation/persistence/toggling, preview with no writes, failed-save preservation and identity-based selection.");
            }
            catch (Exception ex) { failure = ex; Console.Error.WriteLine(ex); }
            finally { main.Close(); }
        };
        Application.Run(main);
        return failure is null ? 0 : 1;
    }

    private static T Find<T>(Control root, string name) where T : Control => (T)root.Controls.Find(name, true).Single();
    private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static async Task Until(Func<bool> condition)
    {
        for (var attempt = 0; attempt < 100; attempt++) { if (condition()) return; await Task.Delay(30); }
        throw new TimeoutException("The expected UI state was not reached.");
    }
    private static void Capture(Form form, string path)
    {
        form.PerformLayout();
        form.Refresh();
        Require(GetClientRect(form.Handle, out var client), "Unable to read the native client area.");
        foreach (var name in new[] { "VersionLabel", "StartMonitoring", "OperationStatus", "RuleGrid", "AddRule" })
        {
            var control = form.Controls.Find(name, true).Single();
            if (!control.Visible) continue;
            var bounds = form.RectangleToClient(control.RectangleToScreen(control.ClientRectangle));
            Require(bounds.Left >= 0 && bounds.Top >= 0 && bounds.Right <= client.Right && bounds.Bottom <= client.Bottom,
                $"{name} is clipped by the native window: {bounds}, viewport {client.Right}x{client.Bottom}.");
        }
        using var bitmap = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
        bitmap.Save(path, ImageFormat.Png);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { public int Left; public int Top; public int Right; public int Bottom; }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetClientRect(IntPtr window, out NativeRect rectangle);
}

internal sealed class FakePlatform : IProcessPlatform
{
    internal static ProcessSnapshot[] Initial => [
        new(101, "notepad", 123456, "C:\\Windows\\notepad.exe", false),
        new(102, "phonepad", 123457, "C:\\Apps\\phonepad.exe", false),
        new(999, "ProcessBlacklist", 123458, "C:\\Apps\\ProcessBlacklist.exe", false),
    ];
    public int CurrentProcessId => 999;
    public IReadOnlyList<ProcessSnapshot> Processes { get; set; } = Initial;
    public int Writes { get; private set; }
    public Task<IReadOnlyList<ProcessSnapshot>> GetProcessesAsync(CancellationToken cancellationToken) => Task.FromResult(Processes);
    public Task<TerminationResult> TerminateAsync(ProcessSnapshot expected, CancellationToken cancellationToken)
    {
        Writes++;
        return Task.FromResult(new TerminationResult(ActionOutcome.Terminated, "Fake termination"));
    }
}
internal sealed class MemoryStore : ISettingsStore
{
    public AppSettings Settings { get; private set; } = new();
    public bool FailSave { get; set; }
    public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(Settings);
    public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        if (FailSave) throw new IOException("Simulated save failure; rules were preserved.");
        Settings = settings.Validate();
        return Task.CompletedTask;
    }
}
