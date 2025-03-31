using DomainModules.Errors;
using DomainModules.Events.Entities;
using FluentValidation;

namespace DomainModules.Events.Validators;

public class EventRemoteFileValidator : AbstractValidator<EventRemoteFile>
{
    public EventRemoteFileValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage(ErrorMessages.EventRemoteFile_FileName_NotEmpty)
            .Length(1, 255)
            .WithMessage(ErrorMessages.EventRemoteFile_FileName_InvalidLength);
            
        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage(ErrorMessages.EventRemoteFile_FileSize_Invalid);
            
        RuleFor(x => x.FileType)
            .NotEmpty()
            .WithMessage(ErrorMessages.EventRemoteFile_FileType_NotEmpty)
            .Length(1, 100)
            .WithMessage(ErrorMessages.EventRemoteFile_FileType_InvalidLength);
            
        RuleFor(x => x.Url)
            .NotEmpty()
            .WithMessage(ErrorMessages.EventRemoteFile_Url_NotEmpty)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage(ErrorMessages.EventRemoteFile_Url_Invalid);
    }
}
