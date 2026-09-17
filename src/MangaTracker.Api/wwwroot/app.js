const listEl = document.getElementById('list');
const statusEl = document.getElementById('status');
const checkAllBtn = document.getElementById('check-all');
const addForm = document.getElementById('add-form');

const STATUS_LABELS = {
    NewChapterFound: 'вышла новая глава',
    NoNewChapter: 'без изменений',
    Initialized: 'добавлена в отслеживание',
    Failed: 'не удалось проверить',
    NoProvider: 'нет провайдера'
};

async function api(path, options = {}) {
    const response = await fetch(`/api/manga${path}`, {
        headers: { 'Content-Type': 'application/json' },
        ...options
    });

    if (!response.ok) {
        const body = await response.text();
        throw new Error(`${response.status}: ${body || response.statusText}`);
    }

    return response.status === 204 ? null : response.json();
}

function formatDate(iso) {
    if (!iso) return 'ещё не проверялась';
    return new Date(iso).toLocaleString('ru', { dateStyle: 'short', timeStyle: 'short' });
}

function renderItem(manga) {
    const li = document.createElement('li');
    if (manga.hasUnreadChapter) li.classList.add('unread');

    const info = document.createElement('div');
    info.className = 'info';

    const title = document.createElement('div');
    title.className = 'title';
    title.textContent = manga.title;

    const meta = document.createElement('div');
    meta.className = 'meta';
    const chapter = manga.lastKnownChapter ?? '—';
    meta.textContent = `${manga.source} · глава ${chapter} · ${formatDate(manga.lastCheckedAt)}`;

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
        readBtn.addEventListener('click', async () => {
            await api(`/${manga.id}/mark-as-read`, { method: 'POST' });
            await load();
        });
        li.append(readBtn);
    }

    const deleteBtn = document.createElement('button');
    deleteBtn.textContent = '✕';
    deleteBtn.title = 'Удалить';
    deleteBtn.addEventListener('click', async () => {
        if (!confirm(`Удалить «${manga.title}»?`)) return;
        await api(`/${manga.id}`, { method: 'DELETE' });
        await load();
    });
    li.append(deleteBtn);

    return li;
}

async function load() {
    const mangas = await api('/');
    listEl.replaceChildren(...mangas.map(renderItem));
}

async function checkAll() {
    checkAllBtn.disabled = true;
    statusEl.textContent = 'Проверяю источники…';

    try {
        const report = await api('/check-all', { method: 'POST' });
        await load();

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

addForm.addEventListener('submit', async (event) => {
    event.preventDefault();

    const data = Object.fromEntries(new FormData(addForm));

    try {
        await api('/', { method: 'POST', body: JSON.stringify(data) });
        addForm.reset();
        statusEl.textContent = '';
        await load();
    } catch (error) {
        statusEl.textContent = `Не удалось добавить: ${error.message}`;
    }
});

checkAllBtn.addEventListener('click', checkAll);

load().catch(error => {
    statusEl.textContent = `Не удалось загрузить список: ${error.message}`;
});