using Emails.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Emails.DataAccess.Configuration;

public class RecipientEntityTypeConfiguration : IEntityTypeConfiguration<Recipient>
{
    public void Configure(EntityTypeBuilder<Recipient> builder)
    {
        //builder.HasKey(e => e.Id);
        //builder.Property(e => e.Email)
        //       .IsRequired();
    }
}
