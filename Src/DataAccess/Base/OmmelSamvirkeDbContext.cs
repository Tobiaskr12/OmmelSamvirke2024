using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using DomainModules.Common;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Base;

public sealed class OmmelSamvirkeDbContext : DbContext
{
    // Emails
    public DbSet<Email> Emails { get; init; }
    public DbSet<Recipient> Recipients { get; init; }
    public DbSet<Attachment> Attachments { get; init; }
    public DbSet<ContactList> ContactLists { get; init; }
    public DbSet<DailyEmailAnalytics> DailyEmailAnalytics { get; init; }
    public DbSet<DailyContactListAnalytics> DailyContactListAnalytics { get; init; }
    public DbSet<ContactListUnsubscription> ContactListUnsubscriptions { get; init; }
    
    // Newsletter
    public DbSet<NewsletterGroup> NewsletterGroups { get; init; }
    public DbSet<NewsletterGroupsCleanupCampaign> NewsletterGroupsCleanupCampaigns { get; init; }
    public DbSet<NewsletterSubscriptionConfirmation> NewsletterSubscriptionConfirmations { get; init; }
    public DbSet<NewsletterUnsubscribeConfirmation> NewsletterUnsubscribeConfirmations { get; init; }

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
                // Allows manually setting DateCreated for integration testing
                if (entry.Entity.DateCreated is not null)
                {   
                    return base.SaveChangesAsync(cancellationToken);
                }
                
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
