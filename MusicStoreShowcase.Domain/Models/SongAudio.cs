using System.Text.Json.Serialization;

namespace MusicStoreShowcase.Domain.Models;

public class SongAudio
{
    public byte[] MidiData { get; set; } = null!; 
    public List<LyricLine> Lyrics { get; set; } = null!;
    public int TempoBpm { get; set; }
}