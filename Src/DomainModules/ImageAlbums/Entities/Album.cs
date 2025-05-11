using DomainModules.Common;

namespace DomainModules.ImageAlbums.Entities;

public class Album : BaseEntity
{
    public required string Name { get; set; }
    
    public string? Description { get; set; }
    
    /// <summary>
    /// All images in this album. At least one required.
    /// </summary>
    public required List<Image> Images { get; set; } = [];

    /// <summary>
    /// Navigation to the cover image.
    /// </summary>
    public Image? CoverImage { get; set; }
}
