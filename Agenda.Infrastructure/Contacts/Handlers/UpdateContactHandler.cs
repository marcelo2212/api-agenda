using Agenda.Application.Contacts.Commands;
using Agenda.Infrastructure.Data;
using MediatR;

namespace Agenda.Infrastructure.Contacts.Handlers;

public class UpdateContactHandler : IRequestHandler<UpdateContactCommand, bool>
{
    private readonly AgendaDbContext _db;

    public UpdateContactHandler(AgendaDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(
        UpdateContactCommand request,
        CancellationToken cancellationToken
    )
    {
        var contact = await _db.Contacts.FindAsync(new object[] { request.Id }, cancellationToken);

        if (contact == null)
            return false;

        contact.Name = request.Contact.Name;
        contact.Email = request.Contact.Email;
        contact.Phone = request.Contact.Phone;

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
