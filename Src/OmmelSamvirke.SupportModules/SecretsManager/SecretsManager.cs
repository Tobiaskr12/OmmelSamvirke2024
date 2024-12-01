using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace OmmelSamvirke.SupportModules.SecretsManager;

public static class SecretsManager
{
    public static IConfigurationBuilder AddKeyVaultSecrets(this IConfigurationBuilder builder, ExecutionEnvironment environment)
    {
        string keyVaultUrl = environment switch
        {
            ExecutionEnvironment.Development => "https://os-key-vault-dev.vault.azure.net/",
            ExecutionEnvironment.Testing => "https://os-key-valut-test.vault.azure.net/",
            ExecutionEnvironment.Production => "https://ommel-samvirke-key-vault.vault.azure.net/",
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
