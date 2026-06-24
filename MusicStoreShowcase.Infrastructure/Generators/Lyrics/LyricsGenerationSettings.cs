namespace MusicStoreShowcase.Infrastructure.Generators.Lyrics;

public class LyricsGenerationSettings
{
    public int BeatsPerBar { get; set; }
    public int BarsPerLyricLine { get; set; }
    public int WordsPerLine { get; set; }
    public int SeedOffset { get; set; }
}