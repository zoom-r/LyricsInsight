namespace LyricsInsight.Core.Models;

public class SongSearchResult
{
    // Уникалното ID
    public required string Id { get; set; }

    // Име на песента
    public required string Title { get; set; }

    // Име на изпълнителя
    public required string Artist { get; set; }
    
    // URL към корицата на албума
    public required string AlbumCoverUrl { get; set; }
    
    public required string ArtistId  { get; set; }
    
    public required string AlbumId { get; set; }
}
