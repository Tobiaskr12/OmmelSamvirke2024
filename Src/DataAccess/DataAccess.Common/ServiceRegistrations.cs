using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess.Common;

public static class ServiceRegistrations
{
    public static async Task AddPersistenceServices(this IServiceCollection services, IConfigurationManager configurationManager)
    {
        await SetupDbConnection(services, configurationManager);
    }

    private static async Task SetupDbConnection(IServiceCollection services, IConfigurationManager configurationManager)
    {
        string? connectionString = configurationManager.GetSection("SqlServerConnectionString").Value;
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<OmmelSamvirkeDbContext>(options =>
                options.UseSqlServer(connectionString));
        }
        else
        {
            throw new Exception("Unable to get database connectionString");
        }
        
        // Run any pending migrations on startup
        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        using IServiceScope scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OmmelSamvirkeDbContext>();
        if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
        {
            await dbContext.Database.MigrateAsync();
        }
    }
}
