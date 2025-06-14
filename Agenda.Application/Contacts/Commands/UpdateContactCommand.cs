using Agenda.Application.Contacts.Dtos;
using MediatR;

namespace Agenda.Application.Contacts.Commands;

public class UpdateContactCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public UpdateContactDto Contact { get; set; }

    public UpdateContactCommand(Guid id, UpdateContactDto contact)
    {
        Id = id;
        Contact = contact;
    }
}
