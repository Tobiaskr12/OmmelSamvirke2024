using DomainModules.BlobStorage.Entities;
using DomainModules.Errors;
using DomainModules.ImageAlbums.Entities;
using FluentValidation;

namespace DomainModules.ImageAlbums.Validators;

public class ImageValidator : AbstractValidator<Image>
{
    public ImageValidator(IValidator<BlobStorageFile> blobValidator)
    {
        // Only original is required at validation; service creates the rest
        RuleFor(x => x.OriginalBlobStorageFile)
            .NotNull()
            .WithMessage(ErrorMessages.Image_OriginalUpload_NotEmpty)
            .SetValidator(blobValidator);

        RuleFor(x => x.DefaultBlobStorageFile)
            .NotNull()
            .WithMessage(ErrorMessages.Image_OriginalUpload_NotEmpty)
            .SetValidator(blobValidator);
        
        RuleFor(x => x.ThumbnailBlobStorageFile)
            .NotNull()
            .WithMessage(ErrorMessages.Image_OriginalUpload_NotEmpty)
            .SetValidator(blobValidator);

        RuleFor(x => x.DateTaken)
            .Must(d => d == null || d <= DateTime.UtcNow)
            .WithMessage(ErrorMessages.Image_DateTaken_InFuture);

        RuleFor(x => x.Location)
            .MaximumLength(256)
            .When(x => x.Location is not null)
            .WithMessage(ErrorMessages.Image_Location_InvalidLength);

        RuleFor(x => x.PhotographerName)
            .MaximumLength(100)
            .When(x => x.PhotographerName is not null)
            .WithMessage(ErrorMessages.Image_PhotographerName_InvalidLength);

        RuleFor(x => x.Title)
            .MaximumLength(100)
            .When(x => x.Title is not null)
            .WithMessage(ErrorMessages.Image_Title_InvalidLength);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null)
            .WithMessage(ErrorMessages.Image_Description_InvalidLength);
    }
}