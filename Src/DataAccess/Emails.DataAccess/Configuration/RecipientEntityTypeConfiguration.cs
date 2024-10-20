using Emails.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Emails.DataAccess.Configuration;

public class RecipientEntityTypeConfiguration : IEntityTypeConfiguration<Recipient>
{
    public void Configure(EntityTypeBuilder<Recipient> builder)
    {
        builder.ToTable("Recipients");
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.EmailAddress)
               .IsRequired()
               .HasMaxLength(256);
    }
}
