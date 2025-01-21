using OmmelSamvirke.DomainModules.Common;

namespace OmmelSamvirke.DomainModules.Emails.Entities;

public class DailyEmailAnalytics : BaseEntity
{
    public required DateTime Date { get; init; }
    public required int SentEmails { get; init; }
    public required int TotalRecipients { get; init; }
}
