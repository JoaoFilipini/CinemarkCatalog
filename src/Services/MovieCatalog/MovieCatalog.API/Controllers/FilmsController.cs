using Microsoft.AspNetCore.Mvc;
using MovieCatalog.Application.DTOs;
using MovieCatalog.Application.Services;
using MovieCatalog.Domain.Enums;

namespace MovieCatalog.API.Controllers;

[ApiController]
[Route("api/v1/films")]
public class FilmsController : ControllerBase
{
    private readonly IFilmAppService _filmAppService;

    public FilmsController(IFilmAppService filmAppService)
    {
        _filmAppService = filmAppService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(FilmOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateFilmInput input, CancellationToken cancellationToken)
    {
        var result = await _filmAppService.CreateAsync(input, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FilmOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _filmAppService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FilmOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] Genre? genre,
        [FromQuery] bool? active,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _filmAppService.GetPagedAsync(genre, active, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(FilmOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateFilmInput input, CancellationToken cancellationToken)
    {
        var result = await _filmAppService.UpdateAsync(id, input, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _filmAppService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
