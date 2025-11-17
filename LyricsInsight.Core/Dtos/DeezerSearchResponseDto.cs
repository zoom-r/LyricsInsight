using System.Collections.Generic;
using System.Text.Json.Serialization; // Това е важно!

namespace LyricsInsight.Core.Dtos;

// Това е основният "контейнер", който Deezer връща
public class DeezerSearchResponseDto
{
    // Казваме на C#, че JSON пропъртито "data"
    // трябва да отиде в нашето "Data" пропърти.
    [JsonPropertyName("data")]
    public List<DeezerSearchTrack> Data { get; set; }
}

// Това описва една песен ("track") в списъка
public class DeezerSearchTrack
{
    [JsonPropertyName("id")]
    public long Id { get; set; } // ID-то от Deezer

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("artist")]
    public DeezerSearchArtist Artist { get; set; } // Вложен обект!

    [JsonPropertyName("album")]
    public DeezerSearchAlbum Album { get; set; } // Вложен обект!
}

// Това описва изпълнителя
public class DeezerSearchArtist
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

// Това описва албума и най-важното - корицата
public class DeezerSearchAlbum
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("cover_medium")] // Deezer има "small", "medium", "big"
    public string CoverUrl { get; set; } // Ще вземем средната корица
}