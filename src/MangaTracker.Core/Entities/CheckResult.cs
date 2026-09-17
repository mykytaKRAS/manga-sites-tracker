namespace MangaTracker.Core.Entities;

public enum CheckStatus
{
    NoNewChapter = 1,
    NewChapterFound = 2,
    Initialized = 3,
    Failed = 4,
    NoProvider = 5
}

public record CheckResult(
    int MangaId,
    string Title,
    CheckStatus Status,
    decimal? LatestChapter,
    string? ChapterUrl);