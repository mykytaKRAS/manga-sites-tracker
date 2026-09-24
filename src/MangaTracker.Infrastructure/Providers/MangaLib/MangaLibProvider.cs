using System.Globalization;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using MangaTracker.Core.Entities;
using MangaTracker.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace MangaTracker.Infrastructure.Providers.MangaLib;

public partial class MangaLibProvider : IMangaSourceProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MangaLibProvider> _logger;

    public MangaLibProvider(HttpClient httpClient, ILogger<MangaLibProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public SourceKind Kind => SourceKind.MangaLibApi;

    public async Task<ChapterInfo?> GetLatestChapterAsync(
        MangaSource source,
        string sourceUrl,
        CancellationToken cancellationToken = default)
    {
        var slug = ExtractSlug(sourceUrl);
        if (slug is null)
        {
            _logger.LogWarning("Could not extract manga slug from URL {Url}", sourceUrl);
            return null;
        }

        try
        {
            using var httpResponse = await _httpClient.GetAsync(
                $"api/manga/{slug}/chapters",
                cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "MangaLib API returned {StatusCode} for slug {Slug}",
                    (int)httpResponse.StatusCode,
                    slug);
                return null;
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<MangaLibChaptersResponse>(
                cancellationToken);

            var chapters = response?.Data;
            if (chapters is null || chapters.Count == 0)
            {
                _logger.LogWarning("No chapters returned for slug {Slug}", slug);
                return null;
            }

            var latest = chapters
                .Select(c => new { Number = ParseChapterNumber(c.Number), Raw = c })
                .Where(x => x.Number.HasValue)
                .OrderByDescending(x => x.Number!.Value)
                .ThenByDescending(x => x.Raw.Index)
                .FirstOrDefault();

            if (latest is null)
            {
                _logger.LogWarning("Could not parse any chapter number for slug {Slug}", slug);
                return null;
            }

            var chapterUrl = BuildChapterUrl(sourceUrl, slug, latest.Raw);

            return new ChapterInfo(latest.Number!.Value, chapterUrl);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while checking {Slug}", slug);
            return null;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Timeout while checking {Slug}", slug);
            return null;
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "Unexpected response format for {Slug}", slug);
            return null;
        }
    }

    private static string? ExtractSlug(string sourceUrl)
    {
        var match = SlugRegex().Match(sourceUrl);
        return match.Success ? match.Groups["slug"].Value : null;
    }

    private static decimal? ParseChapterNumber(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var number)
            ? number
            : null;
    }

    private static string BuildChapterUrl(string sourceUrl, string slug, MangaLibChapter chapter)
    {
        var origin = new Uri(sourceUrl).GetLeftPart(UriPartial.Authority);
        return $"{origin}/ru/{slug}/read/v{chapter.Volume}/c{chapter.Number}";
    }

    [GeneratedRegex(@"/(?<slug>\d+--[a-z0-9\-]+)", RegexOptions.IgnoreCase)]
    private static partial Regex SlugRegex();
}