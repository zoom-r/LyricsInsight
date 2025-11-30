using AutoMapper;
using LyricsInsight.Core.Dtos;
using LyricsInsight.Core.Models;

namespace LyricsInsight.Core.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DeezerSearchTrack, SongSearchResult>()
            .ForMember(
                dest => dest.Artist,
                opt => opt.MapFrom(src => src.Artist.Name)
            )
            .ForMember(
                dest => dest.AlbumCoverUrl,
                opt => opt.MapFrom(src => src.Album.CoverUrl)
            )
            .ForMember(
                dest => dest.ArtistId,
                opt => opt.MapFrom(src => src.Artist.Id)
            )
            .ForMember(
                dest => dest.AlbumId,
                opt => opt.MapFrom(src => src.Album.Id) 
            );
        
        CreateMap<DeezerTrackDto, Track>()
            .ForMember(
                dest => dest.Duration,
                opt => opt.MapFrom(src => TimeSpan.FromSeconds(src.Duration))
            )
            .ForMember(
                dest => dest.Artists,
                opt => opt.MapFrom(src =>
                    string.Join(", ", src.Contributors.Where(c => c.Type == "artist").Select(c => c.Name)))
            );

        CreateMap<DeezerAlbumDto, Album>()
            .ForMember(
                dest => dest.Genres,
                opt => opt.MapFrom(src =>
                                          src.Genres.Data.Select(g => g.Name).ToArray())
            )
            .ForMember(
                dest => dest.Duration,
                opt => opt.MapFrom(src => TimeSpan.FromSeconds(src.Duration))
            );

        CreateMap<DeezerArtistDto, Artist>();
    }
}
