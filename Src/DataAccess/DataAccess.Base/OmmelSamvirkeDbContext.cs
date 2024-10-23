using Domain.Common;
using Emails.Domain.Entities;
using Emails.DataAccess.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DataAccess.Base;

public class OmmelSamvirkeDbContext : DbContext
{
    public DbSet<Email> Emails { get; init; }
    public DbSet<Recipient> Recipients { get; init; }
    public DbSet<Attachment> Attachments { get; init; }
    public DbSet<ContactList> ContactLists { get; init; }
    
    public OmmelSamvirkeDbContext(DbContextOptions<OmmelSamvirkeDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OmmelSamvirkeDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmailEntityTypeConfiguration).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        foreach (EntityEntry<BaseEntity> entry in ChangeTracker.Entries<BaseEntity>().Where(q =>
                     q.State is EntityState.Added or EntityState.Modified))
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.DateCreated = DateTime.UtcNow;
                entry.Entity.DateModified = entry.Entity.DateCreated;
            }
            else
            {
                entry.Property(nameof(BaseEntity.DateCreated)).IsModified = false;
                entry.Entity.DateModified = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
