using MangaTracker.Core.Entities;
using MangaTracker.Core.Interfaces;

namespace MangaTracker.Infrastructure.Providers;

public class MangaSourceProviderFactory : IMangaSourceProviderFactory
{
    private readonly IReadOnlyDictionary<MangaSource, IMangaSourceProvider> _providers;

    public MangaSourceProviderFactory(IEnumerable<IMangaSourceProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.Source);
    }

    public IMangaSourceProvider? GetProvider(MangaSource source)
    {
        return _providers.TryGetValue(source, out var provider) ? provider : null;
    }
}