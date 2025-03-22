using System.Reflection;
using AutoFixture.Kernel;
using DomainModules.Reservations.Entities;

namespace ServiceModules.Tests.Config.Entities.Reservations;

public class ReservationSpecimenBuilder : IEntitySpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo propertyInfo && propertyInfo.DeclaringType == typeof(Reservation))
        {
            switch (propertyInfo.Name)
            {
                case nameof(Reservation.PhoneNumber):
                    return "12345678";
                case nameof(Reservation.ReservationSeriesId):
                    return null!;
                case nameof(Reservation.Email):
                    return "ommelsamvirketest1@gmail.com";
            }
        }
        return new NoSpecimen();
    }
}
