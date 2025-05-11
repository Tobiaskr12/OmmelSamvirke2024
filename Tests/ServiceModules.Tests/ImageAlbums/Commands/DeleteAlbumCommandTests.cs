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
public class DeleteAlbumCommandTests : ServiceTestBase
{
    private IMediator _mediator;
    private IRepository<Album> _albumRepo;
    private IRepository<Image> _imageRepo;
    private IRepository<BlobStorageFile> _blobRepo;
    private BlobContainerClient _blobClient;

    [SetUp]
    public void Init()
    {
        _mediator = GetService<IMediator>();
        _albumRepo = GetService<IRepository<Album>>();
        _imageRepo = GetService<IRepository<Image>>();
        _blobRepo = GetService<IRepository<BlobStorageFile>>();

        var config  = GetService<IConfiguration>();
        string conn = config["AzureBlobStorageConnectionString"]!;
        string cont = config["AzureBlobStorageContainerName"]!;
        _blobClient = new BlobContainerClient(conn, cont);
    }

    [Test]
    public async Task Delete_NonExistingAlbum_ReturnsNotFound()
    {
        Result result = await _mediator.Send(new DeleteAlbumCommand(9999));
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Delete_AlbumWithoutImages_RemovesAlbumOnly()
    {
        // Arrange
        var album = new Album { Name = "EmptyAlbum", Description = "No images", Images = [] };
        await AddTestData(album);

        // Act
        Result result = await _mediator.Send(new DeleteAlbumCommand(album.Id));

        // Assert
        Assert.That(result.IsSuccess);
        Result<Album> getResult = await _albumRepo.GetByIdAsync(album.Id, readOnly: true);
        Assert.That(getResult.IsFailed);
    }

    [Test]
    public async Task Delete_AlbumWithImages_RemovesEntitiesAndBlobs()
    {
        // Arrange
        Image image = await TestDataFactory.CreateAndPersistImageWithAlbumAsync();
        
        BlobStorageFile[] blobs = TestDataFactory.GetBlobStorageFiles(image);
        string[] blobNames = TestDataFactory.GetBlobNames(image);

        // Upload fresh copies
        foreach (string name in blobNames)
        {
            await _blobClient.DeleteBlobIfExistsAsync(name);
            using var ms = new MemoryStream([1, 2, 3]);
            await _blobClient.UploadBlobAsync(name, ms);
        }

        // Act
        Result result = await _mediator.Send(new DeleteAlbumCommand(image.Album.Id));

        // Assert DB cleanup
        Assert.That(result.IsSuccess);

        Result<Album> getAlbumResult = await _albumRepo.GetByIdAsync(image.Album.Id, readOnly: true);
        Assert.That(getAlbumResult.IsFailed, "Album should be gone");

        List<Image>? remainingImages = (await _imageRepo.FindAsync(i => i.Album.Id == image.Album.Id, readOnly: true)).Value;
        Assert.That(remainingImages, Is.Empty, "No images should remain");

        // Blob-metadata gone
        Result<List<BlobStorageFile>> getBlobsResult = await _blobRepo.GetByIdsAsync(blobs.Select(b => b.Id).ToList(), readOnly: true);
        Assert.That(getBlobsResult.IsFailed, "Blob metadata should be deleted");

        // Storage cleanup
        foreach (string name in blobNames)
        {
            bool exists = (await _blobClient.GetBlobClient(name).ExistsAsync()).Value;
            Assert.That(exists, Is.False, $"Blob '{name}' should be deleted");
        }
    }
}
