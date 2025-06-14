using System.ComponentModel.DataAnnotations;

namespace Agenda.Application.Contacts.Dtos;

public class UpdateContactDto
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Phone { get; set; } = null!;
}
