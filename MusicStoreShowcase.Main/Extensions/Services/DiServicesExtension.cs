using MusicStoreShowcase.Application;
using MusicStoreShowcase.Application.Abstractions.Generators;
using MusicStoreShowcase.Application.Abstractions.Translation;
using MusicStoreShowcase.Application.Services;
using MusicStoreShowcase.Domain.Abstractions.Services;
using MusicStoreShowcase.Infrastructure.Generators.Covers;
using MusicStoreShowcase.Infrastructure.Generators.Lyrics;
using MusicStoreShowcase.Infrastructure.Generators.Music;
using MusicStoreShowcase.Infrastructure.Generators.Songs;
using MusicStoreShowcase.Infrastructure.Translation;

namespace MusicStoreShowcase.Extensions.Services;

public static class DiServicesExtension
{
    public static void AddDiServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddSingleton<ICoverGenerator, CoverGenerator>();
        services.AddSingleton<IMusicGenerator, MusicGenerator>();
        services.AddSingleton<ISongGenerator, SongGenerator>();
        services.AddSingleton<ISongService, SongService>();
        services.AddSingleton<ILyricsGenerator, LyricsGenerator>();
        services.AddSingleton<ITranslationService, DeepLTranslationService>();
    }
}