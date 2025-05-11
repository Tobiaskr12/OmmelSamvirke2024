using DomainModules.ImageAlbums.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.ImageAlbums.Configuration;

public class AlbumEntityTypeConfiguration : IEntityTypeConfiguration<Album>
{
    public void Configure(EntityTypeBuilder<Album> builder)
    {
        builder.ToTable("Albums");
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Description)
            .HasMaxLength(500);
        
        // Album to Images (One-to-Many)
        builder.HasMany(a => a.Images)
            .WithOne(i => i.Album)
            .HasForeignKey("AlbumId")
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        // Album to CoverImage (One-to-Zero-or-One)
        builder.HasOne(a => a.CoverImage)
            .WithOne()
            .HasForeignKey<Album>("CoverImageId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(a => a.CoverImage).AutoInclude();
        builder.Navigation(a => a.Images).AutoInclude();
        
        builder.HasIndex(a => a.Name);
        builder.HasIndex(a => a.DateCreated);
    }
}
