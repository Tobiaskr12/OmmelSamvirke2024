using DomainModules.Newsletters.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Newsletters.Configuration;

public class NewsletterUnsubscribeConfirmationEntityTypeConfiguration : IEntityTypeConfiguration<NewsletterUnsubscribeConfirmation>
{
    public void Configure(EntityTypeBuilder<NewsletterUnsubscribeConfirmation> builder)
    {
        builder.ToTable("NewsletterUnsubscribeConfirmations");
        builder.HasKey(nuc => nuc.Id);

        builder.Property(nuc => nuc.ConfirmationToken)
               .IsRequired();

        builder.Property(nuc => nuc.ConfirmationExpiry)
               .IsRequired();

        builder.Property(nuc => nuc.IsConfirmed)
               .IsRequired();

        builder.Property(nuc => nuc.ConfirmationTime);

        builder.HasIndex(nuc => nuc.ConfirmationExpiry);
        
        builder.HasOne(nsc => nsc.Recipient)
               .WithMany(r => r.NewsletterUnsubscribeConfirmations)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(nsc => nsc.NewsletterGroups)
               .WithMany(ng => ng.NewsletterUnsubscribeConfirmations)
               .UsingEntity(j => j.ToTable("NewsletterUnsubscribeConfirmationNewsletterGroups"));
    }
}
