using System.Text.Json;

namespace CimFlowClusterCleaner;

internal static class AppSetting
{
    public static T Load<T>(string path = "appsettings.json")
    {
        var settingsJson = JsonDocument.Parse(File.ReadAllText(path))
            .RootElement.GetProperty("settings").ToString();

        return JsonSerializer.Deserialize<T>(settingsJson) ??
            throw new ArgumentException("Could not deserialize appsettings into settings.");
    }
}
