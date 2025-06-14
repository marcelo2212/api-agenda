using Agenda.Application.Contacts.Commands;
using Agenda.Application.Contacts.Dtos;
using Agenda.Domain.Entities;
using Agenda.Infrastructure.Contacts.Handlers;
using Agenda.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agenda.Tests.Contacts.Handlers
{
    public class UpdateContactHandlerTests
    {
        private AgendaDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AgendaDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AgendaDbContext(options);
        }

        [Fact]
        public async Task Handle_ValidUpdateCommand_ShouldUpdateContact()
        {

            var context = GetInMemoryDbContext();

            var existingContact = new Contact
            {
                Name = "Original",
                Email = "original@email.com",
                Phone = "11911111111"
            };

            context.Contacts.Add(existingContact);
            await context.SaveChangesAsync();

            var handler = new UpdateContactHandler(context);

            var updateDto = new UpdateContactDto
            {
                Name = "Atualizado",
                Email = "novo@email.com",
                Phone = "11999999999"
            };

            var command = new UpdateContactCommand(existingContact.Id, updateDto);

            await handler.Handle(command, CancellationToken.None);

            var updated = await context.Contacts.FindAsync(existingContact.Id);
            Assert.NotNull(updated);
            Assert.Equal("Atualizado", updated!.Name);
            Assert.Equal("novo@email.com", updated.Email);
            Assert.Equal("11999999999", updated.Phone);
        }
    }
}
