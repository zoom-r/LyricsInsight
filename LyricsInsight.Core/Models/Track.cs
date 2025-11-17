namespace LyricsInsight.Core.Models;

public class Track
{
    public required string Link { get; set; }
    
    public required string Title { get; set; }
    
    public required TimeSpan Duration { get; set; }
    
    public int? TrackPosition { get; set; }
    
    public int? DiskNumber { get; set; }
    
    public int? Rank { get; set; }
    
    public required DateOnly ReleaseDate { get; set; }

    public float? Bpm { get; set; }
    
    public required string  Artists { get; set; }
}