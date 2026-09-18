using FluentValidation.TestHelper;
using MovieCatalog.Application.DTOs;
using MovieCatalog.Application.Validators;
using MovieCatalog.Domain.Enums;
using Xunit;

namespace MovieCatalog.UnitTests.Validators;

public class FilmValidatorTests
{
    private readonly CreateFilmInputValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Title_Is_Empty()
    {
        var model = new CreateFilmInput("", "Sinopse", Genre.Acao, DateTime.UtcNow, 120, 8.0m, true);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Have_Error_When_Duration_Is_Zero_Or_Negative()
    {
        var model = new CreateFilmInput("Batman", "Sinopse", Genre.Acao, DateTime.UtcNow, 0, 8.0m, true);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void Should_Have_Error_When_Rating_Is_Out_Of_Range()
    {
        var model = new CreateFilmInput("Batman", "Sinopse", Genre.Acao, DateTime.UtcNow, 120, 11.0m, true);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Input_Is_Valid()
    {
        var model = new CreateFilmInput("Batman", "Sinopse", Genre.Acao, DateTime.UtcNow, 120, 9.5m, true);
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
