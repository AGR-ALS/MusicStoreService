import { state } from './state.js';

export async function fetchSongs(page, mode = 'table') {
    const params = new URLSearchParams({
        seed: state.seed,
        page: page,
        pageSize: state.pageSize,
        language: state.language,
        likes: state.likes,
        mode: mode
    });
    const response = await fetch(`/Songs/GetSongs?${params}`);
    if (!response.ok) throw new Error(`Failed to fetch songs: ${response.status}`);
    return await response.json();
}

export async function fetchExpandedSong(index) {
    const params = new URLSearchParams({
        seed: state.seed,
        index: index,
        language: state.language,
        averageLikes: state.likes
    });
    const response = await fetch(`/Songs/ExpandSong?${params}`);
    if (!response.ok) throw new Error(`Failed to expand song: ${response.status}`);
    return await response.json();
}

export async function fetchLikesUpdate(page) {
    const params = new URLSearchParams({
        seed: state.seed,
        page: page,
        likes: state.likes
    });
    const response = await fetch(`/Songs/UpdateLikes?${params}`);
    if (!response.ok) throw new Error(`Failed to update likes: ${response.status}`);
    return await response.json();
}

export async function fetchMidi(index) {
    const params = new URLSearchParams({
        seed: state.seed,
        index: index,
        language: state.language
    });
    const response = await fetch(`/Songs/GetMidi?${params}`);
    if (!response.ok) throw new Error(`Failed to fetch MIDI: ${response.status}`);
    return await response.arrayBuffer();
}