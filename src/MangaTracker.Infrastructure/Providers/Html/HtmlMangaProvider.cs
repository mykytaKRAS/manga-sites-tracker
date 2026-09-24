using MangaTracker.Core.Entities;
using MangaTracker.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace MangaTracker.Infrastructure.Providers.Html;

public class HtmlMangaProvider : IMangaSourceProvider
{
    private readonly HttpClient _httpClient;
    private readonly IHtmlChapterParser _parser;
    private readonly ILogger<HtmlMangaProvider> _logger;

    public HtmlMangaProvider(
        HttpClient httpClient,
        IHtmlChapterParser parser,
        ILogger<HtmlMangaProvider> logger)
    {
        _httpClient = httpClient;
        _parser = parser;
        _logger = logger;
    }

    public SourceKind Kind => SourceKind.Html;

    public async Task<ChapterInfo?> GetLatestChapterAsync(
        MangaSource source,
        string sourceUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(source.ChapterLinkSelector))
        {
            _logger.LogWarning("Source {Source} has no selector configured", source.Name);
            return null;
        }

        try
        {
            using var response = await _httpClient.GetAsync(sourceUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "{Source} returned {StatusCode} for {Url}",
                    source.Name,
                    (int)response.StatusCode,
                    sourceUrl);
                return null;
            }

            var html = await response.Content.ReadAsStringAsync(cancellationToken);

            var rules = new ChapterExtractionRules(
                source.ChapterLinkSelector,
                source.ChapterNumberPattern,
                source.NumberSource);

            var chapters = await _parser.ParseAsync(html, sourceUrl, rules, cancellationToken);

            if (chapters.Count == 0)
            {
                _logger.LogWarning(
                    "No chapters parsed from {Url} with selector {Selector}",
                    sourceUrl,
                    source.ChapterLinkSelector);
                return null;
            }

            return chapters.MaxBy(c => c.Number);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while loading {Url}", sourceUrl);
            return null;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Timeout while loading {Url}", sourceUrl);
            return null;
        }
    }
}