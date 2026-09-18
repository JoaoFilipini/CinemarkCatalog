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
        var sampleGenre = Enum.GetValues(typeof(Genre)).Cast<Genre>().First();
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
        var sampleGenre = Enum.GetValues(typeof(Genre)).Cast<Genre>().First();
        var cachedFilm = new FilmOutput(id, "Inception", "Synopsis", sampleGenre, DateTime.UtcNow, 148, 8.8m, true, DateTime.UtcNow, null);

        _cacheServiceMock.Setup(c => c.GetAsync<FilmOutput>($"film:{id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedFilm);

        var result = await _service.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
