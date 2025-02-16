using DomainModules.Common;
using DomainModules.Emails.Entities;

namespace DomainModules.Newsletters.Entities;

public class NewsletterGroupsCleanupCampaign : BaseEntity
{
    public required List<Recipient> UncleanedRecipients { get; set; }
    public List<Recipient> CleanedRecipients { get; set; } = [];
    public required DateTime CampaignStart { get; set; }
    public required int CampaignDurationMonths { get; set; }
}
