using System.Text.Json.Serialization;

namespace MangaTracker.Infrastructure.Providers.MangaLib;

internal sealed record MangaLibChaptersResponse(
    [property: JsonPropertyName("data")] IReadOnlyList<MangaLibChapter>? Data);

internal sealed record MangaLibChapter(
    [property: JsonPropertyName("number")] string? Number,
    [property: JsonPropertyName("volume")] string? Volume,
    [property: JsonPropertyName("index")] int Index);