using DomainModules.Common;

namespace DomainModules.Events.Entities;

public class EventRemoteFile : BaseEntity
{
    public required string FileName { get; set; }
    public required long FileSizeBytes { get; set; }
    public required string FileType { get; set; }
    public required string Url { get; set; }
}
