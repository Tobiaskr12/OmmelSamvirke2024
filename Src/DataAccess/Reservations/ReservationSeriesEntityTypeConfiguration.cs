using DomainModules.Reservations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Reservations;

public class ReservationSeriesEntityTypeConfiguration : IEntityTypeConfiguration<ReservationSeries>
{
    public void Configure(EntityTypeBuilder<ReservationSeries> builder)
    {
        builder.ToTable("ReservationSeries");
            
        builder.HasKey(rs => rs.Id);
            
        builder.Property(rs => rs.RecurrenceType)
               .IsRequired();
            
        builder.Property(rs => rs.Interval)
               .IsRequired();
            
        builder.Property(rs => rs.RecurrenceStartDate)
               .IsRequired();
            
        builder.Property(rs => rs.RecurrenceEndDate)
               .IsRequired();
        
        builder.HasMany(rs => rs.Reservations)
               .WithOne()
               .HasForeignKey(r => r.ReservationSeriesId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.Reservations).AutoInclude();
        
        builder.HasIndex(rs => rs.RecurrenceStartDate);
        builder.HasIndex(rs => rs.RecurrenceEndDate);
    }
}
