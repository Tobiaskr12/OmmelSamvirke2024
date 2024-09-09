using EmailWrapper.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmailWrapper.DataAccess.Configuration;

public class EmailEntityTypeConfiguration : IEntityTypeConfiguration<Email>
{
    public void Configure(EntityTypeBuilder<Email> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Subject)
               .IsRequired();
    }
}