using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OmmelSamvirke.DTOs.Emails;
using OmmelSamvirke.Interfaces.Emails;

namespace OmmelSamvirke2024.Api.Controllers;

[ApiController]
[Route("test")]
public class TestController : ControllerBase
{
    private readonly IMediator _mediator;

    public TestController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost("test")]
    public async Task<ActionResult<string>> Fail([FromBody] SendEmailCommand sendEmailCommand)
    {
        Result<EmailSendingStatus> result = await _mediator.Send(sendEmailCommand);
        return Ok(result.Value);
    }
}
