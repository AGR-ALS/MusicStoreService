namespace MusicStoreShowcase.Infrastructure.Generators.Covers;

public class CoverGenerationSettings
{
    public string FontFamily { get; set; } = null!;
    public string ImageUrl { get; set; } = null!; 
    public float ImageBrightness { get; set; } 
    public int ImageResolution { get; set; } 
    public int TitleFontSize { get; set; }
    public int ArtistFontSize { get; set; }
    public int TextHorizontalOffset { get; set; }
    public int TextVerticalTitleOffset { get; set; }
    public int TextVerticalArtistOffset { get; set; }
}