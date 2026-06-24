using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Microsoft.Extensions.Options;
using MusicStoreShowcase.Application.Abstractions.Generators;
using MusicStoreShowcase.Domain.Models;
using MusicStoreShowcase.Infrastructure.Generators.Lyrics;

namespace MusicStoreShowcase.Infrastructure.Generators.Music;

public class MusicGenerator : IMusicGenerator
{
    private readonly MusicGenerationSettings _settings;
    private readonly ILyricsGenerator _lyricsGenerator;
    private const double SecondsPerMinute = 60.0;

    public MusicGenerator(ILyricsGenerator lyricsGenerator,
        IOptions<MusicGenerationSettings> settings)
    {
        _settings = settings.Value;
        _lyricsGenerator = lyricsGenerator;
    }

    public SongAudio GenerateAsync(long seed, int durationBars, string locale)
    {
        var context = CreateContext(seed);

        var musicEvents = GenerateMusicEvents(context, durationBars);

        var (lyricEvents, lyricLines) = GenerateLyricsEvents(
            seed,
            context.TempoBpm,
            durationBars,
            locale);

        musicEvents.AddRange(lyricEvents);

        AppendTimedEvents(context.Track, musicEvents);

        return BuildResult(context.File, context.TempoBpm, lyricLines);
    }
    
    private MusicContext CreateContext(long seed)
    {
        var random = new Random((int)seed);

        int tempoBpm = random.Next(_settings.MinTempoBpm, _settings.MaxTempoBpm + 1);

        bool isMinor = random.NextDouble() < 0.5;

        int rootNote = random.Next(_settings.RootNoteMin, _settings.RootNoteMaxExclusive);

        int[] scale = isMinor ? _settings.MinorScale : _settings.MajorScale;

        var tempoMap = TempoMap.Create(
            new TicksPerQuarterNoteTimeDivision((short)_settings.TicksPerQuarter),
            Tempo.FromBeatsPerMinute(tempoBpm),
            new TimeSignature(_settings.TimeSignatureNumerator, _settings.TimeSignatureDenominator));

        var file = new MidiFile();
        var track = new TrackChunk();

        file.Chunks.Add(track);
        file.ReplaceTempoMap(tempoMap);

        long barLength = _settings.TicksPerQuarter * _settings.TimeSignatureNumerator;

        return new MusicContext(
            random,
            tempoBpm,
            isMinor,
            rootNote,
            scale,
            file,
            track,
            barLength);
    }
    
    private List<(long Time, MidiEvent Event)> GenerateMusicEvents(
        MusicContext ctx,
        int durationBars)
    {
        var events = new List<(long Time, MidiEvent Event)>();

        long currentTick = 0;

        for (int bar = 0; bar < durationBars; bar++)
        {
            int degree = ctx.Random.Next(_settings.NotesInScale);

            int[] chord = BuildChord(ctx.RootNote, ctx.Scale, degree);

            GenerateBar(events, ctx, currentTick, chord);

            currentTick += ctx.BarLength;
        }

        return events;
    }
    
    private void GenerateBar(
        List<(long Time, MidiEvent Event)> events,
        MusicContext ctx,
        long barStart,
        int[] chord)
    {
        AddBassPattern(
            events,
            chord[0] + _settings.BassOctaveOffset,
            barStart,
            ctx.Random);

        AddChordPattern(
            events,
            chord,
            barStart,
            ctx.Random);

        AddMelodyPattern(
            events,
            chord,
            barStart,
            ctx.Random);
    }
    
    private (List<(long Time, MidiEvent Event)>, List<LyricLine>) GenerateLyricsEvents(
        long seed,
        int tempoBpm,
        int durationBars,
        string locale)
    {
        var lyrics = _lyricsGenerator
            .Generate(seed, tempoBpm, durationBars, locale);

        double ticksPerSecond =
            (tempoBpm / SecondsPerMinute) * _settings.TicksPerQuarter;

        var events = new List<(long Time, MidiEvent Event)>();

        foreach (var lyric in lyrics)
        {
            long lyricTick =
                (long)(lyric.Time.TotalSeconds * ticksPerSecond);

            events.Add((lyricTick, new LyricEvent(lyric.Text)));
        }

        return (events, lyrics);
    }
    
    private SongAudio BuildResult(
        MidiFile file,
        int tempoBpm,
        List<LyricLine> lyrics)
    {
        using var memoryStream = new MemoryStream();

        file.Write(memoryStream);

        return new SongAudio
        {
            MidiData = memoryStream.ToArray(),
            TempoBpm = tempoBpm,
            Lyrics = lyrics
        };
    }
    private int[] BuildChord(int root, int[] scale, int degree)
    {
        return
        [
            GetScaleNote(root, scale, degree),
            GetScaleNote(root, scale, degree + 2),
            GetScaleNote(root, scale, degree + 4)
        ];
    }

    private int GetScaleNote(int root, int[] scale, int degree)
    {
        int octave = degree / _settings.NotesInScale;

        int scaleDegree = degree % _settings.NotesInScale;

        return root + scale[scaleDegree] + octave * _settings.SemitonesPerOctave;
    }

    private void AddBassPattern(List<(long Time, MidiEvent Event)> events, int bassNote, long barStart, Random random)
    {
        long halfBar = _settings.TicksPerQuarter * (_settings.TimeSignatureNumerator / 2);

        AddNote(
            events,
            bassNote,
            barStart,
            halfBar,
            random.Next(_settings.BassVelocityMin, _settings.BassVelocityMax));

        AddNote(
            events,
            bassNote,
            barStart + halfBar,
            halfBar,
            random.Next(_settings.BassVelocityMin, _settings.BassVelocityMax));
    }

    private void AddChordPattern(List<(long Time, MidiEvent Event)> events, int[] chord, long barStart, Random random)
    {
        for (int beat = 0; beat < _settings.TimeSignatureNumerator; beat++)
        {
            long beatTime = barStart + beat * _settings.TicksPerQuarter;

            foreach (var note in chord)
            {
                AddNote(
                    events,
                    note,
                    beatTime,
                    _settings.TicksPerQuarter,
                    random.Next(
                        _settings.ChordVelocityMin,
                        _settings.ChordVelocityMax));
            }
        }
    }

    private void AddMelodyPattern(List<(long Time, MidiEvent Event)> events, int[] chord, long barStart, Random random)
    {
        for (int eighth = 0; eighth < _settings.EighthNotesPerBar; eighth++)
        {
            if (random.NextDouble() < _settings.MelodySkipChance)
            {
                continue;
            }

            int sourceNote = chord[random.Next(chord.Length)];

            int melodyNote = sourceNote + _settings.MelodyOctaveOffset;

            AddNote(
                events,
                melodyNote,
                barStart + eighth * (_settings.TicksPerQuarter / 2),
                _settings.TicksPerQuarter / 2,
                random.Next(
                    _settings.MelodyVelocityMin,
                    _settings.MelodyVelocityMax));
        }
    }

    private void AddNote(List<(long Time, MidiEvent Event)> events, int noteNumber, long startTime, long length, int velocity)
    {
        events.Add(
            (
                startTime,
                new NoteOnEvent(
                    (SevenBitNumber)noteNumber,
                    (SevenBitNumber)velocity)
            ));

        events.Add(
            (
                startTime + length,
                new NoteOffEvent(
                    (SevenBitNumber)noteNumber,
                    (SevenBitNumber)0)
            ));
    }

    private void AppendTimedEvents(TrackChunk track, IEnumerable<(long Time, MidiEvent Event)> events)
    {
        long previousTime = 0;

        foreach (var item in events
                     .OrderBy(x => x.Time)
                     .ThenBy(x => x.Event is NoteOffEvent ? 0 : 1))
        {
            item.Event.DeltaTime =
                item.Time - previousTime;

            track.Events.Add(item.Event);

            previousTime = item.Time;
        }
    }
    
    private record MusicContext(
        Random Random,
        int TempoBpm,
        bool IsMinor,
        int RootNote,
        int[] Scale,
        MidiFile File,
        TrackChunk Track,
        long BarLength);
}