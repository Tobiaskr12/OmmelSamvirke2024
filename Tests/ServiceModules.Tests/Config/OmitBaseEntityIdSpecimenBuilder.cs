using System.Reflection;
using AutoFixture.Kernel;
using DomainModules.Common;

namespace ServiceModules.Tests.Config;

public class OmitBaseEntityIdSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo { Name: "Id" } propertyInfo && typeof(BaseEntity).IsAssignableFrom(propertyInfo.DeclaringType))
        {
            return new OmitSpecimen();
        }
        return new NoSpecimen();
    }
}
