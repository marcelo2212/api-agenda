using Agenda.Application.Contacts.Commands;
using Agenda.Application.Contacts.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Agenda.Application.Contacts.Queries;

namespace Agenda.API.Controllers;

[ApiController]
[Route("contacts")]
public class ContactsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContactsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [AllowAnonymous] // remover quando JWT estiver habilitado
    public async Task<IActionResult> Create([FromBody] CreateContactDto dto)
    {
        var id = await _mediator.Send(new CreateContactCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetContactByIdQuery(id));
        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllContactsQuery());
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContactDto dto)
    {
        var command = new UpdateContactCommand(id, dto);
        var success = await _mediator.Send(command);

        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteContactCommand(id);
        var success = await _mediator.Send(command);

        if (!success)
            return NotFound();

        return NoContent();
    }

    
}
