using System.Collections.Generic;
using System.Text.Json.Serialization; // Това е важно!

namespace LyricsInsight.Core.Dtos;

// Това е основният "контейнер", който Deezer връща
public class DeezerSearchResponse
{
    // Казваме на C#, че JSON пропъртито "data"
    // трябва да отиде в нашето "Data" пропърти.
    [JsonPropertyName("data")]
    public List<DeezerTrack> Data { get; set; }
}

// Това описва една песен ("track") в списъка
public class DeezerTrack
{
    [JsonPropertyName("id")]
    public long Id { get; set; } // ID-то от Deezer

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("artist")]
    public DeezerArtist Artist { get; set; } // Вложен обект!

    [JsonPropertyName("album")]
    public DeezerAlbum Album { get; set; } // Вложен обект!
}

// Това описва изпълнителя
public class DeezerArtist
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

// Това описва албума и най-важното - корицата
public class DeezerAlbum
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("cover_medium")] // Deezer има "small", "medium", "big"
    public string CoverMedium { get; set; } // Ще вземем средната корица
}
