using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using ServiceModules.MediatorConfig.PipelineBehaviors;

namespace SupportModules.Tests.MediatorConfig;

[TestFixture, Category("UnitTests")]
public class ValidationBehaviorTests
{
    private ValidationBehavior<TestRequest, TestResponse> _behavior;
    private IValidator<TestRequest> _validatorMock;

    [SetUp]
    public void Setup()
    {
        _validatorMock = Substitute.For<IValidator<TestRequest>>();
        _behavior = new ValidationBehavior<TestRequest, TestResponse>([_validatorMock]);
    }

    [Test]
    public void Handle_Should_ThrowValidationException_When_ValidationFails()
    {
        var request = new TestRequest();
        var validationFailures = new List<ValidationFailure>
        {
            new("Property1", "Validation error 1"),
            new("Property2", "Validation error 2")
        };

        _validatorMock.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ValidationResult(validationFailures)));

        RequestHandlerDelegate<TestResponse> next = () => Task.FromResult(new TestResponse());

        var exception = Assert.ThrowsAsync<ValidationException>(async () =>
            await _behavior.Handle(request, next, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null, "A ValidationException should be thrown.");
            Assert.That(exception.Errors.Count(), Is.EqualTo(validationFailures.Count), "The ValidationException should contain all validation failures.");
        });
    }

    [Test]
    public async Task Handle_Should_CallNext_When_ValidationSucceeds()
    {
        var request = new TestRequest();

        _validatorMock.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ValidationResult()));

        var response = new TestResponse();

        TestResponse result = await _behavior.Handle(request, Next, CancellationToken.None);

        Assert.That(result, Is.EqualTo(response), "The handler should return the response from the next delegate when validation succeeds.");
        return;

        Task<TestResponse> Next() => Task.FromResult(response);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public class TestRequest : IRequest<TestResponse> { }

    private class TestResponse { }
}
