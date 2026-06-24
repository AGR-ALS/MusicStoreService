import { state } from './state.js';
import { elements } from './elements.js';
import { fetchSongs } from './api.js';
import { attachPlayButtons } from './playback.js';

export async function loadGalleryView(reset = false) {
    if (reset) {
        state.galleryPage = 1;
        state.hasMore = true;
        elements.galleryGrid.innerHTML = '';
    }
    if (state.isLoading || !state.hasMore) return;

    state.isLoading = true;
    elements.gallerySentinel.textContent = 'Loading...';
    try {
        const songs = await fetchSongs(state.galleryPage, 'gallery');
        if (songs.length < state.pageSize) {
            state.hasMore = false;
            elements.gallerySentinel.textContent = 'No more songs';
        } else {
            elements.galleryGrid.insertAdjacentHTML('beforeend', songs.map(renderGalleryCard).join(''));
            state.galleryPage++;
            attachPlayButtons();
        }
    } catch (error) {
        elements.gallerySentinel.textContent = 'Error loading songs';
    } finally {
        state.isLoading = false;
    }
}

function renderGalleryCard(song) {
    const coverBase64 = `data:image/png;base64,${song.coverImage}`;
    return `
        <div class="col-6 col-md-4 col-lg-3">
            <div class="card h-100">
                <img src="${coverBase64}" class="card-img-top" alt="${song.albumTitle}">
                <div class="card-body">
                    <h6 class="card-title">${song.title}</h6>
                    <p class="card-text small text-muted">${song.artist}</p>
                    <p class="card-text small">${song.albumTitle}</p>
                    <div class="d-flex justify-content-between align-items-center">
                        <small class="text-muted">${song.genre}</small>
                        <span class="badge bg-primary">${song.amountOfLikes} 👍</span>
                    </div>
                </div>
            </div>
        </div>
    `;
}

export function setupGalleryObserver() {
    if (!elements.gallerySentinel) return;
    const observer = new IntersectionObserver((entries) => {
        if (entries[0].isIntersecting && state.displayMode === 'gallery' && !state.isLoading && state.hasMore) {
            loadGalleryView();
        }
    }, { rootMargin: '100px', threshold: 0.1 });
    observer.observe(elements.gallerySentinel);
}