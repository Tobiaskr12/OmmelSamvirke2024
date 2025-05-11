using DomainModules.ImageAlbums.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.AlbumImages;

public record CreateAlbumCommand(
    string Name,
    string? Description,
    List<ImageUploadInput> Images,
    ImageMetadataInput? DefaultMetadataForAllImages = null
) : IRequest<Result<Album>>;

public record DeleteAlbumCommand(int AlbumId) : IRequest<Result>;

public record UploadImagesToAlbumCommand(
    int AlbumId,
    List<ImageUploadInput> Images,
    ImageMetadataInput? DefaultMetadataForAllImages = null
) : IRequest<Result<List<Image>>>;

public record DeleteImageCommand(int ImageId) : IRequest<Result>;

public record DeleteImagesCommand(List<int> ImageIds) : IRequest<Result>;

public record EditImageMetadataCommand(
    int ImageId,
    DateTime? DateTaken,
    string? Location,
    string? PhotographerName,
    string? Title,
    string? Description
) : IRequest<Result<Image>>;

public record SetAlbumCoverImageCommand(
    int AlbumId,
    int ImageId
) : IRequest<Result>;
