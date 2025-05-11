using DomainModules.BlobStorage.Entities;
using DomainModules.Errors;
using DomainModules.ImageAlbums.Entities;
using DomainModules.ImageAlbums.Validators;
using FluentValidation.Results;

namespace DomainModules.Tests.ImageAlbums;

[TestFixture, Category("UnitTests")]
public class AlbumValidatorTests
{
    private AlbumValidator _validator = null!;

    private static BlobStorageFile CreateValidBlob(string baseName = "test", string ext = "jpg") => new()
    {
        FileBaseName = baseName,
        FileExtension = ext,
        ContentType = $"image/{ext}",
    };

    private static Image CreateValidImage(Album album, int id = 0, string blobBaseName = "img") => new()
    {
        Id = id,
        OriginalBlobStorageFile = CreateValidBlob(blobBaseName),
        DefaultBlobStorageFile = CreateValidBlob(blobBaseName),
        ThumbnailBlobStorageFile = CreateValidBlob(blobBaseName),
        Album = album
    };

    [SetUp]
    public void SetUp()
    {
        _validator = new AlbumValidator();
    }

    private static Album CreateBaseValidAlbum()
    {
        var album = new Album
        {
            Name = "Valid Album Name",
            Description = "A valid description.",
            Images = [],
        };

        Image image1 = CreateValidImage(album, 1, "image1");
        Image image2 = CreateValidImage(album, 2, "image2");

        album.Images = [image1, image2];
        album.CoverImage = image1;

        return album;
    }

    [Test]
    public void Validate_WhenAlbumIsValid_ReturnsSuccess()
    {
        Album album = CreateBaseValidAlbum();
        ValidationResult result = _validator.Validate(album);
        Assert.That(result.IsValid, Is.True, result.Errors.FirstOrDefault()?.ErrorMessage);
    }

    [Test]
    public void Validate_WhenNameIsEmpty_ReturnsFailure()
    {
        Album album = CreateBaseValidAlbum();
        album.Name = string.Empty;
        ValidationResult result = _validator.Validate(album);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.One.With.Property(nameof(ValidationFailure.ErrorMessage)).EqualTo(ErrorMessages.Album_Name_NotEmpty));
        });
    }

    [Test]
    public void Validate_WhenNameIsTooShort_ReturnsFailure()
    {
        Album album = CreateBaseValidAlbum();
        album.Name = "AB";
        ValidationResult result = _validator.Validate(album);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.One.With.Property(nameof(ValidationFailure.ErrorMessage)).EqualTo(ErrorMessages.Album_Name_InvalidLength));
        });
    }

    [Test]
    public void Validate_WhenNameIsTooLong_ReturnsFailure()
    {
        Album album = CreateBaseValidAlbum();
        album.Name = new string('A', 101);
        ValidationResult result = _validator.Validate(album);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.One.With.Property(nameof(ValidationFailure.ErrorMessage)).EqualTo(ErrorMessages.Album_Name_InvalidLength));
        });
    }

     [Test]
    public void Validate_WhenDescriptionIsNull_ReturnsSuccess()
    {
        Album album = CreateBaseValidAlbum();
        album.Description = null;
        ValidationResult result = _validator.Validate(album);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_WhenDescriptionIsTooLong_ReturnsFailure()
    {
        Album album = CreateBaseValidAlbum();
        album.Description = new string('D', 501);
        ValidationResult result = _validator.Validate(album);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.One.With.Property(nameof(ValidationFailure.ErrorMessage)).EqualTo(ErrorMessages.Album_Description_InvalidLength));
        });
    }

    [Test]
    public void Validate_WhenImagesListIsEmpty_ReturnsFailure()
    {
        Album album = CreateBaseValidAlbum();
        album.Images.Clear();
        album.CoverImage = null;
        ValidationResult result = _validator.Validate(album);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.One.With.Property(nameof(ValidationFailure.ErrorMessage)).EqualTo(ErrorMessages.Album_Images_InvalidSize));
        });
    }

    [Test]
    public void Validate_WhenCoverImageIsNull_ReturnsSuccess()
    {
        Album album = CreateBaseValidAlbum();
        album.CoverImage = null;
        ValidationResult result = _validator.Validate(album);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_WhenCoverImageIsNotInImagesList_ReturnsFailure()
    {
        Album album = CreateBaseValidAlbum();
        Image unrelatedImage = CreateValidImage(album, 99, "unrelated");
        album.CoverImage = unrelatedImage;
        ValidationResult result = _validator.Validate(album);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.One.With.Property(nameof(ValidationFailure.ErrorMessage)).EqualTo(ErrorMessages.Album_CoverImage_MustBeInImages));
        });
    }

    [Test]
    public void Validate_WhenCoverImageIsInImagesList_ReturnsSuccess()
    {
        Album album = CreateBaseValidAlbum();
        album.CoverImage = album.Images[1];
        ValidationResult result = _validator.Validate(album);
        Assert.That(result.IsValid, Is.True);
    }
}
