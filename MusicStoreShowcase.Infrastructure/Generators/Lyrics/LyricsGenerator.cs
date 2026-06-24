using Bogus;
using Microsoft.Extensions.Options;
using MusicStoreShowcase.Application.Abstractions.Generators;
using MusicStoreShowcase.Domain.Models;
using Random = System.Random;

namespace MusicStoreShowcase.Infrastructure.Generators.Lyrics;

public class LyricsGenerator : ILyricsGenerator
{
    private readonly LyricsGenerationSettings _settings;
    
    private const double SecondsPerMinute = 60.0;
    public LyricsGenerator(IOptions<LyricsGenerationSettings> settings)
    {
        _settings = settings.Value;
    }

    public List<LyricLine> Generate(long seed, int tempoBpm, int durationBars, string locale)
    {
        int combinedSeed = HashCode.Combine(seed, _settings.SeedOffset, locale);
        Randomizer.Seed = new Random(combinedSeed);
        var faker = new Faker(locale);

        var lyrics = new List<LyricLine>();

        double barDurationSeconds = (SecondsPerMinute / tempoBpm) * _settings.BeatsPerBar;
        double lineIntervalSeconds = barDurationSeconds * _settings.BarsPerLyricLine;

        int linesCount = durationBars / _settings.BarsPerLyricLine;

        for (int lineIndex = 0; lineIndex < linesCount; lineIndex++)
        {
            lyrics.Add(new LyricLine
            {
                Time = TimeSpan.FromSeconds(lineIndex * lineIntervalSeconds),
                Text = GenerateLyricLine(faker)
            });
        }

        return lyrics;
    }

    private string GenerateLyricLine(Faker faker)
    {
        return faker.Random.Words(_settings.WordsPerLine);
    }
}