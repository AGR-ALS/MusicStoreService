export const elements = {};

export function cacheElements() {
    elements.localeSelect = document.getElementById('localeSelect');
    elements.seedInput = document.getElementById('seedInput');
    elements.randomSeedButton = document.getElementById('randomSeedButton');
    elements.likesInput = document.getElementById('likesInput');
    elements.tableMode = document.getElementById('tableMode');
    elements.galleryMode = document.getElementById('galleryMode');
    elements.tableView = document.getElementById('tableView');
    elements.galleryView = document.getElementById('galleryView');
    elements.songsTableBody = document.getElementById('songsTableBody');
    elements.galleryGrid = document.getElementById('galleryGrid');
    elements.gallerySentinel = document.getElementById('gallerySentinel');
    elements.previousPageButton = document.getElementById('previousPageButton');
    elements.nextPageButton = document.getElementById('nextPageButton');
    elements.pageNumber = document.getElementById('pageNumber');
}