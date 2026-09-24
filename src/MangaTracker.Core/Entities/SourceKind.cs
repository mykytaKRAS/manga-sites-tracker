namespace MangaTracker.Core.Entities;

public enum SourceKind
{
    Html = 1,
    MangaLibApi = 2
}

public enum ChapterNumberSource
{
    // Из текста ссылки, при неудаче - из адреса
    LinkTextThenHref = 1,

    // Только из текста ссылки
    LinkText = 2,

    // Только из адреса
    Href = 3
}