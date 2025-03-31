using System.Reflection;
using AutoFixture.Kernel;
using DomainModules.Events.Entities;

namespace ServiceModules.Tests.Config.Entities.Events;

public class EventCoordinatorSpecimenBuilder : IEntitySpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo propertyInfo && propertyInfo.DeclaringType == typeof(EventCoordinator))
        {
            switch (propertyInfo.Name)
            {
                case nameof(EventCoordinator.PhoneNumber):
                    return "12345678";
                case nameof(EventCoordinator.EmailAddress):
                    return "ommelsamvirketest1@gmail.com";
            }
        }
        return new NoSpecimen();
    }
}