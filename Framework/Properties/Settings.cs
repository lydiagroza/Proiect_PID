using System;
using System.IO;
using System.Text.Json;

namespace Framework.Properties
{
    /// <summary>
    /// The user settings, saved as JSON in the user's application data folder
    /// (%APPDATA% on Windows, ~/.config on Linux and macOS).
    /// </summary>
    internal sealed class Settings
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ImageProcessingFramework",
            "settings.json");

        public static Settings Default { get; } = Load();

        public string Theme { get; set; } = "Rose Mist";

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                // The settings are a convenience: the application works without them.
            }
        }

        private static Settings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    return JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new Settings();
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is JsonException)
            {
            }

            return new Settings();
        }
    }
}
