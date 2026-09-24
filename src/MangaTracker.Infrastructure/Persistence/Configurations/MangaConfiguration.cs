using MangaTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MangaTracker.Infrastructure.Persistence.Configurations;

public class MangaConfiguration : IEntityTypeConfiguration<Manga>
{
    public void Configure(EntityTypeBuilder<Manga> builder)
    {
        builder.ToTable("Mangas");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(m => m.SourceUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(m => m.LastChapterUrl)
            .HasMaxLength(500);

        builder.Property(m => m.LastKnownChapter)
            .HasPrecision(8, 2);

        builder.HasIndex(m => m.SourceUrl)
            .IsUnique();
    }
}