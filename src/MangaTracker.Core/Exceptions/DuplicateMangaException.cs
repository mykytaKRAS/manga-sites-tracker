namespace MangaTracker.Core.Exceptions;
public class DuplicateMangaException : Exception
{
    public DuplicateMangaException(string sourceUrl)
        : base($"Manga with URL '{sourceUrl}' is already tracked.")
    {
        SourceUrl = sourceUrl;
    }

    public string SourceUrl { get; }
}

public class InvalidMangaException : Exception
{
    public InvalidMangaException(string message) : base(message)
    {
    }
}