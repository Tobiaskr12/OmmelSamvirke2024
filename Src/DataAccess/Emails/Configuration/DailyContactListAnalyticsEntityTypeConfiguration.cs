using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainModules.Emails.Entities;

namespace DataAccess.Emails.Configuration;

public class DailyContactListAnalyticsEntityTypeConfiguration : IEntityTypeConfiguration<DailyContactListAnalytics>
{
    public void Configure(EntityTypeBuilder<DailyContactListAnalytics> builder)
    {
        builder.ToTable("DailyContactListAnalytics");
        builder.HasKey(dea => dea.Id);

        builder.Property(dea => dea.Date)
               .IsRequired();

        builder.Property(dea => dea.TotalContacts)
               .IsRequired();
        
        builder.Property(dea => dea.IsNewsletter)
               .IsRequired();
    }
}
