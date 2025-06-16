using Agenda.Application.Contacts.Dtos;
using FluentValidation;

namespace Agenda.Application.Contacts.Validators;

public class CreateContactDtoValidator : AbstractValidator<CreateContactDto>
{
    public CreateContactDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome é obrigatório.")
            .MinimumLength(3)
            .WithMessage("Nome deve ter pelo menos 3 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email é obrigatório.")
            .EmailAddress()
            .WithMessage("Email inválido.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Telefone é obrigatório.")
            .MinimumLength(8)
            .WithMessage("Telefone inválido.");
    }
}
