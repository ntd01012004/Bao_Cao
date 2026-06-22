using System;
using System.Collections.Generic;

namespace LibraryAPI.Models;

public partial class Book
{
    public int BookId { get; set; }

    public string Title { get; set; } = null!;

    public string? Author { get; set; }

    public decimal? Price { get; set; }

    public int? PublishYear { get; set; }

    public int? CategoryId { get; set; }

    public virtual Category? Category { get; set; }
}
