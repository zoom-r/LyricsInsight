using AutoMapper;
using Microsoft.Extensions.Logging;

namespace LyricsInsight.Core.Mapping;

public static class AutoMapperConfig
{
    // Статично свойство, което да държи "преводача"
    public static IMapper Mapper { get; private set; }

    public static void Initialize()
    {
        // 1. Създай конфигурация
        var loggerFactory = new LoggerFactory();
        
        var config = new MapperConfiguration(cfg =>
        {
            // 2. Кажи му да зареди всички правила от нашия "Профил"
            cfg.AddProfile<MappingProfile>();
        }, loggerFactory);

        // 3. Създай "Мапер" инстанцията
        Mapper = config.CreateMapper();
    }
}
