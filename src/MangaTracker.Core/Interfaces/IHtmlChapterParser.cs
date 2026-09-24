using MangaTracker.Core.Entities;

namespace MangaTracker.Core.Interfaces;

public interface IHtmlChapterParser
{   
    Task<IReadOnlyList<ChapterInfo>> ParseAsync(
        string html,
        string pageUrl,
        ChapterExtractionRules rules,
        CancellationToken cancellationToken = default);
}