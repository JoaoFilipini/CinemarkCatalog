using MovieCatalog.Domain.Enums;

namespace MovieCatalog.Application.DTOs;

public record CreateFilmInput(
    string Title,
    string Synopsis,
    Genre Genre,
    DateTime ReleaseDate,
    int DurationMinutes,
    decimal Rating,
    bool IsActive = true
);
