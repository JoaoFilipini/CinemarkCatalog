using MovieCatalog.Domain.Enums;

namespace MovieCatalog.Application.DTOs;

public interface IFilmInput
{
    string Title { get; }
    string? Synopsis { get; }
    Genre Genre { get; }
    DateTime ReleaseDate { get; }
    int DurationMinutes { get; }
    decimal Rating { get; }
}