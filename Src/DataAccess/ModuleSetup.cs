using Contracts.DataAccess.Base;
using Contracts.DataAccess.Emails;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DataAccess.Base;
using DataAccess.Emails.Repositories;

namespace DataAccess;

public static class ModuleSetup
{
    public static async Task<IServiceCollection> InitializeDataAccessModule(this IServiceCollection serviceCollection, IConfigurationRoot configurationManager)
    {
        await DbContextSetup.Setup(serviceCollection, configurationManager);
        
        serviceCollection.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        serviceCollection.AddScoped(typeof(IEmailSendingRepository), typeof(EmailSendingRepository));

        return serviceCollection;
    }
}
