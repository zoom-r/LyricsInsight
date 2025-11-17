using System.Text.Json.Serialization;

namespace LyricsInsight.Core.Dtos;

public class DeezerAlbumDto
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    
    [JsonPropertyName("record_type")]
    public required string RecordType { get; set; }
    
    [JsonPropertyName("genres")]
    public required DeezerGenreData Genres { get; set; }
    
    [JsonPropertyName("label")]
    public string? Label { get; set; }
    
    [JsonPropertyName("duration")]
    public required int Duration { get; set; }
    
    [JsonPropertyName("fans")]
    public int? Fans { get; set; }
    
    [JsonPropertyName("link")]
    public required string Link { get; set; }
    
    [JsonPropertyName("cover_small")]
    public required string CoverSmall { get; set; }

    [JsonPropertyName("cover_medium")]
    public required string CoverMedium { get; set; }

    [JsonPropertyName("cover_big")]
    public required string CoverBig { get; set; }
    
    [JsonPropertyName("cover_xl")]
    public required string CoverXl { get; set; }
}

public class DeezerGenreData
{
    [JsonPropertyName("data")]
    public required List<DeezerGenre> Data { get; set; }
}

public class DeezerGenre
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}