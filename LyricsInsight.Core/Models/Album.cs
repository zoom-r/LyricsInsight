namespace LyricsInsight.Core.Models;

public class Album
{
    public required string Title { get; set; }
    
    public required string RecordType { get; set; }
    
    public required string[] Genres { get; set; }
    
    public string? Label { get; set; }
    
    public TimeSpan? Duration { get; set; }
    
    public int? Fans { get; set; }
    
    public required string Link { get; set; }
    
    public required string CoverSmall { get; set; }
    
    public required string CoverMedium { get; set; }
    
    public required string CoverBig { get; set; }
    
    public required string CoverXl { get; set; }
}