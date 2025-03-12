using DomainModules.Reservations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Reservations;

public class BlockedReservationTimeSlotEntityTypeConfiguration : IEntityTypeConfiguration<BlockedReservationTimeSlot>
{
    public void Configure(EntityTypeBuilder<BlockedReservationTimeSlot> builder)
    {
        builder.ToTable("BlockedReservationTimeSlots");
        
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.StartTime)
               .IsRequired();
        
        builder.Property(b => b.EndTime)
               .IsRequired();
        
        builder.HasIndex(b => b.StartTime);
    }
}
