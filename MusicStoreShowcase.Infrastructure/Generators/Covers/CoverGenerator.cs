using Microsoft.Extensions.Options;
using MusicStoreShowcase.Application.Abstractions.Generators;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MusicStoreShowcase.Infrastructure.Generators.Covers;

public class CoverGenerator : ICoverGenerator
{
    private readonly HttpClient _httpClient;
    private readonly CoverGenerationSettings _generationSettings;

    public CoverGenerator(IHttpClientFactory httpClientFactory, IOptions<CoverGenerationSettings> settings)
    {
        _httpClient = httpClientFactory.CreateClient();
        _generationSettings = settings.Value;
    }

    public async Task<byte[]> GenerateCoverAsync(int seed, string albumTitle, string artist, CancellationToken cancellationToken)
    {
        string imageUrl = BuildImageUrl(seed);

        await using Stream imageStream = await DownloadImageAsync(imageUrl);

        using Image<Rgba32> image = await LoadImageAsync(imageStream);

        ApplyBackgroundProcessing(image);

        (Font titleFont, Font artistFont, DrawingOptions drawingOptions) =
            CreateTypographySettings();

        DrawTextLayers(image, albumTitle, artist, titleFont, artistFont, drawingOptions);

        return await SaveImageAsync(image);
    }
    
    private string BuildImageUrl(int seed)
    {
        return $"{_generationSettings.ImageUrl}/{seed}/" +
               $"{_generationSettings.ImageResolution}/{_generationSettings.ImageResolution}";
    }
    
    private async Task<Stream> DownloadImageAsync(string imageUrl)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(imageUrl);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync();
    }
    
    private async Task<Image<Rgba32>> LoadImageAsync(Stream imageStream)
    {
        return await Image.LoadAsync<Rgba32>(imageStream);
    }
    
    private void ApplyBackgroundProcessing(Image<Rgba32> image)
    {
        image.Mutate(context =>
        {
            context.Brightness(_generationSettings.ImageBrightness);
        });
    }
    
    private (Font TitleFont, Font ArtistFont, DrawingOptions Options)
        CreateTypographySettings()
    {
        FontFamily fontFamily = SystemFonts.Families.FirstOrDefault(f =>
            f.Name.Contains(_generationSettings.FontFamily));
        
        if (fontFamily.Name == null)
        {
            throw new InvalidOperationException(
                $"Font family '{_generationSettings.FontFamily}' was not found in the system.");
        }

        Font titleFont = fontFamily.CreateFont(
            _generationSettings.TitleFontSize,
            FontStyle.Bold);

        Font artistFont = fontFamily.CreateFont(
            _generationSettings.ArtistFontSize,
            FontStyle.Regular);

        DrawingOptions drawingOptions = new DrawingOptions
        {
            GraphicsOptions = new GraphicsOptions
            {
                Antialias = true
            }
        };

        return (titleFont, artistFont, drawingOptions);
    }
    
    private void DrawTextLayers(
        Image<Rgba32> image,
        string albumTitle,
        string artist,
        Font titleFont,
        Font artistFont,
        DrawingOptions drawingOptions)
    {
        float textX = _generationSettings.TextHorizontalOffset;

        float titleY = image.Height - _generationSettings.TextVerticalTitleOffset;
        float artistY = image.Height - _generationSettings.TextVerticalArtistOffset;

        image.Mutate(context =>
        {
            context.DrawText(
                drawingOptions,
                albumTitle,
                titleFont,
                Color.White,
                new PointF(textX, titleY));

            context.DrawText(
                drawingOptions,
                artist,
                artistFont,
                Color.LightGray,
                new PointF(textX, artistY));
        });
    }
    
    private async Task<byte[]> SaveImageAsync(Image<Rgba32> image)
    {
        using MemoryStream output = new MemoryStream();

        await image.SaveAsPngAsync(output);

        return output.ToArray();
    }
}