using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainModules.Emails.Entities;

namespace DataAccess.Emails.Configuration;

public class RecipientEntityTypeConfiguration : IEntityTypeConfiguration<Recipient>
{
    public void Configure(EntityTypeBuilder<Recipient> builder)
    {
        builder.ToTable("Recipients");
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.EmailAddress)
               .IsUnique();
        
        builder.Property(r => r.EmailAddress)
               .IsRequired()
               .HasMaxLength(256);

        builder.Property(r => r.Token)
               .IsRequired()
               .HasDefaultValueSql("NEWID()");

        builder.HasIndex(r => r.Token);
    }
}
