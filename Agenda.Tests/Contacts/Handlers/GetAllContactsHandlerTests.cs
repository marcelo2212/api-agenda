using Agenda.Application.Contacts.Handlers;
using Agenda.Application.Contacts.Queries;
using Agenda.Domain.Entities;
using Agenda.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agenda.Tests.Contacts.Handlers
{
    public class GetAllContactsHandlerTests
    {
        private AgendaDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AgendaDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AgendaDbContext(options);
        }

        [Fact]
        public async Task Handle_ShouldReturnAllContacts()
        {
            var context = GetInMemoryDbContext();

            var contacts = new List<Contact>
            {
                new()
                {
                    Name = "Ana",
                    Email = "ana@email.com",
                    Phone = "11999990001",
                },
                new()
                {
                    Name = "Bruno",
                    Email = "bruno@email.com",
                    Phone = "11999990002",
                },
                new()
                {
                    Name = "Carlos",
                    Email = "carlos@email.com",
                    Phone = "11999990003",
                },
            };

            context.Contacts.AddRange(contacts);
            await context.SaveChangesAsync();

            var handler = new GetAllContactsHandler(context);
            var query = new GetAllContactsQuery();

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(3, result.Count());

            var first = result.First();
            Assert.IsType<ContactDto>(first);
            Assert.Contains(result, c => c.Name == "Ana");
            Assert.Contains(result, c => c.Email == "bruno@email.com");
        }

        [Fact]
        public async Task Handle_EmptyDatabase_ShouldReturnEmptyList()
        {
            var context = GetInMemoryDbContext();
            var handler = new GetAllContactsHandler(context);
            var query = new GetAllContactsQuery();

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
