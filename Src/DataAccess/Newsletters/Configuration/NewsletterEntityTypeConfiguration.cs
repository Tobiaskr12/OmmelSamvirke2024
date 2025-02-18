using DomainModules.Newsletters.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Newsletters.Configuration;

public class NewsletterEntityTypeConfiguration : IEntityTypeConfiguration<Newsletter>
{
    public void Configure(EntityTypeBuilder<Newsletter> builder)
    {
        builder.ToTable("Newsletters");
        builder.HasKey(n => n.Id);

        // One-to-One relationship with Email
        builder.HasOne(n => n.Email)
               .WithOne()
               .HasForeignKey<Newsletter>("EmailId")
               .IsRequired();

        // Many-to-Many relationship with NewsletterGroups via a join table
        builder.HasMany(n => n.NewsletterGroups)
               .WithMany()
               .UsingEntity<Dictionary<string, object>>(
                   "NewsletterNewsletterGroup",
                   j => j.HasOne<NewsletterGroup>()
                         .WithMany()
                         .HasForeignKey("NewsletterGroupId")
                         .OnDelete(DeleteBehavior.Cascade),
                   j => j.HasOne<Newsletter>()
                         .WithMany()
                         .HasForeignKey("NewsletterId")
                         .OnDelete(DeleteBehavior.Cascade),
                   j =>
                   {
                       j.HasKey("NewsletterId", "NewsletterGroupId");
                       j.ToTable("NewsletterNewsletterGroups");
                   });

        builder.HasIndex(n => n.DateCreated);
        builder.HasIndex(n => n.DateModified);
    }
}
