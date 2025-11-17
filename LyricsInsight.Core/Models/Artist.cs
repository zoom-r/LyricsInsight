namespace LyricsInsight.Core.Models;

public class Artist
{
    public required string Name { get; set; }
    
    public required string Link { get; set; }
    
    public required string Picture { get; set; }
    
    public int? NbAlbum { get; set; }
    
    public int? NbFan { get; set; }
}