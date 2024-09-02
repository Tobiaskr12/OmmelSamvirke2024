using EmailWrapper.Models;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using ErrorHandling.Interfaces;
using OmmelSamvirke2024.Api.Controllers.Util;

namespace OmmelSamvirke2024.Api.Controllers;

[ApiController]
[Route("test")]
public class TestController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly IValidator _validator;

    public TestController(ILogger logger, IValidator validator)
    {
        _logger = logger;
        _validator = validator;
    }
    
    [HttpGet("hello-world")]
    public ActionResult<string> HelloWorld()
    {
        return Problem();
        
        _logger.LogInformation("This is a test");
        return Ok("Hello World!");
    }

    [HttpGet("fail")]
    public ActionResult<string> Fail(string name, string description)
    {
        var contactListFactory = new ContactListFactory(_validator);
        Result<ContactList> contactListResult = contactListFactory.Create(name, description);
        
        ContactList contactList = ResultHelper.ThrowIfResultIsFailed(contactListResult);

        return Ok(contactList);
    }
}
