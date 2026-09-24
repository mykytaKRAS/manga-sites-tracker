namespace MangaTracker.Core.Exceptions;

public class DuplicateSourceException : Exception
{
    public DuplicateSourceException(string name)
        : base($"Source '{name}' already exists.")
    {
        Name = name;
    }

    public string Name { get; }
}

public class InvalidSourceException : Exception
{
    public InvalidSourceException(string message) : base(message)
    {
    }
}
public class SourceInUseException : Exception
{
    public SourceInUseException(string name)
        : base($"Source '{name}' still has tracked manga and cannot be deleted.")
    {
        Name = name;
    }

    public string Name { get; }
}