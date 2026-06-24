using Bogus;
using Microsoft.Extensions.Options;
using MusicStoreShowcase.Application.Abstractions.Generators;
using MusicStoreShowcase.Application.Abstractions.Translation;
using MusicStoreShowcase.Domain.Models;
using MusicStoreShowcase.Infrastructure.Generators.Covers;
using MusicStoreShowcase.Infrastructure.Generators.Music;

namespace MusicStoreShowcase.Infrastructure.Generators.Songs;

public class SongGenerator : ISongGenerator
{
    private readonly ICoverGenerator _coverGenerator;
    private readonly IMusicGenerator _musicGenerator;
    private readonly ITranslationService _translationService;
    private readonly SongGenerationSettings _songGenerationSettings;
    private const string SourceLanguage = "en";

    public SongGenerator(ICoverGenerator coverGenerator, IMusicGenerator musicGenerator,
        IOptions<SongGenerationSettings> songGenerationSettings, ITranslationService translationService)
    {
        _coverGenerator = coverGenerator;
        _musicGenerator = musicGenerator;
        _translationService = translationService;
        _songGenerationSettings = songGenerationSettings.Value;
    }
    
    public Song GenerateBasicInfo(long seed, int index, string language, double averageLikes)
    {
        int combinedSeed = HashCode.Combine(seed, index, language);
        Randomizer.Seed = new Random(combinedSeed);
        var faker = new Faker(language);

        var song = new Song
        {
            Index = index,
            Title = Capitalize(faker.Music.Random.Word()) + " " + Capitalize(faker.Music.Random.Word()),
            Artist = faker.Music.Random.Words(),
            AlbumTitle = faker.Music.Random.Words(),
            Genre = faker.Music.Genre(),
            Language = language,
            AmountOfLikes = CalculateLikes(seed, index, averageLikes),
        };

        return song;
    }
    
    public async Task<SongAudio> GenerateAudio(long seed, int index, string language, CancellationToken cancellationToken)
    {
        int combinedSeed = HashCode.Combine(seed, index, language);
        var audio = _musicGenerator.GenerateAsync(combinedSeed, _songGenerationSettings.AmountOfBars, SourceLanguage);

        await TranslateSongsLyrics(language, audio, cancellationToken);

        return audio;
    }

    private async Task TranslateSongsLyrics(string language, SongAudio audio, CancellationToken cancellationToken)
    {
        if (!language.Equals(SourceLanguage))
        {
            var lines = audio.Lyrics.Select(l => l.Text);
            var translatedLines = await _translationService.TranslateBatchAsync(lines, language, SourceLanguage, cancellationToken);

            for (int i = 0; i < audio.Lyrics.Count; i++)
            {
                audio.Lyrics[i].Text = translatedLines[i];
            }
        }
    }
    
    public async Task<string> GenerateReviewAsync(long seed, int index, string language, CancellationToken cancellationToken)
    {
        int combinedSeed = HashCode.Combine(seed, index, language, _songGenerationSettings.ReviewSeedOffset);
        Randomizer.Seed = new Random(combinedSeed);
        var faker = new Faker(SourceLanguage);
        var wordsCount = faker.Random.Number(
            _songGenerationSettings.AmountOfReviewWordsMin,
            _songGenerationSettings.AmountOfReviewWordsMax);
        var words = faker.Music.Random.Words(wordsCount);
            
        string englishReview = string.Join(" ", words);

        if (!language.Equals(SourceLanguage))
        {
            return await _translationService.TranslateAsync(englishReview, language, SourceLanguage, cancellationToken);
        }

        return englishReview;
    }
    
    public int CalculateLikes(long seed, int index, double averageLikes)
    {
        if (averageLikes <= _songGenerationSettings.AmountOfLikesMin)
        {
            return _songGenerationSettings.AmountOfLikesMin;
        }

        if (averageLikes >= _songGenerationSettings.AmountOfLikesMax)
        {
            return _songGenerationSettings.AmountOfLikesMax;
        }

        int deterministicSeed = HashCode.Combine(seed, index);
        var random = new Random(deterministicSeed);

        double expectedLikes = averageLikes;

        double likesSpread = _songGenerationSettings.LikesSpread;

        double uniformRandom1 = 1.0 - random.NextDouble();
        double uniformRandom2 = 1.0 - random.NextDouble();

        double standardNormalRandom =
            Math.Sqrt(-2.0 * Math.Log(uniformRandom1)) *
            Math.Cos(2.0 * Math.PI * uniformRandom2);

        double likesValue =
            expectedLikes + standardNormalRandom * likesSpread;

        bool shouldBoostLikes = random.NextDouble() < _songGenerationSettings.AsymmetricCoefficient;

        if (shouldBoostLikes)
        {
            double smallPositiveBoost = random.NextDouble();
            likesValue += smallPositiveBoost;
        }

        likesValue = Math.Clamp(likesValue, _songGenerationSettings.AmountOfLikesMin,
            _songGenerationSettings.AmountOfLikesMax);

        return (int)Math.Round(likesValue);
    }
    
    public async Task<byte[]> GenerateCoverAsync(Song song, long seed, CancellationToken cancellationToken)
    {
        int combinedSeed = HashCode.Combine(seed, song.Index);
        
        return await _coverGenerator.GenerateCoverAsync(combinedSeed, song.AlbumTitle, song.Artist, cancellationToken);
    }

    private string Capitalize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }
        
        
        return char.ToUpper(text[0]) + text[1..];
    }
}