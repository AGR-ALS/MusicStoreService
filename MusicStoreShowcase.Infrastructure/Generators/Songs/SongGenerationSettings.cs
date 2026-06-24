namespace MusicStoreShowcase.Infrastructure.Generators.Songs;

public class SongGenerationSettings
{
    public int AmountOfReviewWordsMin { get; set; }
    public int AmountOfReviewWordsMax { get; set; }
    public int AmountOfLikesMin { get; set; }
    public int AmountOfLikesMax { get; set; }
    public double LikesSpread { get; set; }
    public double AsymmetricCoefficient { get; set; }
    public int AmountOfBars { get; set; }
    public int ReviewSeedOffset { get; set; }
}