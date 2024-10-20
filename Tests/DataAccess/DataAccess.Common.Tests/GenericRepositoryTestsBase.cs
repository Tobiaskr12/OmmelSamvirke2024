using Emails.Domain.Entities;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TestDatabaseFixtures;

namespace DataAccess.Common.Tests;

public class GenericRepositoryTestsBase : TestDatabaseFixture
{
    protected GenericRepository<Email> EmailRepository;
    
    [SetUp]
    public void Setup()
    {
        var logger = Substitute.For<ILogger>();
        EmailRepository = new GenericRepository<Email>(Context, logger);
    }
    
    protected override async Task SeedDatabase()
    {
        await SeedData.AddSeed(Context);
    }
}
