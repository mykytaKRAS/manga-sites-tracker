using MangaTracker.Core.Entities;
using MangaTracker.Core.Interfaces;
using MangaTracker.Core.Exceptions;

namespace MangaTracker.Core.Services;

public class MangaService
{
    private readonly IMangaRepository _repository;

    public MangaService(IMangaRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<Manga>> GetAllAsync(CancellationToken ct = default)
        => _repository.GetAllAsync(ct);

    public Task<Manga?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repository.GetByIdAsync(id, ct);

    public async Task<Manga> AddAsync(string title, MangaSource source, string sourceUrl, CancellationToken ct = default)
    {
        var normalizedUrl = sourceUrl.Trim().TrimEnd('/');

        if (await _repository.ExistsByUrlAsync(normalizedUrl, ct))
        {
            throw new DuplicateMangaException(normalizedUrl);
        }

        var manga = new Manga
        {
            Title = title.Trim(),
            Source = source,
            SourceUrl = normalizedUrl,
            CreatedAt = DateTime.UtcNow,
            HasUnreadChapter = false
        };

        await _repository.AddAsync(manga, ct);
        await _repository.SaveChangesAsync(ct);

        return manga;
    }

    public async Task<bool> MarkAsReadAsync(int id, CancellationToken ct = default)
    {
        var manga = await _repository.GetByIdAsync(id, ct);
        if (manga is null)
        {
            return false;
        }

        manga.HasUnreadChapter = false;
        await _repository.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var manga = await _repository.GetByIdAsync(id, ct);
        if (manga is null)
        {
            return false;
        }

        _repository.Remove(manga);
        await _repository.SaveChangesAsync(ct);

        return true;
    }
}