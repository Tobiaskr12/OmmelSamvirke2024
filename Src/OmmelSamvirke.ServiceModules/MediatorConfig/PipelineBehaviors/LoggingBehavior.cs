// using System.Diagnostics;
// using System.Reflection;
// using FluentResults;
// using FluentValidation;
// using MediatR;
// using Microsoft.Extensions.Logging;
// using OmmelSamvirke.SupportModules.Logging.Interfaces;
//
// namespace OmmelSamvirke.SupportModules.MediatorConfig.PipelineBehaviors;
//
// public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
//     where TRequest : notnull 
//     where TResponse : ResultBase
// {
//     private readonly ILoggingHandler _logger;
//
//     public LoggingBehavior(ILoggingHandler logger)
//     {
//         _logger = logger;
//     }
//
//     public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
//     {
//         DateTime startTime = DateTime.UtcNow;
//         var requestId = startTime.ToString("mm-ss.fffff");
//         var stopwatch = Stopwatch.StartNew();
//         TResponse? response = null;
//
//         try
//         {
//             _logger.LogInformation($"({requestId}) Started handling \"{requestName}\".", requestId,
//                 typeof(TRequest).Name);
//
//             response = await next();
//             if (response.IsFailed) throw new Exception(response.Errors.ToString());
//
//             stopwatch.Stop();
//             _logger.LogInformation("({requestId}) Finished handling \"{requestName}\". Request took {executionTime}ms",
//                 requestId, typeof(TRequest).Name, stopwatch.Elapsed.TotalMilliseconds);
//
//             return response;
//         }
//         catch (ValidationException ex)
//         {
//             _logger.LogError(ex, 
//                 "(requestId{}) Validation failed for \"{requestName}\". Request failed after {executionTime}ms",
//                 requestId,
//                 typeof(TRequest).Name,
//                 stopwatch.Elapsed.TotalMilliseconds);
//             
//             return CreateFailedResult(response);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, 
//                 "(requestId{}) Error handling \"{requestName}\". Request failed after {executionTime}ms", 
//                 requestId, 
//                 typeof(TRequest).Name, 
//                 stopwatch.Elapsed.TotalMilliseconds);
//             
//             return CreateFailedResult(response);
//         }
//     }
//     
//     private TResponse CreateFailedResult(TResponse? response)
//     {
//         Type tResponseType = typeof(TResponse);
//
//         // TResponse is the non-generic Result
//         if (tResponseType == typeof(Result))
//         {
//             return (TResponse)(object)Result.Fail("An unexpected error occurred.");
//         }
//
//         // TResponse is a generic Result<T>, we need to create a generic fail result
//         if (tResponseType.IsGenericType && tResponseType.GetGenericTypeDefinition() == typeof(Result<>))
//         {
//             Type genericArg = tResponseType.GetGenericArguments()[0];
//             
//             MethodInfo? failMethod = typeof(Result)
//                                      .GetMethods()
//                                      .FirstOrDefault(m =>
//                                          m is { Name: "Fail", IsGenericMethod: true }
//                                          && m.GetParameters().Length == 1
//                                          && m.GetParameters()[0].ParameterType == typeof(string));
//
//             if (failMethod is null)
//             {
//                 _logger.LogError("Failed to find a suitable Fail<T> method on Result class.");
//                 return response!;
//             }
//
//             // Make a generic method for the specific T
//             MethodInfo genericFailMethod = failMethod.MakeGenericMethod(genericArg);
//             object? failedResult = genericFailMethod.Invoke(null, ["An unexpected error occurred."]);
//
//             return (TResponse)failedResult!;
//         }
//
//         // If TResponse is not a Result type, just return the response as is, but log the occurrence
//         _logger.LogError(
//             "{requestName} returned an unexpected response type. Expected a type inheriting from \"ResultBase\" type, but was \"{responseTypeName}\"",
//             typeof(TRequest).Name,
//             typeof(TResponse).Name);
//
//         return response!;
//     }
// }
