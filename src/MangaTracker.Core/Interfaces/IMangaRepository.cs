using MangaTracker.Core.Entities;

namespace MangaTracker.Core.Interfaces;

public interface IMangaRepository
{
    Task<IReadOnlyList<Manga>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Manga?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByUrlAsync(string sourceUrl, CancellationToken cancellationToken = default);

    Task AddAsync(Manga manga, CancellationToken cancellationToken = default);

    void Remove(Manga manga);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}