namespace LyricsInsight.Core.Models;

public class SongSearchResult
{
    public required string Id { get; set; }

    public required string Title { get; set; }
    
    public required string Artist { get; set; }
    
    public required string AlbumCoverUrl { get; set; }
    
    public required string ArtistId  { get; set; }
    
    public required string AlbumId { get; set; }
}
