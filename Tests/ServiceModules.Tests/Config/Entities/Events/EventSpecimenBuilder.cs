using System.Reflection;
using AutoFixture.Kernel;
using DomainModules.BlobStorage.Entities;
using DomainModules.Events.Entities;
using DomainModules.Reservations.Entities;

namespace ServiceModules.Tests.Config.Entities.Events;

public class EventSpecimenBuilder : IEntitySpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo propertyInfo && propertyInfo.DeclaringType == typeof(Event))
        {
            switch (propertyInfo.Name)
            {
                case nameof(Event.Reservation):
                    return null!;
                case nameof(Event.StartTime):
                    return DateTime.UtcNow.AddHours(1);
                case nameof(Event.EndTime):
                    return DateTime.UtcNow.AddHours(3);
                case nameof(Event.RemoteFiles):
                    return new List<BlobStorageFile>();
            }
        }
        return new NoSpecimen();
    }
}