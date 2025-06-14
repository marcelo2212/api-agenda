namespace Agenda.Application.Contacts.Dtos;

public class CreateContactDto
{
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
}
