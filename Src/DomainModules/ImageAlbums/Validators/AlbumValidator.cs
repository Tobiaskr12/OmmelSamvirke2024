using DomainModules.Errors;
using DomainModules.ImageAlbums.Entities;
using FluentValidation;

namespace DomainModules.ImageAlbums.Validators;

public class AlbumValidator : AbstractValidator<Album>
{
    public AlbumValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ErrorMessages.Album_Name_NotEmpty)
            .Length(3, 100).WithMessage(ErrorMessages.Album_Name_InvalidLength);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null)
            .WithMessage(ErrorMessages.Album_Description_InvalidLength);
        
        RuleFor(x => x.Images)
            .NotEmpty()
            .WithMessage(ErrorMessages.Album_Images_InvalidSize);
        
        RuleFor(x => x.CoverImage)
            .Must((album, cover) => cover is null || album.Images.Contains(cover))
            .WithMessage(ErrorMessages.Album_CoverImage_MustBeInImages);
    }
}
