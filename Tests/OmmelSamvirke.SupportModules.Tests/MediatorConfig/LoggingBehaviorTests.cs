using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OmmelSamvirke.SupportModules.MediatorConfig.PipelineBehaviors;

namespace OmmelSamvirke.SupportModules.Tests.MediatorConfig;

[TestFixture, Category("UnitTests")]
public class LoggingBehaviorTests
{
    private ILogger _loggerMock;
    private LoggingBehavior<TestRequest, ResultBase> _behavior;

    [SetUp]
    public void Setup()
    {
        _loggerMock = Substitute.For<ILogger>();
        _behavior = new LoggingBehavior<TestRequest, ResultBase>(_loggerMock);
    }

    [Test]
    public async Task Handle_Should_LogInformation_Twice_On_Success()
    {
        var request = new TestRequest();
        Result? response = Result.Ok();
        
        ResultBase result = await _behavior.Handle(request, Next, CancellationToken.None);
        
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(response), "The response should be returned as expected.");

            _loggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("Started handling")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>()!);

            _loggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("Finished handling")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>()!);
        });
        return;

        Task<ResultBase> Next() => Task.FromResult<ResultBase>(response);
    }

    [Test]
    public async Task Handle_Should_LogError_When_Exception_Is_Thrown()
    {
        var request = new TestRequest();
        var exception = new Exception("Test exception");

        RequestHandlerDelegate<ResultBase> next = () => Task.FromException<ResultBase>(exception);
        
        await _behavior.Handle(request, next, CancellationToken.None);
        
        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Error handling")),
            exception,
            Arg.Any<Func<object, Exception, string>>()!);
    }

    private class TestRequest : IRequest<ResultBase>;
}
