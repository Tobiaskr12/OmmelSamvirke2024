using DomainModules.Reservations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Reservations;

public class ReservationEntityTypeConfiguration : IEntityTypeConfiguration<Reservation>
{
       public void Configure(EntityTypeBuilder<Reservation> builder)
       {
              builder.ToTable("Reservations");
            
              builder.HasKey(r => r.Id);
            
              builder.Property(r => r.Email)
                     .IsRequired()
                     .HasMaxLength(256);
            
              builder.Property(r => r.PhoneNumber)
                     .IsRequired()
                     .HasMaxLength(20);
            
              builder.Property(r => r.Name)
                     .IsRequired()
                     .HasMaxLength(100);
            
              builder.Property(r => r.StartTime)
                     .IsRequired();
            
              builder.Property(r => r.EndTime)
                     .IsRequired();
            
              builder.Property(r => r.State)
                     .IsRequired();
            
              builder.Property(r => r.CommunityName)
                     .HasMaxLength(75);
              
              builder.HasOne(r => r.Location)
                     .WithMany() 
                     .HasForeignKey("ReservationLocationId")
                     .IsRequired()
                     .OnDelete(DeleteBehavior.Restrict);
              
              builder.Navigation(r => r.Location).AutoInclude();
             
              builder.Property(r => r.ReservationSeriesId)
                     .IsRequired(false);
            
              builder.HasIndex(r => r.ReservationSeriesId);
              builder.HasIndex(r => r.Email);
       }
}
