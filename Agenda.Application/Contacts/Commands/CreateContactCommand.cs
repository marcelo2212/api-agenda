using Agenda.Application.Contacts.Dtos;
using MediatR;

namespace Agenda.Application.Contacts.Commands;

public class CreateContactCommand : IRequest<Guid>
{
    public CreateContactDto Contact { get; }

    public CreateContactCommand(CreateContactDto contact)
    {
        Contact = contact;
    }
}
