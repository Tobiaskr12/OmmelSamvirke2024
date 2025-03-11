using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainModules.Emails.Entities;

namespace DataAccess.Emails.Configuration;

public class EmailEntityTypeConfiguration : IEntityTypeConfiguration<Email>
{
    public void Configure(EntityTypeBuilder<Email> builder)
    {
        builder.ToTable("Emails");
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.DateCreated);
        
        builder.Property(e => e.SenderEmailAddress)
               .IsRequired()
               .HasMaxLength(256);

        builder.Property(e => e.Subject)
               .IsRequired()
               .HasMaxLength(80);

        builder.Property(e => e.HtmlBody)
               .IsRequired();

        builder.Property(e => e.PlainTextBody)
               .IsRequired();

        builder.Property(e => e.IsNewsletter)
               .IsRequired();

        // Many-to-Many with Recipients
        builder.HasMany(e => e.Recipients)
               .WithMany()
               .UsingEntity<Dictionary<string, object>>(
                   "EmailRecipient",
                   j => j.HasOne<Recipient>()
                         .WithMany()
                         .HasForeignKey("RecipientId")
                         .OnDelete(DeleteBehavior.Cascade),
                   j => j.HasOne<Email>()
                         .WithMany()
                         .HasForeignKey("EmailId")
                         .OnDelete(DeleteBehavior.Cascade),
                   j =>
                   {
                       j.HasKey("EmailId", "RecipientId");
                       j.ToTable("Join_EmailRecipients");
                   });

        // One-to-Many with Attachments
        builder.HasMany(e => e.Attachments)
               .WithOne()
               .HasForeignKey("EmailId")
               .OnDelete(DeleteBehavior.Cascade);
    }
}
