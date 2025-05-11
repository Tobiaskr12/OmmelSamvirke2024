using DomainModules.BlobStorage.Entities;
using DomainModules.BlobStorage.Validators;
using DomainModules.Errors;
using DomainModules.ImageAlbums.Entities;
using DomainModules.ImageAlbums.Validators;
using FluentValidation.Results;

namespace DomainModules.Tests.ImageAlbums;

[TestFixture, Category("UnitTests")]
public class ImageValidatorTests
{
    private ImageValidator _validator = null!;
    private BlobStorageFileValidator _realBlobValidator = null!;

    private static BlobStorageFile CreateValidBlob(string baseName = "test", string ext = "jpg", long size = 1024)
    {
       var blob = new BlobStorageFile
       {
            FileBaseName = baseName,
            FileExtension = ext,
            ContentType = $"image/{ext}",
        };
        if (size > 0)
        {
            blob.SetFileSize(size);
        }
        return blob;
    }

    private static Album CreateValidAlbum() => new()
    {
        Name = "Owning Album",
        Images = [],
    };

    private Image CreateBaseValidImage(Album owningAlbum) => new()
    {
        OriginalBlobStorageFile = CreateValidBlob("original"),
        ThumbnailBlobStorageFile = CreateValidBlob("thumbnail", "jpg", 300),
        DefaultBlobStorageFile = CreateValidBlob("default", "jpg", 2048),
        DateTaken = DateTime.UtcNow.AddDays(-1),
        Location = "Valid Location",
        PhotographerName = "Valid Photographer",
        Title = "Valid Title",
        Description = "Valid Description",
        Album = owningAlbum
    };

    [SetUp]
    public void SetUp()
    {
        _realBlobValidator = new BlobStorageFileValidator();
        _validator = new ImageValidator(_realBlobValidator);
    }

    [Test]
    public void Validate_WhenOriginalBlobIsInvalid_ReturnsFailure()
    {
        Album owningAlbum = CreateValidAlbum();
        Image image = CreateBaseValidImage(owningAlbum);
        image.OriginalBlobStorageFile.FileBaseName = string.Empty;

        ValidationResult result = _validator.Validate(image);

        Assert.That(result.IsValid, Is.False, "Validation should fail due to invalid OriginalBlob.");
    }

    [Test]
    public void Validate_WhenDefaultBlobIsPresentAndInvalid_ReturnsFailure()
    {
        Album owningAlbum = CreateValidAlbum();
        Image image = CreateBaseValidImage(owningAlbum);
        image.DefaultBlobStorageFile = CreateValidBlob("default");
        image.DefaultBlobStorageFile.SetFileSize(0);

        ValidationResult result = _validator.Validate(image);

        Assert.That(result.IsValid, Is.False, "Validation should fail due to invalid DefaultBlob.");
    }

    [Test]
    public void Validate_WhenThumbnailBlobIsPresentAndInvalid_ReturnsFailure()
    {
        Album owningAlbum = CreateValidAlbum();
        Image image = CreateBaseValidImage(owningAlbum);
        image.ThumbnailBlobStorageFile = CreateValidBlob("thumb", "png", 512);
        image.ThumbnailBlobStorageFile.ContentType = string.Empty;

        ValidationResult result = _validator.Validate(image);

        Assert.That(result.IsValid, Is.False, "Validation should fail due to invalid ThumbnailBlob.");
    }

    [Test]
    public void Validate_WhenImageIsValid_ReturnsSuccess()
    {
        Album owningAlbum = CreateValidAlbum();
        Image image = CreateBaseValidImage(owningAlbum);

        ValidationResult result = _validator.Validate(image);

        Assert.That(result.IsValid, Is.True, result.Errors.FirstOrDefault()?.ErrorMessage);
    }

    [Test]
    public void Validate_WhenOriginalBlobIsNull_ReturnsFailure()
    {
        Album owningAlbum = CreateValidAlbum();
        Image image = CreateBaseValidImage(owningAlbum);
#pragma warning disable CS8625
        image.OriginalBlobStorageFile = null;
#pragma warning restore CS8625

        ValidationResult result = _validator.Validate(image);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.One.With.Property(nameof(ValidationFailure.ErrorMessage)).EqualTo(ErrorMessages.Image_OriginalUpload_NotEmpty));
        });
    }

    [Test]
    public void Validate_WhenDefaultAndThumbArePresentAndValid_ReturnsSuccess()
    {
        Album owningAlbum = CreateValidAlbum();
        Image image = CreateBaseValidImage(owningAlbum);
        image.DefaultBlobStorageFile = CreateValidBlob("default", "webp", 1000);
        image.ThumbnailBlobStorageFile = CreateValidBlob("thumb", "gif", 300);

        ValidationResult result = _validator.Validate(image);

        Assert.That(result.IsValid, Is.True, result.Errors.FirstOrDefault()?.ErrorMessage);
    }

    [Test]
    public void Validate_WhenDateTakenIsInFuture_ReturnsFailure()
    {
        Album owningAlbum = CreateValidAlbum();
        Image image = CreateBaseValidImage(owningAlbum);
        image.DateTaken = DateTime.UtcNow.AddDays(1);
        ValidationResult result = _validator.Validate(image);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.One.With.Property(nameof(ValidationFailure.ErrorMessage)).EqualTo(ErrorMessages.Image_DateTaken_InFuture));
        });
    }

    [Test]
    public void Validate_WhenDateTakenIsNull_ReturnsSuccess()
    {
        Album owningAlbum = CreateValidAlbum();
        Image image = CreateBaseValidImage(owningAlbum);
        image.DateTaken = null;
        ValidationResult result = _validator.Validate(image);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_WhenLocationIsTooLong_ReturnsFailure()
    {
        Album owningAlbum = CreateValidAlbum();
        Image image = CreateBaseValidImage(owningAlbum);
        image.Location = new string('L', 257);
        ValidationResult result = _validator.Validate(image);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.One.With.Property(nameof(ValidationFailure.ErrorMessage)).EqualTo(ErrorMessages.Image_Location_InvalidLength));
        });
    }

    [Test]
    public void Validate_WhenLocationIsNull_ReturnsSuccess()
    {
        Album owningAlbum = CreateValidAlbum();
        Image image = CreateBaseValidImage(owningAlbum);
        image.Location = null;
        ValidationResult result = _validator.Validate(image);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_WhenPhotographerNameIsTooLong_ReturnsFailure()
    {
        Album owningAlbum = CreateValidAlbum();
        Image image = CreateBaseValidImage(owningAlbum);
        image.PhotographerName = new string('P', 101);
        ValidationResult result = _validator.Validate(image);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.One.With.Property(nameof(ValidationFailure.ErrorMessage)).EqualTo(ErrorMessages.Image_PhotographerName_InvalidLength));
        });
    }

    [Test]
    public void Validate_WhenPhotographerNameIsNull_ReturnsSuccess()
    {
        Album owningAlbum = CreateValidAlbum();
        Image image = CreateBaseValidImage(owningAlbum);
        image.PhotographerName = null;
        ValidationResult result = _validator.Validate(image);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_WhenTitleIsTooLong_ReturnsFailure()
    {
        Album owningAlbum = CreateValidAlbum();
        Image image = CreateBaseValidImage(owningAlbum);
        image.Title = new string('T', 101);
        ValidationResult result = _validator.Validate(image);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.One.With.Property(nameof(ValidationFailure.ErrorMessage)).EqualTo(ErrorMessages.Image_Title_InvalidLength));
        });
    }

    [Test]
    public void Validate_WhenTitleIsNull_ReturnsSuccess()
    {
        Album owningAlbum = CreateValidAlbum();
        Image image = CreateBaseValidImage(owningAlbum);
        image.Title = null;
        ValidationResult result = _validator.Validate(image);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_WhenDescriptionIsTooLong_ReturnsFailure()
    {
        Album owningAlbum = CreateValidAlbum();
        Image image = CreateBaseValidImage(owningAlbum);
        image.Description = new string('D', 501);
        ValidationResult result = _validator.Validate(image);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.One.With.Property(nameof(ValidationFailure.ErrorMessage)).EqualTo(ErrorMessages.Image_Description_InvalidLength));
        });
    }

    [Test]
    public void Validate_WhenDescriptionIsNull_ReturnsSuccess()
    {
        Album owningAlbum = CreateValidAlbum();
        Image image = CreateBaseValidImage(owningAlbum);
        image.Description = null;
        ValidationResult result = _validator.Validate(image);
        Assert.That(result.IsValid, Is.True);
    }
}
