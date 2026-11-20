using System.Text.Json;
using System.Text.Json.Serialization;

namespace Clock2;

// Generates the JSON code at compile time, so settings still work after AOT/trimming.
[JsonSerializable(typeof(AppSettings))]
internal partial class AppSettingsJson : JsonSerializerContext;

public sealed class AppSettings
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Clock2",
        "settings.json");

    public int? X { get; set; }

    public int? Y { get; set; }

    public bool Topmost { get; set; } = true;

    public bool ShowSecondHand { get; set; } = true;

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                return JsonSerializer.Deserialize(File.ReadAllText(FilePath), AppSettingsJson.Default.AppSettings) ?? new AppSettings();
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException)
        {
            // Corrupt or unreadable settings: fall back to defaults.
        }

        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, AppSettingsJson.Default.AppSettings));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            // Not worth crashing a clock over.
        }
    }
}
