using System.Net.Mime;
using DomainModules.Common;

namespace DomainModules.Emails.Entities;

public class Attachment : BaseEntity
{
    public required string Name { get; set; }
    public required Uri ContentPath { get; set; }
    public required ContentType ContentType { get; set; }
    public byte[]? BinaryContent { get; set; }
    public long ContentSize => BinaryContent?.Length ?? 0;
}
