using System.Reflection;
using AutoFixture.Kernel;
using DomainModules.Reservations.Entities;

namespace ServiceModules.Tests.Config.Entities.Reservations;

public class ReservationHistorySpecimenBuilder : IEntitySpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo propertyInfo && propertyInfo.DeclaringType == typeof(ReservationHistory))
        {
            switch (propertyInfo.Name)
            {
                case nameof(ReservationHistory.Email):
                    return "ommelsamvirketest1@gmail.com";
            }
        }
        return new NoSpecimen();
    }
}