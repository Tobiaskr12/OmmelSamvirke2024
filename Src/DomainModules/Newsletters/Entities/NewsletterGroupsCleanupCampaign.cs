using DomainModules.Common;
using DomainModules.Emails.Entities;

namespace DomainModules.Newsletters.Entities;

public class NewsletterGroupsCleanupCampaign : BaseEntity
{
    public List<Recipient> UnconfirmedRecipients { get; set; } = [];
    public required DateTime CampaignStart { get; set; }
    public DateTime? LastReminderSent { get; set; }
    public required int CampaignDurationMonths { get; set; }
    public bool IsCampaignStarted { get; set; }
}
