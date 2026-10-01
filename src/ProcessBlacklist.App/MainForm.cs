using System.Diagnostics;
using System.ComponentModel;
using System.Reflection;
using ProcessBlacklist.Core;
using ProcessBlacklist.Windows;

namespace ProcessBlacklist.App;

public sealed class MainForm : Form
{
    private readonly IProcessPlatform platform;
    private readonly MonitorEngine engine;
    private readonly ISettingsStore store;
    private readonly string settingsDirectory;
    private AppSettings settings = new();
    private IReadOnlyList<ProcessSnapshot> processes = [];
    private readonly Dictionary<Guid, (int Count, DateTimeOffset? Last)> statistics = [];
    private HashSet<string> previousActions = [];
    private CancellationTokenSource? monitorCancellation;
    private Task? monitorTask;
    private bool settingsLoaded;
    private bool busy;
    private bool stopping;
    private bool closingAllowed;

    private readonly DataGridView processGrid = new() { Name = "ProcessGrid", Dock = DockStyle.Fill };
    private readonly DataGridView ruleGrid = new() { Name = "RuleGrid", Dock = DockStyle.Fill };
    private readonly DataGridView activityGrid = new() { Name = "ActivityGrid", Dock = DockStyle.Fill };
    private readonly TextBox filter = new() { Name = "ProcessFilter", PlaceholderText = "Filter by name, PID or path", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10) };
    private readonly TextBox pattern = new() { Name = "RulePattern", PlaceholderText = "e.g. notepad.exe", Width = 215, Margin = new Padding(0, 0, 7, 5) };
    private readonly ComboBox matchMode = new() { Name = "MatchMode", DropDownStyle = ComboBoxStyle.DropDownList, Width = 160, Margin = new Padding(0, 0, 7, 5) };
    private readonly CheckBox terminateMode = new() { Name = "TerminateMode", Text = "Terminate matching processes", AutoSize = true, Margin = new Padding(5, 9, 15, 0) };
    private readonly NumericUpDown interval = new() { Name = "CheckInterval", Minimum = 500, Maximum = 60000, Increment = 500, Value = 1000, Width = 90, Margin = new Padding(0, 7, 12, 0) };
    private readonly Label status = new() { Name = "OperationStatus", AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 8), Text = "Loading rules and processes…" };
    private readonly Label modeStatus = new() { Name = "ModeStatus", AutoSize = true, Text = "Stopped · Preview selected", Padding = new Padding(0, 5, 0, 3) };
    private readonly Label ruleSummary = new() { Name = "RuleSummary", AutoSize = true, Text = "No saved rules", Dock = DockStyle.Fill, Margin = new Padding(0, 5, 0, 5) };
    private readonly Label processSummary = new() { Name = "ProcessSummary", AutoSize = true, Text = "Processes", Dock = DockStyle.Fill };
    private readonly TextBox details = new() { Name = "ProcessDetails", ReadOnly = true, Multiline = true, Dock = DockStyle.Fill, Height = 58, ScrollBars = ScrollBars.Vertical, TabStop = true };
    private readonly Button start;
    private readonly Button stop;
    private readonly Button refresh;
    private readonly Button addRule;
    private readonly Button toggleRule;
    private readonly Button removeRule;
    private readonly Button fromSelected;
    private readonly Button endSelected;
    private readonly NotifyIcon tray;
    private readonly ContextMenuStrip trayMenu;

    public MainForm() : this(new WindowsProcessPlatform(), new JsonSettingsStore(AppPaths.SettingsFile), AppPaths.SettingsDirectory) { }

    public MainForm(IProcessPlatform processPlatform, ISettingsStore settingsStore, string directory)
    {
        platform = processPlatform;
        engine = new(platform);
        store = settingsStore;
        settingsDirectory = directory;
        Text = "ProcessBlacklist";
        Font = new Font("Segoe UI", 9.5F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1130, 700);
        MinimumSize = new Size(800, 520);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UiTheme.Background;
        ForeColor = UiTheme.Text;
        Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;

        start = UiTheme.Button("&Start preview", "StartMonitoring", (_, _) => StartMonitoring());
        stop = UiTheme.Button("S&top", "StopMonitoring", async (_, _) => await StopMonitoringAsync());
        refresh = UiTheme.Button("&Refresh", "RefreshProcesses", async (_, _) => await RunCommandAsync(RefreshProcessesAsync));
        addRule = UiTheme.Button("&Add rule", "AddRule", async (_, _) => await RunCommandAsync(AddRuleAsync));
        toggleRule = UiTheme.Button("&Enable / disable", "ToggleRule", async (_, _) => await RunCommandAsync(ToggleRuleAsync));
        removeRule = UiTheme.Button("&Remove rule", "RemoveRule", async (_, _) => await RunCommandAsync(RemoveRuleAsync));
        fromSelected = UiTheme.Button("&Use selected name", "UseSelectedName", (_, _) =>
        {
            if (SelectedProcess is { } selected) { pattern.Text = selected.Name + ".exe"; matchMode.SelectedIndex = 0; pattern.Focus(); }
        });
        endSelected = UiTheme.Button("End selected…", "EndSelected", async (_, _) => await RunCommandAsync(EndSelectedAsync));

        trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("Show ProcessBlacklist", null, (_, _) => RestoreWindow());
        trayMenu.Items.Add("Stop monitoring", null, async (_, _) => await StopMonitoringAsync());
        trayMenu.Items.Add("Exit", null, (_, _) => Close());
        tray = new NotifyIcon { Text = "ProcessBlacklist · Stopped", Icon = Icon, ContextMenuStrip = trayMenu };
        tray.DoubleClick += (_, _) => RestoreWindow();
        BuildLayout();
        Load += async (_, _) => await RunCommandAsync(InitializeAsync);
        UpdateControls();
    }

    private bool Monitoring => monitorCancellation is not null;
    private ProcessSnapshot? SelectedProcess => processGrid.CurrentRow?.Tag as ProcessSnapshot;
    private BlacklistRule? SelectedRule => ruleGrid.CurrentRow?.Tag as BlacklistRule;

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(16), BackColor = UiTheme.Background };
        root.ColumnStyles.Add(new(SizeType.Percent, 100));
        root.RowStyles.Add(new(SizeType.AutoSize));
        root.RowStyles.Add(new(SizeType.AutoSize));
        root.RowStyles.Add(new(SizeType.AutoSize));
        root.RowStyles.Add(new(SizeType.Percent, 100));
        root.RowStyles.Add(new(SizeType.AutoSize));
        Controls.Add(root);

        var header = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, Padding = new Padding(16, 10, 16, 10), Margin = new Padding(0, 0, 0, 10), BackColor = SystemInformation.HighContrast ? SystemColors.Window : Color.FromArgb(25, 40, 59) };
        header.ColumnStyles.Add(new(SizeType.Percent, 100));
        header.ColumnStyles.Add(new(SizeType.AutoSize));
        var headerColor = SystemInformation.HighContrast ? SystemColors.WindowText : Color.White;
        header.Controls.Add(new Label { Text = "ProcessBlacklist", AutoSize = true, Font = new Font(Font.FontFamily, 21F, FontStyle.Bold), ForeColor = headerColor }, 0, 0);
        var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "2.0.0-alpha.1";
        header.Controls.Add(new Label { Name = "VersionLabel", Text = "v" + version, AutoSize = true, Anchor = AnchorStyles.Right, ForeColor = headerColor }, 1, 0);
        var description = new Label { Text = "See what matches. Choose when to act.", AutoSize = true, ForeColor = headerColor, Margin = new Padding(0, 3, 0, 0) };
        header.Controls.Add(description, 0, 1);
        header.SetColumnSpan(description, 2);
        root.Controls.Add(header, 0, 0);

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Margin = new Padding(0) };
        toolbar.Controls.AddRange([start, stop, refresh, terminateMode, new Label { Text = "Check every (ms)", AutoSize = true, Margin = new Padding(0, 9, 5, 0) }, interval]);
        toolbar.Controls.Add(UiTheme.Button("Minimize to tray", "MinimizeToTray", (_, _) => { tray.Visible = true; Hide(); }));
        toolbar.Controls.Add(UiTheme.Button("Settings folder", "SettingsFolder", (_, _) =>
        {
            try { Directory.CreateDirectory(settingsDirectory); Process.Start(new ProcessStartInfo(settingsDirectory) { UseShellExecute = true }); }
            catch (Exception ex) { SetStatus(ex.Message, true); }
        }));
        root.Controls.Add(toolbar, 0, 1);
        root.Controls.Add(modeStatus, 0, 2);
        var tabs = new TabControl { Name = "MainTabs", Dock = DockStyle.Fill, Margin = new Padding(0, 5, 0, 0) };
        var workspace = new TabPage("Processes & rules") { BackColor = UiTheme.Background, Padding = new Padding(10), AutoScroll = true };
        var activity = new TabPage("Activity") { BackColor = UiTheme.Surface, Padding = new Padding(8) };
        tabs.TabPages.AddRange([workspace, activity]);
        root.Controls.Add(tabs, 0, 3);
        root.Controls.Add(status, 0, 4);

        var columns = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
        columns.ColumnStyles.Add(new(SizeType.Percent, 52));
        columns.ColumnStyles.Add(new(SizeType.Percent, 48));
        columns.RowStyles.Add(new(SizeType.Percent, 100));
        workspace.Controls.Add(columns);
        var processPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(0, 0, 10, 0), Margin = new Padding(0) };
        processPanel.ColumnStyles.Add(new(SizeType.Percent, 100));
        processPanel.RowStyles.Add(new(SizeType.AutoSize));
        processPanel.RowStyles.Add(new(SizeType.AutoSize));
        processPanel.RowStyles.Add(new(SizeType.Percent, 100));
        processPanel.RowStyles.Add(new(SizeType.AutoSize));
        processPanel.RowStyles.Add(new(SizeType.AutoSize));
        columns.Controls.Add(processPanel, 0, 0);
        processPanel.Controls.Add(processSummary, 0, 0);
        processPanel.Controls.Add(filter, 0, 1);
        processPanel.Controls.Add(processGrid, 0, 2);
        var processButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0, 7, 0, 0) };
        processButtons.Controls.AddRange([fromSelected, endSelected]);
        processButtons.Controls.Add(UiTheme.Button("Copy details", "CopyProcessDetails", (_, _) =>
        {
            try { if (SelectedProcess is not null) Clipboard.SetText(details.Text); }
            catch (Exception ex) { SetStatus("Could not copy details: " + ex.Message, true); }
        }));
        processPanel.Controls.Add(processButtons, 0, 3);
        processPanel.Controls.Add(details, 0, 4);

        var rulesPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Margin = new Padding(0) };
        rulesPanel.ColumnStyles.Add(new(SizeType.Percent, 100));
        rulesPanel.RowStyles.Add(new(SizeType.AutoSize));
        rulesPanel.RowStyles.Add(new(SizeType.AutoSize));
        rulesPanel.RowStyles.Add(new(SizeType.AutoSize));
        rulesPanel.RowStyles.Add(new(SizeType.Percent, 100));
        rulesPanel.RowStyles.Add(new(SizeType.AutoSize));
        columns.Controls.Add(rulesPanel, 1, 0);
        rulesPanel.Controls.Add(ruleSummary, 0, 0);
        matchMode.Items.AddRange(["Exact name", "Name contains text"]);
        matchMode.SelectedIndex = 0;
        var editor = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0), WrapContents = true };
        editor.Controls.AddRange([pattern, matchMode, addRule]);
        rulesPanel.Controls.Add(editor, 0, 1);
        var ruleHelp = new Label { Text = "Exact name is the default. If several rules match, the first enabled rule handles the process.", AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 3, 0, 8) };
        rulesPanel.Controls.Add(ruleHelp, 0, 2);
        rulesPanel.Controls.Add(ruleGrid, 0, 3);
        var ruleButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Margin = new Padding(0, 7, 0, 0) };
        ruleButtons.Controls.AddRange([toggleRule, removeRule]);
        ruleButtons.Controls.Add(UiTheme.Button("Reset counters", "ResetCounters", (_, _) => { statistics.Clear(); RenderRules(); SetStatus("Session termination counters reset."); }));
        rulesPanel.Controls.Add(ruleButtons, 0, 4);

        UiTheme.StyleGrid(processGrid);
        processGrid.Columns.Add("Name", "Process name");
        processGrid.Columns.Add("PID", "PID");
        processGrid.Columns.Add("State", "State");
        processGrid.Columns[0].FillWeight = 125;
        processGrid.Columns[1].FillWeight = 45;
        processGrid.Columns[2].FillWeight = 95;
        foreach (DataGridViewColumn column in processGrid.Columns) column.SortMode = DataGridViewColumnSortMode.Automatic;
        UiTheme.StyleGrid(ruleGrid);
        ruleGrid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Enabled", HeaderText = "On", FillWeight = 35 });
        ruleGrid.Columns.Add("Pattern", "Pattern");
        ruleGrid.Columns.Add("Mode", "Mode");
        ruleGrid.Columns.Add("Matches", "Matches");
        ruleGrid.Columns.Add("Closed", "Closed");
        ruleGrid.Columns[1].FillWeight = 130;
        ruleGrid.Columns[2].FillWeight = 85;
        ruleGrid.Columns[3].FillWeight = 55;
        ruleGrid.Columns[4].FillWeight = 55;
        foreach (DataGridViewColumn column in ruleGrid.Columns) column.SortMode = DataGridViewColumnSortMode.NotSortable;
        UiTheme.StyleGrid(activityGrid);
        activityGrid.Columns.Add("Time", "Time");
        activityGrid.Columns.Add("Process", "Process / PID");
        activityGrid.Columns.Add("Rule", "Rule");
        activityGrid.Columns.Add("Result", "Result");
        activityGrid.Columns.Add("Message", "Details");
        activityGrid.Columns[4].FillWeight = 230;
        activity.Controls.Add(activityGrid);
        filter.TextChanged += (_, _) => RenderProcesses();
        pattern.TextChanged += (_, _) => UpdateControls();
        processGrid.SelectionChanged += (_, _) => ShowSelectedProcess();
        ruleGrid.SelectionChanged += (_, _) => UpdateControls();
        ruleGrid.CellContentClick += async (_, e) => { if (e.RowIndex >= 0 && e.ColumnIndex == 0) await RunCommandAsync(ToggleRuleAsync); };
        terminateMode.CheckedChanged += (_, _) => UpdateControls();
        interval.ValueChanged += async (_, _) =>
        {
            if (settingsLoaded && !busy && !Monitoring)
                await RunCommandAsync(async () =>
                {
                    try { await PersistAsync(settings with { IntervalMilliseconds = (int)interval.Value }); }
                    catch { interval.Value = settings.IntervalMilliseconds; throw; }
                });
        };
    }

    private async Task InitializeAsync()
    {
        try
        {
            settings = (await store.LoadAsync()).Validate();
            interval.Value = settings.IntervalMilliseconds;
            settingsLoaded = true;
            RenderRules();
        }
        catch (Exception ex)
        {
            settingsLoaded = false;
            SetStatus("Could not load settings; the existing file was preserved. Open Settings folder, repair or rename settings.json, then restart. " + ex.Message, true);
            processes = await platform.GetProcessesAsync(CancellationToken.None);
            RenderProcesses();
            return;
        }
        await RefreshProcessesAsync();
        var area = Screen.FromControl(this).WorkingArea;
        Size = new Size(Math.Min(Width, area.Width), Math.Min(Height, area.Height));
        SetStatus("Ready · Monitoring is stopped. Add a rule and start Preview to see its matches.");
    }

    private async Task RunCommandAsync(Func<Task> command)
    {
        if (busy || Monitoring) return;
        busy = true;
        UpdateControls();
        try { await command(); }
        catch (Exception ex) { SetStatus(ex.Message, true); }
        finally { busy = false; UpdateControls(); }
    }

    private async Task RefreshProcessesAsync()
    {
        processes = await platform.GetProcessesAsync(CancellationToken.None);
        RenderProcesses();
        SetStatus($"Read {processes.Count} processes. Monitoring remains stopped.");
    }

    private async Task PersistAsync(AppSettings proposed)
    {
        var validated = proposed.Validate();
        await store.SaveAsync(validated);
        settings = validated;
        RenderRules();
    }

    private async Task AddRuleAsync()
    {
        if (!settingsLoaded) return;
        var rule = BlacklistRule.Create(pattern.Text, matchMode.SelectedIndex == 0 ? MatchMode.Exact : MatchMode.Contains);
        await PersistAsync(settings with { Rules = [.. settings.Rules, rule] });
        pattern.Clear();
        SelectRule(rule.Id);
        SetStatus("Rule saved. Monitoring is stopped; start Preview to inspect matches.");
    }

    private async Task ToggleRuleAsync()
    {
        if (!settingsLoaded || SelectedRule is not { } selected) return;
        await PersistAsync(settings with { Rules = settings.Rules.Select(r => r.Id == selected.Id ? r with { Enabled = !r.Enabled } : r).ToArray() });
        SelectRule(selected.Id);
        SetStatus("Rule state saved.");
    }

    private async Task RemoveRuleAsync()
    {
        if (!settingsLoaded || SelectedRule is not { } selected) return;
        await PersistAsync(settings with { Rules = settings.Rules.Where(r => r.Id != selected.Id).ToArray() });
        statistics.Remove(selected.Id);
        SetStatus("Rule removed and settings saved.");
    }

    private void StartMonitoring()
    {
        if (busy || Monitoring || !settingsLoaded || !settings.Rules.Any(r => r.Enabled)) return;
        var mode = terminateMode.Checked ? MonitorMode.Terminate : MonitorMode.Preview;
        if (mode == MonitorMode.Terminate && MessageBox.Show(this,
            $"Start automatic termination for {settings.Rules.Count(r => r.Enabled)} enabled rule(s)?\n\nMatching processes will be force-closed and may lose unsaved work. Use Preview first to check the rules.",
            "Start termination", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        monitorCancellation = new();
        previousActions.Clear();
        UpdateControls();
        monitorTask = MonitorAsync(mode, monitorCancellation.Token);
    }

    private async Task MonitorAsync(MonitorMode mode, CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = await engine.ScanAsync(settings.Rules, mode, cancellationToken);
                processes = result.Processes;
                RenderProcesses();
                var currentActions = new HashSet<string>();
                foreach (var action in result.Actions)
                {
                    var key = $"{action.Process.Id}:{action.Process.CreationTime}:{action.RuleId}:{action.Outcome}:{action.Message}";
                    currentActions.Add(key);
                    if (!previousActions.Contains(key)) AddActivity(action);
                    if (action.Outcome == ActionOutcome.Terminated && action.RuleId is { } id)
                    {
                        var old = statistics.GetValueOrDefault(id);
                        statistics[id] = (old.Count + 1, action.Time);
                    }
                }
                previousActions = currentActions;
                RenderRules(result.Actions);
                var matched = result.Actions.Count;
                var closed = result.Actions.Count(a => a.Outcome == ActionOutcome.Terminated);
                SetStatus(mode == MonitorMode.Preview ? $"Preview · {matched} matches · No termination requested. See Activity for details."
                    : $"Monitoring · {matched} matches · {closed} confirmed termination(s) this scan. See Activity for errors or skipped processes.");
                await Task.Delay(settings.IntervalMilliseconds, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { SetStatus("Monitoring stopped."); }
        catch (Exception ex) { SetStatus("Monitoring stopped: " + ex.Message, true); }
        finally
        {
            monitorCancellation?.Dispose();
            monitorCancellation = null;
            UpdateControls();
        }
    }

    private async Task StopMonitoringAsync()
    {
        if (!Monitoring) return;
        if (stopping) { if (monitorTask is not null) await monitorTask; return; }
        stopping = true;
        monitorCancellation!.Cancel();
        SetStatus("Stopping; waiting for the current operation to finish…");
        UpdateControls();
        try { if (monitorTask is not null) await monitorTask; }
        finally { stopping = false; monitorTask = null; UpdateControls(); }
    }

    private async Task EndSelectedAsync()
    {
        if (SelectedProcess is not { } selected) return;
        if (MessageBox.Show(this, $"Force-close {selected.Name}.exe (PID {selected.Id})?\n\nUnsaved work in this process may be lost.", "End selected process",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        var action = await engine.TerminateOneAsync(selected);
        AddActivity(action);
        processes = await platform.GetProcessesAsync(CancellationToken.None);
        RenderProcesses();
        SetStatus(action.Message, action.Outcome is not ActionOutcome.Terminated and not ActionOutcome.GoneOrChanged);
    }

    private void RenderProcesses()
    {
        var selected = SelectedProcess;
        var sortedColumn = processGrid.SortedColumn?.Name;
        var descending = processGrid.SortOrder == SortOrder.Descending;
        var scroll = processGrid.FirstDisplayedScrollingRowIndex;
        var term = filter.Text.Trim();
        var filtered = processes.Where(p => term.Length == 0 || p.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            p.Id.ToString().Contains(term, StringComparison.Ordinal) || (p.ExecutablePath?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)).ToArray();
        processGrid.Rows.Clear();
        var selectionRestored = false;
        foreach (var process in filtered)
        {
            var reason = ProcessProtection.GetReason(process, platform.CurrentProcessId);
            var state = reason is not null ? "Protected" : process.CreationTime is not > 0 || process.IsCritical is null ? "Unverified" : "Available";
            var index = processGrid.Rows.Add(process.Name + ".exe", process.Id, state);
            var row = processGrid.Rows[index];
            row.Tag = process;
            row.Cells[2].ToolTipText = reason ?? process.InspectionError ?? "Process identity was read successfully.";
            if (selected is not null && process.Id == selected.Id && process.CreationTime == selected.CreationTime)
            {
                processGrid.CurrentCell = row.Cells[0];
                selectionRestored = true;
            }
        }
        if (sortedColumn is not null) processGrid.Sort(processGrid.Columns[sortedColumn]!, descending ? ListSortDirection.Descending : ListSortDirection.Ascending);
        if (selected is not null && !selectionRestored) { processGrid.ClearSelection(); processGrid.CurrentCell = null; }
        if (scroll >= 0 && processGrid.Rows.Count > 0) processGrid.FirstDisplayedScrollingRowIndex = Math.Min(scroll, processGrid.Rows.Count - 1);
        processSummary.Text = $"Running processes · {filtered.Length} shown / {processes.Count} total";
        ShowSelectedProcess();
    }

    private void ShowSelectedProcess()
    {
        details.Text = SelectedProcess is { } p ? $"{p.Name}.exe · PID {p.Id}\r\n{p.ExecutablePath ?? p.InspectionError ?? "Path unavailable"}" : "Select a process to inspect it.";
        UpdateControls();
    }

    private void RenderRules(IReadOnlyList<ProcessAction>? actions = null)
    {
        var selected = SelectedRule?.Id;
        ruleGrid.Rows.Clear();
        foreach (var rule in settings.Rules)
        {
            var count = statistics.GetValueOrDefault(rule.Id);
            var index = ruleGrid.Rows.Add(rule.Enabled, rule.Pattern, rule.Mode == MatchMode.Exact ? "Exact" : "Contains", actions?.Count(a => a.RuleId == rule.Id) ?? 0, count.Count);
            var row = ruleGrid.Rows[index];
            row.Tag = rule;
            row.Cells[4].ToolTipText = count.Last is { } last ? "Last confirmed termination: " + last.ToLocalTime().ToString("G") : "No confirmed termination in this session.";
        }
        if (selected is { } id) SelectRule(id);
        ruleSummary.Text = $"Saved rules · {settings.Rules.Count} total / {settings.Rules.Count(r => r.Enabled)} enabled";
        UpdateControls();
    }

    private void SelectRule(Guid id)
    {
        foreach (DataGridViewRow row in ruleGrid.Rows)
            if (row.Tag is BlacklistRule rule && rule.Id == id) { ruleGrid.CurrentCell = row.Cells[1]; break; }
    }

    private void AddActivity(ProcessAction action)
    {
        var rule = action.RuleId is { } id ? settings.Rules.FirstOrDefault(r => r.Id == id)?.Pattern ?? "Removed rule" : "Manual";
        activityGrid.Rows.Insert(0, action.Time.ToLocalTime().ToString("HH:mm:ss"), $"{action.Process.Name}.exe / {action.Process.Id}", rule, action.Outcome.ToString(), action.Message);
        while (activityGrid.Rows.Count > 300) activityGrid.Rows.RemoveAt(activityGrid.Rows.Count - 1);
    }

    private void UpdateControls()
    {
        var editable = settingsLoaded && !busy && !Monitoring && !stopping;
        start.Enabled = editable && settings.Rules.Any(r => r.Enabled);
        stop.Enabled = Monitoring && !stopping;
        refresh.Enabled = !busy && !Monitoring && !stopping;
        pattern.Enabled = matchMode.Enabled = terminateMode.Enabled = interval.Enabled = editable;
        var valid = false;
        try { BlacklistRule.Normalize(pattern.Text); valid = true; } catch (ArgumentException) { }
        addRule.Enabled = editable && valid;
        toggleRule.Enabled = removeRule.Enabled = editable && SelectedRule is not null;
        fromSelected.Enabled = editable && SelectedProcess is not null;
        endSelected.Enabled = editable && SelectedProcess is { } p && p.CreationTime is > 0 && p.IsCritical == false &&
            p.ExecutablePath is not null && ProcessProtection.GetReason(p, platform.CurrentProcessId) is null;
        start.Text = terminateMode.Checked ? "&Start termination" : "&Start preview";
        UiTheme.PrimaryButton(start, terminateMode.Checked);
        modeStatus.Text = Monitoring ? terminateMode.Checked ? "Running · Termination enabled" : "Running · Preview only"
            : terminateMode.Checked ? "Stopped · Termination selected" : "Stopped · Preview selected";
        tray.Text = "ProcessBlacklist · " + (Monitoring ? terminateMode.Checked ? "Monitoring" : "Preview" : "Stopped");
    }

    private void SetStatus(string message, bool error = false)
    {
        status.Text = message;
        status.ForeColor = error && !SystemInformation.HighContrast ? UiTheme.Danger : UiTheme.Text;
    }

    private void RestoreWindow()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
        tray.Visible = false;
    }

    protected override async void OnFormClosing(FormClosingEventArgs e)
    {
        if (!closingAllowed && e.CloseReason == CloseReason.UserClosing)
        {
            if (busy) { e.Cancel = true; SetStatus("Wait for the current action to finish before closing."); return; }
            if (Monitoring)
            {
                e.Cancel = true;
                await StopMonitoringAsync();
                closingAllowed = true;
                Close();
                return;
            }
        }
        monitorCancellation?.Cancel();
        base.OnFormClosing(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { monitorCancellation?.Cancel(); tray.Dispose(); trayMenu.Dispose(); }
        base.Dispose(disposing);
    }
}
