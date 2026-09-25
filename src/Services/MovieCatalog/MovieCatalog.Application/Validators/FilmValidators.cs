using FluentValidation;
using MovieCatalog.Application.DTOs;

namespace MovieCatalog.Application.Validators;

public abstract class FilmInputValidator<T> : AbstractValidator<T> where T : IFilmInput
{
    protected FilmInputValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("O título do filme é obrigatório.")
            .MaximumLength(200).WithMessage("O título deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Synopsis)
            .MaximumLength(2000).WithMessage("A sinopse deve ter no máximo 2000 caracteres.");

        RuleFor(x => x.Genre)
            .IsInEnum().WithMessage("Gênero inválido.");

        RuleFor(x => x.ReleaseDate)
            .NotEqual(default(DateTime)).WithMessage("A data de lançamento é obrigatória.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("A duração deve ser maior que zero minutos.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(0m, 10m).WithMessage("A nota deve estar entre 0.0 e 10.0.")
            .PrecisionScale(3, 1, false).WithMessage("A nota deve ter no máximo uma casa decimal.");
    }
}

public class CreateFilmInputValidator : FilmInputValidator<CreateFilmInput> { }
public class UpdateFilmInputValidator : FilmInputValidator<UpdateFilmInput> { }