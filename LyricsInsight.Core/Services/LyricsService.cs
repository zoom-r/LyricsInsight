using LyricsScraperNET;
using LyricsScraperNET.Models.Requests;
using System.Threading.Tasks;
using LyricsInsight.Core.Models;

namespace LyricsInsight.Core;

    public class LyricsService
    {
        private readonly ILyricsScraperClient _scraperClient;

        public LyricsService()
        {
            // Инициализираме клиента. Той автоматично ще търси в Genius, Musixmatch и др.
            _scraperClient = new LyricsScraperClient().WithAllProviders();
        }

        /// <summary>
        /// Опитва се да намери текст на песен по изпълнител и заглавие.
        /// </summary>
        /// <param name="artist">Изпълнител</param>
        /// <param name="song">Заглавие на песента</param>
        /// <returns>Текстът на песента като string, или null, ако не е намерен.</returns>
        public async Task<LyricsResult> GetLyricsAsync(string artist, string song)
        {
            var request = new ArtistAndSongSearchRequest(artist, song);
            var result = await _scraperClient.SearchLyricAsync(request);

            if (result.IsEmpty())
            {
                return null;
            }

            return new LyricsResult
            {
                Text = result.LyricText,
                Source = result.ExternalProviderType.ToString() // <-- ЕТО Я МАГИЯТА!
            };
        }
    }