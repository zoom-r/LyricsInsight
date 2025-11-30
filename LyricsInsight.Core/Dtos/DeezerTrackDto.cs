using System.Text.Json.Serialization;

namespace LyricsInsight.Core.Dtos;

public class DeezerTrackDto
{
    [JsonPropertyName("link")]
    public required string Link { get; set; }
    
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    
    [JsonPropertyName("duration")]
    public required int Duration { get; set; }
    
    [JsonPropertyName("track_position")]
    public int? TrackPosition { get; set; }
    
    [JsonPropertyName("disk_number")]
    public int? DiskNumber { get; set; }
    
    [JsonPropertyName("rank")]
    public int? Rank { get; set; }
    
    [JsonPropertyName("release_date")]
    public required DateOnly ReleaseDate { get; set; }
    
    [JsonPropertyName("bpm")]
    public float? Bpm { get; set; }
    
    [JsonPropertyName("contributors")]
    public required TrackContributor[] Contributors { get; set; }
}

public class TrackContributor
{
    [JsonPropertyName("id")]
    public required int Id { get; set; }
    
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    [JsonPropertyName("type")]
    public required string Type { get; set; }
    
    [JsonPropertyName("role")]
    public required string Role { get; set; }
}