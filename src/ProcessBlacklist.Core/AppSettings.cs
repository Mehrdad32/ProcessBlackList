namespace ProcessBlacklist.Core;

public sealed record AppSettings
{
    public int SchemaVersion { get; init; } = 1;
    public int IntervalMilliseconds { get; init; } = 1000;
    public IReadOnlyList<BlacklistRule> Rules { get; init; } = [];

    public AppSettings Validate()
    {
        if (SchemaVersion != 1) throw new ArgumentException("Unsupported settings format. The existing file was not changed.");
        if (IntervalMilliseconds is < 500 or > 60000) throw new ArgumentException("The check interval must be between 500 and 60000 ms.");
        if (Rules is null || Rules.Count > 250) throw new ArgumentException("Settings must contain at most 250 rules.");
        var ids = new HashSet<Guid>();
        var patterns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var rules = new List<BlacklistRule>();
        foreach (var rule in Rules)
        {
            if (rule is null) throw new ArgumentException("A rule cannot be null.");
            var validated = rule.Validate();
            if (!ids.Add(validated.Id)) throw new ArgumentException("Duplicate rule ID.");
            if (!patterns.Add($"{(int)validated.Mode}:{validated.Pattern}"))
                throw new ArgumentException("A rule with this pattern and mode already exists.");
            rules.Add(validated);
        }
        return this with { Rules = rules.ToArray() };
    }
}

public interface ISettingsStore
{
    Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default);
}
