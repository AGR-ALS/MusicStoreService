using MusicStoreShowcase.Application.Abstractions.Generators;
using MusicStoreShowcase.Application.Abstractions.Translation;
using MusicStoreShowcase.Domain.Abstractions.Services;
using MusicStoreShowcase.Domain.Models;

namespace MusicStoreShowcase.Application.Services;

public class SongService : ISongService
{
    private readonly ISongGenerator _songGenerator;
    private readonly ITranslationService _translationService;
    private const string SourceLanguage = "en";

    public SongService(ISongGenerator songGenerator, ITranslationService translationService)
    {
        _songGenerator = songGenerator;
        _translationService = translationService;
    }

    public async Task<List<Song>> GetTablePageAsync(long seed, int page, int pageSize, string language,
        double averageLikes, CancellationToken cancellationToken)
    {
        var songs = new List<Song>(pageSize);

        for (int i = 0; i < pageSize; i++)
        {
            int index = (page - 1) * pageSize + i + 1;
            var song = _songGenerator.GenerateBasicInfo(seed, index, SourceLanguage, averageLikes);
            songs.Add(song);
        }

        await TranslateSongsInfo(language, songs, cancellationToken);

        return songs;
    }

    private async Task TranslateSongsInfo(string language, List<Song> songs, CancellationToken cancellationToken)
    {
        if (!language.Equals(SourceLanguage))
        {
            var titles = songs.Select(s => s.Title).ToList();
            var artists = songs.Select(s => s.Artist).ToList();
            var albums = songs.Select(s => s.AlbumTitle).ToList();
            var genres = songs.Select(s => s.Genre).ToList();

            var translatedTitlesTask = _translationService.TranslateBatchAsync(titles, language, SourceLanguage, cancellationToken);
            var translatedArtistsTask = _translationService.TranslateBatchAsync(artists, language, SourceLanguage, cancellationToken);
            var translatedAlbumsTask = _translationService.TranslateBatchAsync(albums, language, SourceLanguage, cancellationToken);
            var translatedGenresTask = _translationService.TranslateBatchAsync(genres, language, SourceLanguage, cancellationToken);
            await Task.WhenAll(translatedTitlesTask, translatedArtistsTask, translatedAlbumsTask, translatedGenresTask);
            var translatedTitles = await translatedTitlesTask;
            var translatedArtists = await translatedArtistsTask;
            var translatedAlbums = await translatedAlbumsTask;
            var translatedGenres = await translatedGenresTask;
            
            for (int i = 0; i < songs.Count; i++)
            {
                songs[i].Title = translatedTitles[i];
                songs[i].Artist = translatedArtists[i];
                songs[i].AlbumTitle = translatedAlbums[i];
                songs[i].Genre = translatedGenres[i];
                songs[i].Language = language;
            }
        }
    }

    public async Task<List<Song>> GetGalleryPageAsync(long seed, int page, int pageSize, string language, double averageLikes, CancellationToken cancellationToken)
    {
        var songs = await GetTablePageAsync(seed, page, pageSize, language, averageLikes, cancellationToken);

        var coverGenerationTasks = songs.Select(song => _songGenerator.GenerateCoverAsync(song, seed, cancellationToken));
        var covers = await Task.WhenAll(coverGenerationTasks);

        for (int i = 0; i < songs.Count; i++)
        {
            songs[i].CoverImage = covers[i];
        }

        return songs;
    }

    public async Task<Song> ExpandSong(Song song, long seed, CancellationToken cancellationToken)
    {
        song.CoverImage ??= await _songGenerator.GenerateCoverAsync(song, seed, cancellationToken);
        song.Audio ??= await _songGenerator.GenerateAudio(seed, song.Index, song.Language, cancellationToken);
        song.ReviewText ??= await _songGenerator.GenerateReviewAsync(seed, song.Index, song.Language, cancellationToken);

        return song;
    }

    public List<Song> UpdateLikes(List<Song> songs, long seed, double averageLikes)
    {
        foreach (var song in songs)
        {
            song.AmountOfLikes = _songGenerator.CalculateLikes(seed, song.Index, averageLikes);
        }

        return songs;
    }

    public List<(int Index, int Likes)> GetLikesForPage(long seed, int page, int pageSize, double averageLikes)
    {
        var result = new List<(int Index, int Likes)>(pageSize);

        for (int i = 0; i < pageSize; i++)
        {
            int index = (page - 1) * pageSize + i + 1;
            int likes = _songGenerator.CalculateLikes(seed, index, averageLikes);
            result.Add((index, likes));
        }

        return result;
    }
}