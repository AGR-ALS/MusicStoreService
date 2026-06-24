using MusicStoreShowcase.Domain.Models;

namespace MusicStoreShowcase.Domain.Abstractions.Services;

public interface ISongService
{
    Task<List<Song>> GetTablePageAsync(long seed, int page, int pageSize, string language, double averageLikes, CancellationToken cancellationToken);
    Task<List<Song>> GetGalleryPageAsync(long seed, int page, int pageSize, string language, double averageLikes, CancellationToken cancellationToken);
    Task<Song> ExpandSong(Song song, long seed, CancellationToken cancellationToken);
    List<Song> UpdateLikes(List<Song> songs, long seed, double averageLikes);
    List<(int Index, int Likes)> GetLikesForPage(long seed, int page, int pageSize, double averageLikes);
}