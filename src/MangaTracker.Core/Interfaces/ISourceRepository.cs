using MangaTracker.Core.Entities;

namespace MangaTracker.Core.Interfaces;

public interface ISourceRepository
{
    Task<IReadOnlyList<MangaSource>> GetAllAsync(CancellationToken ct = default);
    Task<MangaSource?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<bool> HasMangasAsync(int sourceId, CancellationToken ct = default);
    Task AddAsync(MangaSource source, CancellationToken ct = default);
    void Remove(MangaSource source);
    Task SaveChangesAsync(CancellationToken ct = default);
}