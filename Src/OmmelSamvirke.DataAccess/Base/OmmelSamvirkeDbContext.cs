using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OmmelSamvirke.DomainModules.Common;
using OmmelSamvirke.DomainModules.Emails.Entities;

namespace OmmelSamvirke.DataAccess.Base;

public class OmmelSamvirkeDbContext : DbContext
{
    public DbSet<Email> Emails { get; init; }
    public DbSet<Recipient> Recipients { get; init; }
    public DbSet<Attachment> Attachments { get; init; }
    public DbSet<ContactList> ContactLists { get; init; }
    public DbSet<DailyEmailAnalytics> DailyEmailAnalytics { get; init; }
    public DbSet<DailyContactListAnalytics> DailyContactListAnalytics { get; init; }
    public DbSet<ContactListUnsubscription> ContactListUnsubscriptions { get; init; }
    
    public OmmelSamvirkeDbContext(DbContextOptions<OmmelSamvirkeDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OmmelSamvirkeDbContext).Assembly);
        
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
