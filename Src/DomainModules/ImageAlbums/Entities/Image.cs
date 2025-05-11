using DomainModules.Common;
using DomainModules.BlobStorage.Entities;

namespace DomainModules.ImageAlbums.Entities;

public class Image : BaseEntity
{
    /// <summary>
    /// The uploaded/original file. Service will generate the other two versions
    /// </summary>
    public required BlobStorageFile OriginalBlobStorageFile { get; set; }

    /// <summary>
    /// System-generated compressed/upscaled version
    /// </summary>
    public required BlobStorageFile DefaultBlobStorageFile { get; set; }

    /// <summary>
    /// System-generated thumbnail (max 300×300px)
    /// </summary>
    public required BlobStorageFile ThumbnailBlobStorageFile { get; set; }

    /// <summary>
    /// Optional metadata
    /// </summary>
    public DateTime? DateTaken { get; set; }
    public string? Location { get; set; }
    public string? PhotographerName { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }

    /// <summary>
    /// Back‑pointer to album.
    /// </summary>
    public required Album Album { get; set; }
}
