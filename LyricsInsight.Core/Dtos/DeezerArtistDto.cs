using System.Text.Json.Serialization;

namespace LyricsInsight.Core.Dtos;

public class DeezerArtistDto
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    [JsonPropertyName("link")]
    public required string Link { get; set; }
    
    [JsonPropertyName("picture_medium")]
    public required string Picture { get; set; }
    
    [JsonPropertyName("nb_album")]
    public int? NbAlbum { get; set; }
    
    [JsonPropertyName("nb_fan")]
    public int? NbFan { get; set; }
}