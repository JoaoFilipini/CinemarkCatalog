using MovieCatalog.Domain.Enums;

namespace MovieCatalog.Application.DTOs;

public record FilmOutput(
    string Id,
    string Title,
    string Synopsis,
    Genre Genre,
    DateTime ReleaseDate,
    int DurationMinutes,
    decimal Rating,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
