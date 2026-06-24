namespace MusicStoreShowcase.Application.Abstractions.Translation;

public interface ITranslationService
{
    Task<string> TranslateAsync(string text, string targetLanguage, string sourceLanguage, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> TranslateBatchAsync(IEnumerable<string> texts, string targetLanguage, string sourceLanguage, CancellationToken cancellationToken);
}