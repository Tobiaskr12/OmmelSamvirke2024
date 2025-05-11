using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Contracts.DataAccess.Base;
using Contracts.ServiceModules.AlbumImages;
using DomainModules.BlobStorage.Entities;
using DomainModules.ImageAlbums.Entities;
using FluentResults;
using MediatR;

namespace ServiceModules.Tests.ImageAlbums.Commands;

[TestFixture, Category("IntegrationTests")]
public class DeleteImageCommandTests : ServiceTestBase
{
    private IMediator _mediator;
    private IRepository<Image> _imageRepo;
    private IRepository<BlobStorageFile> _blobRepo;
    private BlobContainerClient _blobClient;

    [SetUp]
    public void Init()
    {
        _mediator = GetService<IMediator>();
        _imageRepo = GetService<IRepository<Image>>();
        _blobRepo = GetService<IRepository<BlobStorageFile>>();

        var config  = GetService<IConfiguration>();
        string conn = config["AzureBlobStorageConnectionString"]!;
        string cont = config["AzureBlobStorageContainerName"]!;
        _blobClient = new BlobContainerClient(conn, cont);
    }

    [Test]
    public async Task Delete_NonExistingImage_ReturnsNotFound()
    {
        // Act
        Result result = await _mediator.Send(new DeleteImageCommand(9999));

        // Assert
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Delete_Image_RemovesDbRecordsAndBlobs()
    {
        // Arrange
        Image image = await TestDataFactory.CreateAndPersistImageWithAlbumAsync();
        string[] blobNames = TestDataFactory.GetBlobNames(image);

        // Upload blobs
        foreach (string name in blobNames)
        {
            // Remove blob if it still exits from a previous test run
            await _blobClient.DeleteBlobIfExistsAsync(name);
            
            using var ms = new MemoryStream([1, 2, 3]);
            await _blobClient.UploadBlobAsync(name, ms);
        }

        // Act
        Result result = await _mediator.Send(new DeleteImageCommand(image.Id));

        // Assert DB cleanup
        Assert.That(result.IsSuccess);
        
        // Assert that Image entities have been deleted
        List<Image>? remaining = (await _imageRepo.FindAsync(i => i.Id == image.Id, readOnly: true)).Value;
        Assert.That(remaining, Is.Empty);

        // Assert blob‐metadata have been deleted
        Result<List<BlobStorageFile>> metaRes = await _blobRepo.GetByIdsAsync(TestDataFactory.GetBlobStorageFiles(image).Select(x => x.Id).ToList());
        Assert.That(metaRes.IsFailed);

        // Assert actual blobs have been deleted
        foreach (string name in blobNames)
        {
            bool exists = (await _blobClient.GetBlobClient(name).ExistsAsync()).Value;
            Assert.That(exists, Is.False);
        }
    }

    [Test]
    public async Task Delete_OneOfMultipleImages_LeavesOthersIntact()
    {
        // Arrange two images
        Image img1 = await TestDataFactory.CreateAndPersistImageWithAlbumAsync();
        Image img2 = await TestDataFactory.CreateAndPersistImageWithAlbumAsync();

        // Upload both sets of blobs
        foreach (Image img in new[] { img1, img2 })
        {
            foreach (string name in TestDataFactory.GetBlobNames(img))
            {
                await _blobClient.DeleteBlobIfExistsAsync(name);
                using var ms = new MemoryStream([4, 5, 6]);
                await _blobClient.UploadBlobAsync(name, ms);
            }
        }

        // Act: delete the first image
        Result result = await _mediator.Send(new DeleteImageCommand(img1.Id));
        Assert.That(result.IsSuccess);

        // Assert img1 gone, img2 remains
        List<Image>? rem1 = (await _imageRepo.FindAsync(i => i.Id == img1.Id)).Value;
        List<Image>? rem2 = (await _imageRepo.FindAsync(i => i.Id == img2.Id)).Value;
        Assert.Multiple(() =>
        {
            Assert.That(rem1, Is.Empty);
            Assert.That(rem2, Has.Count.EqualTo(1));
        });

        // Storage blobs for img1 deleted, for img2 remain
        foreach (string name in TestDataFactory.GetBlobNames(img1))
            Assert.That((await _blobClient.GetBlobClient(name).ExistsAsync()).Value, Is.False);

        foreach (string name in TestDataFactory.GetBlobNames(img2))
            Assert.That((await _blobClient.GetBlobClient(name).ExistsAsync()).Value, Is.True);
    }
}
