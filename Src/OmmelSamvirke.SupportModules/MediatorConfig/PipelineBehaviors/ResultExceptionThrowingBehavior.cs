using FluentResults;
using MediatR;
using OmmelSamvirke.SupportModules.MediatRConfig.Exceptions;

namespace OmmelSamvirke.SupportModules.MediatorConfig.PipelineBehaviors;

public class ResultExceptionThrowingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse response = await next();
        if (response is not ResultBase result) return response;
        
        if (result.IsFailed)
        {
            throw new ResultException(result);
        }
        
        return response;
    }
}
