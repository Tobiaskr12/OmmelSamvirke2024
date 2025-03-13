using DomainModules.Reservations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Reservations;

public class ReservationLocationEntityTypeConfiguration : IEntityTypeConfiguration<ReservationLocation>
{
    public void Configure(EntityTypeBuilder<ReservationLocation> builder)
    {
        builder.ToTable("ReservationLocations");

        builder.HasKey(rl => rl.Id);

        builder.Property(rl => rl.Name)
               .IsRequired()
               .HasMaxLength(75);
    }
}
