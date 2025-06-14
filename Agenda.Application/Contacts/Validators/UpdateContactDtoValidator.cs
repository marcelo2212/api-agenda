using Agenda.Application.Contacts.Dtos;
using FluentValidation;

namespace Agenda.Application.Contacts.Validators;

public class UpdateContactDtoValidator : AbstractValidator<UpdateContactDto>
{
    public UpdateContactDtoValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Nome é obrigatório");

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");

        RuleFor(c => c.Phone)
            .NotEmpty().WithMessage("Telefone é obrigatório");
    }
}
