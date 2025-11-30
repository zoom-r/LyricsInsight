using System.Text.Json.Serialization;

namespace LyricsInsight.Core.Dtos;

public class DeezerSearchResponseDto
{
    [JsonPropertyName("data")]
    public List<DeezerSearchTrack> Data { get; set; }
}

public class DeezerSearchTrack
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("artist")]
    public DeezerSearchArtist Artist { get; set; }

    [JsonPropertyName("album")]
    public DeezerSearchAlbum Album { get; set; }
}

public class DeezerSearchArtist
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class DeezerSearchAlbum
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("cover_medium")]
    public string CoverUrl { get; set; }
}