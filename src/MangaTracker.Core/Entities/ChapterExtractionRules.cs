namespace MangaTracker.Core.Entities;

public record ChapterExtractionRules(
    string Selector,
    string? NumberPattern,
    ChapterNumberSource NumberSource);