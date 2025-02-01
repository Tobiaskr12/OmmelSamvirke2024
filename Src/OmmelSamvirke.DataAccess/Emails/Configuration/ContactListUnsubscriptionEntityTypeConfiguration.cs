using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OmmelSamvirke.DomainModules.Emails.Entities;

namespace OmmelSamvirke.DataAccess.Emails.Configuration;

public class ContactListUnsubscriptionEntityTypeConfiguration : IEntityTypeConfiguration<ContactListUnsubscription>
{
    public void Configure(EntityTypeBuilder<ContactListUnsubscription> builder)
    {
        builder.ToTable("ContactListUnsubscriptions");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.UndoToken)
               .IsRequired();

        builder.Property(c => c.EmailAddress)
               .IsRequired()
               .HasMaxLength(256);

        builder.Property(c => c.ContactListId)
               .IsRequired();
    }
}
