using System;
using System.IO;
using System.Threading.Tasks;

namespace LyricsInsight.Core.Services;
public class SettingsService
{
    // Дефинираме името на файла
    private const string SettingsFileName = "lyricsinsight_settings.key";

    // Намираме папката на приложението
    private readonly string _settingsFilePath = 
        Path.Combine(AppContext.BaseDirectory, SettingsFileName);

    // Асинхронен метод за ЗАПИСВАНЕ на ключа
    public async Task SaveKeyAsync(string apiKey)
    {
        try
        {
            await File.WriteAllTextAsync(_settingsFilePath, apiKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving API key: {ex.Message}");
        }
    }

    // Асинхронен метод за ЗАРЕЖДАНЕ на ключа
    public async Task<string> LoadKeyAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var apiKey = await File.ReadAllTextAsync(_settingsFilePath);
                return string.IsNullOrWhiteSpace(apiKey) ? null : apiKey.Trim();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading API key: {ex.Message}");
        }

        return null; // Връщаме null, ако файлът не съществува или е празен
    }
}
