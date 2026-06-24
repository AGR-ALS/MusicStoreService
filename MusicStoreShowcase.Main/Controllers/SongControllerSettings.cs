namespace MusicStoreShowcase.Controllers;

public class SongControllerSettings
{
    public int PageSize { get; set; }
    public long InitialSeed { get; set; }
    public string InitialLanguage { get; set; } = null!;
    public double InitialLikes { get; set; }
}