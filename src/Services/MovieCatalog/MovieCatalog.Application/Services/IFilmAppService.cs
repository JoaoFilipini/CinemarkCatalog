using MovieCatalog.Application.DTOs;
using MovieCatalog.Domain.Enums;

namespace MovieCatalog.Application.Services;

public interface IFilmAppService
{
    Task<FilmOutput> CreateAsync(CreateFilmInput input, CancellationToken cancellationToken = default);
    Task<FilmOutput?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<PagedResult<FilmOutput>> GetPagedAsync(Genre? genre, bool? active, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<FilmOutput> UpdateAsync(string id, UpdateFilmInput input, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
