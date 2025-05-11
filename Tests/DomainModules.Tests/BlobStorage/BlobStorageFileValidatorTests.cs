using DomainModules.BlobStorage.Entities;
using DomainModules.BlobStorage.Validators;
using FluentValidation.Results;
using DomainModules.Errors;

namespace DomainModules.Tests.BlobStorage;

[TestFixture, Category("UnitTests")]
public class BlobStorageFileValidatorTests
{
    private BlobStorageFileValidator _validator = null!;
    private BlobStorageFile _validBlob = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new BlobStorageFileValidator();
        _validBlob = new BlobStorageFile
        {
            FileBaseName = "TestDocument",
            FileExtension = "txt",
            ContentType = "text/plain",
            // FileContent is ignored by validator, but size matters
            FileContent = new MemoryStream("This is file content."u8.ToArray()) // Use UTF8 literal
        };
        // Set the size explicitly as FileContent might be null in some scenarios
        _validBlob.SetFileSize(_validBlob.FileContent?.Length ?? 100);
    }

    [Test]
    public void ValidBlobStorageFile_PassesValidation()
    {
        // Act
        ValidationResult result = _validator.Validate(_validBlob);

        // Assert
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void EmptyFileBaseName_FailsValidation()
    {
        // Arrange
        _validBlob.FileBaseName = "";

        // Act
        ValidationResult result = _validator.Validate(_validBlob);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(e =>
                e.ErrorMessage.Equals(ErrorMessages.BlobStorageFile_FileBaseName_NotEmpty)));
        });
    }

    [Test]
    public void EmptyFileExtension_FailsValidation()
    {
        // Arrange
        _validBlob.FileExtension = "";

        // Act
        ValidationResult result = _validator.Validate(_validBlob);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(e =>
                e.ErrorMessage.Equals(ErrorMessages.BlobStorageFile_FileExtension_NotEmpty)));
        });
    }

    [Test]
    public void InvalidFileExtension_FailsValidation()
    {
        // Arrange
        _validBlob.FileExtension = "txt?"; // Invalid character

        // Act
        ValidationResult result = _validator.Validate(_validBlob);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(e =>
                e.ErrorMessage.Equals(ErrorMessages.BlobStorageFile_FileExtension_Invalid)));
        });
    }

    [Test]
    public void ZeroFileSize_FailsValidation()
    {
        // Arrange
        _validBlob.FileContent = null; // Ensure FileContent doesn't interfere
        _validBlob.SetFileSize(0);

        // Act
        ValidationResult result = _validator.Validate(_validBlob);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(e =>
                e.ErrorMessage.Equals(ErrorMessages.BlobStorageFile_FileSize_GreaterThanZero)));
        });
    }

    [Test]
    public void NegativeFileSize_FailsValidation() // Added test case
    {
        // Arrange
        _validBlob.FileContent = null;
        _validBlob.SetFileSize(-100);

        // Act
        ValidationResult result = _validator.Validate(_validBlob);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(e =>
                e.ErrorMessage.Equals(ErrorMessages.BlobStorageFile_FileSize_GreaterThanZero)));
        });
    }

    [Test]
    public void EmptyContentType_FailsValidation()
    {
        // Arrange
        _validBlob.ContentType = "";

        // Act
        ValidationResult result = _validator.Validate(_validBlob);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(e =>
                e.ErrorMessage.Equals(ErrorMessages.BlobStorageFile_ContentType_NotEmpty)));
        });
    }
}
