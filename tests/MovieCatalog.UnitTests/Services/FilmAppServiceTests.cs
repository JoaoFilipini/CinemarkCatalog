using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;
using Moq;
using MovieCatalog.Application.Configurations;
using MovieCatalog.Application.DTOs;
using MovieCatalog.Application.Interfaces;
using MovieCatalog.Application.Services;
using MovieCatalog.Domain.Entities;
using MovieCatalog.Domain.Enums;
using MovieCatalog.Domain.Interfaces;
using System.Linq.Expressions;
using MovieCatalog.Domain.Exceptions;
using Xunit;

namespace MovieCatalog.UnitTests.Services;

public class FilmAppServiceTests
{
    private readonly Mock<IFilmRepository> _repositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IEventProducer> _eventProducerMock;
    private readonly Mock<IValidator<CreateFilmInput>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateFilmInput>> _updateValidatorMock;
    private readonly IOptions<CacheSettings> _cacheSettingsOptions;
    private readonly FilmAppService _service;
    private static Film NewFilm(string id = "abc") =>
        new("Inception", null, Genre.Acao, DateTime.UtcNow, 148, 8.8m, true) { Id = id };
    public FilmAppServiceTests()
    {
        _repositoryMock = new Mock<IFilmRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _eventProducerMock = new Mock<IEventProducer>();
        _createValidatorMock = new Mock<IValidator<CreateFilmInput>>();
        _updateValidatorMock = new Mock<IValidator<UpdateFilmInput>>();
        _cacheSettingsOptions = Options.Create(new CacheSettings { DefaultTtlMinutes = 5 });

        _createValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateFilmInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _updateValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateFilmInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _service = new FilmAppService(
            _repositoryMock.Object,
            _cacheServiceMock.Object,
            _eventProducerMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            _cacheSettingsOptions
        );
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ShouldCreateAndReturnOutput()
    {
        var sampleGenre = Enum.GetValues<Genre>().Cast<Genre>().First();
        var input = new CreateFilmInput("Inception", "Synopsis", sampleGenre, DateTime.UtcNow, 148, 8.8m, true);
        _repositoryMock.Setup(r => r.ExistsByTitleAsync(input.Title, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CreateAsync(input);

        Assert.NotNull(result);
        Assert.Equal(input.Title, result.Title);
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Film>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<FilmOutput>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventProducerMock.Verify(e => e.PublishAsync("FilmCreated", It.IsAny<FilmEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenInCache_ShouldReturnCachedValue()
    {
        var id = "film-123";
        var sampleGenre = Enum.GetValues<Genre>().Cast<Genre>().First();
        var cachedFilm = new FilmOutput(id, "Inception", "Synopsis", sampleGenre, DateTime.UtcNow, 148, 8.8m, true, DateTime.UtcNow, null);

        _cacheServiceMock.Setup(c => c.GetAsync<FilmOutput>($"films:{id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedFilm);

        var result = await _service.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DuplicateTitle_ShouldThrowBusinessRuleException()
    {
        var input = new CreateFilmInput("Inception", null, Genre.Acao, DateTime.UtcNow, 148, 8.8m);
        _repositoryMock.Setup(r => r.ExistsByTitleAsync(input.Title, null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateAsync(input));
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Film>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_InvalidInput_ShouldThrowValidationException()
    {
        var input = new CreateFilmInput("", null, Genre.Acao, DateTime.UtcNow, 148, 8.8m);
        _createValidatorMock.Setup(v => v.ValidateAsync(input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Title", "obrigatório") }));

        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(input));
    }

    [Fact]
    public async Task GetByIdAsync_CacheMiss_ShouldLoadFromRepositoryAndCache()
    {
        _cacheServiceMock.Setup(c => c.GetAsync<FilmOutput>("films:abc", It.IsAny<CancellationToken>())).ReturnsAsync((FilmOutput?)null);
        _repositoryMock.Setup(r => r.GetByIdAsync("abc", It.IsAny<CancellationToken>())).ReturnsAsync(NewFilm());

        var result = await _service.GetByIdAsync("abc");

        Assert.Equal("Inception", result.Title);
        _cacheServiceMock.Verify(c => c.SetAsync("films:abc", It.IsAny<FilmOutput>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ShouldThrowKeyNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync("abc", It.IsAny<CancellationToken>())).ReturnsAsync((Film?)null);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByIdAsync("abc"));
    }

    [Fact]
    public async Task GetByIdAsync_DeletedFilm_ShouldThrowKeyNotFound()
    {
        var film = NewFilm();
        film.MarkAsDeleted();
        _repositoryMock.Setup(r => r.GetByIdAsync("abc", It.IsAny<CancellationToken>())).ReturnsAsync(film);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByIdAsync("abc"));
    }

    [Fact]
    public async Task GetPagedAsync_InvalidPaging_ShouldNormalizePageAndSize()
    {
        _repositoryMock.Setup(r => r.GetPagedAsync(It.IsAny<Expression<Func<Film, bool>>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IEnumerable<Film>)new List<Film>(), 0L));

        await _service.GetPagedAsync(null, null, 0, 500);

        _repositoryMock.Verify(r => r.GetPagedAsync(It.IsAny<Expression<Func<Film, bool>>>(), 1, 100, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Valid_ShouldUpdateInvalidateCacheAndPublish()
    {
        var input = new UpdateFilmInput("Inception 2", null, Genre.Acao, DateTime.UtcNow, 150, 9.0m, true);
        _repositoryMock.Setup(r => r.GetByIdAsync("abc", It.IsAny<CancellationToken>())).ReturnsAsync(NewFilm());
        _repositoryMock.Setup(r => r.ExistsByTitleAsync(input.Title, "abc", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _service.UpdateAsync("abc", input);

        Assert.Equal("Inception 2", result.Title);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Film>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync("films:list:", It.IsAny<CancellationToken>()), Times.Once);
        _eventProducerMock.Verify(e => e.PublishAsync("FilmUpdated", It.IsAny<FilmEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateTitle_ShouldThrowBusinessRuleException()
    {
        var input = new UpdateFilmInput("Outro", null, Genre.Acao, DateTime.UtcNow, 150, 9.0m, true);
        _repositoryMock.Setup(r => r.GetByIdAsync("abc", It.IsAny<CancellationToken>())).ReturnsAsync(NewFilm());
        _repositoryMock.Setup(r => r.ExistsByTitleAsync(input.Title, "abc", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.UpdateAsync("abc", input));
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ShouldThrowKeyNotFound()
    {
        var input = new UpdateFilmInput("X", null, Genre.Acao, DateTime.UtcNow, 150, 9.0m, true);
        _repositoryMock.Setup(r => r.GetByIdAsync("abc", It.IsAny<CancellationToken>())).ReturnsAsync((Film?)null);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync("abc", input));
    }

    [Fact]
    public async Task DeleteAsync_Existing_ShouldSoftDeleteAndPublish()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync("abc", It.IsAny<CancellationToken>())).ReturnsAsync(NewFilm());

        await _service.DeleteAsync("abc");

        _repositoryMock.Verify(r => r.SoftDeleteAsync("abc", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync("films:abc", It.IsAny<CancellationToken>()), Times.Once);
        _eventProducerMock.Verify(e => e.PublishAsync("FilmDeleted", It.IsAny<FilmEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ShouldThrowKeyNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync("abc", It.IsAny<CancellationToken>())).ReturnsAsync((Film?)null);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync("abc"));
        _eventProducerMock.Verify(e => e.PublishAsync(It.IsAny<string>(), It.IsAny<FilmEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
