using DomainModules.Events.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Events.Configuration;

public class EventEntityTypeConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");
        builder.HasKey(e => e.Id);
            
        builder.Property(e => e.Title)
               .IsRequired()
               .HasMaxLength(100);
            
        builder.Property(e => e.Description)
               .IsRequired()
               .HasMaxLength(5000);
            
        builder.Property(e => e.StartTime)
               .IsRequired();
            
        builder.Property(e => e.EndTime)
               .IsRequired();
            
        builder.Property(e => e.Location)
               .IsRequired()
               .HasMaxLength(50);
            
        // One-to-many: One event can have many remote files
        builder.HasMany(e => e.RemoteFiles)
               .WithOne()
               .HasForeignKey("EventId")
               .OnDelete(DeleteBehavior.Cascade);
            
        // Many-to-one: Many Events can have the same EventCoordinator
        builder.HasOne(e => e.EventCoordinator)
               .WithMany()
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);
        
        // One-to-one (optional): An Event may be linked to a Reservation
        builder.HasOne(e => e.Reservation)
               .WithOne(r => r.Event)
               .HasForeignKey<Event>("ReservationId")
               .OnDelete(DeleteBehavior.SetNull);
    }
}
