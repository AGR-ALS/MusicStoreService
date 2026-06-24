namespace MusicStoreShowcase.Infrastructure.Generators.Music;

public class MusicGenerationSettings
{
    public int MinTempoBpm {get; set;}
    public int MaxTempoBpm {get; set;}

    public int TicksPerQuarter {get; set;}

    public int NotesInScale {get; set;}

    public double MelodySkipChance {get; set;}

    public int MelodyOctaveOffset {get; set;}
    public int BassOctaveOffset {get; set;}

    public int ChordVelocityMin {get; set;}
    public int ChordVelocityMax {get; set;}

    public int MelodyVelocityMin {get; set;}
    public int MelodyVelocityMax {get; set;}

    public int BassVelocityMin {get; set;}
    public int BassVelocityMax {get; set;}

    public int[] MajorScale { get; set; } = null!;

    public int[] MinorScale {get; set;} = null!;
    
    public int TimeSignatureNumerator {get; set;}
    public int TimeSignatureDenominator {get; set;}
    
    public int RootNoteMin {get; set;}
    public int RootNoteMaxExclusive {get; set;}

    public int SemitonesPerOctave {get; set;}
    public int EighthNotesPerBar {get; set;}
}