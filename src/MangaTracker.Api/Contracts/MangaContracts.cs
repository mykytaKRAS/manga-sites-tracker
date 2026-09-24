using MangaTracker.Core.Entities;

namespace MangaTracker.Api.Contracts;

public record CreateMangaRequest(
    string Title,
    int SourceId,
    string SourceUrl);

public record MangaResponse(
    int Id,
    string Title,
    string Source,
    string SourceUrl,
    decimal? LastKnownChapter,
    string? LastChapterUrl,
    DateTime? LastCheckedAt,
    bool HasUnreadChapter);