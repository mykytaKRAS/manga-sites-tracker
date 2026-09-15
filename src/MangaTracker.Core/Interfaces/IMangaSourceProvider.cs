using MangaTracker.Core.Entities;

namespace MangaTracker.Core.Interfaces;

public interface IMangaSourceProvider
{
    MangaSource Source { get; }

    Task<ChapterInfo?> GetLatestChapterAsync(string sourceUrl, CancellationToken cancellationToken = default);
}