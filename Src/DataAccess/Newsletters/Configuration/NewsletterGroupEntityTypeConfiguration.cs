using DomainModules.Newsletters.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Newsletters.Configuration;

public class NewsletterGroupEntityTypeConfiguration : IEntityTypeConfiguration<NewsletterGroup>
{
    public void Configure(EntityTypeBuilder<NewsletterGroup> builder)
    {
        builder.ToTable("NewsletterGroups");
        builder.HasKey(ng => ng.Id);

        builder.Property(ng => ng.Name)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(ng => ng.Description)
               .IsRequired()
               .HasMaxLength(500);

        // Relationship with ContactList
        builder.HasOne(ng => ng.ContactList)
               .WithOne()
               .HasForeignKey<NewsletterGroup>("ContactListId")
               .IsRequired();

        builder.HasIndex(ng => ng.Name);
    }
}
