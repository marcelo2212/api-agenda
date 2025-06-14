using Agenda.Application.Contacts.Commands;
using Agenda.Domain.Entities;
using Agenda.Infrastructure.Contacts.Handlers;
using Agenda.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agenda.Tests.Contacts.Handlers
{
    public class DeleteContactHandlerTests
    {
        private AgendaDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AgendaDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AgendaDbContext(options);
        }

        [Fact]
        public async Task Handle_ValidId_ShouldDeleteContact()
        {
            var context = GetInMemoryDbContext();

            var contact = new Contact
            {
                Name = "João Excluir",
                Email = "joao@delete.com",
                Phone = "11999990000"
            };

            context.Contacts.Add(contact);
            await context.SaveChangesAsync();

            var handler = new DeleteContactHandler(context);
            var command = new DeleteContactCommand(contact.Id);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var deleted = await context.Contacts.FindAsync(contact.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task Handle_InvalidId_ShouldNotThrowAndDoNothing()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var handler = new DeleteContactHandler(context);

            var fakeId = Guid.NewGuid();
            var command = new DeleteContactCommand(fakeId);

            // Act & Assert
            await handler.Handle(command, CancellationToken.None);

            // Confirma que nenhum contato foi removido (db vazio)
            Assert.Empty(await context.Contacts.ToListAsync());
        }
    }
}
