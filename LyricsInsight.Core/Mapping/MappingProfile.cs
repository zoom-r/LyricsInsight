using AutoMapper;
using LyricsInsight.Core.Dtos;
using LyricsInsight.Core.Models;
using System.Linq;

namespace LyricsInsight.Core.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // --- 1. Търсене: DeezerSearchTrack (DTO) -> SongSearchResult (Model) ---
        CreateMap<DeezerSearchTrack, SongSearchResult>()
            .ForMember(
                dest => dest.Artist, // Вземи 'ArtistName' на модела
                opt => opt.MapFrom(src => src.Artist.Name) // ...и го попълни от ВЛОЖЕНИЯ 'Artist.Name'
            )
            .ForMember(
                dest => dest.AlbumCoverUrl, // Вземи 'AlbumCoverUrl'
                opt => opt.MapFrom(src => src.Album.CoverUrl) // ...от 'Album.CoverMedium'
            )
            .ForMember(
                dest => dest.ArtistId, // Вземи 'ArtistId'
                opt => opt.MapFrom(src => src.Artist.Id) // ...от 'Artist.Id'
            )
            .ForMember(
                dest => dest.AlbumId, // Вземи 'AlbumId'
                opt => opt.MapFrom(src => src.Album.Id) // ...от 'Album.Id'
            );
        
        // --- 2. Детайли за песен: DeezerTrackDto (DTO) -> Track (Model) ---
        CreateMap<DeezerTrackDto, Track>()
            .ForMember(
                dest => dest.Duration,
                opt => opt.MapFrom(src => TimeSpan.FromSeconds(src.Duration)) // Превръщаме секунди в "mm:ss"
            )
            .ForMember(
                dest => dest.Artists, // Вземи 'Artists' (string)
                opt => opt.MapFrom(src =>
                    string.Join(", ",
                        src.Contributors.Where(c => c.Type == "artist").Select(c => c.Name)))
            );

        // --- 3. Детайли за албум: DeezerAlbumDto (DTO) -> Album (Model) ---
        CreateMap<DeezerAlbumDto, Album>()
            .ForMember(
                dest => dest.Genres, // Вземи 'Genres' (string)
                opt => opt.MapFrom(src =>
                                          src.Genres.Data.Select(g => g.Name).ToArray())
            )
            .ForMember(
                dest => dest.Duration,
                opt => opt.MapFrom(src => TimeSpan.FromSeconds(src.Duration))
            );

        // --- 4. Детайли за изпълнител: DeezerArtistDto (DTO) -> Artist (Model) ---
        CreateMap<DeezerArtistDto, Artist>();
    }
}
