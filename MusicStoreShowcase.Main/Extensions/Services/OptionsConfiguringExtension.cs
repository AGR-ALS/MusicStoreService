using MusicStoreShowcase.Controllers;
using MusicStoreShowcase.Infrastructure.Generators.Covers;
using MusicStoreShowcase.Infrastructure.Generators.Lyrics;
using MusicStoreShowcase.Infrastructure.Generators.Music;
using MusicStoreShowcase.Infrastructure.Generators.Songs;
using MusicStoreShowcase.Infrastructure.Translation;

namespace MusicStoreShowcase.Extensions.Services;

public static class OptionsConfiguringExtension
{
    public static void ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CoverGenerationSettings>(configuration.GetSection(nameof(CoverGenerationSettings)));
        services.Configure<LyricsGenerationSettings>(configuration.GetSection(nameof(LyricsGenerationSettings)));
        services.Configure<MusicGenerationSettings>(configuration.GetSection(nameof(MusicGenerationSettings)));
        services.Configure<SongGenerationSettings>(configuration.GetSection(nameof(SongGenerationSettings)));
        services.Configure<DeepLTranslationSettings>(configuration.GetSection(nameof(DeepLTranslationSettings)));
        services.Configure<SongControllerSettings>(configuration.GetSection(nameof(SongControllerSettings)));
    }
}