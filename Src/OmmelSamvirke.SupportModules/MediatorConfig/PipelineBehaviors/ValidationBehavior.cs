﻿using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace OmmelSamvirke.SupportModules.MediatorConfig.PipelineBehaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any()) return await next();
        var context = new ValidationContext<TRequest>(request);

        ValidationResult[] validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        List<ValidationFailure> failures = validationResults
                                           .SelectMany(result => result.Errors)
                                           .Where(failure => failure is not null)
                                           .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }
        
        return await next();
    }
}
