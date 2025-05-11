using DomainModules.BlobStorage.Entities;
using DomainModules.Errors;
using FluentValidation;

namespace DomainModules.BlobStorage.Validators;

public class BlobStorageFileValidator : AbstractValidator<BlobStorageFile>
{
    public BlobStorageFileValidator()
    {
        RuleFor(x => x.FileBaseName)
            .NotEmpty()
            .WithMessage(ErrorMessages.BlobStorageFile_FileBaseName_NotEmpty);

        RuleFor(x => x.FileExtension)
            .NotEmpty()
            .WithMessage(ErrorMessages.BlobStorageFile_FileExtension_NotEmpty)
            .Matches("^[a-zA-Z0-9]+$")
            .WithMessage(ErrorMessages.BlobStorageFile_FileExtension_Invalid);

        RuleFor(x => x.FileSizeInBytes)
            .GreaterThan(0)
            .WithMessage(ErrorMessages.BlobStorageFile_FileSize_GreaterThanZero);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage(ErrorMessages.BlobStorageFile_ContentType_NotEmpty);
    }
}
