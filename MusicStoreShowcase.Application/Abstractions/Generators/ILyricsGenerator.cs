using MusicStoreShowcase.Domain.Models;

namespace MusicStoreShowcase.Application.Abstractions.Generators;

public interface ILyricsGenerator
{
    List<LyricLine> Generate(long seed, int tempoBpm, int durationBars, string locale);
}