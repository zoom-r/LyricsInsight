using System.ComponentModel.DataAnnotations;

namespace LyricsInsight.Core.Models;

public class DeezerTrack
{
    
    public required string Title { get; set; }
    public required string Artist { get; set; }
    public string? AlbumTitle { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? Link { get; set; }
    public string? AlbumCoverSmall  { get; set; }
    public string? AlbumCoverMedium { get; set; }
    public string? AlbumCoverLarge { get; set; }
    public string? PreviewAudio { get; set; }
    
}