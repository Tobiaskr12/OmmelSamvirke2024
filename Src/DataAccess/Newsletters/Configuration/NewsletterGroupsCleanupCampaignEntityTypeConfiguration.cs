using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Newsletters.Configuration;

public class NewsletterGroupsCleanupCampaignEntityTypeConfiguration : IEntityTypeConfiguration<NewsletterGroupsCleanupCampaign>
{
    public void Configure(EntityTypeBuilder<NewsletterGroupsCleanupCampaign> builder)
    {
        builder.ToTable("NewsletterGroupsCleanupCampaigns");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CampaignStart)
               .IsRequired();

        builder.Property(c => c.CampaignDurationMonths)
               .IsRequired();

        // Many-to-Many for UncleanedRecipients
        builder.HasMany(c => c.UnconfirmedRecipients)
               .WithMany()
               .UsingEntity<Dictionary<string, object>>(
                    "CampaignUncleanedRecipient",
                    j => j.HasOne<Recipient>()
                          .WithMany()
                          .HasForeignKey("RecipientId")
                          .OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne<NewsletterGroupsCleanupCampaign>()
                          .WithMany()
                          .HasForeignKey("CampaignId")
                          .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("CampaignId", "RecipientId");
                        j.ToTable("Join_CampaignRecipients");
                    });

        builder.HasIndex(c => c.CampaignStart);
    }
}
