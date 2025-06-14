using Agenda.Application.Contacts.Commands;
using Agenda.Application.Contacts.Dtos;
using Agenda.Domain.Entities;
using Agenda.Infrastructure.Data;
using MediatR;

namespace Agenda.Infrastructure.Contacts.Handlers;

public class CreateContactHandler : IRequestHandler<CreateContactCommand, Guid>
{
    private readonly AgendaDbContext _db;

    public CreateContactHandler(AgendaDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        var entity = new Contact
        {
            Name = request.Contact.Name,
            Email = request.Contact.Email,
            Phone = request.Contact.Phone
        };

        _db.Contacts.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
