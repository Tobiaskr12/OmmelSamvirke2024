using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OmmelSamvirke.SupportModules.MediatorConfig.PipelineBehaviors;

namespace OmmelSamvirke.SupportModules.Tests.MediatorConfig;

[TestFixture, Category("UnitTest")]
public class LoggingBehaviorTests
{
    private ILogger _loggerMock;
    private LoggingBehavior<TestRequest, TestResponse> _behavior;

    [SetUp]
    public void Setup()
    {
        _loggerMock = Substitute.For<ILogger>();
        _behavior = new LoggingBehavior<TestRequest, TestResponse>(_loggerMock);
    }

    [Test]
    public async Task Handle_Should_LogInformation_Twice_On_Success()
    {
        var request = new TestRequest();
        var response = new TestResponse();
        
        TestResponse result = await _behavior.Handle(request, Next, CancellationToken.None);
        
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

        Task<TestResponse> Next() => Task.FromResult(response);
    }

    [Test]
    public void Handle_Should_LogError_When_Exception_Is_Thrown()
    {
        var request = new TestRequest();
        var exception = new Exception("Test exception");

        RequestHandlerDelegate<TestResponse> next = () => Task.FromException<TestResponse>(exception);
        
        // Simulate exception during logging
        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await _behavior.Handle(request, next, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(ex, Is.EqualTo(exception), "The thrown exception should be the one from the handler.");

            _loggerMock.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("Error handling")),
                exception,
                Arg.Any<Func<object, Exception, string>>()!);
        });
    }

    private class TestRequest : IRequest<TestResponse>;
    private class TestResponse;
}
