using System.Net.Mime;
using Emails.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EmailWrapper.DataAccess.Configuration;

public class AttachmentEntityTypeConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder
            .Property(a => a.ContentType)
            .HasConversion(new ContentTypeConverter());
    }
}

public class ContentTypeConverter : ValueConverter<ContentType, string>
{
    public ContentTypeConverter() : base(
        contentType => contentType.ToString(),
        str => new ContentType(str)
    ) { }
}
