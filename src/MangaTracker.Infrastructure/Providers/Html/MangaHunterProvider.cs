using MangaTracker.Core.Entities;
using Microsoft.Extensions.Logging;

namespace MangaTracker.Infrastructure.Providers.Html;

public class MangaHunterProvider : HtmlMangaProviderBase
{
    public MangaHunterProvider(HttpClient httpClient, ILogger<MangaShiProvider> logger)
        : base(httpClient, logger)
    {
    }

    public override MangaSource Source => MangaSource.ReadHxh;

    protected override string ChapterLinkSelector => "tr td a";
}