using DomainModules.Common;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;

namespace Web.Components.Shared.MultiStepForm;

public abstract class AbstractStepSection<T> : ComponentBase where T : BaseEntity
{
    [CascadingParameter(Name = "ParentStep")]
    public required Step<T> ParentStep { get; set; }
    
    [CascadingParameter(Name = "ParentMultiStepForm")]
    public required MultiStepForm<T> ParentForm { get; set; }
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        ParentStep.RegisterChild(this);
    }
    
    protected abstract IValidator<T>? Validator { get; set; }
    
    public ValidationResult Validate()
    {
        return Validator is null
            ? new ValidationResult()
            : Validator.Validate(new ValidationContext<T>(ParentForm.Entity));
    }

    public T Entity => ParentForm.Entity;
}