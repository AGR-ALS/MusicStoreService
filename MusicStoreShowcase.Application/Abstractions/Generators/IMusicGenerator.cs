using MusicStoreShowcase.Domain.Models;

namespace MusicStoreShowcase.Application.Abstractions.Generators;

public interface IMusicGenerator
{
    SongAudio GenerateAsync(long seed, int durationBars, string locale);
}