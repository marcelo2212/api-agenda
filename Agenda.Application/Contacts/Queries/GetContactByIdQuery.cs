using Agenda.Application.Contacts.Dtos;
using MediatR;
using System;

namespace Agenda.Application.Contacts.Queries
{
    public class GetContactByIdQuery : IRequest<ContactDto>
    {
        public Guid Id { get; }

        public GetContactByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
