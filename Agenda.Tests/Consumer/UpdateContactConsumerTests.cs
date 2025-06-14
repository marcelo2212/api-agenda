using Agenda.Application.Contacts.Commands;
using Agenda.Application.Contacts.Dtos;
using Agenda.Domain.Entities;
using Agenda.Infrastructure.Contacts.Handlers;
using Agenda.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agenda.Tests.Contacts.Handlers
{
    public class UpdateContacConsumertHandlerTests
    {
        [Fact]
        public async Task HandleValidUpdateCommandShouldUpdateContact()
        {
            var context = TestDbContextFactory.Create();
            var existingContact = new Contact
            {
                Name = "Original Name",
                Email = "original@email.com",
                Phone = "1111111111"
            };

            context.Contacts.Add(existingContact);
            await context.SaveChangesAsync();

            var dto = new UpdateContactDto
            {
                Name = "Updated Name",
                Email = "updated@email.com",
                Phone = "2222222222"
            };

            var command = new UpdateContactCommand(existingContact.Id, dto);
            var handler = new UpdateContactHandler(context);

            await handler.Handle(command, CancellationToken.None);

            var updatedContact = await context.Contacts.FindAsync(existingContact.Id);
            Assert.NotNull(updatedContact);
            Assert.Equal(dto.Name, updatedContact!.Name);
            Assert.Equal(dto.Email, updatedContact.Email);
            Assert.Equal(dto.Phone, updatedContact.Phone);
        }
    }

    public static class TestDbContextFactory
    {
        public static AgendaDbContext Create()
        {
            var options = new DbContextOptionsBuilder<AgendaDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AgendaDbContext(options);
        }
    }
}
