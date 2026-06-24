import { state } from './state.js';
import { elements } from './elements.js';
import { fetchSongs, fetchExpandedSong } from './api.js';
import { attachPlayButtons } from './playback.js';

export async function loadTableView() {
    elements.songsTableBody.innerHTML = '<tr><td colspan="7" class="text-center py-4">Loading...</td></tr>';
    try {
        const songs = await fetchSongs(state.currentPage, 'table');
        elements.songsTableBody.innerHTML = '';
        songs.forEach(song => {
            const rowFragment = renderTableRow(song);
            elements.songsTableBody.appendChild(rowFragment);
        });
        elements.pageNumber.textContent = `Page ${state.currentPage}`;
        elements.previousPageButton.disabled = state.currentPage === 1;
        attachTableRowListeners();
        attachPlayButtons();
    } catch (error) {
        elements.songsTableBody.innerHTML = `<tr><td colspan="7" class="text-center text-danger py-4">Error: ${error.message}</td></tr>`;
    }
}

function renderTableRow(song) {
    const template = document.getElementById('song-row-template');
    const clone = template.content.cloneNode(true);

    const rows = clone.querySelectorAll('tr');
    const songRow = rows[0];
    const detailsRow = rows[1];

    songRow.dataset.index = song.index;
    detailsRow.dataset.index = song.index;

    songRow.querySelector('.col-index').textContent = song.index;
    songRow.querySelector('.col-title').textContent = song.title;
    songRow.querySelector('.col-artist').textContent = song.artist;
    songRow.querySelector('.col-album').textContent = song.albumTitle;
    songRow.querySelector('.col-genre').textContent = song.genre;

    const likesCell = songRow.querySelector('.likes-cell');
    likesCell.dataset.index = song.index;

    const countSpan = likesCell.querySelector('.likes-cell__count');
    if (countSpan) {
        countSpan.textContent = song.amountOfLikes;
    }

    const fragment = document.createDocumentFragment();
    fragment.appendChild(songRow);
    fragment.appendChild(detailsRow);

    return fragment;
}

export function attachTableRowListeners() {
    document.querySelectorAll('.song-row').forEach(row => {
        row.addEventListener('click', async function() {
            const index = this.dataset.index;
            const detailsRow = document.querySelector(`.song-details[data-index="${index}"]`);
            const icon = this.querySelector('.expand-icon');

            if (detailsRow.classList.contains('d-none')) {
                document.querySelectorAll('.song-details').forEach(d => d.classList.add('d-none'));
                document.querySelectorAll('.expand-icon').forEach(i => {
                    i.classList.remove('bi-chevron-up');
                    i.classList.add('bi-chevron-down');
                });

                if (!state.expandedSongs.has(index)) {
                    try {
                        const expandedSong = await fetchExpandedSong(index);
                        state.expandedSongs.set(index, expandedSong);
                        detailsRow.querySelector('td').innerHTML = renderSongDetails(expandedSong);
                        attachPlayButtons();
                    } catch (error) {
                        detailsRow.querySelector('td').innerHTML = '<div class="text-center text-danger py-4">Error loading song details</div>';
                    }
                }

                detailsRow.classList.remove('d-none');
                icon.classList.remove('bi-chevron-down');
                icon.classList.add('bi-chevron-up');
            } else {
                detailsRow.classList.add('d-none');
                icon.classList.remove('bi-chevron-up');
                icon.classList.add('bi-chevron-down');
            }
        });
    });
}

function renderSongDetails(song) {
    const coverBase64 = `data:image/png;base64,${song.coverImage}`;
    const lyricsHtml = (song.audio?.lyrics || []).map(lyric =>
        `<p class="lyric-line mb-2" data-time="${Number(lyric.time).toFixed(2)}">${lyric.text}</p>`
    ).join('');

    return `
        <div class="player-container p-4">
            <div class="row align-items-center">
                <div class="col-md-2">
                    <img src="${coverBase64}" alt="${song.albumTitle}" class="img-fluid rounded shadow" style="max-height: 200px;">
                    <div class="text-center mt-2">
                        <span class="badge bg-primary">${song.amountOfLikes} 👍</span>
                    </div>
                </div>
                <div class="col-md-10">
                    <h3 class="mb-1">
                        ${song.title}
                        <button class="btn btn-primary btn-sm play-btn ms-2"
                                data-seed="${state.seed}"
                                data-index="${song.index}"
                                data-language="${song.language}">
                            <i class="bi bi-play-fill"></i> Play
                        </button>
                        <span class="time-display ms-2 text-muted">0:00 / 0:00</span>
                    </h3>
                    <p class="text-muted mb-2">from <strong>${song.albumTitle}</strong> by <em>${song.artist}</em></p>
                    <div class="progress-container mb-3">
                        <input type="range" class="form-range progress-bar-custom" id="progress-${song.index}" min="0" max="100" value="0" step="0.01">
                    </div>
                    <ul class="nav nav-tabs">
                        <li class="nav-item"><a class="nav-link active" data-bs-toggle="tab" href="#lyrics-${song.index}">Lyrics</a></li>
                        <li class="nav-item"><a class="nav-link" data-bs-toggle="tab" href="#review-${song.index}">Review</a></li>
                    </ul>
                    <div class="tab-content p-3 border border-top-0 rounded-bottom">
                        <div class="tab-pane fade show active" id="lyrics-${song.index}">
                            <div class="lyrics-container" style="max-height: 200px; overflow-y: auto;">
                                ${lyricsHtml}
                            </div>
                        </div>
                        <div class="tab-pane fade" id="review-${song.index}">
                            <p class="fst-italic">${song.reviewText}</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;
}