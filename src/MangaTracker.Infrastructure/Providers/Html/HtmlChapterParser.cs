using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using MangaTracker.Core.Entities;
using MangaTracker.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace MangaTracker.Infrastructure.Providers.Html;

public partial class HtmlChapterParser : IHtmlChapterParser
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(200);

    private readonly ILogger<HtmlChapterParser> _logger;

    public HtmlChapterParser(ILogger<HtmlChapterParser> logger)
    {
        _logger = logger;
    }

    public async Task<IReadOnlyList<ChapterInfo>> ParseAsync(
        string html,
        string pageUrl,
        ChapterExtractionRules rules,
        CancellationToken cancellationToken = default)
    {
        var parser = new HtmlParser();
        var document = await parser.ParseDocumentAsync(html, cancellationToken);

        var links = document.QuerySelectorAll(rules.Selector);

        var chapters = new List<ChapterInfo>(links.Length);

        foreach (var link in links)
        {
            var href = link.GetAttribute("href");
            if (string.IsNullOrWhiteSpace(href))
            {
                continue;
            }

            var number = ExtractNumber(link.TextContent, href, rules);
            if (number is null)
            {
                continue;
            }

            chapters.Add(new ChapterInfo(number.Value, ToAbsoluteUrl(href, pageUrl)));
        }

        return chapters;
    }

    private decimal? ExtractNumber(string? linkText, string href, ChapterExtractionRules rules)
    {
        return rules.NumberSource switch
        {
            ChapterNumberSource.LinkText => Parse(linkText, rules.NumberPattern),
            ChapterNumberSource.Href => Parse(href, rules.NumberPattern),
            _ => Parse(linkText, rules.NumberPattern) ?? Parse(href, rules.NumberPattern)
        };
    }

    private decimal? Parse(string? input, string? pattern)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        string candidate;

        try
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                var match = DefaultNumberRegex().Match(input);
                if (!match.Success)
                {
                    return null;
                }
                candidate = match.Value;
            }
            else
            {
                var regex = new Regex(pattern, RegexOptions.None, RegexTimeout);
                var match = regex.Match(input);
                if (!match.Success)
                {
                    return null;
                }

                candidate = match.Groups.Count > 1 && match.Groups[1].Success
                    ? match.Groups[1].Value
                    : match.Value;
            }
        }
        catch (RegexMatchTimeoutException)
        {
            _logger.LogWarning("Regex timed out on input of length {Length}", input.Length);
            return null;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid regex pattern {Pattern}", pattern);
            return null;
        }

        candidate = candidate.Replace(',', '.');

        return decimal.TryParse(
            candidate, NumberStyles.Number, CultureInfo.InvariantCulture, out var number)
            ? number
            : null;
    }

    private static string ToAbsoluteUrl(string href, string pageUrl)
    {
        return Uri.TryCreate(new Uri(pageUrl), href, out var absolute)
            ? absolute.ToString()
            : href;
    }

    [GeneratedRegex(@"\d+(?:[.,]\d+)?", RegexOptions.None, 200)]
    private static partial Regex DefaultNumberRegex();
}