using MovieCatalog.Domain.Entities;
using MovieCatalog.Domain.Enums;

namespace MovieCatalog.UnitTests.Domain;

public class FilmTests
{
    [Fact]
    public void Constructor_ShouldSetFieldsAndCreatedAt()
    {
        var film = new Film("Batman", null, Genre.Acao, DateTime.UtcNow, 120, 8m, true);
        Assert.Equal("Batman", film.Title);
        Assert.False(film.IsDeleted);
        Assert.Null(film.UpdatedAt);
    }

    [Fact]
    public void Update_ShouldChangeFieldsAndSetUpdatedAt()
    {
        var film = new Film("Batman", null, Genre.Acao, DateTime.UtcNow, 120, 8m, true);
        film.Update("Batman 2", "Sinopse", Genre.Drama, DateTime.UtcNow, 130, 9m, false);
        Assert.Equal("Batman 2", film.Title);
        Assert.False(film.IsActive);
        Assert.NotNull(film.UpdatedAt);
    }

    [Fact]
    public void MarkAsDeleted_ShouldSetIsDeleted()
    {
        var film = new Film("Batman", null, Genre.Acao, DateTime.UtcNow, 120, 8m, true);
        film.MarkAsDeleted();
        Assert.True(film.IsDeleted);
    }
}