using DomainModules.Errors;
using DomainModules.Events.Entities;
using DomainModules.Events.Validators;
using FluentValidation.TestHelper;

namespace DomainModules.Tests.Events;

[TestFixture, Category("UnitTests")]
public class EventRemoteFileValidatorTests
{
    private EventRemoteFileValidator _validator;
    private EventRemoteFile _validRemoteFile;

    [SetUp]
    public void SetUp()
    {
        _validator = new EventRemoteFileValidator();
        _validRemoteFile = new EventRemoteFile
        {
            FileName = "document.pdf",
            FileSizeBytes = 1024,
            FileType = "application/pdf",
            Url = "https://example.com/document.pdf"
        };
    }

    [Test]
    public void ValidRemoteFile_PassesValidation()
    {
        TestValidationResult<EventRemoteFile>? result = _validator.TestValidate(_validRemoteFile);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void EmptyFileName_FailsValidation()
    {
        _validRemoteFile.FileName = "";
        TestValidationResult<EventRemoteFile>? result = _validator.TestValidate(_validRemoteFile);
        result.ShouldHaveValidationErrorFor(x => x.FileName)
              .WithErrorMessage(ErrorMessages.EventRemoteFile_FileName_NotEmpty);
    }

    [Test]
    public void FileNameTooLong_FailsValidation()
    {
        _validRemoteFile.FileName = new string('a', 256);
        TestValidationResult<EventRemoteFile>? result = _validator.TestValidate(_validRemoteFile);
        result.ShouldHaveValidationErrorFor(x => x.FileName)
              .WithErrorMessage(ErrorMessages.EventRemoteFile_FileName_InvalidLength);
    }

    [Test]
    public void InvalidFileSize_FailsValidation()
    {
        _validRemoteFile.FileSizeBytes = 0;
        TestValidationResult<EventRemoteFile>? result = _validator.TestValidate(_validRemoteFile);
        result.ShouldHaveValidationErrorFor(x => x.FileSizeBytes)
              .WithErrorMessage(ErrorMessages.EventRemoteFile_FileSize_Invalid);
    }

    [Test]
    public void EmptyFileType_FailsValidation()
    {
        _validRemoteFile.FileType = "";
        TestValidationResult<EventRemoteFile>? result = _validator.TestValidate(_validRemoteFile);
        result.ShouldHaveValidationErrorFor(x => x.FileType)
              .WithErrorMessage(ErrorMessages.EventRemoteFile_FileType_NotEmpty);
    }

    [Test]
    public void FileTypeTooLong_FailsValidation()
    {
        _validRemoteFile.FileType = new string('a', 101);
        TestValidationResult<EventRemoteFile>? result = _validator.TestValidate(_validRemoteFile);
        result.ShouldHaveValidationErrorFor(x => x.FileType)
              .WithErrorMessage(ErrorMessages.EventRemoteFile_FileType_InvalidLength);
    }

    [Test]
    public void EmptyUrl_FailsValidation()
    {
        _validRemoteFile.Url = "";
        TestValidationResult<EventRemoteFile>? result = _validator.TestValidate(_validRemoteFile);
        result.ShouldHaveValidationErrorFor(x => x.Url)
              .WithErrorMessage(ErrorMessages.EventRemoteFile_Url_NotEmpty);
    }

    [Test]
    public void InvalidUrl_FailsValidation()
    {
        _validRemoteFile.Url = "not-a-valid-url";
        TestValidationResult<EventRemoteFile>? result = _validator.TestValidate(_validRemoteFile);
        result.ShouldHaveValidationErrorFor(x => x.Url)
              .WithErrorMessage(ErrorMessages.EventRemoteFile_Url_Invalid);
    }
}
