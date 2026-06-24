export const state = {
    seed: 12345,
    language: 'en',
    likes: 5.0,
    currentPage: 1,
    pageSize: 50,
    displayMode: 'table',
    galleryPage: 1,
    isLoading: false,
    hasMore: true,
    currentCleanup: null,
    currentLyrics: [],
    expandedSongs: new Map(),
    pausedPosition: null,
    pausedSongIndex: null,
    createSpeechTimeouts: null,
    isSeeking: false
};

export function initFromDataAttributes() {
    const appRoot = document.getElementById('app');
    if (!appRoot) return;

    state.seed = parseInt(appRoot.dataset.seed, 10);
    state.language = appRoot.dataset.language;
    state.likes = parseFloat(appRoot.dataset.likes);
    state.currentPage = parseInt(appRoot.dataset.page, 10);
}