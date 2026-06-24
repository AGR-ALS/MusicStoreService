import { state, initFromDataAttributes } from './state.js';
import { elements, cacheElements } from './elements.js';
import { loadTableView, attachTableRowListeners } from './table.js';
import { loadGalleryView, setupGalleryObserver } from './gallery.js';
import { attachPlayButtons } from './playback.js';
import { fetchLikesUpdate } from './api.js';

document.addEventListener('DOMContentLoaded', init);

function init() {
    initFromDataAttributes();
    cacheElements();
    syncHeaderWithState();
    bindEvents();
    attachTableRowListeners();
    attachPlayButtons();
    setupGalleryObserver();
}

function syncHeaderWithState() {
    if (elements.localeSelect) elements.localeSelect.value = state.language;
    if (elements.seedInput) elements.seedInput.value = state.seed;
    if (elements.likesInput) elements.likesInput.value = state.likes.toFixed(1);
}

function bindEvents() {
    elements.localeSelect.addEventListener('change', (e) => {
        state.language = e.target.value;
        resetAndReload();
    });

    let seedDebounceTimer;
    elements.seedInput.addEventListener('input', (e) => {
        e.target.value = e.target.value.replace(/\D/g, '');
        const newSeed = parseInt(e.target.value, 10);

        clearTimeout(seedDebounceTimer);
        seedDebounceTimer = setTimeout(() => {
            if (!isNaN(newSeed) && newSeed !== state.seed) {
                state.seed = newSeed;
                resetAndReload();
            }
        }, 400);
    });

    let likesDebounceTimer;
    elements.likesInput.addEventListener('input', (e) => {
        const newLikes = parseFloat(e.target.value);

        clearTimeout(likesDebounceTimer);
        likesDebounceTimer = setTimeout(() => {
            if (!isNaN(newLikes) && newLikes !== state.likes) {
                state.likes = newLikes;
                updateLikesOnly();
            }
        }, 400);
    });

    elements.randomSeedButton.addEventListener('click', () => {
        state.seed = Math.floor(Math.random() * Number.MAX_SAFE_INTEGER);
        elements.seedInput.value = state.seed;
        resetAndReload();
    });

    elements.tableMode.addEventListener('change', () => {
        state.displayMode = 'table';
        elements.tableView.classList.remove('d-none');
        elements.galleryView.classList.add('d-none');
        resetAndReload();
    });

    elements.galleryMode.addEventListener('change', () => {
        state.displayMode = 'gallery';
        elements.tableView.classList.add('d-none');
        elements.galleryView.classList.remove('d-none');
        loadGalleryView(true);
    });

    elements.previousPageButton.addEventListener('click', () => {
        if (state.currentPage > 1) {
            state.currentPage--;
            loadTableView();
        }
    });

    elements.nextPageButton.addEventListener('click', () => {
        state.currentPage++;
        loadTableView();
    });
}

async function updateLikesOnly() {
    try {
        const likesData = await fetchLikesUpdate(state.currentPage);
        likesData.forEach(({ index, likes }) => {
            const likesCell = document.querySelector(`.likes-cell[data-index="${index}"]`);
            if (likesCell) {
                const countElement = likesCell.querySelector('.likes-cell__count');
                if (countElement) {
                    countElement.textContent = likes;
                }
            }

            const expandedSong = state.expandedSongs.get(index);
            if (expandedSong) {
                expandedSong.amountOfLikes = likes;
                const detailsRow = document.querySelector(`.song-details[data-index="${index}"]`);
                if (detailsRow) {
                    const badgeCount = detailsRow.querySelector('.likes-cell__count');
                    if (badgeCount) {
                        badgeCount.textContent = likes;
                    }
                }
            }
        });
    } catch (error) {
        console.error('Error updating likes:', error);
    }
}
function resetAndReload() {
    if (state.currentCleanup) {
        state.currentCleanup();
        state.currentCleanup = null;
    }
    state.currentPage = 1;
    state.expandedSongs.clear();

    if (state.displayMode === 'table') {
        loadTableView();
    } else {
        loadGalleryView(true);
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
}