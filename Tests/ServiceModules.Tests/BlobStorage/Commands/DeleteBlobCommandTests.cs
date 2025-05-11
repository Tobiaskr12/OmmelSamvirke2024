using Contracts.DataAccess.Base;
using Contracts.ServiceModules.BlobStorage;
using DomainModules.BlobStorage.Entities;
using FluentResults;

namespace ServiceModules.Tests.BlobStorage.Commands;

[TestFixture, Category("IntegrationTests")]
public class DeleteBlobCommandTests : ServiceTestBase
{
    private BlobStorageFile _existingBlobMetadata = null!;
    private string _existingBlobName = null!;

    // Helper to create a blob before each relevant test
    private async Task CreateTestBlobAsync()
    {
        const string fileBaseName = "ToDeleteNunit";
        const string fileExtension = "dmp";
        const string contentType = "application/octet-stream";
        byte[] fileContentBytes = "NUnit temporary file content"u8.ToArray();
        await using var fileStream = new MemoryStream(fileContentBytes);

        var createCommand = new CreateAndUploadBlobCommand(fileStream, fileBaseName, fileExtension, contentType);
        Result<BlobStorageFile> createResult = await GlobalTestSetup.Mediator.Send(createCommand);
        
        Assert.That(createResult.IsSuccess, Is.True);
        _existingBlobMetadata = createResult.Value;
        _existingBlobName = $"{fileBaseName}-{_existingBlobMetadata.Id}.{fileExtension}";

        bool exists = await DoesBlobExistAsync(_existingBlobName);
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task Handle_WithExistingBlob_ShouldDeleteBlobAndMetadata_AndReturnSuccess()
    {
        // Arrange
        await CreateTestBlobAsync();
        var command = new DeleteBlobCommand(_existingBlobMetadata.Id);

        // Act
        Result result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        // 1. Check command result
        Assert.That(result.IsSuccess, Is.True);

        // 2. Verify metadata is deleted from DB
        var repo = GetService<IRepository<BlobStorageFile>>();
        Result<BlobStorageFile> dbResult = await repo.GetByIdAsync(_existingBlobMetadata.Id);
        Assert.Multiple(() =>
        {
            Assert.That(dbResult.IsSuccess, Is.False);
            Assert.That(dbResult.Errors, Is.Not.Empty);
        });

        // 3. Verify blob is deleted from storage
        bool blobExists = await DoesBlobExistAsync(_existingBlobName);
        Assert.That(blobExists, Is.False);
    }

    [Test]
    public async Task Handle_WithNonExistentMetadataId_ShouldReturnFailure() // Renamed for clarity
    {
        // Arrange
        const int nonExistentId = 999999;
        var command = new DeleteBlobCommand(nonExistentId);

        // Act
        Result result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors, Is.Not.Empty);
        });
    }

     [Test]
    public async Task Handle_WithExistingMetadataButMissingBlob_ShouldDeleteMetadataAndReturnSuccess()
    {
        // Arrange
        var metadataOnly = new BlobStorageFile
        {
            FileBaseName = "MetaOnlyNunit",
            FileExtension = "info",
            ContentType = "text/plain"
        };
        metadataOnly.SetFileSize(10);
        await AddTestData(metadataOnly);
        Assert.That(metadataOnly.Id, Is.GreaterThan(0));
        
        string nonExistentBlobName = $"{metadataOnly.FileBaseName}-{metadataOnly.Id}.{metadataOnly.FileExtension}";
        bool exists = await DoesBlobExistAsync(nonExistentBlobName);
        Assert.That(exists, Is.False);

        var command = new DeleteBlobCommand(metadataOnly.Id);

        // Act
        Result result = await GlobalTestSetup.Mediator.Send(command);

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        // Verify metadata is deleted from DB
        var repo = GetService<IRepository<BlobStorageFile>>();
        Result<BlobStorageFile> dbResult = await repo.GetByIdAsync(metadataOnly.Id);
        Assert.Multiple(() =>
        {
            Assert.That(dbResult.IsSuccess, Is.False);
            Assert.That(dbResult.Errors, Is.Not.Empty);
        });
    }
}
