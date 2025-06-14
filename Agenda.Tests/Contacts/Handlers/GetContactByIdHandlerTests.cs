using Agenda.Application.Contacts.Handlers;
using Agenda.Application.Contacts.Queries;
using Agenda.Domain.Entities;
using Agenda.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agenda.Tests.Contacts.Handlers
{
    public class GetContactByIdHandlerTests
    {
        private AgendaDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AgendaDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AgendaDbContext(options);
        }

        [Fact]
        public async Task Handle_ExistingContactId_ShouldReturnContactDto()
        {
            var context = GetInMemoryDbContext();

            var contact = new Contact
            {
                Name = "Maria",
                Email = "maria@email.com",
                Phone = "11988887777"
            };

            context.Contacts.Add(contact);
            await context.SaveChangesAsync();

            var handler = new GetContactByIdHandler(context);
            var query = new GetContactByIdQuery(contact.Id);
            var result = await handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.IsType<ContactDto>(result);
            Assert.Equal(contact.Name, result!.Name);
            Assert.Equal(contact.Email, result.Email);
            Assert.Equal(contact.Phone, result.Phone);
        }

        [Fact]
        public async Task Handle_NonexistentContactId_ShouldReturnNull()
        {
            var context = GetInMemoryDbContext();
            var handler = new GetContactByIdHandler(context);
            var query = new GetContactByIdQuery(Guid.NewGuid());

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Null(result);
        }
    }
}
