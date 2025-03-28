using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainModules.Emails.Entities;

namespace DataAccess.Emails.Configuration;

public class ContactListEntityTypeConfiguration : IEntityTypeConfiguration<ContactList>
{
    public void Configure(EntityTypeBuilder<ContactList> builder)
    {
        builder.ToTable("ContactLists");
        builder.HasKey(cl => cl.Id);
        
        builder.Property(cl => cl.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(cl => cl.Description)
               .IsRequired()
               .HasMaxLength(2000);
        
        builder.Property(cl => cl.UnsubscribeToken)
               .IsRequired();

        // Many-to-Many relationship with Recipients
        builder.HasMany(cl => cl.Contacts)
           .WithMany()
           .UsingEntity<Dictionary<string, object>>(
               "ContactListRecipient",
               j => j.HasOne<Recipient>()
                     .WithMany()
                     .HasForeignKey("RecipientId")
                     .OnDelete(DeleteBehavior.Cascade),
               j => j.HasOne<ContactList>()
                     .WithMany()
                     .HasForeignKey("ContactListId")
                     .OnDelete(DeleteBehavior.Cascade),
               j =>
               {
                   j.HasKey("ContactListId", "RecipientId");
                   j.ToTable("Join_ContactListRecipients");
               });
        
        builder.Navigation(cl => cl.Contacts).AutoInclude();
    }
}
