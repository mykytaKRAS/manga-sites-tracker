using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaTracker.Core.Entities
{
    public class MangaSource
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public SourceKind Kind { get; set; }
        public string? ChapterLinkSelector { get; set; }
        public string? ChapterNumberPattern { get; set; }
        public ChapterNumberSource NumberSource { get; set; } = ChapterNumberSource.LinkTextThenHref;
        public bool IsEnabled { get; set; } = true;
        public string? DisabledReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<Manga> Mangas { get; set; } = new List<Manga>();
    }
}
