using Contracts.SupportModules.Logging;
using NSubstitute;
using DataAccess.Base;
using DomainModules.Emails.Entities;
using TestDatabaseFixtures;

namespace DataAccess.Tests.Common;

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
