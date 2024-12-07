using FluentResults;
using MediatR;
using OmmelSamvirke.SupportModules.MediatorConfig.PipelineBehaviors;
using OmmelSamvirke.SupportModules.MediatRConfig.Exceptions;

namespace OmmelSamvirke.SupportModules.Tests.MediatorConfig;

[TestFixture, Category("UnitTest")]
public class ResultExceptionThrowingBehaviorTests
{
    private ResultExceptionThrowingBehavior<TestRequest, TestResponse> _behavior;

    [SetUp]
    public void Setup()
    {
        _behavior = new ResultExceptionThrowingBehavior<TestRequest, TestResponse>();
    }

    [Test]
    public void Handle_Should_ThrowResultException_When_ResultIsFailed()
    {
        var request = new TestRequest();
        Result? failedResult = Result.Fail("Error occurred");

        // Simulate the `next` delegate returning a ResultBase-derived response
        RequestHandlerDelegate<ResultBase> next = () => Task.FromResult<ResultBase>(failedResult);
        
        var behavior = new ResultExceptionThrowingBehavior<TestRequest, ResultBase>();

        var exception = Assert.ThrowsAsync<ResultException>(async () =>
            await behavior.Handle(request, next, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null, "A ResultException should be thrown.");
            Assert.That(exception.Result, Is.EqualTo(failedResult), "The thrown exception should contain the failed result.");
        });
    }


    [Test]
    public async Task Handle_Should_ReturnRawResponse_When_ResponseIsNotResultBase()
    {
        var request = new TestRequest();
        var rawResponse = new TestResponse();

        TestResponse response = await _behavior.Handle(request, Next, CancellationToken.None);

        Assert.That(response, Is.EqualTo(rawResponse), "The raw response should be returned unchanged.");
        return;

        Task<TestResponse> Next() => Task.FromResult(rawResponse);
    }

    private class TestRequest : IRequest<TestResponse>;
    private class TestResponse;
}
