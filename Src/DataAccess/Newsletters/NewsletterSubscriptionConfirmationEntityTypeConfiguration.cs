using DomainModules.Newsletters.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Newsletters;

public class NewsletterSubscriptionConfirmationEntityTypeConfiguration : IEntityTypeConfiguration<NewsletterSubscriptionConfirmation>
{
    public void Configure(EntityTypeBuilder<NewsletterSubscriptionConfirmation> builder)
    {
        builder.ToTable("NewsletterSubscriptionConfirmations");
        builder.HasKey(nsc => nsc.Id);

        builder.Property(nsc => nsc.ConfirmationToken)
               .IsRequired();

        builder.Property(nsc => nsc.ConfirmationExpiry)
               .IsRequired();

        builder.Property(nsc => nsc.IsConfirmed)
               .IsRequired();

        builder.Property(nsc => nsc.ConfirmationTime);

        builder.HasIndex(nsc => nsc.ConfirmationExpiry);
    }
}
