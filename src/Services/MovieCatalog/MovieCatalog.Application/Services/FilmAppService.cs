using FluentValidation;
using Microsoft.Extensions.Options;
using MovieCatalog.Application.Configurations;
using MovieCatalog.Application.DTOs;
using MovieCatalog.Application.Interfaces;
using MovieCatalog.Domain.Entities;
using MovieCatalog.Domain.Enums;
using MovieCatalog.Domain.Exceptions;
using MovieCatalog.Domain.Interfaces;

namespace MovieCatalog.Application.Services;

public class FilmAppService : IFilmAppService
{
    private const string ListPrefix = "films:list:";
    private static string FilmKey(string id) => $"films:{id}";

    private readonly IFilmRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly IEventProducer _eventProducer;
    private readonly IValidator<CreateFilmInput> _createValidator;
    private readonly IValidator<UpdateFilmInput> _updateValidator;
    private readonly TimeSpan _cacheTtl;

    public FilmAppService(
        IFilmRepository repository,
        ICacheService cacheService,
        IEventProducer eventProducer,
        IValidator<CreateFilmInput> createValidator,
        IValidator<UpdateFilmInput> updateValidator,
        IOptions<CacheSettings> cacheSettings)
    {
        _repository = repository;
        _cacheService = cacheService;
        _eventProducer = eventProducer;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _cacheTtl = TimeSpan.FromMinutes(cacheSettings.Value.DefaultTtlMinutes <= 0 ? 5 : cacheSettings.Value.DefaultTtlMinutes);
    }

    public async Task<FilmOutput> CreateAsync(CreateFilmInput input, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _repository.ExistsByTitleAsync(input.Title, null, cancellationToken))
            throw new BusinessRuleException($"Já existe um filme cadastrado com o título '{input.Title}'.");

        var film = new Film(input.Title, input.Synopsis, input.Genre, input.ReleaseDate, input.DurationMinutes, input.Rating, input.IsActive);
        await _repository.CreateAsync(film, cancellationToken);

        var output = MapToOutput(film);
        await _cacheService.SetAsync(FilmKey(film.Id), output, _cacheTtl, cancellationToken);
        await _cacheService.RemoveByPrefixAsync(ListPrefix, cancellationToken);

        await PublishAsync("FilmCreated", film, cancellationToken);
        return output;
    }

    public async Task<FilmOutput> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var cacheKey = FilmKey(id);
        var cached = await _cacheService.GetAsync<FilmOutput>(cacheKey, cancellationToken);
        if (cached != null) return cached;

        var film = await GetActiveFilmAsync(id, cancellationToken);

        var output = MapToOutput(film);
        await _cacheService.SetAsync(cacheKey, output, _cacheTtl, cancellationToken);
        return output;
    }

    public async Task<PagedResult<FilmOutput>> GetPagedAsync(Genre? genre, bool? isActive, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var cacheKey = $"{ListPrefix}genre={genre}:active={isActive}:page={page}:size={pageSize}";
        var cachedResult = await _cacheService.GetAsync<PagedResult<FilmOutput>>(cacheKey, cancellationToken);
        if (cachedResult != null) return cachedResult;

        var (items, total) = await _repository.GetPagedAsync(
            x => !x.IsDeleted &&
                 (!genre.HasValue || x.Genre == genre.Value) &&
                 (!isActive.HasValue || x.IsActive == isActive.Value),
            page, pageSize, cancellationToken);

        var result = new PagedResult<FilmOutput>(items.Select(MapToOutput).ToList(), total, page, pageSize);

        await _cacheService.SetAsync(cacheKey, result, _cacheTtl, cancellationToken);
        return result;
    }

    public async Task<FilmOutput> UpdateAsync(string id, UpdateFilmInput input, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var film = await GetActiveFilmAsync(id, cancellationToken);

        if (await _repository.ExistsByTitleAsync(input.Title, id, cancellationToken))
            throw new BusinessRuleException($"Já existe outro filme cadastrado com o título '{input.Title}'.");

        film.Update(input.Title, input.Synopsis, input.Genre, input.ReleaseDate, input.DurationMinutes, input.Rating, input.IsActive);
        await _repository.UpdateAsync(film, cancellationToken);

        var output = MapToOutput(film);
        await _cacheService.SetAsync(FilmKey(id), output, _cacheTtl, cancellationToken);
        await _cacheService.RemoveByPrefixAsync(ListPrefix, cancellationToken);

        await PublishAsync("FilmUpdated", film, cancellationToken);
        return output;
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var film = await GetActiveFilmAsync(id, cancellationToken);

        await _repository.SoftDeleteAsync(id, cancellationToken);
        await _cacheService.RemoveAsync(FilmKey(id), cancellationToken);
        await _cacheService.RemoveByPrefixAsync(ListPrefix, cancellationToken);

        await PublishAsync("FilmDeleted", film, cancellationToken);
    }

    private async Task<Film> GetActiveFilmAsync(string id, CancellationToken cancellationToken)
    {
        var film = await _repository.GetByIdAsync(id, cancellationToken);
        if (film is null || film.IsDeleted)
            throw new KeyNotFoundException($"Filme com ID '{id}' não encontrado.");
        return film;
    }

    private Task PublishAsync(string eventName, Film film, CancellationToken cancellationToken) =>
        _eventProducer.PublishAsync(
            eventName,
            new FilmEvent(Guid.NewGuid().ToString(), film.Id, film.Title, eventName, DateTime.UtcNow),
            cancellationToken);

    private static FilmOutput MapToOutput(Film f)
    {
        return new(f.Id, f.Title, f.Synopsis ?? string.Empty, f.Genre, f.ReleaseDate, f.DurationMinutes, f.Rating, f.IsActive, f.CreatedAt, f.UpdatedAt);
    }

}