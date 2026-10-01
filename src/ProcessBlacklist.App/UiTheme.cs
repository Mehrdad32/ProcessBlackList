namespace ProcessBlacklist.App;

internal static class UiTheme
{
    internal static Color Background => SystemInformation.HighContrast ? SystemColors.Window : Color.FromArgb(243, 246, 251);
    internal static Color Surface => SystemColors.Window;
    internal static Color Text => SystemInformation.HighContrast ? SystemColors.WindowText : Color.FromArgb(25, 42, 61);
    internal static Color Primary => SystemInformation.HighContrast ? SystemColors.Highlight : Color.FromArgb(36, 86, 190);
    internal static Color Danger => SystemInformation.HighContrast ? SystemColors.Highlight : Color.FromArgb(163, 45, 47);

    internal static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.RowHeadersVisible = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.ReadOnly = true;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        grid.EnableHeadersVisualStyles = true;
        grid.DefaultCellStyle.ForeColor = Text;
        grid.DefaultCellStyle.BackColor = Surface;
        grid.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
        grid.DefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
        grid.DefaultCellStyle.Padding = new Padding(3);
        if (!SystemInformation.HighContrast) grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 249, 253);
    }

    internal static Button Button(string text, string name, EventHandler onClick)
    {
        var button = new Button { Text = text, Name = name, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(9, 6, 9, 6), Margin = new Padding(0, 0, 7, 4), UseVisualStyleBackColor = true };
        button.Click += onClick;
        return button;
    }

    internal static void PrimaryButton(Button button, bool danger = false)
    {
        button.BackColor = button.Enabled ? danger ? Danger : Primary : SystemColors.Control;
        button.ForeColor = button.Enabled ? SystemColors.HighlightText : SystemColors.GrayText;
        button.FlatStyle = SystemInformation.HighContrast ? FlatStyle.Standard : FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
    }
}
