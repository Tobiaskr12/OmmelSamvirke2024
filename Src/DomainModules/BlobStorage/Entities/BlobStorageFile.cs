using DomainModules.Common;

namespace DomainModules.BlobStorage.Entities;

public class BlobStorageFile : BaseEntity
{
    private long? _storedFileSize = 0;
    
    public Guid BlobGuid { get; set; } = Guid.NewGuid();
    public required string FileBaseName { get; set; }
    public required string FileExtension { get; set; }
    public Stream? FileContent { get; set; }
    public required string ContentType { get; set; }

    // Computed properties
    public long FileSizeInBytes => FileContent?.Length ?? (_storedFileSize ?? 0);
    public string FileName => $"{FileBaseName}###{BlobGuid}.{FileExtension}";
    
    /// <summary>
    /// Sets the stored file size. This is used internally during upload/download.
    /// </summary>
    /// <param name="size">The size of the file in bytes.</param>
    public void SetFileSize(long size)
    {
        _storedFileSize = size;
    }
}
