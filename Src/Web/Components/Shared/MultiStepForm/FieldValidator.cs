using FluentValidation;
using FluentValidation.Results;

namespace Web.Components.Shared.MultiStepForm;

public class FieldValidator<TProperty>
{
    private readonly InlineValidator<TProperty> _validator;

    /// <param name="configureRules">
    /// A lambda that allows creating a rule with FluentValidation methods
    /// like .NotEmpty(), .Length(), .Must(), etc.
    /// </param>
    /// <summary>
    /// Specified the rule(s) for the validator
    /// </summary>
    public FieldValidator(Action<IRuleBuilderInitial<TProperty, TProperty>> configureRules)
    {
        ArgumentNullException.ThrowIfNull(configureRules);
        _validator = new InlineValidator<TProperty>();
        configureRules(_validator.RuleFor(x => x!));
    }
    
    /// <summary>
    /// Returns a Func that treats null as an error with the given message,
    /// otherwise runs InlineValidator rules.
    /// Works with MudBlazor by calling this via the "Validation" property of input elements
    /// </summary>
    public Func<TProperty?, string> ValidateAsRequired(string nullErrorMessage) 
        => value => ValidateAsRequired(value, nullErrorMessage);

    /// <summary>
    /// Returns a Func that skips validation on null (or default), 
    /// otherwise runs InlineValidator rules.
    /// Works with MudBlazor by calling this via the "Validation" property of input elements
    /// </summary>
    public Func<TProperty?, string> ValidateAsOptional() => ValidateAsOptional;
    
    private string ValidateAsRequired(TProperty? value, string nullErrorMessage)
    {
        if (value is null) return nullErrorMessage;

        ValidationResult? result = _validator.Validate(value);
        return result.IsValid 
            ? string.Empty 
            : result.Errors.First().ErrorMessage;
    }
    
    private string ValidateAsOptional(TProperty? value)
    {
        if (value is null) return string.Empty;

        ValidationResult? result = _validator.Validate(value);
        return result.IsValid 
            ? string.Empty 
            : result.Errors.First().ErrorMessage;
    }
}
