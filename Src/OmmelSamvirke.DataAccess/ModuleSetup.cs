using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DataAccess.Emails.Interfaces;
using OmmelSamvirke.DataAccess.Emails.Repositories;

namespace OmmelSamvirke.DataAccess;

public static class ModuleSetup
{
    public static async Task<IServiceCollection> InitializeDataAccessModule(this IServiceCollection serviceCollection, IConfigurationManager configurationManager)
    {
        await DbContextSetup.Setup(serviceCollection, configurationManager);
        
        serviceCollection.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        serviceCollection.AddScoped(typeof(IEmailSendingRepository), typeof(EmailSendingRepository));

        return serviceCollection;
    }
}
