using Contracts.SupportModules.SecretsManager;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OmmelSamvirke.Bootstrapper;
using OmmelSamvirke.SupportModules.SecretsManager;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Initialize configuration
ExecutionEnvironment executionEnvironment = builder.Environment.IsDevelopment()
            ? ExecutionEnvironment.Development
            : ExecutionEnvironment.Production;

IConfigurationRoot configuration =
            builder.Configuration
                   .AddKeyVaultSecrets(executionEnvironment)
                   .AddEnvironmentVariables()
                   .Build();

builder.Services.AddSingleton(configuration);

// Register all service-layers
builder.Services.InitializeAllServices(configuration, executionEnvironment);

builder.Build().Run();
