using DomainModules.BlobStorage.Entities;
using DomainModules.BlobStorage.Validators;
using FluentValidation.Results;
using DomainModules.Errors;

namespace DomainModules.Tests.BlobStorage;

[TestFixture, Category("UnitTests")]
public class BlobStorageFileValidatorTests
{
    private BlobStorageFileValidator _validator;
    private BlobStorageFile _validBlob;

    [SetUp]
    public void SetUp()
    {
        _validator = new BlobStorageFileValidator();
        _validBlob = new BlobStorageFile
        {
            FileBaseName = "TestFile",
            FileExtension = "pdf",
            ContentType = "application/pdf",
            // Simulate file content with a length of 1024 bytes.
            FileContent = new MemoryStream(new byte[1024])
        };
        _validBlob.SetFileSize(1024);
    }

    [Test]
    public void ValidBlobStorageFile_PassesValidation()
    {
        ValidationResult result = _validator.Validate(_validBlob);
        
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void EmptyFileBaseName_FailsValidation()
    {
        _validBlob.FileBaseName = "";
        ValidationResult result = _validator.Validate(_validBlob);
        
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
        _validBlob.FileExtension = "";
        ValidationResult result = _validator.Validate(_validBlob);
        
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
        _validBlob.FileExtension = "pdf!";
        ValidationResult result = _validator.Validate(_validBlob);
        
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
        _validBlob.FileContent = null;
        _validBlob.SetFileSize(0);
        ValidationResult result = _validator.Validate(_validBlob);
        
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
        _validBlob.ContentType = "";
        ValidationResult result = _validator.Validate(_validBlob);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(e =>
                e.ErrorMessage.Equals(ErrorMessages.BlobStorageFile_ContentType_NotEmpty)));
        });
    }

    [Test]
    public void EmptyBlobGuid_FailsValidation()
    {
        _validBlob = new BlobStorageFile
        {
            BlobGuid = Guid.Empty,
            FileBaseName = "TestFile",
            FileExtension = "pdf",
            ContentType = "application/pdf",
            FileContent = new MemoryStream(new byte[1024])
        };
        _validBlob.SetFileSize(1024);
        ValidationResult result = _validator.Validate(_validBlob);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(e =>
                e.ErrorMessage.Equals(ErrorMessages.BlobStorageFile_BlobGuid_NotEmpty)));
        });
    }
}
