namespace LyricsInsight.Core.Models
{
    public class SongSearchResult
    {
        // Уникалното ID
        public string Id { get; set; }

        // Име на песента
        public string Title { get; set; }

        // Име на изпълнителя
        public string Artist { get; set; }

        // Име на албума (ако го има)
        public string Album { get; set; }
        
        // URL към корицата на албума
        public string AlbumCoverUrl { get; set; }
    }
}