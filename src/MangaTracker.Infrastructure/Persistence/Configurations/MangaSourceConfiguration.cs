using MangaTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MangaTracker.Infrastructure.Persistence.Configurations;

public class MangaSourceConfiguration : IEntityTypeConfiguration<MangaSource>
{
    public void Configure(EntityTypeBuilder<MangaSource> builder)
    {
        builder.ToTable("Sources");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Kind)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.NumberSource)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.ChapterLinkSelector).HasMaxLength(500);
        builder.Property(s => s.ChapterNumberPattern).HasMaxLength(500);
        builder.Property(s => s.DisabledReason).HasMaxLength(300);

        builder.HasIndex(s => s.Name).IsUnique();

        builder.HasMany(s => s.Mangas)
            .WithOne(m => m.Source)
            .HasForeignKey(m => m.SourceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}