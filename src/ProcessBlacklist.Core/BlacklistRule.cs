namespace ProcessBlacklist.Core;

public enum MatchMode { Exact, Contains }

public sealed record BlacklistRule(Guid Id, string Pattern, MatchMode Mode, bool Enabled = true)
{
    public static BlacklistRule Create(string pattern, MatchMode mode) =>
        new BlacklistRule(Guid.NewGuid(), Normalize(pattern), mode).Validate();

    public BlacklistRule Validate()
    {
        ArgumentNullException.ThrowIfNull(Pattern);
        if (Id == Guid.Empty) throw new ArgumentException("A rule needs a non-empty ID.");
        if (!Enum.IsDefined(Mode)) throw new ArgumentException("Unknown rule matching mode.");
        return this with { Pattern = ValidatePattern(Pattern.Trim()) };
    }

    public bool Matches(string processName) => Mode switch
    {
        MatchMode.Exact => string.Equals(processName, Pattern, StringComparison.OrdinalIgnoreCase),
        MatchMode.Contains => processName.Contains(Pattern, StringComparison.OrdinalIgnoreCase),
        _ => false,
    };

    public static string Normalize(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var name = value.Trim();
        if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) name = name[..^4];
        return ValidatePattern(name);
    }

    // Stored patterns are canonical base names. Re-validation must not strip a second suffix.
    private static string ValidatePattern(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 260 || name is "." or ".." ||
            name.Any(char.IsControl) || name.IndexOfAny(['\\', '/', ':', '*', '?', '"', '<', '>', '|']) >= 0)
            throw new ArgumentException("Enter a process name or name fragment, without a path or wildcard.");
        return name;
    }
}
