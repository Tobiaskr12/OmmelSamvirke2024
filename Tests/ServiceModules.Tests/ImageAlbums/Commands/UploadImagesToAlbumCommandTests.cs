using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Contracts.DataAccess.Base;
using Contracts.ServiceModules.AlbumImages;
using DomainModules.ImageAlbums.Entities;
using DomainModules.BlobStorage.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Tests.ImageAlbums.Commands;

[TestFixture, Category("IntegrationTests")]
public class UploadImagesToAlbumCommandTests : ServiceTestBase
{
    private IMediator _mediator;
    private IRepository<BlobStorageFile> _blobRepo;
    private BlobContainerClient _blobClient;

    [SetUp]
    public void Init()
    {
        _mediator = GetService<IMediator>();
        GetService<IRepository<Album>>();
        _blobRepo = GetService<IRepository<BlobStorageFile>>();

        var config = GetService<IConfiguration>();
        string conn = config["AzureBlobStorageConnectionString"]!;
        string cont = config["AzureBlobStorageContainerName"]!;
        _blobClient = new BlobContainerClient(conn, cont);
    }

    [Test]
    public async Task Upload_NoImagesProvided_ReturnsFailure()
    {
        // Arrange
        int albumId = await BuildAndPersistAlbumAsync();

        // Act
        var cmd = new UploadImagesToAlbumCommand(
            AlbumId: albumId,
            Images: [],
            DefaultMetadataForAllImages: null
        );
        Result<List<Image>> result = await _mediator.Send(cmd);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors.Select(e => e.Message), Does.Contain(ErrorMessages.Album_Images_MustProvideAtLeastOne));
        });
    }

    [Test]
    public async Task Upload_NonExistingAlbum_ReturnsNotFound()
    {
        // Arrange
        var dummyInput = new ImageUploadInput(
            OriginalFileName: "foo.jpg",
            FileContent: [1,2,3],
            ContentType: "image/jpeg",
            Metadata: null
        );

        // Act
        var cmd = new UploadImagesToAlbumCommand(
            AlbumId: -99,
            Images: [dummyInput],
            DefaultMetadataForAllImages: null
        );
        Result<List<Image>> result = await _mediator.Send(cmd);

        // Assert
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Upload_SingleImage_SuccessAndUploadsBlobs()
    {
        // Arrange
        int albumId = await BuildAndPersistAlbumAsync();

        string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "ImageAlbums", "TestImages", "TestImage1.jpg");
        byte[] content = await File.ReadAllBytesAsync(path);

        var input = new ImageUploadInput(
            OriginalFileName: "TestImage1.jpg",
            FileContent: content,
            ContentType: "image/jpeg",
            Metadata: null
        );

        // Act
        Result<List<Image>> result = await _mediator.Send(new UploadImagesToAlbumCommand(
            AlbumId: albumId,
            Images: [input],
            DefaultMetadataForAllImages: null
        ));

        // Assert
        Assert.That(result.IsSuccess);
        List<Image>? created = result.Value;
        Assert.That(created, Has.Count.EqualTo(1));

        // Check DB blobs persisted
        List<int> blobIds = created
            .SelectMany(TestDataFactory.GetBlobStorageFiles)
            .Select(b => b.Id)
            .ToList();
        Result<List<BlobStorageFile>> metaRes = await _blobRepo.GetByIdsAsync(blobIds, readOnly: true);
        
        Assert.Multiple(() =>
        {
            Assert.That(metaRes.IsSuccess);
            Assert.That(metaRes.Value, Has.Count.EqualTo(3));
        });

        // Check actual blob storage
        foreach (Image img in created)
        {
            foreach (string blobName in TestDataFactory.GetBlobNames(img))
            {
                bool exists = (await _blobClient.GetBlobClient(blobName).ExistsAsync()).Value;
                Assert.That(exists, Is.True, $"Expected blob '{blobName}' to exist");
            }
        }
    }

    [Test]
    public async Task Upload_MultipleImages_SuccessAndAllUploaded()
    {
        // Arrange
        int albumId = await BuildAndPersistAlbumAsync();

        string imagesDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "ImageAlbums", "TestImages");
        List<string> jpgPaths = Directory.GetFiles(imagesDir, "TestImage*.jpg").Take(2).ToList();

        List<ImageUploadInput> inputs = jpgPaths.Select(p => new ImageUploadInput(
            OriginalFileName: Path.GetFileName(p),
            FileContent: File.ReadAllBytes(p),
            ContentType: "image/jpeg",
            Metadata: null
        )).ToList();

        // Act
        var cmd = new UploadImagesToAlbumCommand(
            AlbumId: albumId,
            Images: inputs,
            DefaultMetadataForAllImages: null
        );
        Result<List<Image>> result = await _mediator.Send(cmd);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Has.Count.EqualTo(inputs.Count));
        });

        // Are all blobs in storage?
        foreach (Image img in result.Value)
        {
            foreach (string blobName in TestDataFactory.GetBlobNames(img))
            {
                bool exists = (await _blobClient.GetBlobClient(blobName).ExistsAsync()).Value;
                Assert.That(exists, Is.True);
            }
        }
    }

    // Helper to create an empty album
    private static async Task<int> BuildAndPersistAlbumAsync()
    {
        var album = new Album { Name = "ImgUploadAlbum", Images = [], CoverImage = null };
        await AddTestData(album);
        return album.Id;
    }
}
