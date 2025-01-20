using OmmelSamvirke.DomainModules.Common;

namespace OmmelSamvirke.DomainModules.Emails.Entities;

public class DailyEmailAnalytics : BaseEntity
{
    public DateTime Date { get; init; }
    public int SentEmails { get; init; }
    public int TotalRecipients { get; init; }
}
