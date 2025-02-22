using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainModules.Emails.Entities;

namespace DataAccess.Emails.Configuration;

public class AttachmentEntityTypeConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");
        builder.HasKey(a => a.Id);
    
        builder.Property(a => a.Name)
               .IsRequired()
               .HasMaxLength(256);

        builder.Property(a => a.ContentPath)
               .IsRequired()
               .HasConversion(
                   v => v.ToString(),
                   v => new Uri(v))
               .HasMaxLength(2048);

        builder.Property(a => a.ContentType)
               .IsRequired()
               .HasConversion(
                   v => v.ToString(),
                   v => new ContentType(v))
               .HasMaxLength(256);
        
        builder.Ignore(a => a.BinaryContent); // Ignore binary content as it will be stored in cloud storage
        builder.Ignore(a => a.ContentSize); // Ignore Computed Property
    }
}
