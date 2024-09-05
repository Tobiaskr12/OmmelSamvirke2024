using ErrorHandling.Models;
using ErrorHandling.Services.Errors;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ErrorHandling.Tests.Services;

public class ErrorFactoryTests
{
    private ILogger _logger;
    private ErrorFactory _errorFactory;
    
    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger>();
        _errorFactory = new ErrorFactory(_logger);
    }
    
    [Test]
    public void GivenErrorIsCreatedFromArguments_WhenMethodHasExecuted_ErrorObjectIsReturned()
    {
        const string errorMessage = "Test message";
        const int errorStatusCode = 500;
        
        Error createdError = _errorFactory.CreateError(errorMessage, errorStatusCode);
        
        Assert.Multiple(() =>
        {
            Assert.That(createdError.Message, Is.EqualTo(errorMessage));
            Assert.That(createdError.StatusCode, Is.EqualTo(errorStatusCode));
        });
    }
    
    [Test]
    public void GivenErrorIsCreatedFromException_WhenMethodHasExecuted_ErrorIsLogged()
    {
        const string errorMessage = "Test message";
        var exception = new Exception(errorMessage);
        
        Error createdError = _errorFactory.CreateError(exception);
        
        Assert.Multiple(() =>
        {
            Assert.That(createdError.Message, Is.EqualTo(errorMessage));
            Assert.That(createdError.StatusCode, Is.EqualTo(500));
            StringAssert.DoesNotContain("Stack Trace: None", createdError.ToString());
        });
    }
}
