using MovieCatalog.Domain.Entities;

namespace MovieCatalog.Domain.Interfaces;

public interface IFilmRepository : IRepository<Film>
{
    Task<bool> ExistsByTitleAsync(string title, string? ignoreId = null, CancellationToken cancellationToken = default);
}
