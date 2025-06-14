using MediatR;

namespace Agenda.Application.Contacts.Queries
{
    public class GetAllContactsQuery : IRequest<List<ContactDto>>
    {
    }
}
