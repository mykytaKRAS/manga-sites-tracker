using MangaTracker.Core.Entities;
using MangaTracker.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaTracker.Infrastructure.Persistence.Repositories;

public class SourceRepository : ISourceRepository
{
    private readonly MangaTrackerDbContext _context;

    public SourceRepository(MangaTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MangaSource>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Sources
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<MangaSource?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Sources
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Sources
            .AnyAsync(s => s.Name == name, ct);
    }

    public async Task<bool> HasMangasAsync(int sourceId, CancellationToken ct = default)
    {
        return await _context.Mangas
            .AnyAsync(m => m.SourceId == sourceId, ct);
    }

    public async Task AddAsync(MangaSource source, CancellationToken ct = default)
    {
        await _context.Sources.AddAsync(source, ct);
    }

    public void Remove(MangaSource source)
    {
        _context.Sources.Remove(source);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}