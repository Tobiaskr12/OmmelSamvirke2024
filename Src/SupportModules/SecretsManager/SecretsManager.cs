using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace SecretsManager;

public static class SecretsManager
{
    public static IConfigurationBuilder AddKeyVaultSecrets(this IConfigurationBuilder builder, ExecutionEnvironment environment)
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("KeyVaultUrls.secrets.json")
            .AddEnvironmentVariables() 
            .Build();

        string? keyVaultUrl = environment switch
        {
            ExecutionEnvironment.Development => config["KEY_VAULT_URL_DEV"],
            ExecutionEnvironment.Testing => config["KEY_VAULT_URL_TEST"],
            ExecutionEnvironment.Production => config["KEY_VAULT_URL_PROD"],
            _ => 
                throw new ArgumentOutOfRangeException(
                    nameof(environment),
                    environment,
                    $"{nameof(environment)} is not a valid execution environment"
                )
        };

        if (string.IsNullOrEmpty(keyVaultUrl))
        {
            throw new Exception("KEY_VAULT_URL_<ENV> must be specified for this environment");
        }
        
        builder.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential(includeInteractiveCredentials: false));

        return builder;
    }
}
