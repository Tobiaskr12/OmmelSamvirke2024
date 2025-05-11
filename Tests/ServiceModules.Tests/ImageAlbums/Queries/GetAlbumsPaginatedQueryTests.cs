using Azure.Storage.Blobs;
using Contracts.DataAccess;
using Contracts.DataAccess.Base;
using Contracts.Infrastructure.BlobStorage;
using Contracts.ServiceModules.AlbumImages;
using DomainModules.BlobStorage.Entities;
using DomainModules.ImageAlbums.Entities;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace ServiceModules.Tests.ImageAlbums.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetAlbumsPaginatedQueryTests : ServiceTestBase
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
    public async Task GetAlbums_Empty_ReturnsEmptyPage()
    {
        // No albums in DB
        var query = new GetAlbumsPaginatedQuery(
            Page: 1,
            PageSize: 5,
            SortBy: SortDirection.NewestFirst
        );

        Result<PaginatedResult<AlbumSummaryDto>> result = await _mediator.Send(query);

        Assert.That(result.IsSuccess);

        PaginatedResult<AlbumSummaryDto>? page = result.Value;
        Assert.Multiple(() =>
        {
            Assert.That(page.ItemsCount, Is.EqualTo(0));
            Assert.That(page.Items, Is.Empty);
            Assert.That(page.Page, Is.EqualTo(1));
            Assert.That(page.PageSize, Is.EqualTo(5));
            Assert.That(page.PageCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GetAlbums_SingleAlbum_NoCover_ReturnsDtoWithNullThumbnailAndZeroImageCount()
    {
        // Arrange
        var album = new Album { Name = "Solo", Description = "Just one", Images = [], CoverImage = null };
        await AddTestData(album);

        // Act
        var query = new GetAlbumsPaginatedQuery(
            Page: 1,
            PageSize: 10,
            SortBy: SortDirection.NewestFirst
        );
        Result<PaginatedResult<AlbumSummaryDto>> result = await _mediator.Send(query);

        // Assert
        Assert.That(result.IsSuccess);
        PaginatedResult<AlbumSummaryDto>? page = result.Value;
        Assert.Multiple(() =>
        {
            Assert.That(page.ItemsCount, Is.EqualTo(1));
            Assert.That(page.Items, Has.Count.EqualTo(1));
        });

        AlbumSummaryDto dto = page.Items.First();
        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(album.Id));
            Assert.That(dto.Name, Is.EqualTo(album.Name));
            Assert.That(dto.Description, Is.EqualTo(album.Description));
            Assert.That(dto.CoverImageThumbnailUrl, Is.Null);
            Assert.That(dto.ImageCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GetAlbums_WithCoverImage_IncludesThumbnailUrlAndCorrectImageCount()
    {
        // Arrange
        var album = new Album { Name = "HasCover", Description = "One image only", Images = [], CoverImage = null };
        await AddTestData(album);

        // Persist an image & link to album
        Image img = await TestDataFactory.CreateAndPersistImageWithAlbumAsync(album);

        // Now set it as cover
        var albumRepo = GetService<IRepository<Album>>();
        Result<Album> getAlbum = await albumRepo.GetByIdAsync(album.Id, readOnly: false);
        Album toUpdate = getAlbum.Value!;
        toUpdate.CoverImage = img;
        await albumRepo.UpdateAsync(toUpdate);

        // Upload the thumbnail blob so the generated URL will point to an existing blob
        BlobStorageFile thumbMeta   = img.ThumbnailBlobStorageFile;
        string blobName = $"{thumbMeta.FileBaseName}-{thumbMeta.Id}.{thumbMeta.FileExtension}";
        await _blobClient.DeleteBlobIfExistsAsync(blobName);
        using (var ms = new MemoryStream([1, 2, 3])) await _blobClient.UploadBlobAsync(blobName, ms);

        // Act
        var query = new GetAlbumsPaginatedQuery(
            Page: 1,
            PageSize: 5,
            SortBy: SortDirection.NewestFirst
        );
        Result<PaginatedResult<AlbumSummaryDto>> result = await _mediator.Send(query);

        // Assert
        Assert.That(result.IsSuccess);
        PaginatedResult<AlbumSummaryDto>? page = result.Value;
        Assert.Multiple(() =>
        {
            Assert.That(page.ItemsCount, Is.EqualTo(1));
            Assert.That(page.Items, Has.Count.EqualTo(1));
        });

        AlbumSummaryDto dto = page.Items.First();
        string baseUrl = _blobService.GetPublicBlobBaseUrl();
        Assert.Multiple(() =>
        {
            Assert.That(dto.ImageCount, Is.EqualTo(1));
            Assert.That(dto.CoverImageThumbnailUrl, Does.StartWith(baseUrl).And.Contain($"{thumbMeta.FileBaseName}-{thumbMeta.Id}.{thumbMeta.FileExtension}"));
        });
    }

    [Test]
    public async Task GetAlbums_PaginatesCorrectly()
    {
        // Arrange: create 5 albums with no cover
        var created = new List<Album>();
        for (int i = 0; i < 5; i++)
        {
            var alb = new Album { Name = $"A{i}", Description = $"Desc{i}", Images = [], CoverImage = null };
            await AddTestData(alb);
            created.Add(alb);
        }

        // Act: request page 2, size 2 → should return albums[2] & albums[3] when sorted NewestFirst
        Result<PaginatedResult<AlbumSummaryDto>> result = await _mediator.Send(new GetAlbumsPaginatedQuery(
            Page: 2,
            PageSize: 2,
            SortBy: SortDirection.NewestFirst
        ));

        // Assert
        Assert.That(result.IsSuccess);
        PaginatedResult<AlbumSummaryDto>? page = result.Value;
        Assert.Multiple(() =>
        {
            Assert.That(page.ItemsCount, Is.EqualTo(5));
            Assert.That(page.Page, Is.EqualTo(2));
            Assert.That(page.PageSize, Is.EqualTo(2));
            Assert.That(page.Items, Has.Count.EqualTo(2));
            Assert.That(page.PageCount, Is.EqualTo(3));
        });

        // Determine expected IDs for NewestFirst: reverse the list, then skip( (2-1)*2 ) => skip 2
        List<int> expected = 
            created
                .OrderByDescending(a => a.DateCreated)
                .Skip(2)
                .Take(2)
                .Select(a => a.Id)
                .OrderBy(id => id)
                .ToList();

        List<int> actual = page.Items.Select(d => d.Id).OrderBy(id => id).ToList();
        Assert.That(actual, Is.EqualTo(expected));
    }
}
