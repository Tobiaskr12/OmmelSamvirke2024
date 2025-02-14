using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using OmmelSamvirke.ServiceModules.MediatorConfig.PipelineBehaviors;
using OmmelSamvirke.ServiceModules.Errors;
using Contracts.SupportModules.Logging;
using Contracts.SupportModules.Logging.Util;

namespace OmmelSamvirke.Tests.MediatorConfig.PipelineBehaviors
{
    public class TestRequest { }

    [TestFixture]
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
            var expectedResult = Result.Ok();
            RequestHandlerDelegate<Result> next = () => Task.FromResult(expectedResult);
            var behavior = new ResponseHandlingBehavior<TestRequest, Result>(_logger, _traceHandler, _correlationContext, _shortIdGenerator);

            // Act
            var result = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result, Is.EqualTo(expectedResult));
                _traceHandler.Received(1)
                    .Trace(Arg.Any<string>(), true, Arg.Any<long>(), Arg.Is(request.GetType().Name));
            });
        }

        [Test]
        public async Task Handle_FailedResponse_NonGeneric_ReturnsSameResponse()
        {
            // Arrange
            var request = new TestRequest();
            var failedResult = Result.Fail(new List<Error> { new("Some error") });
            RequestHandlerDelegate<Result> next = () => Task.FromResult(failedResult);
            var behavior = new ResponseHandlingBehavior<TestRequest, Result>(_logger, _traceHandler, _correlationContext, _shortIdGenerator);

            // Act
            var result = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result, Is.EqualTo(failedResult));
                _traceHandler.Received(1)
                    .Trace(Arg.Any<string>(), false, Arg.Any<long>(), Arg.Is(request.GetType().Name));
            });
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
            RequestHandlerDelegate<Result> next = () => throw new ValidationException(failures);
            var behavior = new ResponseHandlingBehavior<TestRequest, Result>(_logger, _traceHandler, _correlationContext, _shortIdGenerator);

            // Act
            var result = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Errors.Count, Is.EqualTo(2));
                Assert.That(result.Errors.Any(e => e.Message == "Error 1"), Is.True);
                Assert.That(result.Errors.Any(e => e.Message == "Error 2"), Is.True);
                _traceHandler.Received(1)
                    .Trace(Arg.Any<string>(), false, Arg.Any<long>(), Arg.Is(request.GetType().Name));
                AssertLoggerWarningCalled("Validation failed");
            });
        }

        [Test]
        public async Task Handle_NonValidationException_NonGeneric_ReturnsErrorWithExceptionMessage()
        {
            // Arrange
            var request = new TestRequest();
            var exceptionMessage = ErrorMessages.GenericErrorWithRetryPrompt;
            RequestHandlerDelegate<Result> next = () => throw new Exception(exceptionMessage);
            var behavior = new ResponseHandlingBehavior<TestRequest, Result>(_logger, _traceHandler, _correlationContext, _shortIdGenerator);

            // Act
            var result = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Errors.Count, Is.EqualTo(1));
                Assert.That(result.Errors[0].Message, Is.EqualTo(exceptionMessage));
                _traceHandler.Received(1)
                    .Trace(Arg.Any<string>(), false, Arg.Any<long>(), Arg.Is(request.GetType().Name));
                AssertLoggerErrorCalled(exceptionMessage);
            });
        }

        #endregion

        #region Generic Result (TResponse = Result<string>)

        [Test]
        public async Task Handle_SuccessfulResponse_Generic_ReturnsResponseWithoutModification()
        {
            // Arrange
            var request = new TestRequest();
            var expectedResult = Result.Ok("Success");
            RequestHandlerDelegate<Result<string>> next = () => Task.FromResult(expectedResult);
            var behavior = new ResponseHandlingBehavior<TestRequest, Result<string>>(_logger, _traceHandler, _correlationContext, _shortIdGenerator);

            // Act
            var result = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result, Is.EqualTo(expectedResult));
                _traceHandler.Received(1)
                    .Trace(Arg.Any<string>(), true, Arg.Any<long>(), Arg.Is(request.GetType().Name));
            });
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
            RequestHandlerDelegate<Result<string>> next = () => throw new ValidationException(failures);
            var behavior = new ResponseHandlingBehavior<TestRequest, Result<string>>(_logger, _traceHandler, _correlationContext, _shortIdGenerator);

            // Act
            var result = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Errors.Count, Is.EqualTo(2));
                Assert.That(result.Errors.Any(e => e.Message == "Error 1"), Is.True);
                Assert.That(result.Errors.Any(e => e.Message == "Error 2"), Is.True);
                _traceHandler.Received(1)
                    .Trace(Arg.Any<string>(), false, Arg.Any<long>(), Arg.Is(request.GetType().Name));
                AssertLoggerWarningCalled("Validation failed");
            });
        }

        [Test]
        public async Task Handle_NonValidationException_Generic_ReturnsErrorWithExceptionMessage()
        {
            // Arrange
            var request = new TestRequest();
            var exceptionMessage = ErrorMessages.GenericErrorWithRetryPrompt;
            RequestHandlerDelegate<Result<string>> next = () => throw new Exception(exceptionMessage);
            var behavior = new ResponseHandlingBehavior<TestRequest, Result<string>>(_logger, _traceHandler, _correlationContext, _shortIdGenerator);

            // Act
            var result = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Errors.Count, Is.EqualTo(1));
                Assert.That(result.Errors[0].Message, Is.EqualTo(exceptionMessage));
                _traceHandler.Received(1)
                    .Trace(Arg.Any<string>(), false, Arg.Any<long>(), Arg.Is(request.GetType().Name));
                AssertLoggerErrorCalled(exceptionMessage);
            });
        }
        #endregion
    }
}
