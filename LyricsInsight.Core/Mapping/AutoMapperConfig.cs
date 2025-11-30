using AutoMapper;
using Microsoft.Extensions.Logging;

namespace LyricsInsight.Core.Mapping;

public static class AutoMapperConfig
{
    public static IMapper Mapper { get; private set; }

    public static void Initialize()
    {
        var loggerFactory = new LoggerFactory();
        
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        }, loggerFactory);

        Mapper = config.CreateMapper();
    }
}
