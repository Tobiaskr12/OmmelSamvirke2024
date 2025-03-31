using DomainModules.Errors;
using DomainModules.Events.Entities;
using FluentValidation;

namespace DomainModules.Events.Validators;

public class EventValidator : AbstractValidator<Event>
{
    public EventValidator(IValidator<EventCoordinator> eventCoordinatorValidator, IValidator<EventRemoteFile> remoteFileValidator)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(ErrorMessages.Event_Title_NotEmpty)
            .Length(3, 100)
            .WithMessage(ErrorMessages.Event_Title_InvalidLength);
        
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(ErrorMessages.Event_Description_NotEmpty)
            .Length(10, 5000)
            .WithMessage(ErrorMessages.Event_Description_InvalidLength);
        
        RuleFor(x => x.StartTime)
            .Must(start => start > DateTime.UtcNow)
            .WithMessage(ErrorMessages.Event_StartTime_MustBeInFuture);
        
        RuleFor(x => x.EndTime)
            .Must((e, endTime) => endTime > e.StartTime)
            .WithMessage(ErrorMessages.Event_EndTime_MustBeAfterStart);
        
        RuleFor(x => x.EndTime)
            .Must((e, endTime) => (endTime - e.StartTime).TotalMinutes >= 15)
            .WithMessage(ErrorMessages.Event_Duration_Minimum15Minutes);
        
        RuleFor(x => x.EventCoordinator)
            .NotNull()
            .WithMessage(ErrorMessages.Event_EventCoordinator_NotNull)
            .SetValidator(eventCoordinatorValidator);
        
        RuleFor(x => x.Location)
            .NotEmpty()
            .WithMessage(ErrorMessages.Event_Location_NotEmpty)
            .Length(3, 50)
            .WithMessage(ErrorMessages.Event_Location_InvalidLength);
        
        RuleFor(x => x.RemoteFiles)
            .NotNull()
            .WithMessage(ErrorMessages.Event_RemoteFiles_NotNull)
            // Ensure uniqueness based on URL (case-insensitive)
            .Must(files => files.Select(f => f.Url.ToLowerInvariant()).Distinct().Count() == files.Count)
            .WithMessage(ErrorMessages.Event_RemoteFiles_MustBeUnique)
            .ForEach(fileRule => fileRule.SetValidator(remoteFileValidator));
    }
}
