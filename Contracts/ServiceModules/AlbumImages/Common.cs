namespace Contracts.ServiceModules.AlbumImages;

public record ImageUploadInput(
    byte[] FileContent,
    string OriginalFileName,
    string ContentType,
    ImageMetadataInput? Metadata = null
);

public record ImageMetadataInput(
    DateTime? DateTaken = null,
    string? Location = null,
    string? PhotographerName = null,
    string? Title = null,
    string? Description = null
);

public record AlbumSummaryDto(
    int Id,
    string Name,
    string? Description,
    string? CoverImageThumbnailUrl,
    int ImageCount,
    DateTime DateCreated
);


public record AlbumImageDto(
    int Id,
    string ThumbnailUrl,
    string DefaultImageUrl,
    string? Title,
    string? Description,
    DateTime DateCreated
);
