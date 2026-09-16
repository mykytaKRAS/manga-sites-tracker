using MangaTracker.Core.Entities;
using Microsoft.Extensions.Logging;

namespace MangaTracker.Infrastructure.Providers.Html;

public class MangaShiProvider : HtmlMangaProviderBase
{
    public MangaShiProvider(HttpClient httpClient, ILogger<MangaShiProvider> logger)
        : base(httpClient, logger)
    {
    }

    public override MangaSource Source => MangaSource.MangaShi;

    protected override string ChapterLinkSelector => "div#chapters-list a";
}