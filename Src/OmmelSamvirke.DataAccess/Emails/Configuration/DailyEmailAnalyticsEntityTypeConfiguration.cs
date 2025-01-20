using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmmelSamvirke.DomainModules.Emails.Entities;

namespace OmmelSamvirke.DataAccess.Emails.Configuration;

public class DailyEmailAnalyticsEntityTypeConfiguration : IEntityTypeConfiguration<DailyEmailAnalytics>
{
    public void Configure(EntityTypeBuilder<DailyEmailAnalytics> builder)
    {
        builder.ToTable("DailyEmailAnalytics");
        builder.HasKey(dea => dea.Id);

        builder.Property(dea => dea.Date)
               .IsRequired();

        builder.Property(dea => dea.SentEmails)
               .IsRequired();
        
        builder.Property(dea => dea.TotalRecipients)
               .IsRequired();
    }
}
