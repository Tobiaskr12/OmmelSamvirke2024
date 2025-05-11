using Azure.Storage.Blobs;
using Contracts.DataAccess;
using Contracts.Infrastructure.BlobStorage;
using Contracts.ServiceModules.AlbumImages;
using DomainModules.BlobStorage.Entities;
using DomainModules.ImageAlbums.Entities;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace ServiceModules.Tests.ImageAlbums.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetAlbumImagesPaginatedQueryTests : ServiceTestBase
{
    private IMediator _mediator;
    private IBlobStorageService _blobService;
    private BlobContainerClient _blobClient;

    [SetUp]
    public void Init()
    {
        _mediator = GetService<IMediator>();
        _blobService = GetService<IBlobStorageService>();

        var config = GetService<IConfiguration>();
        string connString = config["AzureBlobStorageConnectionString"]!;
        string container = config["AzureBlobStorageContainerName"]!;
        _blobClient = new BlobContainerClient(connString, container);
    }

    [Test]
    public async Task GetImages_EmptyAlbum_ReturnsEmptyPage()
    {
        // Arrange
        var album = new Album { Name = "Empty", Images = [], CoverImage = null };
        await AddTestData(album);

        // Act
        var query = new GetAlbumImagesPaginatedQuery(
            AlbumId: album.Id,
            Page: 1,
            PageSize: 5,
            SortBy: SortDirection.OldestFirst
        );
        Result<PaginatedResult<AlbumImageDto>> result = await _mediator.Send(query);

        // Assert
        Assert.That(result.IsSuccess);
        
        PaginatedResult<AlbumImageDto>? page = result.Value;
        Assert.Multiple(() =>
        {
            Assert.That(page.ItemsCount, Is.EqualTo(0));
            Assert.That(page.Items, Is.Empty);
            Assert.That(page.Page, Is.EqualTo(1));
            Assert.That(page.PageSize, Is.EqualTo(5));
        });
    }

    [Test]
    public async Task GetImages_SingleImage_ReturnsCorrectDto()
    {
        // Arrange
        var album = new Album { Name = "OneImg", Images = [], CoverImage = null };
        await AddTestData(album);

        Image img = await TestDataFactory.CreateAndPersistImageWithAlbumAsync(album);

        // Upload to Blob Storage
        foreach (BlobStorageFile b in TestDataFactory.GetBlobStorageFiles(img))
        {
            string name = $"{b.Id}.{b.FileExtension}";
            await _blobClient.DeleteBlobIfExistsAsync(name);
            using var ms = new MemoryStream([9, 9, 9]);
            await _blobClient.UploadBlobAsync(name, ms);
        }

        // Act
        var query = new GetAlbumImagesPaginatedQuery(
            AlbumId: album.Id,
            Page: 1,
            PageSize: 10,
            SortBy: SortDirection.OldestFirst
        );
        Result<PaginatedResult<AlbumImageDto>> result = await _mediator.Send(query);

        // Assert
        Assert.That(result.IsSuccess);
        
        PaginatedResult<AlbumImageDto>? page = result.Value;
        Assert.Multiple(() =>
        {
            Assert.That(page.ItemsCount, Is.EqualTo(1));
            Assert.That(page.Items, Has.Count.EqualTo(1));
        });

        AlbumImageDto dto = page.Items.First();
        string baseUrl = _blobService.GetPublicBlobBaseUrl();
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(img.Id));
            Assert.That(dto.ThumbnailUrl, Does.StartWith(baseUrl).And.Contain($"{img.ThumbnailBlobStorageFile.FileBaseName}"));
            Assert.That(dto.DefaultImageUrl, Does.StartWith(baseUrl).And.Contain($"{img.DefaultBlobStorageFile.FileBaseName}"));
            Assert.That(dto.Title, Is.EqualTo(img.Title));
            Assert.That(dto.Description, Is.EqualTo(img.Description));
        });
    }

    [Test]
    public async Task GetImages_MultipleImages_PaginatesCorrectly()
    {
        // Arrange
        var album = new Album { Name = "ManyImages", Images = [], CoverImage = null };
        await AddTestData(album);

        var created = new List<Image>();
        for (int i = 0; i < 5; i++)
        {
            Image img = await TestDataFactory.CreateAndPersistImageWithAlbumAsync(album);
            created.Add(img);

            // Upload each image’s blobs
            foreach (BlobStorageFile b in TestDataFactory.GetBlobStorageFiles(img))
            {
                string name = $"{b.Id}.{b.FileExtension}";
                await _blobClient.DeleteBlobIfExistsAsync(name);
                using var ms = new MemoryStream([7, 7, 7]);
                await _blobClient.UploadBlobAsync(name, ms);
            }
        }

        // Act: page 2 of size 2 → should return items 3 & 4 (0-based index)
        var query = new GetAlbumImagesPaginatedQuery(
            AlbumId: album.Id,
            Page: 2,
            PageSize: 2,
            SortBy: SortDirection.OldestFirst
        );
        Result<PaginatedResult<AlbumImageDto>> result = await _mediator.Send(query);

        // Assert
        Assert.That(result.IsSuccess);
        
        PaginatedResult<AlbumImageDto>? page = result.Value;
        Assert.Multiple(() =>
        {
            Assert.That(page.ItemsCount, Is.EqualTo(5));
            Assert.That(page.Page, Is.EqualTo(2));
            Assert.That(page.PageSize, Is.EqualTo(2));
            Assert.That(page.Items, Has.Count.EqualTo(2));
        });

        List<int> expectedIds = created.Skip(2).Take(2).Select(x => x.Id).OrderBy(x => x).ToList();
        List<int> actualIds   = page.Items.Select(d => d.Id).OrderBy(x => x).ToList();
        Assert.That(actualIds, Is.EqualTo(expectedIds));
    }
}
