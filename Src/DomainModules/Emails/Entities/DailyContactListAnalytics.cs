using DomainModules.Common;

namespace DomainModules.Emails.Entities;

public class DailyContactListAnalytics : BaseEntity
{
    public required string ContactListName { get; init; }
    public required int TotalContacts { get; init; }
    public required DateTime Date { get; init; }
    public required bool IsNewsletter { get; set; }
}
