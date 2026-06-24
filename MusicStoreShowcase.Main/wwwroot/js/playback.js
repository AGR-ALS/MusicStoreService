import { state } from './state.js';
import { fetchMidi } from './api.js';

export function attachPlayButtons() {
    document.querySelectorAll('.play-btn').forEach(btn => {
        if (btn.dataset.bound === 'true') return;
        btn.dataset.bound = 'true';
        btn.addEventListener('click', onPlayButtonClick);
    });
}

async function onPlayButtonClick(e) {
    e.stopPropagation();
    e.preventDefault();

    const btn = e.currentTarget;
    const index = btn.dataset.index;

    if (btn.innerHTML.includes('bi-pause-fill')) {
        state.pausedPosition = Tone.Transport.seconds;
        state.pausedSongIndex = index;

        if (state.currentCleanup) {
            state.currentCleanup(true);
            state.currentCleanup = null;
        }

        btn.innerHTML = '<i class="bi bi-play-fill"></i> Resume';
        btn.disabled = false;
        return;
    }

    if (btn.innerHTML.includes('Resume') && state.pausedSongIndex === index) {
        await resumePlayback(btn, index);
        return;
    }

    await startNewPlayback(btn, index);
}

async function startNewPlayback(btn, index) {
    const detailsRow = document.querySelector(`.song-details[data-index="${index}"]`);
    const lyricsContainer = detailsRow?.querySelector('.lyrics-container');
    const timeDisplay = detailsRow?.querySelector('.time-display');
    const progressBar = detailsRow?.querySelector('.progress-bar-custom');

    if (state.currentCleanup) {
        state.currentCleanup(false);
        state.currentCleanup = null;
    }

    try {
        await Tone.start();

        if (window.speechSynthesis.speaking) {
            window.speechSynthesis.cancel();
        }

        btn.innerHTML = '<i class="bi bi-hourglass-split"></i> Loading...';
        btn.disabled = true;

        const midiArrayBuffer = await fetchMidi(index);
        const midi = new Midi(midiArrayBuffer);
        const duration = midi.duration;

        if (progressBar) {
            progressBar.max = duration;
            progressBar.value = 0;
        }

        state.currentLyrics = [];
        if (lyricsContainer) {
            lyricsContainer.querySelectorAll('.lyric-line').forEach(line => {
                const time = parseFloat(line.dataset.time);
                const text = line.textContent.trim();
                if (!isNaN(time) && text) {
                    state.currentLyrics.push({ time, text });
                }
            });
        }

        Tone.Transport.stop();
        Tone.Transport.cancel();
        Tone.Transport.position = 0;

        const synths = createSynths();
        scheduleNotes(midi, synths.bassSynth, synths.chordSynth, synths.melodySynth);

        Tone.Transport.start();

        btn.innerHTML = '<i class="bi bi-pause-fill"></i> Pause';
        btn.disabled = false;

        state.pausedPosition = null;
        state.pausedSongIndex = null;

        setupPlaybackUI(btn, index, duration, lyricsContainer, timeDisplay, progressBar, btn.dataset.language, 0);

    } catch (error) {
        alert('Error playing song: ' + error.message);
        btn.innerHTML = '<i class="bi bi-play-fill"></i> Play';
        btn.disabled = false;
    }
}

async function resumePlayback(btn, index) {
    const detailsRow = document.querySelector(`.song-details[data-index="${index}"]`);
    const lyricsContainer = detailsRow?.querySelector('.lyrics-container');
    const timeDisplay = detailsRow?.querySelector('.time-display');
    const progressBar = detailsRow?.querySelector('.progress-bar-custom');

    try {
        await Tone.start();

        const midiArrayBuffer = await fetchMidi(index);
        const midi = new Midi(midiArrayBuffer);
        const duration = midi.duration;
        const startPosition = state.pausedPosition || 0;

        if (progressBar) {
            progressBar.value = startPosition;
        }

        Tone.Transport.stop();
        Tone.Transport.cancel();
        Tone.Transport.position = startPosition;

        const synths = createSynths();
        scheduleNotes(midi, synths.bassSynth, synths.chordSynth, synths.melodySynth);

        Tone.Transport.start();

        btn.innerHTML = '<i class="bi bi-pause-fill"></i> Pause';
        btn.disabled = false;

        state.pausedPosition = null;
        state.pausedSongIndex = null;

        setupPlaybackUI(btn, index, duration, lyricsContainer, timeDisplay, progressBar, btn.dataset.language, startPosition);

    } catch (error) {
        alert('Error resuming playback: ' + error.message);
        btn.innerHTML = '<i class="bi bi-play-fill"></i> Play';
        btn.disabled = false;
    }
}

function createSynths() {
    const bassSynth = new Tone.MonoSynth({
        oscillator: { type: 'triangle' },
        envelope: { attack: 0.02, decay: 0.3, sustain: 0.4, release: 0.5 },
        volume: -8
    }).toDestination();

    const chordSynth = new Tone.PolySynth(Tone.Synth, {
        oscillator: { type: 'sine' },
        envelope: { attack: 0.05, decay: 0.3, sustain: 0.3, release: 0.8 },
        volume: -15
    }).toDestination();

    const melodySynth = new Tone.Synth({
        oscillator: { type: 'square' },
        envelope: { attack: 0.01, decay: 0.2, sustain: 0.2, release: 0.4 },
        volume: -10
    }).toDestination();

    return { bassSynth, chordSynth, melodySynth };
}

function scheduleNotes(midi, bassSynth, chordSynth, melodySynth) {
    midi.tracks.forEach(track => {
        track.notes.forEach(note => {
            const midiNum = note.midi;
            const synth = midiNum < 55 ? bassSynth : (midiNum < 75 ? chordSynth : melodySynth);
            Tone.Transport.schedule((time) => {
                try {
                    synth.triggerAttackRelease(note.name, note.duration, time, note.velocity);
                } catch (err) {}
            }, note.time);
        });
    });
}

function setupPlaybackUI(btn, index, duration, lyricsContainer, timeDisplay, progressBar, language, startPosition) {
    let animationFrameId;
    let speechTimeouts = [];
    let currentSpeakingIndex = -1;
    let isPausedState = false;

    if (progressBar && !progressBar.dataset.seekBound) {
        progressBar.dataset.seekBound = 'true';

        progressBar.addEventListener('input', (e) => {
            state.isSeeking = true;
            const newTime = parseFloat(e.target.value);

            if (Tone.Transport.state === 'started') {
                Tone.Transport.seconds = newTime;
                if (state.createSpeechTimeouts) {
                    state.createSpeechTimeouts(newTime);
                }
            } else {
                state.pausedPosition = newTime;
                isPausedState = true;
            }
        });

        progressBar.addEventListener('change', () => {
            state.isSeeking = false;
        });
    }

    function createSpeechTimeouts(startPos) {
        speechTimeouts.forEach(id => clearTimeout(id));
        speechTimeouts = [];
        currentSpeakingIndex = -1;

        if (window.speechSynthesis) {
            window.speechSynthesis.cancel();
        }

        if (state.currentLyrics.length > 0 && window.speechSynthesis) {
            const langMap = { 'en': 'en-US', 'de': 'de-DE' };
            const targetLang = langMap[language] || 'en-US';

            let currentLyricIndex = -1;
            for (let i = 0; i < state.currentLyrics.length; i++) {
                if (state.currentLyrics[i].time <= startPos) {
                    currentLyricIndex = i;
                }
            }

            const nextLyricIndex = currentLyricIndex + 1;
            const SKIP_THRESHOLD_MS = 1000;
            let skipCurrent = false;

            if (currentLyricIndex >= 0 && nextLyricIndex < state.currentLyrics.length) {
                const timeUntilNextLine = (state.currentLyrics[nextLyricIndex].time - startPos) * 1000;
                if (timeUntilNextLine < SKIP_THRESHOLD_MS) {
                    skipCurrent = true;
                }
            }

            const firstLyricToSpeak = skipCurrent ? nextLyricIndex : currentLyricIndex;

            state.currentLyrics.forEach((lyric, idx) => {
                if (idx < firstLyricToSpeak) return;

                let delayMs;
                if (idx === currentLyricIndex && !skipCurrent) {
                    delayMs = 100;
                } else {
                    delayMs = (lyric.time - startPos) * 1000;
                }

                const timeoutId = setTimeout(() => {
                    try {
                        const utterance = new SpeechSynthesisUtterance(lyric.text);
                        utterance.rate = 0.9;
                        utterance.pitch = 1.0;
                        utterance.volume = 0.8;
                        utterance.lang = targetLang;

                        const voices = window.speechSynthesis.getVoices();
                        const matchingVoice = voices.find(v => v.lang === targetLang)
                            || voices.find(v => v.lang.startsWith(targetLang.split('-')[0]));
                        if (matchingVoice) utterance.voice = matchingVoice;
    
                        utterance.onstart = () => {
                            currentSpeakingIndex = idx;
                        };

                        window.speechSynthesis.speak(utterance);
                    } catch (err) {
                        console.error('Error creating utterance:', err);
                    }
                }, delayMs);
                speechTimeouts.push(timeoutId);
            });
        }
    }

    state.createSpeechTimeouts = createSpeechTimeouts;
    createSpeechTimeouts(startPosition);

    const updateUI = () => {
        const elapsed = Tone.Transport.seconds;

        if (elapsed >= duration) {
            if (timeDisplay) timeDisplay.textContent = `0:00 / ${formatTime(duration)}`;
            if (progressBar) progressBar.value = 0;

            if (state.currentCleanup) {
                state.currentCleanup(false);
                state.currentCleanup = null;
            }
            return;
        }

        if (timeDisplay) timeDisplay.textContent = `${formatTime(elapsed)} / ${formatTime(duration)}`;

        if (!state.isSeeking && !isPausedState && progressBar) {
            progressBar.value = elapsed;
        }

        if (lyricsContainer) {
            const allLines = lyricsContainer.querySelectorAll('.lyric-line');
            let activeIndex = currentSpeakingIndex;

            if (activeIndex < 0) {
                let lastActiveTime = -1;
                allLines.forEach((line, idx) => {
                    const lineTime = parseFloat(line.dataset.time);
                    if (lineTime <= elapsed && lineTime > lastActiveTime) {
                        activeIndex = idx;
                        lastActiveTime = lineTime;
                    }
                });
            }

            allLines.forEach((line, idx) => {
                if (idx === activeIndex) {
                    line.classList.add('active');
                    line.style.fontWeight = 'bold';
                    line.style.color = '#0d6efd';
                } else {
                    line.classList.remove('active');
                    line.style.fontWeight = 'normal';
                    line.style.color = '';
                }
            });

            if (activeIndex >= 0 && activeIndex < allLines.length) {
                const lastActiveLine = allLines[activeIndex];
                const containerRect = lyricsContainer.getBoundingClientRect();
                const lineRect = lastActiveLine.getBoundingClientRect();
                const isVisible = lineRect.top >= containerRect.top && lineRect.bottom <= containerRect.bottom;

                if (!isVisible) {
                    const relativeTop = lineRect.top - containerRect.top;
                    const containerHeight = containerRect.height;
                    const lineHeight = lineRect.height;
                    const scrollTo = lyricsContainer.scrollTop + relativeTop - (containerHeight / 2) + (lineHeight / 2);

                    lyricsContainer.scrollTo({ top: scrollTo, behavior: 'smooth' });
                }
            }
        }

        animationFrameId = requestAnimationFrame(updateUI);
    };

    animationFrameId = requestAnimationFrame(updateUI);

    state.currentCleanup = (isPause = false) => {
        currentSpeakingIndex = -1;
        cancelAnimationFrame(animationFrameId);
        speechTimeouts.forEach(id => clearTimeout(id));
        speechTimeouts = [];

        if (!isPause) {
            Tone.Transport.stop();
            Tone.Transport.cancel();
            state.createSpeechTimeouts = null;
        } else {
            Tone.Transport.pause();
            isPausedState = true;
        }

        if (window.speechSynthesis) window.speechSynthesis.cancel();

        if (!isPause) {
            btn.innerHTML = '<i class="bi bi-play-fill"></i> Play';
            isPausedState = false;
        }
        btn.disabled = false;
    };
}

function formatTime(seconds) {
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins}:${secs.toString().padStart(2, '0')}`;
}