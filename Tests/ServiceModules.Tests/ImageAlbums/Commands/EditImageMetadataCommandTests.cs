using Contracts.DataAccess.Base;
using Contracts.ServiceModules.AlbumImages;
using DomainModules.ImageAlbums.Entities;
using FluentResults;
using MediatR;

namespace ServiceModules.Tests.ImageAlbums.Commands;

[TestFixture, Category("IntegrationTests")]
public class EditImageMetadataCommandTests : ServiceTestBase
{
    private IMediator _mediator;
    private IRepository<Image> _imageRepo;

    [SetUp]
    public void Init()
    {
        _mediator = GetService<IMediator>();
        _imageRepo = GetService<IRepository<Image>>();
    }

    [Test]
    public async Task Edit_NonExistingImage_ReturnsNotFound()
    {
        // Act
        Result<Image> result = await _mediator.Send(new EditImageMetadataCommand(
            ImageId: 9999,
            DateTaken: DateTime.UtcNow,
            Location: "Any",
            PhotographerName: "Name",
            Title: "Title",
            Description: "Desc"
        ));

        // Assert
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Edit_ExistingImage_UpdatesMetadata()
    {
        // Arrange
        Image image = await TestDataFactory.CreateAndPersistImageWithAlbumAsync();

        var newDate = new DateTime(2025, 1, 1);
        const string newLoc = "Copenhagen";
        const string newPhotographer = "Alice";
        const string newTitle = "Winter";
        const string newDesc = "First snow";

        var cmd = new EditImageMetadataCommand(
            ImageId: image.Id,
            DateTaken: newDate,
            Location: newLoc,
            PhotographerName: newPhotographer,
            Title: newTitle,
            Description: newDesc
        );

        // Act
        Result<Image> result = await _mediator.Send(cmd);

        // Assert handler response
        Assert.That(result.IsSuccess);
        Image updated = result.Value;
        Assert.Multiple(() =>
        {
            Assert.That(updated.DateTaken, Is.EqualTo(newDate));
            Assert.That(updated.Location, Is.EqualTo(newLoc));
            Assert.That(updated.PhotographerName, Is.EqualTo(newPhotographer));
            Assert.That(updated.Title, Is.EqualTo(newTitle));
            Assert.That(updated.Description, Is.EqualTo(newDesc));
        });

        // Assert persistence
        Result<Image> getRes = await _imageRepo.GetByIdAsync(image.Id, readOnly: true);
        Assert.That(getRes.IsSuccess);
        Image fromDb = getRes.Value!;
        Assert.Multiple(() =>
        {
            Assert.That(fromDb.DateTaken, Is.EqualTo(newDate));
            Assert.That(fromDb.Location, Is.EqualTo(newLoc));
            Assert.That(fromDb.PhotographerName, Is.EqualTo(newPhotographer));
            Assert.That(fromDb.Title, Is.EqualTo(newTitle));
            Assert.That(fromDb.Description, Is.EqualTo(newDesc));
        });
    }

    [Test]
    public async Task Edit_CanClearMetadataFields()
    {
        // Arrange
        Image image = await TestDataFactory.CreateAndPersistImageWithAlbumAsync();

        // First set some metadata
        await _mediator.Send(new EditImageMetadataCommand(
            ImageId: image.Id,
            DateTaken: DateTime.UtcNow,
            Location: "Loc",
            PhotographerName: "Name",
            Title: "Title",
            Description: "Desc"
        ));

        // Act
        Result<Image> clearResult = await _mediator.Send(new EditImageMetadataCommand(
            ImageId: image.Id,
            DateTaken: null,
            Location: null,
            PhotographerName: null,
            Title: null,
            Description: null
        ));

        // Assert
        Assert.That(clearResult.IsSuccess);
        Image cleared = clearResult.Value;
        Assert.Multiple(() =>
        {
            Assert.That(cleared.DateTaken, Is.Null);
            Assert.That(cleared.Location, Is.Null);
            Assert.That(cleared.PhotographerName, Is.Null);
            Assert.That(cleared.Title, Is.Null);
            Assert.That(cleared.Description, Is.Null);
        });
    }
}
