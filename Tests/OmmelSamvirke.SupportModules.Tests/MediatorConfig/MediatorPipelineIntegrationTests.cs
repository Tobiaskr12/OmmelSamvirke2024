using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OmmelSamvirke.SupportModules.MediatRConfig.Exceptions;

namespace OmmelSamvirke.SupportModules.Tests.MediatorConfig;

[TestFixture, Category("IntegrationTests")]
public class MediatorPipelineIntegrationTests
{
    private IMediator _mediator;
    private ILogger _logger;

    [SetUp]
    public void Setup()
    {
        _mediator = ServiceRegistry.GetService<IMediator>();
        _logger = ServiceRegistry.GetService<ILogger>();
    }
    
    #region TestCommandClasses
    public record TestCommand(string Name, bool ShouldFail) : IRequest<Result<string>>;
    
    [UsedImplicitly]
    public class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name must not be empty.")
                .MinimumLength(5)
                .WithMessage("Name must be at least 5 characters long.");
        }
    }
    
    public class TestCommandHandler : IRequestHandler<TestCommand, Result<string>>
    {
        private readonly ILogger _logger;

        public TestCommandHandler(ILogger logger)
        {
            _logger = logger;
        }

        public Task<Result<string>> Handle(TestCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing command with name: {Name}", request.Name);

            return Task.FromResult(request.ShouldFail ? 
                Result.Fail<string>("Command failed.") : 
                Result.Ok($"Processed: {request.Name}"));
        }
    }
    #endregion
    
    [Test]
    public void FullFlow_Should_ThrowValidationException_When_ValidationFails()
    {
        var command = new TestCommand(string.Empty, ShouldFail: false);

        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
            await _mediator.Send(command, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null, "A ValidationException should be thrown.");
            Assert.That(exception.Errors.Any(e => e.PropertyName == "Name"), "Validation failure should be for the Name property.");
        });
    }

    [Test]
    public async Task FullFlow_Should_ProcessSuccessfully_When_ValidationSucceeds()
    {
        var command = new TestCommand("ValidName", ShouldFail: false);

        Result<string> result = await _mediator.Send(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null, "The result should not be null.");
            Assert.That(result.IsSuccess, Is.True, "The result should be successful.");
            Assert.That(result.Value, Is.EqualTo("Processed: ValidName"), "The result value should match the expected output.");
        });
    }

    [Test]
    public void FullFlow_Should_ThrowResultException_When_CommandFails()
    {
        var command = new TestCommand("ValidName", ShouldFail: true);

        var exception = Assert.ThrowsAsync<ResultException>(async () =>
            await _mediator.Send(command, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null, "A ResultException should be thrown.");
            Assert.That(exception.Result.IsFailed, Is.True, "The exception should contain the failed result.");
        });
    }

    [Test]
    public async Task FullFlow_Should_LogCorrectly_ForSuccessfulProcessing()
    {
        _logger.ClearReceivedCalls();
        var command = new TestCommand("ValidName", ShouldFail: false);

        await _mediator.Send(command, CancellationToken.None);

        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Started handling")),
            null,
            Arg.Any<Func<object, Exception?, string>>());

        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Finished handling")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Test]
    public void FullFlow_Should_LogCorrectly_ForFailedProcessing()
    {
        _logger.ClearReceivedCalls();
        var command = new TestCommand("ValidName", ShouldFail: true);

        Assert.ThrowsAsync<ResultException>(async () =>
            await _mediator.Send(command, CancellationToken.None));

        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Started handling")),
            null,
            Arg.Any<Func<object, Exception?, string>>());

        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Error handling")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
