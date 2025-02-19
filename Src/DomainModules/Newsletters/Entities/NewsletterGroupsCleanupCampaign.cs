using DomainModules.Common;
using DomainModules.Emails.Entities;

namespace DomainModules.Newsletters.Entities;

public class NewsletterGroupsCleanupCampaign : BaseEntity
{
    public List<Recipient> UncleanedRecipients { get; set; } = [];
    public List<Recipient> CleanedRecipients { get; set; } = [];
    public required DateTime CampaignStart { get; set; }
    public DateTime? LastReminderSent { get; set; }
    public required int CampaignDurationMonths { get; set; }
    public bool IsCampaignStarted { get; set; }
}
