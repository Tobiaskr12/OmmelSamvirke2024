using System.Reflection;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using OmmelSamvirke.DataAccess.Base;

namespace TestDatabaseFixtures;

public abstract class TestDatabaseFixture
{
    protected OmmelSamvirkeDbContext Context { get; private set; }

    [SetUp]
    public async Task TestSetup()
    {
        DbContextOptions<OmmelSamvirkeDbContext> options = 
            new DbContextOptionsBuilder<OmmelSamvirkeDbContext>()
                .UseSqlite("Data Source=:memory:")
                .EnableSensitiveDataLogging()
                .Options;

        Context = new OmmelSamvirkeDbContext(options);

        await Context.Database.OpenConnectionAsync();
        await Context.Database.EnsureCreatedAsync();

        await SeedDatabase();
        await Context.SaveChangesAsync();
    }

    [TearDown]
    public async Task TestTearDown()
    {
        if (!IsContextDisposed())
        {
            await Context.Database.CloseConnectionAsync();
            await Context.DisposeAsync();
        }
    }
    
    protected abstract Task SeedDatabase();
    
    private bool IsContextDisposed()
    {
        var result = true;            
        Type typeDbContext = typeof(OmmelSamvirkeDbContext);
        FieldInfo? isDisposedTypeField = typeDbContext.GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance);

        if (isDisposedTypeField != null)
        {
            result = (bool)(isDisposedTypeField.GetValue(Context) ?? true);
        }

        return result;
    }
}
