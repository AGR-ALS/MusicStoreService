namespace MusicStoreShowcase.Domain.Models;

public class Song
{
    public int Index { get; set; }
    public string Title { get; set; } = null!;
    public string Artist { get; set; } = null!;
    public string AlbumTitle { get; set; } = null!;
    public string Genre { get; set; } = null!;
    public string Language { get; set; } = null!;
    public int AmountOfLikes { get; set; }
    public string ReviewText { get; set; } = null!;
    public SongAudio Audio { get; set; } = null!;
    public byte[] CoverImage { get; set; } = null!;
}