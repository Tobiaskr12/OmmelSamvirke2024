using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmmelSamvirke.DomainModules.Emails.Entities;

namespace OmmelSamvirke.DataAccess.Emails.Configuration;

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
