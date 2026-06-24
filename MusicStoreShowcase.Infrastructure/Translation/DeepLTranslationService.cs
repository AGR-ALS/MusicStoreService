using DeepL;
using Microsoft.Extensions.Options;
using MusicStoreShowcase.Application.Abstractions.Translation;

namespace MusicStoreShowcase.Infrastructure.Translation;

public class DeepLTranslationService : ITranslationService
{
    private readonly Translator _translator;

    public DeepLTranslationService(IOptions<DeepLTranslationSettings> settings)
    {
        _translator = new Translator(settings.Value.ApiKey); 
    }

    public async Task<string> TranslateAsync(string text, string targetLanguage, string sourceLanguage, CancellationToken cancellationToken)
    {
        var result = await _translator.TranslateTextAsync(text, sourceLanguage, targetLanguage.ToUpper(), cancellationToken: cancellationToken);
        
        return result.Text;
    }

    public async Task<IReadOnlyList<string>> TranslateBatchAsync(IEnumerable<string> texts, string targetLanguage, string sourceLanguage, CancellationToken cancellationToken)
    {
        var textArray = texts.ToArray();
        if (textArray.Length == 0)
        {
            return Array.Empty<string>();
        }
        var results = await _translator.TranslateTextAsync(textArray, sourceLanguage, targetLanguage.ToUpper(), cancellationToken: cancellationToken);
        
        return results.Select(r => r.Text).ToList();
    }
}