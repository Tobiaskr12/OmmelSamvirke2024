using Contracts.DataAccess.Base;
using Contracts.ServiceModules.AlbumImages;
using DomainModules.ImageAlbums.Entities;
using FluentResults;
using MediatR;

namespace ServiceModules.Tests.ImageAlbums.Commands;

[TestFixture, Category("IntegrationTests")]
public class SetAlbumCoverImageCommandTests : ServiceTestBase
{
    private IMediator _mediator;
    private IRepository<Album> _albumRepo;

    [SetUp]
    public void Init()
    {
        _mediator = GetService<IMediator>();
        _albumRepo = GetService<IRepository<Album>>();
    }

    [Test]
    public async Task SetCover_NonExistingAlbum_ReturnsNotFound()
    {
        // Act
        Result result = await _mediator.Send(new SetAlbumCoverImageCommand(
            AlbumId: 9999,
            ImageId: 1
        ));

        // Assert
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task SetCover_NonExistingImage_ReturnsNotFound()
    {
        // Arrange: create an album only
        var album = new Album { Name = "NoImgAlbum", Images = new List<Image>(), CoverImage = null };
        await AddTestData(album);

        // Act
        Result result = await _mediator.Send(new SetAlbumCoverImageCommand(
            AlbumId: album.Id,
            ImageId: 9999
        ));

        // Assert
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task SetCover_ValidImage_UpdatesAlbumCover()
    {
        // Arrange: create album with one image
        Image img = await TestDataFactory.CreateAndPersistImageWithAlbumAsync();
        int albumId = img.Album.Id;

        // Act
        Result result = await _mediator.Send(new SetAlbumCoverImageCommand(
            AlbumId: albumId,
            ImageId: img.Id
        ));

        // Assert
        Assert.That(result.IsSuccess);

        // Fetch album and verify cover is set
        Result<Album> getRes = await _albumRepo.GetByIdAsync(albumId, readOnly: true);
        Assert.That(getRes.IsSuccess);
        
        Album updated = getRes.Value;
        Assert.Multiple(() =>
        {
            Assert.That(updated.CoverImage, Is.Not.Null);
            Assert.That(updated.CoverImage?.Id, Is.EqualTo(img.Id));
        });
    }
}
