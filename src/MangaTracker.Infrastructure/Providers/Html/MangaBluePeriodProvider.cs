using MangaTracker.Core.Entities;
using Microsoft.Extensions.Logging;

namespace MangaTracker.Infrastructure.Providers.Html;

public class MangaBluePeriodProvider : HtmlMangaProviderBase
{
    public MangaBluePeriodProvider(HttpClient httpClient, ILogger<MangaShiProvider> logger)
        : base(httpClient, logger)
    {
    }

    public override MangaSource Source => MangaSource.BluePeriodChapters;

    protected override string ChapterLinkSelector => "div#Chapters_List ul li a";
}