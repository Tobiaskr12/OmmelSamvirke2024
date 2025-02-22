using System.Net.Mime;
using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;

namespace ServiceModules.Tests.Config;

public static class FixtureFactory
{
    public static Fixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new OmitBaseEntityIdSpecimenBuilder());
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        fixture.Register(() => new ContentType("application/octet-stream"));

        IEnumerable<Type> customSpecimenBuilders = Assembly.GetExecutingAssembly().GetTypes().Where(t => 
            typeof(IEntitySpecimenBuilder).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false }
        );
        
        foreach (Type builderType in customSpecimenBuilders)
        {
            var builder = (ISpecimenBuilder)Activator.CreateInstance(builderType)!;
            fixture.Customizations.Add(builder);
        }

        return fixture;
    }
}
