const listEl = document.getElementById('list');
const statusEl = document.getElementById('status');
const checkAllBtn = document.getElementById('check-all');
const addMangaForm = document.getElementById('add-manga-form');
const sourceSelect = addMangaForm.querySelector('select[name="sourceId"]');
const sourcesListEl = document.getElementById('sources-list');
const addSourceForm = document.getElementById('add-source-form');
const testSelectorBtn = document.getElementById('test-selector');
const testResultEl = document.getElementById('test-result');

async function api(path, options = {}) {
    const response = await fetch(path, {
        headers: { 'Content-Type': 'application/json' },
        ...options
    });

    if (!response.ok) {
        let message = response.statusText;
        try {
            const body = await response.json();
            message = body.message
                ?? Object.values(body.errors ?? {}).flat().join(', ')
                ?? message;
        } catch {
            // тело не JSON — оставляем statusText
        }
        throw new Error(`${response.status}: ${message}`);
    }

    return response.status === 204 ? null : response.json();
}

function formatDate(iso) {
    if (!iso) return 'ещё не проверялась';
    return new Date(iso).toLocaleString('ru', { dateStyle: 'short', timeStyle: 'short' });
}

function renderManga(manga) {
    const li = document.createElement('li');
    if (manga.hasUnreadChapter) li.classList.add('unread');

    const info = document.createElement('div');
    info.className = 'info';

    const title = document.createElement('div');
    title.className = 'title';
    title.textContent = manga.title;

    const meta = document.createElement('div');
    meta.className = 'meta';
    meta.textContent = `${manga.source} · глава ${manga.lastKnownChapter ?? '—'} · ${formatDate(manga.lastCheckedAt)}`;

    info.append(title, meta);
    li.append(info);

    if (manga.hasUnreadChapter) {
        const badge = document.createElement('span');
        badge.className = 'badge';
        badge.textContent = 'NEW';
        li.append(badge);
    }

    if (manga.lastChapterUrl) {
        const link = document.createElement('a');
        link.href = manga.lastChapterUrl;
        link.target = '_blank';
        link.rel = 'noopener noreferrer';
        link.textContent = 'Читать';
        li.append(link);
    }

    if (manga.hasUnreadChapter) {
        const readBtn = document.createElement('button');
        readBtn.textContent = 'Прочитано';
        readBtn.addEventListener('click', () => run(async () => {
            await api(`/api/manga/${manga.id}/mark-as-read`, { method: 'POST' });
            await loadMangas();
        }));
        li.append(readBtn);
    }

    const deleteBtn = document.createElement('button');
    deleteBtn.className = 'danger';
    deleteBtn.textContent = '✕';
    deleteBtn.title = 'Удалить';
    deleteBtn.addEventListener('click', () => run(async () => {
        if (!confirm(`Удалить «${manga.title}»?`)) return;
        await api(`/api/manga/${manga.id}`, { method: 'DELETE' });
        await loadMangas();
    }));
    li.append(deleteBtn);

    return li;
}

function renderSource(source) {
    const li = document.createElement('li');
    if (!source.isEnabled) li.classList.add('disabled');

    const info = document.createElement('div');
    info.className = 'info';

    const title = document.createElement('div');
    title.className = 'title';
    title.textContent = source.name;

    const meta = document.createElement('div');
    meta.className = 'meta';
    meta.textContent = source.kind === 'Html'
        ? `${source.kind} · ${source.chapterLinkSelector ?? 'селектор не задан'}`
        : source.kind;

    info.append(title, meta);
    li.append(info);

    if (!source.isEnabled) {
        const badge = document.createElement('span');
        badge.className = 'badge muted';
        badge.textContent = source.disabledReason || 'выключен';
        li.append(badge);
    }

    const toggleBtn = document.createElement('button');
    toggleBtn.textContent = source.isEnabled ? 'Выключить' : 'Включить';
    toggleBtn.addEventListener('click', () => run(async () => {
        const reason = source.isEnabled
            ? prompt('Причина (необязательно)') ?? ''
            : null;

        await api(`/api/sources/${source.id}/enabled`, {
            method: 'POST',
            body: JSON.stringify({ isEnabled: !source.isEnabled, reason })
        });
        await loadSources();
    }));
    li.append(toggleBtn);

    const deleteBtn = document.createElement('button');
    deleteBtn.className = 'danger';
    deleteBtn.textContent = '✕';
    deleteBtn.title = 'Удалить источник';
    deleteBtn.addEventListener('click', () => run(async () => {
        if (!confirm(`Удалить источник «${source.name}»?`)) return;
        await api(`/api/sources/${source.id}`, { method: 'DELETE' });
        await loadSources();
    }));
    li.append(deleteBtn);

    return li;
}

async function loadMangas() {
    const mangas = await api('/api/manga');
    listEl.replaceChildren(...mangas.map(renderManga));
}

async function loadSources() {
    const sources = await api('/api/sources');

    sourcesListEl.replaceChildren(...sources.map(renderSource));

    sourceSelect.replaceChildren(...sources
        .filter(s => s.isEnabled)
        .map(s => {
            const option = document.createElement('option');
            option.value = s.id;
            option.textContent = s.name;
            return option;
        }));
}

async function run(action) {
    try {
        await action();
    } catch (error) {
        statusEl.textContent = error.message;
    }
}

async function checkAll() {
    checkAllBtn.disabled = true;
    statusEl.textContent = 'Проверяю источники…';

    try {
        const report = await api('/api/manga/check-all', { method: 'POST' });
        await loadMangas();

        const fresh = report.results
            .filter(r => r.status === 'NewChapterFound')
            .map(r => r.title);

        statusEl.textContent = fresh.length
            ? `Новые главы: ${fresh.join(', ')}`
            : `Новых глав нет. Проверено ${report.total}, ошибок ${report.failed}.`;
    } catch (error) {
        statusEl.textContent = `Ошибка: ${error.message}`;
    } finally {
        checkAllBtn.disabled = false;
    }
}

async function testSelector() {
    const data = Object.fromEntries(new FormData(addSourceForm));

    if (!data.testUrl) {
        testResultEl.textContent = 'Укажи ссылку для проверки.';
        return;
    }

    testSelectorBtn.disabled = true;
    testResultEl.textContent = 'Загружаю страницу…';

    try {
        const result = await api('/api/sources/test', {
            method: 'POST',
            body: JSON.stringify({
                pageUrl: data.testUrl,
                selector: data.chapterLinkSelector,
                numberPattern: data.chapterNumberPattern || null,
                numberSource: data.numberSource
            })
        });

        testResultEl.replaceChildren();

        const message = document.createElement('div');
        message.className = result.success ? 'ok' : 'fail';
        message.textContent = result.message;
        testResultEl.append(message);

        for (const chapter of result.chapters) {
            const row = document.createElement('div');
            row.className = 'row';
            row.textContent = `${chapter.number} → ${chapter.url}`;
            testResultEl.append(row);
        }
    } catch (error) {
        testResultEl.textContent = error.message;
    } finally {
        testSelectorBtn.disabled = false;
    }
}

addMangaForm.addEventListener('submit', (event) => {
    event.preventDefault();

    run(async () => {
        const data = Object.fromEntries(new FormData(addMangaForm));

        await api('/api/manga', {
            method: 'POST',
            body: JSON.stringify({
                title: data.title,
                sourceId: Number(data.sourceId),
                sourceUrl: data.sourceUrl
            })
        });

        addMangaForm.reset();
        statusEl.textContent = '';
        await loadMangas();
    });
});

addSourceForm.addEventListener('submit', (event) => {
    event.preventDefault();

    run(async () => {
        const data = Object.fromEntries(new FormData(addSourceForm));

        await api('/api/sources', {
            method: 'POST',
            body: JSON.stringify({
                name: data.name,
                kind: data.kind,
                chapterLinkSelector: data.chapterLinkSelector || null,
                chapterNumberPattern: data.chapterNumberPattern || null,
                numberSource: data.numberSource
            })
        });

        addSourceForm.reset();
        testResultEl.textContent = '';
        await loadSources();
    });
});

checkAllBtn.addEventListener('click', checkAll);
testSelectorBtn.addEventListener('click', testSelector);

run(async () => {
    await loadSources();
    await loadMangas();
});