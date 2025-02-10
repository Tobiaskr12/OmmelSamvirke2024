using NSubstitute;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.SupportModules.Logging.Interfaces;
using TestDatabaseFixtures;

namespace OmmelSamvirke.DataAccess.Tests.Common;

public class GenericRepositoryTestsBase : TestDatabaseFixture
{
    protected GenericRepository<Email> EmailRepository;
    
    [SetUp]
    public void Setup()
    {
        var logger = Substitute.For<ILoggingHandler>();
        EmailRepository = new GenericRepository<Email>(Context, logger);
    }
    
    protected override async Task SeedDatabase()
    {
        await SeedData.AddSeed(Context);
    }
}
