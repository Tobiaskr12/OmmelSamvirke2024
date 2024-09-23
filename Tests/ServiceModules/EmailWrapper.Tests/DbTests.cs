using DataAccess.Common;
using TestDatabaseFixtures;

namespace EmailWrapper.Tests;

public class DbTests
{
    private OmmelSamvirkeDbContext _dbContext;
    
    [SetUp]
    public void Setup()
    {
        _dbContext = TestDatabaseFixture.GetInstance().GetContext();
    }

    [Test]
    public void Test()
    {
        var email = _dbContext.Emails.First();
        var newBodyText = "This is a test!";

        _dbContext.Database.BeginTransaction();
        email.Body = newBodyText;
        _dbContext.SaveChanges();
        _dbContext.ChangeTracker.Clear();

        var savedEmail = _dbContext.Emails.Single(e => e.Id == email.Id);
        Assert.That(newBodyText, Is.EqualTo(savedEmail.Body));
    }
    
    [TearDown]
    public async Task Teardown()
    {
        await _dbContext.DisposeAsync();
    }
}
