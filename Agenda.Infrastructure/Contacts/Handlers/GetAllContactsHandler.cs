using Agenda.Application.Contacts.Queries;
using Agenda.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Agenda.Application.Contacts.Handlers
{
    public class GetAllContactsHandler : IRequestHandler<GetAllContactsQuery, List<ContactDto>>
    {
        private readonly AgendaDbContext _db;

        public GetAllContactsHandler(AgendaDbContext db)
        {
            _db = db;
        }

        public async Task<List<ContactDto>> Handle(
            GetAllContactsQuery request,
            CancellationToken cancellationToken
        )
        {
            return await _db
                .Contacts.AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ContactDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Phone = c.Phone,
                    CreatedAt = c.CreatedAt,
                })
                .ToListAsync(cancellationToken);
        }
    }
}
