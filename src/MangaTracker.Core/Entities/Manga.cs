using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaTracker.Core.Entities;

public class Manga
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int SourceId { get; set; }
    public MangaSource Source { get; set; } = null!;
    public string SourceUrl { get; set; } = null!;
    public decimal? LastKnownChapter { get; set; }
    public string? LastChapterUrl { get; set; }
    public DateTime? LastCheckedAt { get; set; }
    public bool HasUnreadChapter { get; set; }
    public DateTime CreatedAt { get; set; }
}
