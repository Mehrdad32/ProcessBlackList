using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProcessBlacklist.Core;

public sealed class JsonSettingsStore(string filePath) : ISettingsStore
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },
    };

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(filePath)) return new AppSettings();
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, Options, cancellationToken)
                ?? throw new InvalidDataException("The settings file is empty.");
            return settings.Validate();
        }
        finally { gate.Release(); }
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        var validated = settings.Validate();
        await gate.WaitAsync(cancellationToken);
        var temporary = filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(filePath))!;
            Directory.CreateDirectory(directory);
            await using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await JsonSerializer.SerializeAsync(stream, validated, Options, cancellationToken);
                await stream.FlushAsync(cancellationToken);
                stream.Flush(flushToDisk: true);
            }
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporary, filePath, overwrite: true);
        }
        finally
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); }
            finally { gate.Release(); }
        }
    }
}
