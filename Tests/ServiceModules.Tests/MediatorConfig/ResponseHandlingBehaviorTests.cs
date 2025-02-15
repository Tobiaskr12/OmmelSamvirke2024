using Contracts.SupportModules.Logging;
using Contracts.SupportModules.Logging.Util;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using ServiceModules.Errors;
using ServiceModules.MediatorConfig.PipelineBehaviors;

namespace ServiceModules.Tests.MediatorConfig;

public class TestRequest;

[TestFixture]
// ReSharper disable ExplicitCallerInfoArgument
public class ResponseHandlingBehaviorTests
{
    private ILoggingHandler _logger;
    private ITraceHandler _traceHandler;
    private ICorrelationContext _correlationContext;
    private IShortIdGenerator _shortIdGenerator;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILoggingHandler>();
        _traceHandler = Substitute.For<ITraceHandler>();
        _correlationContext = Substitute.For<ICorrelationContext>();
        _shortIdGenerator = Substitute.For<IShortIdGenerator>();
    }

    private void AssertLoggerWarningCalled(string expectedContent)
    {
        _logger.Received(1)
               .LogWarning(
                   Arg.Is<string>(msg => msg.Contains(expectedContent)),
                   Arg.Any<string>(),
                   Arg.Any<int>(),
                   Arg.Any<string>());
    }

    private void AssertLoggerErrorCalled(string expectedContent)
    {
        _logger.Received(1)
               .LogError(
                   Arg.Is<Exception>(ex => ex != null && ex.Message.Contains(expectedContent)),
                   Arg.Any<string>(),
                   Arg.Any<string>(),
                   Arg.Any<int>(),
                   Arg.Any<string>());
    }

    #region Non-Generic Result (TResponse = Result)

    [Test]
    public async Task Handle_SuccessfulResponse_NonGeneric_ReturnsResponseWithoutModification()
    {
        // Arrange
        var request = new TestRequest();
        Result? expectedResult = Result.Ok();
        var behavior = new ResponseHandlingBehavior<TestRequest, Result>(_logger, _traceHandler, _correlationContext, _shortIdGenerator);

        // Act
        Result result = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result, Is.EqualTo(expectedResult));
            _traceHandler.Received(1)
                         .Trace(Arg.Any<string>(), true, Arg.Any<long>(), Arg.Is(request.GetType().Name));
        });
        
        return;
        Task<Result> Next() => Task.FromResult(expectedResult);
    }

    [Test]
    public async Task Handle_FailedResponse_NonGeneric_ReturnsSameResponse()
    {
        // Arrange
        var request = new TestRequest();
        Result? failedResult = Result.Fail(new List<Error> { new("Some error") });
        var behavior = new ResponseHandlingBehavior<TestRequest, Result>(_logger, _traceHandler, _correlationContext, _shortIdGenerator);

        // Act
        Result result = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result, Is.EqualTo(failedResult));
            _traceHandler.Received(1)
                         .Trace(Arg.Any<string>(), false, Arg.Any<long>(), Arg.Is(request.GetType().Name));
        });
        
        return;
        Task<Result> Next() => Task.FromResult(failedResult);
    }

    [Test]
    public async Task Handle_ValidationException_NonGeneric_AggregatesErrors()
    {
        // Arrange
        var request = new TestRequest();
        var failures = new List<ValidationFailure>
        {
            new("Field1", "Error 1"),
            new("Field2", "Error 2")
        };
        var behavior = new ResponseHandlingBehavior<TestRequest, Result>(_logger, _traceHandler, _correlationContext, _shortIdGenerator);

        // Act
        Result result = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors, Has.Count.EqualTo(2));
            Assert.That(result.Errors.Any(e => e.Message == "Error 1"), Is.True);
            Assert.That(result.Errors.Any(e => e.Message == "Error 2"), Is.True);
            _traceHandler.Received(1)
                         .Trace(Arg.Any<string>(), false, Arg.Any<long>(), Arg.Is(request.GetType().Name));
            AssertLoggerWarningCalled("Validation failed");
        });
        
        return;
        Task<Result> Next() => throw new ValidationException(failures);
    }

    [Test]
    public async Task Handle_NonValidationException_NonGeneric_ReturnsErrorWithExceptionMessage()
    {
        // Arrange
        var request = new TestRequest();
        string? exceptionMessage = ErrorMessages.GenericErrorWithRetryPrompt;
        var behavior = new ResponseHandlingBehavior<TestRequest, Result>(_logger, _traceHandler, _correlationContext, _shortIdGenerator);

        // Act
        Result result = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.That(result.Errors[0].Message, Is.EqualTo(exceptionMessage));
            _traceHandler.Received(1)
                         .Trace(Arg.Any<string>(), false, Arg.Any<long>(), Arg.Is(request.GetType().Name));
            AssertLoggerErrorCalled(exceptionMessage);
        });
        
        return;
        Task<Result> Next() => throw new Exception(exceptionMessage);
    }

    #endregion

    #region Generic Result (TResponse = Result<string>)

    [Test]
    public async Task Handle_SuccessfulResponse_Generic_ReturnsResponseWithoutModification()
    {
        // Arrange
        var request = new TestRequest();
        Result<string>? expectedResult = Result.Ok("Success");
        var behavior = new ResponseHandlingBehavior<TestRequest, Result<string>>(_logger, _traceHandler, _correlationContext, _shortIdGenerator);

        // Act
        Result<string> result = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result, Is.EqualTo(expectedResult));
            _traceHandler.Received(1)
                         .Trace(Arg.Any<string>(), true, Arg.Any<long>(), Arg.Is(request.GetType().Name));
        });
        
        return;
        Task<Result<string>> Next() => Task.FromResult(expectedResult);
    }

    [Test]
    public async Task Handle_ValidationException_Generic_AggregatesErrors()
    {
        // Arrange
        var request = new TestRequest();
        var failures = new List<ValidationFailure>
        {
            new("Field1", "Error 1"),
            new("Field2", "Error 2")
        };
        var behavior = new ResponseHandlingBehavior<TestRequest, Result<string>>(_logger, _traceHandler, _correlationContext, _shortIdGenerator);

        // Act
        Result<string> result = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors, Has.Count.EqualTo(2));
            Assert.That(result.Errors.Any(e => e.Message == "Error 1"), Is.True);
            Assert.That(result.Errors.Any(e => e.Message == "Error 2"), Is.True);
            _traceHandler.Received(1)
                         .Trace(Arg.Any<string>(), false, Arg.Any<long>(), Arg.Is(request.GetType().Name));
            AssertLoggerWarningCalled("Validation failed");
        });
        
        return;
        Task<Result<string>> Next() => throw new ValidationException(failures);
    }

    [Test]
    public async Task Handle_NonValidationException_Generic_ReturnsErrorWithExceptionMessage()
    {
        // Arrange
        var request = new TestRequest();
        string? exceptionMessage = ErrorMessages.GenericErrorWithRetryPrompt;
        var behavior = new ResponseHandlingBehavior<TestRequest, Result<string>>(_logger, _traceHandler, _correlationContext, _shortIdGenerator);

        // Act
        Result<string> result = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.That(result.Errors[0].Message, Is.EqualTo(exceptionMessage));
            _traceHandler.Received(1)
                         .Trace(Arg.Any<string>(), false, Arg.Any<long>(), Arg.Is(request.GetType().Name));
            AssertLoggerErrorCalled(exceptionMessage);
        });
        
        return;
        Task<Result<string>> Next() => throw new Exception(exceptionMessage);
    }
    #endregion
}
