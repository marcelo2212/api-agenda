using Agenda.Application.Contacts.Commands;
using Agenda.Infrastructure.Data;
using MediatR;

namespace Agenda.Infrastructure.Contacts.Handlers;

public class DeleteContactHandler : IRequestHandler<DeleteContactCommand, bool>
{
    private readonly AgendaDbContext _db;

    public DeleteContactHandler(AgendaDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(
        DeleteContactCommand request,
        CancellationToken cancellationToken
    )
    {
        var contact = await _db.Contacts.FindAsync(new object[] { request.Id }, cancellationToken);

        if (contact == null)
            return false;

        _db.Contacts.Remove(contact);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
