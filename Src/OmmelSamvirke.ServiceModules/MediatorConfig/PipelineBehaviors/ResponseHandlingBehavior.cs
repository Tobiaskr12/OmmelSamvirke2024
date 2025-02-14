using Contracts.SupportModules.Logging;
using Contracts.SupportModules.Logging.Util;
using FluentResults;
using FluentValidation;
using MediatR;
using OmmelSamvirke.ServiceModules.Errors;
using System.Diagnostics;
using System.Reflection;

namespace OmmelSamvirke.ServiceModules.MediatorConfig.PipelineBehaviors;

public class ResponseHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : ResultBase
{
    private readonly ILoggingHandler _logger;
    private readonly ITraceHandler _traceHandler;
    private readonly ICorrelationContext _correlationContext;
    private readonly IShortIdGenerator _shortIdGenerator;

    public ResponseHandlingBehavior(
        ILoggingHandler logger, 
        ITraceHandler traceHandler, 
        ICorrelationContext correlationContext,
        IShortIdGenerator shortIdGenerator)
    {
        _logger = logger;
        _traceHandler = traceHandler;
        _correlationContext = correlationContext;
        _shortIdGenerator = shortIdGenerator;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestName = request.GetType().Name;
        var operationType = requestName.Contains("Query") ? "Query" : "Command";
        _correlationContext.OperationId = _shortIdGenerator.Generate();

        try
        {
            TResponse response = await next();

            if (response.IsFailed)
            {
                _traceHandler.Trace(operationType, isSuccess: false, stopwatch.ElapsedMilliseconds, requestName);
                return response;
            }

            _traceHandler.Trace(operationType, isSuccess: true, stopwatch.ElapsedMilliseconds, requestName);
            return response;
        }
        catch (ValidationException ex)
        {
            _traceHandler.Trace(operationType, isSuccess: false, stopwatch.ElapsedMilliseconds, requestName);
            _logger.LogWarning($"Validation failed: {ex.Message}");

            // Gather all validation errors
            var errorMessages = ex.Errors
                                  .Select(e => e.ErrorMessage)
                                  .Where(msg => !string.IsNullOrWhiteSpace(msg))
                                  .ToList();

            // If nothing was in the ErrorMessage list, try ex.Message
            if (errorMessages.Count == 0 && !string.IsNullOrEmpty(ex.Message))
            {
                errorMessages.Add(ex.Message);
            }

            // If still nothing, use a fallback
            if (errorMessages.Count == 0)
            {
                errorMessages.Add(ErrorMessages.GenericValidationError);
            }

            return CreateFailedResult(errorMessages);
        }
        catch (Exception ex)
        {
            _traceHandler.Trace(operationType, isSuccess: false, stopwatch.ElapsedMilliseconds, requestName);
            _logger.LogError(ex);

            return CreateFailedResult([ErrorMessages.GenericErrorWithRetryPrompt]);
        }
    }

    /// <summary>
    /// Creates a failed result (either <see cref="Result"/> or <see cref="Result{T}"/>) containing the provided error messages.
    /// </summary>
    private TResponse CreateFailedResult(IEnumerable<string> errorMessages)
    {
        List<Error> errors = errorMessages.Select(m => new Error(m)).ToList();

        Type responseType = typeof(TResponse);

        // 1. Handle the non-generic Result
        if (responseType == typeof(Result))
        {
            var result = Result.Fail(errors);
            return (TResponse)(object)result;
        }

        // 2. Handle the generic Result<TValue>
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            // Get the T in "Result<T>"
            Type genericTypeArg = responseType.GetGenericArguments()[0];

            // Invoke the static "Result.Fail<T>(IEnumerable<IError>)" method via reflection
            MethodInfo? failMethod = typeof(Result)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m =>
                    m.Name == "Fail"
                    && m.IsGenericMethodDefinition
                    && m.GetParameters().Length == 1
                    && m.GetParameters()[0].ParameterType == typeof(IEnumerable<IError>)
                );

            if (failMethod == null)
            {
                _logger.LogError(new Exception("Failed to find a suitable Fail<T>(IEnumerable<IError>) method on Result class."));
                return (TResponse)(object)(Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt));
            }

            MethodInfo genericFailMethod = failMethod.MakeGenericMethod(genericTypeArg);

            // Invoke: "Result.Fail<T>(errors)" => returns a "Result<T>"
            var failedResult = genericFailMethod.Invoke(null, [errors]);
            return (TResponse)failedResult!;
        }

        // 3. If TResponse isn't a Result or Result<T>, just log and return default or throw
        _logger.LogWarning($"Expected TResponse to be Result or Result<T>, but it was {responseType.Name}.");
        return (TResponse)(object)(Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt));
    }
}
