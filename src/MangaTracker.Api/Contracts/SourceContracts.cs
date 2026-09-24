using MangaTracker.Core.Entities;

namespace MangaTracker.Api.Contracts;

public record CreateSourceRequest(
    string Name,
    SourceKind Kind,
    string? ChapterLinkSelector,
    string? ChapterNumberPattern,
    ChapterNumberSource NumberSource);

public record UpdateSourceRequest(
    string? ChapterLinkSelector,
    string? ChapterNumberPattern,
    ChapterNumberSource NumberSource);

public record SetSourceEnabledRequest(
    bool IsEnabled,
    string? Reason);

public record SourceResponse(
    int Id,
    string Name,
    string Kind,
    string? ChapterLinkSelector,
    string? ChapterNumberPattern,
    string NumberSource,
    bool IsEnabled,
    string? DisabledReason);

public record TestSourceRequest(
    string PageUrl,
    string Selector,
    string? NumberPattern,
    ChapterNumberSource NumberSource);

public record TestChapterPreview(decimal Number, string Url);

public record TestSourceResponse(
    bool Success,
    string Message,
    IReadOnlyList<TestChapterPreview> Chapters);