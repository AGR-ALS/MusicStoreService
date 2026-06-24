using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MusicStoreShowcase.Application;
using MusicStoreShowcase.Application.Abstractions.Generators;
using MusicStoreShowcase.Domain.Abstractions.Services;
using MusicStoreShowcase.Domain.Models;
using MusicStoreShowcase.ViewModels;

namespace MusicStoreShowcase.Controllers;

public class SongsController : Controller
{
    private readonly ISongService _songService;
    private readonly ISongGenerator _songGenerator;
    private readonly SongControllerSettings _settings;
    private const string MidiContentType = "audio/midi";

    public SongsController(ISongService songService, ISongGenerator songGenerator,
        IOptions<SongControllerSettings> settings)
    {
        _songService = songService;
        _songGenerator = songGenerator;
        _settings = settings.Value;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var songs = await _songService.GetTablePageAsync(_settings.InitialSeed, 1, _settings.PageSize,
            _settings.InitialLanguage, _settings.InitialLikes, cancellationToken);

        var viewModel = new SongsViewModel
        {
            Songs = songs,
            CurrentPage = 1,
            Seed = _settings.InitialSeed,
            Language = _settings.InitialLanguage,
            AverageLikes = _settings.InitialLikes
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> GetSongsAsync(long seed, int page, string language, double likes, ViewMode mode,
        CancellationToken cancellationToken = default)
    {
        List<Song> songs;

        if (mode == ViewMode.Gallery)
        {
            songs = await _songService.GetGalleryPageAsync(seed, page, _settings.PageSize, language, likes,
                cancellationToken);
        }
        else
        {
            songs = await _songService.GetTablePageAsync(seed, page, _settings.PageSize, language, likes,
                cancellationToken);
        }

        return Json(songs);
    }

    [HttpGet]
    public IActionResult UpdateLikes(long seed, int page, double likes)
    {
        var likesData = _songService.GetLikesForPage(seed, page, _settings.PageSize, likes);
        return Json(likesData.Select(x => new { index = x.Index, likes = x.Likes }));
    }

    [HttpGet]
    public async Task<IActionResult> ExpandSongAsync(long seed, int index, string language, double averageLikes,
        CancellationToken cancellationToken = default)
    {
        var song = _songGenerator.GenerateBasicInfo(seed, index, language, averageLikes);
        song = await _songService.ExpandSong(song, seed, cancellationToken);

        return Json(song);
    }

    [HttpGet]
    public async Task<IActionResult> GetMidiAsync(long seed, int index, string language, double averageLikes,
        CancellationToken cancellationToken = default)
    {
        var song = _songGenerator.GenerateBasicInfo(seed, index, language, averageLikes);
        song.Audio = await _songGenerator.GenerateAudio(seed, index, language, cancellationToken);

        if (song?.Audio?.MidiData == null)
        {
            return NotFound();
        }

        return File(song.Audio.MidiData, MidiContentType, $"song_{index}.mid");
    }
}