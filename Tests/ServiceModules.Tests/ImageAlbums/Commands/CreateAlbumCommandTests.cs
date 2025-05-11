using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Contracts.DataAccess.Base;
using Contracts.ServiceModules.AlbumImages;
using DomainModules.ImageAlbums.Entities;
using ServiceModules.Errors;
using FluentResults;

namespace ServiceModules.Tests.ImageAlbums.Commands;

[TestFixture, Category("IntegrationTests")]
public class CreateAlbumCommandTests : ServiceTestBase
{
    private BlobContainerClient GetBlobContainerClient()
    {
        var config = GetService<IConfiguration>();
        string connectionString = config["AzureBlobStorageConnectionString"] ?? throw new Exception("AzureBlobStorageConnectionString not found in config");
        string containerName = config["AzureBlobStorageContainerName"] ?? throw new Exception("AzureBlobStorageContainerName not found in config");
        return new BlobContainerClient(connectionString, containerName);
    }

    [Test]
    public async Task CreateAlbum_WithValidImages_ReturnsSuccess_AndUploadsBlobs()
    {
        // Arrange
        string testImagePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "ImageAlbums", "TestImages", "TestImage1.jpg");
        byte[] fileContent = await File.ReadAllBytesAsync(testImagePath);

        var imageInput = new ImageUploadInput(
            OriginalFileName: "test.jpg",
            FileContent: fileContent,
            ContentType: "image/jpeg",
            Metadata: null
        );

        var cmd = new CreateAlbumCommand(
            Name: "Summer Festival 2024",
            Description: "Pictures from a great day",
            Images: [imageInput],
            DefaultMetadataForAllImages: null
        );

        // Act
        Result<Album> result = await GlobalTestSetup.Mediator.Send(cmd);

        // Basic assertions
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.Images, Has.Count.EqualTo(1));
            Assert.That(result.Value.CoverImage, Is.Not.Null);
            Assert.That(result.Value.Id, Is.GreaterThan(0));
        });

        // Assert blobs exist in remote storage
        BlobContainerClient blobClient = GetBlobContainerClient();
        foreach (Image image in result.Value.Images)
        {
            string[] blobNames =
            [
                $"{image.OriginalBlobStorageFile.FileBaseName}-{image.OriginalBlobStorageFile.Id}.{image.OriginalBlobStorageFile.FileExtension}",
                $"{image.DefaultBlobStorageFile.FileBaseName}-{image.DefaultBlobStorageFile.Id}.{image.DefaultBlobStorageFile.FileExtension}",
                $"{image.ThumbnailBlobStorageFile.FileBaseName}-{image.ThumbnailBlobStorageFile.Id}.{image.ThumbnailBlobStorageFile.FileExtension}"
            ];

            foreach (string blobName in blobNames)
            {
                bool exists = (await blobClient.GetBlobClient(blobName).ExistsAsync()).Value;
                Assert.That(exists);
            }
        }
    }

    [Test]
    public async Task CreateAlbum_NoImagesProvided_ReturnsFailure()
    {
        // Arrange
        var cmd = new CreateAlbumCommand(
            Name: "Empty Album",
            Description: null,
            Images: [],
            DefaultMetadataForAllImages: null
        );

        // Act
        Result<Album> result = await GlobalTestSetup.Mediator.Send(cmd);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors.Any(e =>
                e.Message.Equals(ErrorMessages.Album_Images_InvalidSize)));
        });
    }

    [Test]
    public async Task CreateAlbum_WithMultipleImages_ReturnsSuccess_AndUploadsAllBlobs()
    {
        // Arrange
        string imagesDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "ImageAlbums", "TestImages");
        List<string> filePaths = Directory.GetFiles(imagesDir, "*.jpg").Take(3).ToList();
        List<ImageUploadInput> inputs = filePaths.Select(path => new ImageUploadInput(
            OriginalFileName: Path.GetFileName(path),
            FileContent: File.ReadAllBytes(path),
            ContentType: "image/png",
            Metadata: null
        )).ToList();

        var cmd = new CreateAlbumCommand(
            Name: "Gallery",
            Description: null,
            Images: inputs,
            DefaultMetadataForAllImages: null
        );

        // Act
        Result<Album> result = await GlobalTestSetup.Mediator.Send(cmd);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.Images, Has.Count.EqualTo(inputs.Count));
        });

        BlobContainerClient blobClient = GetBlobContainerClient();
        foreach (Image image in result.Value.Images)
        {
            string[] blobNames =
            [
                $"{image.OriginalBlobStorageFile.FileBaseName}-{image.OriginalBlobStorageFile.Id}.{image.OriginalBlobStorageFile.FileExtension}",
                $"{image.DefaultBlobStorageFile.FileBaseName}-{image.DefaultBlobStorageFile.Id}.{image.DefaultBlobStorageFile.FileExtension}",
                $"{image.ThumbnailBlobStorageFile.FileBaseName}-{image.ThumbnailBlobStorageFile.Id}.{image.ThumbnailBlobStorageFile.FileExtension}"
            ];

            foreach (string blobName in blobNames)
            {
                Assert.That((await blobClient.GetBlobClient(blobName).ExistsAsync()).Value, Is.True);
            }
        }
    }

    [Test]
    public async Task CreateAlbum_WithDefaultMetadata_AppliesMetadataToAllImages()
    {
        // Arrange
        string testImagePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "ImageAlbums", "TestImages", "TestImage1.jpg");
        byte[] fileContent = await File.ReadAllBytesAsync(testImagePath);

        var imageInput = new ImageUploadInput(
            OriginalFileName: "test.jpg",
            FileContent: fileContent,
            ContentType: "image/jpeg",
            Metadata: null
        );

        var defaultMeta = new ImageMetadataInput
        {
            DateTaken = new DateTime(2024, 6, 1),
            Location = "Copenhagen",
            PhotographerName = "Alice",
            Title = "Event",
            Description = "Description"
        };

        var cmd = new CreateAlbumCommand(
            Name: "MetadataAlbum",
            Description: null,
            Images: [imageInput],
            DefaultMetadataForAllImages: defaultMeta
        );

        // Act
        Result<Album> createResult = await GlobalTestSetup.Mediator.Send(cmd);
        Assert.That(createResult.IsSuccess);

        // Retrieve from repository to verify metadata persisted
        var repo = GetService<IRepository<Album>>();
        Album albumInDb = (await repo.GetByIdAsync(createResult.Value.Id)).Value;

        foreach (Image img in albumInDb.Images)
        {
            Assert.Multiple(() =>
            {
                Assert.That(img.DateTaken, Is.EqualTo(defaultMeta.DateTaken));
                Assert.That(img.Location, Is.EqualTo(defaultMeta.Location));
                Assert.That(img.PhotographerName, Is.EqualTo(defaultMeta.PhotographerName));
                Assert.That(img.Title, Is.EqualTo(defaultMeta.Title));
                Assert.That(img.Description, Is.EqualTo(defaultMeta.Description));
            });
        }
    }
    
    [Test]
    public async Task CreateAlbum_WithPngImage_ReturnsSuccess_AndUploadsBlobs()
    {
        // Arrange
        string testImagePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "ImageAlbums", "TestImages", "TestImage4.png");
        byte[] fileContent = await File.ReadAllBytesAsync(testImagePath);

        var imageInput = new ImageUploadInput(
            OriginalFileName: "TestImage4.png",
            FileContent: fileContent,
            ContentType: "image/png",
            Metadata: null
        );

        var cmd = new CreateAlbumCommand(
            Name: "PNGAlbum",
            Description: "PNG Test",
            Images: [imageInput],
            DefaultMetadataForAllImages: null
        );

        // Act
        Result<Album> result = await GlobalTestSetup.Mediator.Send(cmd);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.Images, Has.Count.EqualTo(1));
        });

        BlobContainerClient blobClient = GetBlobContainerClient();
        foreach (Image image in result.Value.Images)
        {
            string[] blobNames =
            [
                $"{image.OriginalBlobStorageFile.FileBaseName}-{image.OriginalBlobStorageFile.Id}.{image.OriginalBlobStorageFile.FileExtension}",
                $"{image.DefaultBlobStorageFile.FileBaseName}-{image.DefaultBlobStorageFile.Id}.{image.DefaultBlobStorageFile.FileExtension}",
                $"{image.ThumbnailBlobStorageFile.FileBaseName}-{image.ThumbnailBlobStorageFile.Id}.{image.ThumbnailBlobStorageFile.FileExtension}"
            ];

            foreach (string blobName in blobNames)
            {
                Assert.That((await blobClient.GetBlobClient(blobName).ExistsAsync()).Value, Is.True);
            }
        }
    }
}
