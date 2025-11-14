// Файл: LyricsInsight.Core/Services/DeezerApiService.cs

using System;
using System.Collections.Generic;
using System.Linq; // <-- Ще ни трябва за "mapping"
using System.Threading.Tasks;
using E.Deezer; // <-- Новата библиотека
using E.Deezer.Api;
// using LyricsInsight.Core.Dtos;
using LyricsInsight.Core.Models;
using DeezerTrack = LyricsInsight.Core.Models.DeezerTrack;

namespace LyricsInsight.Core.Services
{
    public class DeezerService
    {
        // 1. Вече нямаме нужда от HttpClient!
        // Дефинираме клиента на E.Deezer
        private readonly Deezer _deezerSession;

        public DeezerService()
        {
            // 2. Инициализираме клиента
            _deezerSession = DeezerSession.CreateNew();
        }

        // 3. МЕТОДЪТ ЗА ТЪРСЕНЕ (ПРЕНАПИСАН)
        public async Task<List<DeezerTrack>> SearchSongsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<DeezerTrack>();
            }

            try
            {
                // Извикваме библиотеката
                var results = await _deezerSession.Search.Tracks(query);
                return results.Select(track => new DeezerTrack()
                {
                    Title = track.Title,
                    Artist = track.ArtistName,
                    AlbumTitle = track.AlbumName,
                    ReleaseDate = track.ReleaseDate.Date,
                    AlbumCoverSmall = track.GetPicture(PictureSize.Small),
                    AlbumCoverMedium = track.GetPicture(PictureSize.Medium),
                    AlbumCoverLarge = track.GetPicture(PictureSize.Large),
                    Link = track.Link,
                    PreviewAudio = track.Preview
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"E.Deezer search error: {ex.Message}");
                return new List<DeezerTrack>();
            }
        }
    }
}