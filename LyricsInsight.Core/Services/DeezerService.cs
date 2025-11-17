using System.Net.Http.Json;
using AutoMapper; // <-- 1. ДОБАВИ AUTO MAPPER
using LyricsInsight.Core.Dtos;
using LyricsInsight.Core.Mapping; // <-- 2. ДОБАВИ НАШИЯ CONFIG
using LyricsInsight.Core.Models;

namespace LyricsInsight.Core.Services;

public class DeezerService
{
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper; // <-- 3. ПОЛЕ ЗА "ПРЕВОДАЧА"
    private const string urlRoot = "https://api.deezer.com";

    public DeezerService()
    {
        _httpClient = new HttpClient();
        // 4. Вземи "преводача" от нашия статичен клас
        _mapper = AutoMapperConfig.Mapper; 
    }

    // --- 5. ВИЖ КОЛКО ПО-ЧИСТ Е МЕТОДЪТ СЕГА ---
    public async Task<List<SongSearchResult>> SearchSongsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<SongSearchResult>();

        try
        {
            var url = $"{urlRoot}/search/track?q={query}";
            var dto = await _httpClient.GetFromJsonAsync<DeezerSearchResponseDto>(url);

            if (dto == null || dto.Data == null)
                return new List<SongSearchResult>();

            // AutoMapper "превежда" целия списък DTO -> Model
            return _mapper.Map<List<SongSearchResult>>(dto.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Грешка при търсене в Deezer: {ex.Message}");
            return new List<SongSearchResult>();
        }
    }

    public async Task<Track> GetTrackDetailsAsync(string trackId)
    {
        if (string.IsNullOrWhiteSpace(trackId)) return null;
        try
        {
            var url = $"{urlRoot}/track/{trackId}";
            var dto = await _httpClient.GetFromJsonAsync<DeezerTrackDto>(url);
            
            // AutoMapper "превежда" DTO -> Model
            return _mapper.Map<Track>(dto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Грешка при извличане на детайли за песен: {ex.Message}");
            return null;
        }
    }
    
    public async Task<Album> GetAlbumDetailsAsync(string albumId)
    {
        if (string.IsNullOrWhiteSpace(albumId)) return null;
        try
        {
            var url = $"{urlRoot}/album/{albumId}";
            var dto = await _httpClient.GetFromJsonAsync<DeezerAlbumDto>(url);
            
            // AutoMapper "превежда" DTO -> Model
            return _mapper.Map<Album>(dto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Грешка при извличане на детайли за албум: {ex.Message}");
            return null;
        }
    }
    
    public async Task<Artist> GetArtistDetailsAsync(string artistId)
    {
        if (string.IsNullOrWhiteSpace(artistId)) return null;
        try
        {
            var url = $"{urlRoot}/artist/{artistId}";
            var dto = await _httpClient.GetFromJsonAsync<DeezerArtistDto>(url);
            
            // AutoMapper "превежда" DTO -> Model
            return _mapper.Map<Artist>(dto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Грешка при извличане на детайли за артиста: {ex.Message}");
            return null;
        }
    }
}
