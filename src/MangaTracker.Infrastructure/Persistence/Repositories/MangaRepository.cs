using MangaTracker.Core.Entities;
using MangaTracker.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaTracker.Infrastructure.Persistence.Repositories;

public class MangaRepository : IMangaRepository
{
    private readonly MangaTrackerDbContext _context;

    public MangaRepository(MangaTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Manga>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Mangas
            .AsNoTracking()
            .OrderByDescending(m => m.HasUnreadChapter)
            .ThenBy(m => m.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<Manga?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Mangas
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByUrlAsync(string sourceUrl, CancellationToken cancellationToken = default)
    {
        return await _context.Mangas
            .AnyAsync(m => m.SourceUrl == sourceUrl, cancellationToken);
    }

    public async Task AddAsync(Manga manga, CancellationToken cancellationToken = default)
    {
        await _context.Mangas.AddAsync(manga, cancellationToken);
    }

    public void Remove(Manga manga)
    {
        _context.Mangas.Remove(manga);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Manga>> GetAllForUpdateAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Mangas
            .OrderBy(m => m.Id)
            .ToListAsync(cancellationToken);
    }
}