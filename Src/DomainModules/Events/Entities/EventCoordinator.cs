using DomainModules.Common;

namespace DomainModules.Events.Entities;

public class EventCoordinator : BaseEntity
{
    public required string Name { get; set; }
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }
}
