using MovieCatalog.Domain.Enums;

namespace MovieCatalog.Domain.Entities;

public class Film : BaseEntity
{
    public string Title { get; private set; } = default!;
    public string Synopsis { get; private set; } = default!;
    public Genre Genre { get; private set; } = default!;
    public DateTime ReleaseDate { get; private set; }
    public int DurationMinutes { get; private set; }
    public decimal Rating { get; private set; }
    public bool IsActive { get; private set; }

    public Film() { }

    public Film(string title, string synopsis, Genre genre, DateTime releaseDate, int durationMinutes, decimal rating, bool isActive)
    {
        Title = title;
        Synopsis = synopsis;
        Genre = genre;
        ReleaseDate = releaseDate;
        DurationMinutes = durationMinutes;
        Rating = rating;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string synopsis, Genre genre, DateTime releaseDate, int durationMinutes, decimal rating, bool isActive)
    {
        Title = title;
        Synopsis = synopsis;
        Genre = genre;
        ReleaseDate = releaseDate;
        DurationMinutes = durationMinutes;
        Rating = rating;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
