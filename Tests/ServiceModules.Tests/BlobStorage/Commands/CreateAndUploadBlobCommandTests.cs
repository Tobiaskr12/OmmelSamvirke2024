using Contracts.DataAccess.Base;
using Contracts.ServiceModules.BlobStorage;
using DomainModules.BlobStorage.Entities;
using FluentResults;
using System.Text;

namespace ServiceModules.Tests.BlobStorage.Commands;

[TestFixture, Category("IntegrationTests")]
public class CreateAndUploadBlobCommandTests : ServiceTestBase
{
    [Test]
    public async Task Handle_WithValidData_ShouldCreateMetadataAndUploadBlob_AndReturnSuccess()
    {
        // Arrange
        const string fileBaseName = "TestDocumentNunit";
        const string fileExtension = "txt";
        const string contentType = "text/plain";
        const string fileContentString = "NUnit test file content.";
        byte[] fileContentBytes = Encoding.UTF8.GetBytes(fileContentString);
        await using var fileStream = new MemoryStream(fileContentBytes);

        var command = new CreateAndUploadBlobCommand(
            FileContent: fileStream,
            FileBaseName: fileBaseName,
            FileExtension: fileExtension,
            ContentType: contentType
        );

        // Act
        Result<BlobStorageFile> result = await GlobalTestSetup.Mediator.Send(command);
        BlobStorageFile createdMetadata = result.Value;

        // Assert
        // Check command result
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(createdMetadata, Is.Not.Null);
            Assert.That(createdMetadata.Id, Is.GreaterThan(0));
            Assert.That(createdMetadata.FileBaseName, Is.EqualTo(fileBaseName));
            Assert.That(createdMetadata.FileExtension, Is.EqualTo(fileExtension));
            Assert.That(createdMetadata.ContentType, Is.EqualTo(contentType));
            Assert.That(createdMetadata.FileSizeInBytes, Is.EqualTo(fileContentBytes.Length));
        });

        // 2. Verify metadata in DB
        var repo = GetService<IRepository<BlobStorageFile>>();
        Result<BlobStorageFile> dbResult = await repo.GetByIdAsync(createdMetadata.Id);
        Assert.Multiple(() =>
        {
            Assert.That(dbResult.IsSuccess, Is.True);
            Assert.That(dbResult.Value, Is.Not.Null);
            Assert.That(dbResult.Value.FileBaseName, Is.EqualTo(fileBaseName));
        });

        // 3. Verify blob exists in storage
        string expectedBlobName = $"{fileBaseName}-{createdMetadata.Id}.{fileExtension}";
        bool blobExists = await DoesBlobExistAsync(expectedBlobName);
        Assert.That(blobExists, Is.True, $"Blob '{expectedBlobName}' should exist in storage.");
    }

    [Test]
    public async Task Handle_WithZeroLengthStream_ShouldFail()
    {
        // Arrange
        const string fileBaseName = "EmptyFileNunit";
        const string fileExtension = "dat";
        const string contentType = "application/octet-stream";
        await using var fileStream = new MemoryStream();

        var command = new CreateAndUploadBlobCommand(
            FileContent: fileStream,
            FileBaseName: fileBaseName,
            FileExtension: fileExtension,
            ContentType: contentType
        );

        // Act
        Result<BlobStorageFile> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors, Is.Not.Empty);
        });
    }

     [Test]
    public async Task Handle_WithEmptyBaseName_ShouldFail()
    {
        // Arrange
        const string fileContentString = "Some content for Nunit.";
        byte[] fileContentBytes = Encoding.UTF8.GetBytes(fileContentString);
        await using var fileStream = new MemoryStream(fileContentBytes);

        var command = new CreateAndUploadBlobCommand(
            FileContent: fileStream,
            FileBaseName: "", // Invalid
            FileExtension: "txt",
            ContentType: "text/plain"
        );

        // Act
        Result<BlobStorageFile> result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors, Is.Not.Empty);
        });
    }
}
