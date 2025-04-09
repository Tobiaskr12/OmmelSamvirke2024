using Contracts.ServiceModules.BlobStorage;
using DomainModules.BlobStorage.Entities;
using FluentResults;
using MediatR;

namespace ServiceModules.Tests.BlobStorage.Commands;

[TestFixture, Category("IntegrationTests")]
public class UploadBlobCommandHandlerIntegrationTests : ServiceTestBase
{
    private IMediator _mediator;
        
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _mediator = GetService<IMediator>();
    }
        
    [Test]
    public async Task UploadBlobCommand_ReturnsSuccessAndPersistsBlob()
    {
        // Arrange: Create a blob entity and a memory stream with dummy data
        byte[] dummyData = new byte[1024];
        new Random().NextBytes(dummyData);
        using var fileContent = new MemoryStream(dummyData);
            
        var blobEntity = new BlobStorageFile
        {
            FileBaseName = "IntegrationUploadTest",
            FileExtension = "txt",
            ContentType = "text/plain"
        };
            
        var command = new UploadBlobCommand(fileContent, blobEntity);
            
        // Act: Send the upload command
        Result<BlobStorageFile> result = await _mediator.Send(command);
            
        // Assert: Verify that the upload succeeded and an ID was assigned
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Id, Is.GreaterThan(0));
        });
    }
}
