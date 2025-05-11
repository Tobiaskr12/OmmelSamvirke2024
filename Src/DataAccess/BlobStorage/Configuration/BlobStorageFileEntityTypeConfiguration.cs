using DomainModules.BlobStorage.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.BlobStorage.Configuration;

public class BlobStorageFileEntityTypeConfiguration : IEntityTypeConfiguration<BlobStorageFile>
{
    public void Configure(EntityTypeBuilder<BlobStorageFile> builder)
    {
        builder.ToTable("BlobStorageFiles");

        builder.HasKey(b => b.Id);
        builder.HasIndex(b => b.DateCreated);

        builder.Ignore(b => b.FileContent);
        builder.Ignore(b => b.FileSizeInBytes);

        builder.Property(b => b.FileBaseName)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(b => b.FileExtension)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property<long?>("_storedFileSize")
               .HasColumnName("FileSizeInBytes")
               .IsRequired();

        builder.Property(b => b.ContentType)
               .IsRequired()
               .HasMaxLength(100);
    }
}
