using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess.Base;

public static class DbContextSetup
{
    public static async Task Setup(IServiceCollection services, IConfigurationRoot configurationManager)
    {
        try
        {
            string? connectionString = configurationManager.GetSection("SqlServerConnectionString").Value;
        
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Unable to get database connectionString");
            }
            
            services.AddDbContext<OmmelSamvirkeDbContext>(options => options.UseSqlServer(connectionString));
            
            await using ServiceProvider serviceProvider = services.BuildServiceProvider();
            using IServiceScope scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OmmelSamvirkeDbContext>();
            
            await dbContext.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
