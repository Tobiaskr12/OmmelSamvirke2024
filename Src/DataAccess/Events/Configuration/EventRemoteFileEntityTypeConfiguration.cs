using DomainModules.Events.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Events.Configuration;

public class EventRemoteFileEntityTypeConfiguration : IEntityTypeConfiguration<EventRemoteFile>
{
    public void Configure(EntityTypeBuilder<EventRemoteFile> builder)
    {
        builder.ToTable("EventRemoteFiles");
        builder.HasKey(rf => rf.Id);
            
        builder.Property(rf => rf.FileName)
               .IsRequired()
               .HasMaxLength(255);
            
        builder.Property(rf => rf.FileSizeBytes)
               .IsRequired();
            
        builder.Property(rf => rf.FileType)
               .IsRequired()
               .HasMaxLength(100);
            
        builder.Property(rf => rf.Url)
               .IsRequired()
               .HasMaxLength(2048);
    }
}