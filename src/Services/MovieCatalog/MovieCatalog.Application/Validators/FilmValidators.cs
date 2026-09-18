using FluentValidation;
using MovieCatalog.Application.DTOs;

namespace MovieCatalog.Application.Validators;

public class CreateFilmInputValidator : AbstractValidator<CreateFilmInput>
{
    public CreateFilmInputValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("O título do filme é obrigatório.")
            .MaximumLength(200).WithMessage("O título deve ter no máximo 200 caracteres.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("A duração deve ser maior que zero minutos.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(0m, 10m).WithMessage("A nota deve estar entre 0.0 e 10.0.");

        RuleFor(x => x.Genre)
            .IsInEnum().WithMessage("Gênero inválido.");
    }
}

public class UpdateFilmInputValidator : AbstractValidator<UpdateFilmInput>
{
    public UpdateFilmInputValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("O título do filme é obrigatório.")
            .MaximumLength(200).WithMessage("O título deve ter no máximo 200 caracteres.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("A duração deve ser maior que zero minutos.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(0m, 10m).WithMessage("A nota deve estar entre 0.0 e 10.0.");

        RuleFor(x => x.Genre)
            .IsInEnum().WithMessage("Gênero inválido.");
    }
}
