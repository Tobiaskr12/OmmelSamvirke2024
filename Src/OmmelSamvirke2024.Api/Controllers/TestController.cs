using EmailWrapper.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OmmelSamvirke2024.Api.Controllers.Util;

namespace OmmelSamvirke2024.Api.Controllers;

[ApiController]
[Route("test")]
public class TestController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly IStringLocalizer _localizer;

    public TestController(ILogger logger, IStringLocalizer<TestController> localizer)
    {
        _logger = logger;
        _localizer = localizer;
    }
    
    [HttpGet("test")]
    public ActionResult<string> Fail(string name, string description)
    {
        // var contactListFactory = new ContactListFactory();
        // ContactList contactList = ResultHelper.ThrowIfResultIsFailed(contactListFactory.Create(name, description));

        return Ok();
    }
}
