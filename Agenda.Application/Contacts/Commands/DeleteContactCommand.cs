using MediatR;

namespace Agenda.Application.Contacts.Commands;

public class DeleteContactCommand : IRequest<bool>
{
    public Guid Id { get; }

    public DeleteContactCommand(Guid id)
    {
        Id = id;
    }
}
