using MangaTracker.Core.Entities;

namespace MangaTracker.Core.Interfaces;

public interface IMangaSourceProvider
{
    SourceKind Kind { get; }

    Task<ChapterInfo?> GetLatestChapterAsync(
        MangaSource source,
        string sourceUrl,
        CancellationToken cancellationToken = default);
}