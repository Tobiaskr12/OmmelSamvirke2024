using DomainModules.Reservations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Reservations;

public class ReservationHistoryEntityTypeConfiguration : IEntityTypeConfiguration<ReservationHistory>
{
    public void Configure(EntityTypeBuilder<ReservationHistory> builder)
    {
        builder.ToTable("ReservationHistories");
        
        builder.HasKey(rh => rh.Id);
        
        builder.Property(rh => rh.Email)
               .IsRequired()
               .HasMaxLength(256);
        
        builder.Property(rh => rh.Token)
               .IsRequired();
        
        builder.HasIndex(rh => rh.Email);
        builder.HasIndex(rh => rh.Token);
        
        builder.HasMany(rh => rh.Reservations)
               .WithOne()
               .HasForeignKey("ReservationHistoryId");
        
        builder.Navigation(ng => ng.Reservations).AutoInclude();
    }
}
