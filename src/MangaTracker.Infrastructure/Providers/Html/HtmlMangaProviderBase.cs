using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using MangaTracker.Core.Entities;
using MangaTracker.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace MangaTracker.Infrastructure.Providers.Html;

public abstract partial class HtmlMangaProviderBase : IMangaSourceProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    protected HtmlMangaProviderBase(HttpClient httpClient, ILogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public abstract MangaSource Source { get; }

    protected abstract string ChapterLinkSelector { get; }

    public async Task<ChapterInfo?> GetLatestChapterAsync(
        string sourceUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var html = await _httpClient.GetStringAsync(sourceUrl, cancellationToken);

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(html, cancellationToken);

            var links = document.QuerySelectorAll(ChapterLinkSelector);
            if (links.Length == 0)
            {
                _logger.LogWarning(
                    "Selector {Selector} matched nothing on {Url}. Markup may have changed.",
                    ChapterLinkSelector,
                    sourceUrl);
                return null;
            }

            var latest = links
                .Select(link => new
                {
                    Number = ExtractChapterNumber(link.TextContent, link.GetAttribute("href")),
                    Href = link.GetAttribute("href")
                })
                .Where(x => x.Number.HasValue && !string.IsNullOrWhiteSpace(x.Href))
                .OrderByDescending(x => x.Number!.Value)
                .FirstOrDefault();

            if (latest is null)
            {
                _logger.LogWarning("Could not parse any chapter number on {Url}", sourceUrl);
                return null;
            }

            var absoluteUrl = ToAbsoluteUrl(latest.Href!, sourceUrl);

            return new ChapterInfo(latest.Number!.Value, absoluteUrl);
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

    protected virtual decimal? ExtractChapterNumber(string? linkText, string? href)
    {
        return ParseFirstNumber(linkText) ?? ParseFirstNumber(href);
    }

    protected static decimal? ParseFirstNumber(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var match = NumberRegex().Match(input);
        if (!match.Success)
        {
            return null;
        }

        return decimal.TryParse(
            match.Value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var number)
            ? number
            : null;
    }

    private static string ToAbsoluteUrl(string href, string pageUrl)
    {
        return Uri.TryCreate(new Uri(pageUrl), href, out var absolute)
            ? absolute.ToString()
            : href;
    }

    [GeneratedRegex(@"\d+(?:[.,]\d+)?")]
    private static partial Regex NumberRegex();
}