using System.Net.Mime;
using Domain.Common;

namespace Emails.Domain.Entities;

public class Attachment : BaseEntity
{
    public required string Name { get; set; }
    public required Uri ContentPath { get; set; }
    public required ContentType ContentType { get; set; }
    public byte[]? BinaryContent { get; set; }
    public long ContentSize { get; set; } = 0;
}
