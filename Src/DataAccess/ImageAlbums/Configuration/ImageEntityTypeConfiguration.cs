using DomainModules.ImageAlbums.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.ImageAlbums.Configuration;

public class ImageEntityTypeConfiguration : IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.ToTable("Images");
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.DateTaken);

        builder.Property(i => i.Location)
            .HasMaxLength(256);

        builder.Property(i => i.PhotographerName)
            .HasMaxLength(100);

        builder.Property(i => i.Title)
            .HasMaxLength(100);

        builder.Property(i => i.Description)
            .HasMaxLength(500);

        // Image to OriginalBlobStorageFile (One-to-One, Required)
        builder.HasOne(i => i.OriginalBlobStorageFile)
            .WithOne()
            .HasForeignKey<Image>("OriginalBlobStorageFileId")
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        // Image to DefaultBlobStorageFile (One-to-One, Optional)
        builder.HasOne(i => i.DefaultBlobStorageFile)
            .WithOne()
            .HasForeignKey<Image>("DefaultBlobStorageFileId")
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        // Image to ThumbnailBlobStorageFile (One-to-One, Optional)
        builder.HasOne(i => i.ThumbnailBlobStorageFile)
            .WithOne()
            .HasForeignKey<Image>("ThumbnailBlobStorageFileId")
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(i => i.OriginalBlobStorageFile).AutoInclude();
        builder.Navigation(i => i.DefaultBlobStorageFile).AutoInclude();
        builder.Navigation(i => i.ThumbnailBlobStorageFile).AutoInclude();
        
        builder.HasIndex(i => i.DateTaken);
        builder.HasIndex(i => i.DateCreated);
    }
}
