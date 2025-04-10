using DomainModules.BlobStorage.Entities;
using DomainModules.Common;
using DomainModules.Reservations.Entities;

namespace DomainModules.Events.Entities;

public class Event : BaseEntity
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public required EventCoordinator EventCoordinator { get; set; }
    public required string Location { get; set; }
    public List<BlobStorageFile> RemoteFiles { get; set; } = [];
    public Reservation? Reservation { get; set; }
    
    // Used as identifier for ICS-file
    public Guid Uid { get; set; } = Guid.NewGuid();
}
