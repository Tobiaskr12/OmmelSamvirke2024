using Contracts.DataAccess.Base;
using Contracts.ServiceModules.BlobStorage;
using DomainModules.BlobStorage.Entities;
using FluentResults;
using MediatR;

namespace ServiceModules.Tests.BlobStorage.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetBlobMetadataQueryHandlerIntegrationTests : ServiceTestBase
{
    private IMediator _mediator;
    private IRepository<BlobStorageFile> _repository;
        
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _mediator = GetService<IMediator>();
        _repository = GetService<IRepository<BlobStorageFile>>();
    }
        
    [Test]
    public async Task GetBlobMetadataQuery_ReturnsCorrectMetadata()
    {
        // Arrange
        var blobEntity = new BlobStorageFile
        {
            FileBaseName = "MetadataTestFile",
            FileExtension = "pdf",
            ContentType = "application/pdf"
        };
        blobEntity.SetFileSize(1024);
        Result<BlobStorageFile> addResult = await _repository.AddAsync(blobEntity);
        Assert.That(addResult.IsSuccess, "Blob must be persisted successfully");
            
        var query = new GetBlobMetadataQuery(blobEntity.Id);
            
        // Act
        Result<BlobStorageFile> queryResult = await _mediator.Send(query);
            
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(queryResult.IsSuccess);
            Assert.That(queryResult.Value.Id, Is.EqualTo(blobEntity.Id));
        });
    }
}
