using MangaTracker.Core.Entities;
using Microsoft.Extensions.Logging;

namespace MangaTracker.Infrastructure.Providers.Html;

public class MangaRecordOfRagnarokProvider : HtmlMangaProviderBase
{
    public MangaRecordOfRagnarokProvider(HttpClient httpClient, ILogger<MangaShiProvider> logger)
        : base(httpClient, logger)
    {
    }

    public override MangaSource Source => MangaSource.RagnarokManga;

    protected override string ChapterLinkSelector => "div.su-expand-content.su-u-trim li a";
}