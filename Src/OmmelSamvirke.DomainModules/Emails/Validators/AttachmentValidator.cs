using FluentValidation;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DomainModules.Errors;

namespace OmmelSamvirke.DomainModules.Emails.Validators;

public class AttachmentValidator : AbstractValidator<Attachment>
{
    public AttachmentValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ErrorMessages.Attachment_Name_InvalidLength)
            .Length(5, 256)
            .WithMessage(ErrorMessages.Attachment_Name_InvalidLength);

        RuleFor(x => x.BinaryContent)
            .Must(x =>
            {
                if (x is null) return true;
                return x.Length <= ServiceLimits.MaxEmailRequestSizeInBytes;
            })
            .WithMessage(ErrorMessages.Attachment_BinaryContent_TooLarge)
            .Must(x =>
            {
                if (x is null) return true;
                return x.Length != 0;
            })
            .WithMessage(ErrorMessages.Attachment_BinaryContent_IsEmpty);
    }
}