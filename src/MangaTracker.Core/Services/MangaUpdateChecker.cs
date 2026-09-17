using MangaTracker.Core.Entities;
using MangaTracker.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace MangaTracker.Core.Services;

public class MangaUpdateChecker
{
    private readonly IMangaRepository _repository;
    private readonly IMangaSourceProviderFactory _providerFactory;
    private readonly ILogger<MangaUpdateChecker> _logger;

    public MangaUpdateChecker(
        IMangaRepository repository,
        IMangaSourceProviderFactory providerFactory,
        ILogger<MangaUpdateChecker> logger)
    {
        _repository = repository;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    public async Task<CheckResult?> CheckOneAsync(int mangaId, CancellationToken ct = default)
    {
        var manga = await _repository.GetByIdAsync(mangaId, ct);
        if (manga is null)
        {
            return null;
        }

        var result = await CheckAsync(manga, ct);
        await _repository.SaveChangesAsync(ct);

        return result;
    }

    public async Task<IReadOnlyList<CheckResult>> CheckAllAsync(CancellationToken ct = default)
    {
        var mangas = await _repository.GetAllForUpdateAsync(ct);
        var results = new List<CheckResult>(mangas.Count);

        foreach (var manga in mangas)
        {
            ct.ThrowIfCancellationRequested();

            var result = await CheckAsync(manga, ct);
            results.Add(result);
        }

        await _repository.SaveChangesAsync(ct);

        return results;
    }

    private async Task<CheckResult> CheckAsync(Manga manga, CancellationToken ct)
    {
        var provider = _providerFactory.GetProvider(manga.Source);
        if (provider is null)
        {
            _logger.LogWarning("No provider registered for source {Source}", manga.Source);
            return new CheckResult(manga.Id, manga.Title, CheckStatus.NoProvider, null, null);
        }

        var latest = await provider.GetLatestChapterAsync(manga.SourceUrl, ct);

        manga.LastCheckedAt = DateTime.UtcNow;

        if (latest is null)
        {
            _logger.LogWarning("Check failed for {Title} ({Source})", manga.Title, manga.Source);
            return new CheckResult(manga.Id, manga.Title, CheckStatus.Failed, null, null);
        }

        if (manga.LastKnownChapter is null)
        {
            manga.LastKnownChapter = latest.Number;
            manga.LastChapterUrl = latest.Url;

            _logger.LogInformation(
                "Initialized {Title} at chapter {Chapter}",
                manga.Title,
                latest.Number);

            return new CheckResult(
                manga.Id, manga.Title, CheckStatus.Initialized, latest.Number, latest.Url);
        }

        if (latest.Number > manga.LastKnownChapter)
        {
            _logger.LogInformation(
                "New chapter for {Title}: {Old} -> {New}",
                manga.Title,
                manga.LastKnownChapter,
                latest.Number);

            manga.LastKnownChapter = latest.Number;
            manga.LastChapterUrl = latest.Url;
            manga.HasUnreadChapter = true;

            return new CheckResult(
                manga.Id, manga.Title, CheckStatus.NewChapterFound, latest.Number, latest.Url);
        }

        return new CheckResult(
            manga.Id, manga.Title, CheckStatus.NoNewChapter, manga.LastKnownChapter, manga.LastChapterUrl);
    }
}