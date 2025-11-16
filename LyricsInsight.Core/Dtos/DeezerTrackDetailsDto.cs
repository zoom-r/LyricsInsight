// Файл: LyricsInsight.Core/Dtos/DeezerTrackDetailsDto.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LyricsInsight.Core.Dtos
{
    // Отговорът от /track/{id}
    public class DeezerTrackDetailsDto
    {
        [JsonPropertyName("release_date")]
        public DateTime ReleaseDate { get; set; }
        
        [JsonPropertyName("album")]
        public DeezerAlbumDetails Album { get; set; }
    }
    
    public class DeezerAlbumDetails
    {
        [JsonPropertyName("cover_small")]
        public string CoverSmall { get; set; }

        [JsonPropertyName("cover_medium")]
        public string CoverMedium { get; set; }

        [JsonPropertyName("cover_big")]
        public string CoverBig { get; set; }
    }

}