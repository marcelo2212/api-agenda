using Agenda.Application.Contacts.Dtos;
using MediatR;
using System.Collections.Generic;

namespace Agenda.Application.Contacts.Queries
{
    public class GetAllContactsQuery : IRequest<List<ContactDto>>
    {
    }
}
