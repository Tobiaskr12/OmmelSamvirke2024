using Contracts.ServiceModules.BlobStorage;
using DomainModules.BlobStorage.Entities;
using FluentResults;
using MediatR;

namespace ServiceModules.Tests.BlobStorage.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetBulkBlobMetadataQueryHandlerIntegrationTests : ServiceTestBase
{
    private IMediator _mediator;
        
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _mediator = GetService<IMediator>();
    }
        
    [Test]
    public async Task GetBulkBlobMetadataQuery_ReturnsListOfBlobs()
    {
        // Arrange
        var blob1 = new BlobStorageFile
        {
            FileBaseName = "BulkTestFile1",
            FileExtension = "docx",
            ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        };
        var blob2 = new BlobStorageFile
        {
            FileBaseName = "BulkTestFile2",
            FileExtension = "xlsx",
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };
        blob1.SetFileSize(1024);
        blob2.SetFileSize(1024);
        await AddTestData(blob1);
        await AddTestData(blob2);
            
        var query = new GetBulkBlobMetadataQuery([blob1.Id, blob2.Id]);
            
        // Act
        Result<List<BlobStorageFile>> queryResult = await _mediator.Send(query);
            
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(queryResult.IsSuccess);
            Assert.That(queryResult.Value, Is.Not.Empty);
            Assert.That(queryResult.Value.Count, Is.EqualTo(2));
        });
    }
}
