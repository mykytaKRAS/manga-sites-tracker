using System.Text.RegularExpressions;
using MangaTracker.Core.Entities;
using MangaTracker.Core.Exceptions;
using MangaTracker.Core.Interfaces;

namespace MangaTracker.Core.Services;

public class SourceService
{
    private static readonly TimeSpan RegexValidationTimeout = TimeSpan.FromMilliseconds(200);

    private readonly ISourceRepository _repository;

    public SourceService(ISourceRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<MangaSource>> GetAllAsync(CancellationToken ct = default)
        => _repository.GetAllAsync(ct);

    public Task<MangaSource?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repository.GetByIdAsync(id, ct);

    public async Task<MangaSource> AddAsync(
        string name,
        SourceKind kind,
        string? chapterLinkSelector,
        string? chapterNumberPattern,
        ChapterNumberSource numberSource,
        CancellationToken ct = default)
    {
        var normalizedName = name.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new InvalidSourceException("Name is required.");
        }

        if (await _repository.ExistsByNameAsync(normalizedName, ct))
        {
            throw new DuplicateSourceException(normalizedName);
        }

        var selector = Normalize(chapterLinkSelector);
        var pattern = Normalize(chapterNumberPattern);

        ValidateRules(kind, selector, pattern);

        var source = new MangaSource
        {
            Name = normalizedName,
            Kind = kind,
            ChapterLinkSelector = selector,
            ChapterNumberPattern = pattern,
            NumberSource = numberSource,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(source, ct);
        await _repository.SaveChangesAsync(ct);

        return source;
    }

    public async Task<MangaSource?> UpdateAsync(
        int id,
        string? chapterLinkSelector,
        string? chapterNumberPattern,
        ChapterNumberSource numberSource,
        CancellationToken ct = default)
    {
        var source = await _repository.GetByIdAsync(id, ct);
        if (source is null)
        {
            return null;
        }

        var selector = Normalize(chapterLinkSelector);
        var pattern = Normalize(chapterNumberPattern);

        ValidateRules(source.Kind, selector, pattern);

        source.ChapterLinkSelector = selector;
        source.ChapterNumberPattern = pattern;
        source.NumberSource = numberSource;

        await _repository.SaveChangesAsync(ct);

        return source;
    }

    public async Task<bool> SetEnabledAsync(
        int id,
        bool isEnabled,
        string? disabledReason,
        CancellationToken ct = default)
    {
        var source = await _repository.GetByIdAsync(id, ct);
        if (source is null)
        {
            return false;
        }

        source.IsEnabled = isEnabled;
        source.DisabledReason = isEnabled ? null : Normalize(disabledReason);

        await _repository.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var source = await _repository.GetByIdAsync(id, ct);
        if (source is null)
        {
            return false;
        }

        if (await _repository.HasMangasAsync(id, ct))
        {
            throw new SourceInUseException(source.Name);
        }

        _repository.Remove(source);
        await _repository.SaveChangesAsync(ct);

        return true;
    }

    private static void ValidateRules(SourceKind kind, string? selector, string? pattern)
    {
        if (kind == SourceKind.Html && string.IsNullOrWhiteSpace(selector))
        {
            throw new InvalidSourceException("HTML source requires a chapter link selector.");
        }

        if (pattern is null)
        {
            return;
        }

        try
        {
            _ = new Regex(pattern, RegexOptions.None, RegexValidationTimeout);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidSourceException($"Invalid regular expression: {ex.Message}");
        }
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}