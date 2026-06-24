using MusicStoreShowcase.Domain.Models;

namespace MusicStoreShowcase.Application.Abstractions.Generators;

public interface ISongGenerator
{
    Song GenerateBasicInfo(long seed, int index, string language, double averageLikes);
    Task<SongAudio> GenerateAudio(long seed, int index, string language, CancellationToken cancellationToken);
    Task<string> GenerateReviewAsync(long seed, int index, string language, CancellationToken cancellationToken);
    int CalculateLikes(long seed, int index, double averageLikes);
    Task<byte[]> GenerateCoverAsync(Song song, long seed, CancellationToken cancellationToken);
}