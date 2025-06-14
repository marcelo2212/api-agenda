using Agenda.Application.Contacts.Queries;
using Agenda.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Agenda.Application.Contacts.Handlers
{
    public class GetContactByIdHandler : IRequestHandler<GetContactByIdQuery, ContactDto?>
    {
        private readonly AgendaDbContext _db;

        public GetContactByIdHandler(AgendaDbContext db)
        {
            _db = db;
        }

        public async Task<ContactDto?> Handle(GetContactByIdQuery request, CancellationToken cancellationToken)
        {
            var contact = await _db.Contacts
                .AsNoTracking()
                .Where(c => c.Id == request.Id)
                .Select(c => new ContactDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Phone = c.Phone,
                    CreatedAt = c.CreatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            return contact;
        }
    }
}
