namespace MovieCatalog.Application.DTOs;

public record FilmEvent(
    string EventId,
    string FilmId,
    string Title,
    string EventType,
    DateTime Timestamp
);
