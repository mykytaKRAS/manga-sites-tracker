using MangaTracker.Core.Entities;

namespace MangaTracker.Core.Interfaces;

public interface IMangaSourceProviderFactory
{
    IMangaSourceProvider? GetProvider(SourceKind kind);
}