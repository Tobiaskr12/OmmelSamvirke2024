using DataAccess.Common;
using EmailWrapper.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OmmelSamvirke2024.Domain;
using SecretsManager;

namespace TestDatabaseFixtures;

public class TestDatabaseFixture
{
    private static readonly object Lock = new();
    private static bool _databaseInitialized;
    private static TestDatabaseFixture? _instance;
    private readonly string _connectionString;

    public static TestDatabaseFixture GetInstance() => _instance ??= new TestDatabaseFixture();
    
    private TestDatabaseFixture()
    {
        _connectionString = GetDbConnectionString();
        
        lock (Lock)
        {
            if (_databaseInitialized) return;
            
            using (OmmelSamvirkeDbContext context = GetContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // context.AddRange(
                //     AddEntityData(Email.Create("admins@ommelsamvirke.com", "test2", "test body 2", Recipient.Create(["tobias@example.com"])).Value)
                // );
                
                context.SaveChanges();
            }

            _databaseInitialized = true;
        }
    }

    public OmmelSamvirkeDbContext GetContext()
    {
        return new OmmelSamvirkeDbContext(
            new DbContextOptionsBuilder<OmmelSamvirkeDbContext>()
                .UseSqlServer(_connectionString)
                .Options);
    }

    private static BaseEntity AddEntityData(BaseEntity entity)
    {
        DateTime now = DateTime.UtcNow;
        entity.DateCreated = now;
        entity.DateModified = now;

        return entity;
    }

    private static string GetDbConnectionString()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddKeyVaultSecrets(ExecutionEnvironment.Testing)
            .Build();
        
        string? connectionString = config.GetSection("SqlServerConnectionString").Value;
        if (connectionString is null)
            throw new Exception($"Cannot create {nameof(TestDatabaseFixture)}. No DB connection string could be found");

        return connectionString;
    }
}
