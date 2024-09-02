using ErrorHandling.Models;
using ErrorHandling.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ErrorHandling.Tests.Services;

public class ErrorHandlerTests
{
    private ILogger _logger;
    private ErrorHandler _errorHandler;
    
    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger>();
        _errorHandler = new ErrorHandler(_logger);
    }
    
    [Test]
    public void GivenErrorIsCreatedFromArguments_WhenMethodHasExecuted_ErrorObjectIsReturned()
    {
        const string errorMessage = "Test message";
        const int errorStatusCode = 500;
        
        Error createdError = _errorHandler.CreateError(errorMessage, errorStatusCode);
        
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
        
        Error createdError = _errorHandler.CreateError(exception);
        
        Assert.Multiple(() =>
        {
            Assert.That(createdError.Message, Is.EqualTo(errorMessage));
            Assert.That(createdError.StatusCode, Is.EqualTo(500));
            StringAssert.DoesNotContain("Stack Trace: None", createdError.ToString());
        });
    }
}
