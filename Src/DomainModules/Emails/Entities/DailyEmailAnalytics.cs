using DomainModules.Common;

namespace DomainModules.Emails.Entities;

public class DailyEmailAnalytics : BaseEntity
{
    public required DateTime Date { get; set; }
    public required int SentEmails { get; init; }
    public required int TotalRecipients { get; init; }
}
