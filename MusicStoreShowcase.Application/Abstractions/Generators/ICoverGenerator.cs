namespace MusicStoreShowcase.Application.Abstractions.Generators;

public interface ICoverGenerator
{
    Task<byte[]> GenerateCoverAsync(int seed, string albumTitle, string artist, CancellationToken cancellationToken);
}