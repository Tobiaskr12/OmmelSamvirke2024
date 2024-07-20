using Microsoft.AspNetCore.Mvc;

namespace OmmelSamvirke2024.ApiService.Controllers;

[ApiController]
[Route("test")]
public class TestController : ControllerBase
{
    private readonly ILogger _logger;

    public TestController(ILogger logger)
    {
        _logger = logger;
    }
    
    [HttpGet("hello-world")]
    public ActionResult<string> HelloWorld()
    {
        _logger.LogInformation("This is a test");
        return Ok("Hello World!");
    }
}
