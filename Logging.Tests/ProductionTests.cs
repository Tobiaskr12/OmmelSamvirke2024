using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SecretsManager;

namespace Logging.Tests;

public class ProductionTests
{
    private ILogger _logger;
    private IConfigurationRoot _config;

    [SetUp]
    public void SetUp()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        
        // While the ASPNETCORE_ENVIRONMENT is set to production, we still use the testing database
        _config = new ConfigurationBuilder()
            .AddKeyVaultSecrets(ExecutionEnvironment.Testing)
            .Build();
        
        _logger = AppLoggerFactory.CreateLogger(_config);
    }

    [Test]
    public void GivenConfiguredDbLogger_WhenLoggingInfo_DbContainsLoggedMessage()
    {
        const string logMessage = "This is a test log message";
        _logger.LogInformation(logMessage);
        
        // Batching and sending logs takes up to 5 seconds
        Thread.Sleep(5001);
        
        Assert.That(GetLatestLogMessageInDatabase(), Is.EqualTo(logMessage));
    }
    
    private string GetLatestLogMessageInDatabase()
    {
        using var connection = new SqlConnection(_config.GetValue<string>("SqlServerConnectionString"));
        connection.Open();
        
        IEnumerable<string> logEntries = connection.Query<string>("SELECT TOP 1 Message FROM Logs ORDER BY Id DESC");
        return logEntries.First();
    }
}
