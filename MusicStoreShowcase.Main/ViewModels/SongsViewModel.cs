using MusicStoreShowcase.Domain.Models;

namespace MusicStoreShowcase.ViewModels;

public class SongsViewModel
{
    public List<Song> Songs { get; set; } = null!;
    public int CurrentPage { get; set; }
    public long Seed { get; set; }
    public string Language { get; set; } = "en";
    public double AverageLikes { get; set; }
}