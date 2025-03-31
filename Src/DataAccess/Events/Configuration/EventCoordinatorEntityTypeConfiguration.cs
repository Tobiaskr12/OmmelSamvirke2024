using DomainModules.Events.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Events.Configuration;

public class EventCoordinatorEntityTypeConfiguration : IEntityTypeConfiguration<EventCoordinator>
{
    public void Configure(EntityTypeBuilder<EventCoordinator> builder)
    {
        builder.ToTable("EventCoordinators");
        builder.HasKey(ec => ec.Id);
            
        builder.Property(ec => ec.Name)
               .IsRequired()
               .HasMaxLength(100);
            
        builder.Property(ec => ec.EmailAddress)
               .HasMaxLength(256);
            
        builder.Property(ec => ec.PhoneNumber)
               .HasMaxLength(50);
    }
}
