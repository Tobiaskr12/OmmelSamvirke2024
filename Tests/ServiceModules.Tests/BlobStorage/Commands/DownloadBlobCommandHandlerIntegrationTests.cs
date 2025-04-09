using Contracts.ServiceModules.BlobStorage;
using DomainModules.BlobStorage.Entities;
using FluentResults;
using MediatR;

namespace ServiceModules.Tests.BlobStorage.Commands;

[TestFixture, Category("IntegrationTests")]
public class DownloadBlobCommandHandlerIntegrationTests : ServiceTestBase
{
    private IMediator _mediator;
        
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _mediator = GetService<IMediator>();
    }
        
    [Test]
    public async Task DownloadBlobCommand_ReturnsBlobWithContent()
    {
        // Arrange: First, upload a blob so it exists in the system.
        byte[] dummyData = new byte[2048];
        new Random().NextBytes(dummyData);
        using var fileContent = new MemoryStream(dummyData);
            
        var blobEntity = new BlobStorageFile
        {
            FileBaseName = "IntegrationDownloadTest",
            FileExtension = "txt",
            ContentType = "text/plain"
        };
            
        var uploadCommand = new UploadBlobCommand(fileContent, blobEntity);
        Result<BlobStorageFile> uploadResult = await _mediator.Send(uploadCommand);
        Assert.That(uploadResult.IsSuccess, "Pre-upload must succeed");
            
        // Act: Download the blob using its persisted entity.
        var downloadCommand = new DownloadBlobCommand(blobEntity);
        Result<BlobStorageFile> downloadResult = await _mediator.Send(downloadCommand);
            
        // Assert: Verify that the downloaded blob includes binary content.
        Assert.Multiple(() =>
        {
            Assert.That(downloadResult.IsSuccess);
            Assert.That(downloadResult.Value.FileContent, Is.Not.Null);
            Assert.That(downloadResult.Value.FileSizeInBytes, Is.EqualTo(fileContent.Length));
        });
    }
}
