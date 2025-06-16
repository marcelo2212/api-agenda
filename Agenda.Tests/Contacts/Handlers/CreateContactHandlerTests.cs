using Agenda.Application.Contacts.Commands;
using Agenda.Application.Contacts.Dtos;
using Agenda.Infrastructure.Contacts.Handlers;
using Agenda.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agenda.Tests.Contacts.Handlers
{
    public class CreateContactHandlerTests
    {
        private AgendaDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AgendaDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AgendaDbContext(options);
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldCreateContact()
        {
            var context = GetInMemoryDbContext();
            var handler = new CreateContactHandler(context);

            var dto = new CreateContactDto
            {
                Name = "João Silva",
                Email = "joao@email.com",
                Phone = "11999998888",
            };

            var command = new CreateContactCommand(dto);

            var result = await handler.Handle(command, CancellationToken.None);

            var savedContact = await context.Contacts.FindAsync(result);
            Assert.NotNull(savedContact);
            Assert.Equal(dto.Name, savedContact!.Name);
            Assert.Equal(dto.Email, savedContact.Email);
            Assert.Equal(dto.Phone, savedContact.Phone);
        }
    }
}
