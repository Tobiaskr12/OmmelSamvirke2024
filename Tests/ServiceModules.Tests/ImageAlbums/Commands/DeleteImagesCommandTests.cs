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
public class DeleteImagesCommandTests : ServiceTestBase
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

        var config = GetService<IConfiguration>();
        string conn = config["AzureBlobStorageConnectionString"]!;
        string cont = config["AzureBlobStorageContainerName"]!;
        _blobClient = new BlobContainerClient(conn, cont);
    }

    [Test]
    public async Task Delete_NoImageIds_ReturnsSuccess()
    {
        // Arrange
        var cmd = new DeleteImagesCommand([]);

        // Act
        Result result = await _mediator.Send(cmd);

        // Assert
        Assert.That(result.IsSuccess);
    }

    [Test]
    public async Task Delete_NonExistingImages_ReturnsSuccess()
    {
        // Arrange
        var cmd = new DeleteImagesCommand([999, 1000]);

        // Act
        Result result = await _mediator.Send(cmd);

        // Assert
        Assert.That(result.IsSuccess);
    }

    [Test]
    public async Task Delete_MultipleImages_RemovesEntitiesAndBlobs()
    {
        // Arrange
        Image img1 = await TestDataFactory.CreateAndPersistImageWithAlbumAsync();
        Image img2 = await TestDataFactory.CreateAndPersistImageWithAlbumAsync();
        Image[] images = [img1, img2];
        string[] blobNames1 = TestDataFactory.GetBlobNames(img1);
        string[] blobNames2 = TestDataFactory.GetBlobNames(img2);

        // Upload both sets of blobs
        foreach (string name in blobNames1.Concat(blobNames2))
        {
            await _blobClient.DeleteBlobIfExistsAsync(name);
            using var ms = new MemoryStream([1, 2, 3]);
            await _blobClient.UploadBlobAsync(name, ms);
        }

        var cmd = new DeleteImagesCommand([img1.Id, img2.Id]);

        // Act
        Result result = await _mediator.Send(cmd);

        // Assert success
        Assert.That(result.IsSuccess);

        // DB: no images remain
        foreach (Image img in images)
        {
            List<Image>? rem = (await _imageRepo.FindAsync(i => i.Id == img.Id, readOnly: true)).Value;
            Assert.That(rem, Is.Empty);
        }

        // DB: blob metadata removed
        List<int> allBlobIds = TestDataFactory.GetBlobStorageFiles(img1)
                                              .Concat(TestDataFactory.GetBlobStorageFiles(img2))
                                              .Select(b => b.Id)
                                              .ToList();
        Result<List<BlobStorageFile>> metaRes = await _blobRepo.GetByIdsAsync(allBlobIds, readOnly: true);
        Assert.That(metaRes.IsFailed);

        // Storage: blobs gone
        foreach (string name in blobNames1.Concat(blobNames2))
        {
            bool exists = (await _blobClient.GetBlobClient(name).ExistsAsync()).Value;
            Assert.That(exists, Is.False, $"Blob '{name}' should be deleted");
        }
    }

    [Test]
    public async Task Delete_PartialExistingImages_OnlyDeletesSpecified()
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

        // Act: delete only img1
        var cmd = new DeleteImagesCommand([img1.Id]);
        Result result = await _mediator.Send(cmd);
        Assert.That(result.IsSuccess);

        // DB: img1 gone, img2 remains
        List<Image>? rem1 = (await _imageRepo.FindAsync(i => i.Id == img1.Id, readOnly: true)).Value;
        List<Image>? rem2 = (await _imageRepo.FindAsync(i => i.Id == img2.Id, readOnly: true)).Value;
        Assert.Multiple(() =>
        {
            Assert.That(rem1, Is.Empty);
            Assert.That(rem2, Has.Count.EqualTo(1));
        });

        // Storage: blobs for img1 deleted
        foreach (string name in TestDataFactory.GetBlobNames(img1))
            Assert.That((await _blobClient.GetBlobClient(name).ExistsAsync()).Value, Is.False);

        // Storage: blobs for img2 remain
        foreach (string name in TestDataFactory.GetBlobNames(img2))
            Assert.That((await _blobClient.GetBlobClient(name).ExistsAsync()).Value, Is.True);
    }
}
