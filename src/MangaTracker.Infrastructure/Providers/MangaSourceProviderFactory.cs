using MangaTracker.Core.Entities;
using MangaTracker.Core.Interfaces;

namespace MangaTracker.Infrastructure.Providers;

public class MangaSourceProviderFactory : IMangaSourceProviderFactory
{
    private readonly IReadOnlyDictionary<SourceKind, IMangaSourceProvider> _providers;

    public MangaSourceProviderFactory(IEnumerable<IMangaSourceProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.Kind);
    }

    public IMangaSourceProvider? GetProvider(SourceKind kind)
    {
        return _providers.TryGetValue(kind, out var provider) ? provider : null;
    }
}