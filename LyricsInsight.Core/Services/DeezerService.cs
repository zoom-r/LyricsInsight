using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http; // За HttpClient
using System.Net.Http.Json; // За GetFromJsonAsync (пакетът, който инсталирахме)
using System.Threading.Tasks;
using LyricsInsight.Core.Dtos; // Нашите DTO класове
using LyricsInsight.Core.Models; // Нашият чист модел

namespace LyricsInsight.Core.Services;

public class DeezerService
{
    // HttpClient е "работникът", който прави уеб заявките.
    // Добра практика е да имаме ЕДИН, статичен HttpClient за цялото приложение.
    private static readonly HttpClient _httpClient = new HttpClient();

    // Основният URL, към който ще правим заявки.
    private const string ApiBaseUrl = "https://api.deezer.com";

    // Това е нашият публичен метод, който ViewModel-ът ще вика.
    public async Task<IEnumerable<SongSearchResult>> SearchSongsAsync(string searchTerm)
    {
        // Ако търсенето е празно, връщаме празен списък.
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Enumerable.Empty<SongSearchResult>();
        }

        // Конструираме пълния URL за търсене на "track" (песен)
        // https://api.deezer.com/search/track?q=...
        var requestUrl = $"{ApiBaseUrl}/search/track?q={Uri.EscapeDataString(searchTerm)}";

        try
        {
            // ТОВА Е МАГИЯТА:
            // 1. Извикваме URL-a
            // 2. Чакаме отговора
            // 3. System.Net.Http.Json АВТОМАТИЧНО чете JSON-а
            // 4. ...и го превръща в нашия DeezerSearchResponse DTO обект!
            var response = await _httpClient.GetFromJsonAsync<DeezerSearchResponse>(requestUrl);

            // Ако отговорът е валиден и има песни (Data)
            if (response?.Data != null)
            {
                // ТОВА Е "MAPPING"-ът:
                // Превръщаме (с .Select) списъка от сложни DeezerTrack DTO-та
                // в нашия чист и прост SongSearchResult модел.
                var results = response.Data.Select(track => new SongSearchResult
                {
                    Id = track.Id.ToString(), // Превръщаме long в string
                    Title = track.Title,
                    Artist = track.Artist.Name, // Взимаме само името
                    Album = track.Album.Title,
                    AlbumCoverUrl = track.Album.CoverMedium // Взимаме URL-а за корицата
                });

                return results;
            }
        }
        catch (Exception ex)
        {
            // Ако API-то гръмне или няма интернет,
            // ще го видим в конзолата и ще върнем празен списък.
            Console.WriteLine($"Deezer API error: {ex.Message}");
        }

        // Ако нещо се обърка, връщаме празен списък.
        return Enumerable.Empty<SongSearchResult>();
    }
    
    public async Task<DeezerTrackDetailsDto> GetTrackDetailsAsync(string trackId)
    {
        if (string.IsNullOrWhiteSpace(trackId))
        {
            return null;
        }

        var requestUrl = $"{ApiBaseUrl}/track/{trackId}";

        try
        {
            var response = await _httpClient.GetFromJsonAsync<DeezerTrackDetailsDto>(requestUrl);
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Deezer API details error: {ex.Message}");
            return null;
        }
    }
}