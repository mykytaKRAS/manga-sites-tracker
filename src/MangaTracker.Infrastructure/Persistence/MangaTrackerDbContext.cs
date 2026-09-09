using MangaTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MangaTracker.Infrastructure.Persistence;

public class MangaTrackerDbContext : DbContext
{
    public MangaTrackerDbContext(DbContextOptions<MangaTrackerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Manga> Mangas => Set<Manga>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MangaTrackerDbContext).Assembly);
    }
}