using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Logging.Tests;

public class ProductionTests
{
    private ILogger _logger;
    private IConfigurationRoot _config;

    [SetUp]
    public void SetUp()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        
        _config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Testing.json")
            .AddEnvironmentVariables() 
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
        using var connection = new SqlConnection(_config.GetConnectionString("DefaultDbConnection"));
        connection.Open();
        
        IEnumerable<string> logEntries = connection.Query<string>("SELECT TOP 1 Message FROM Logs ORDER BY Id DESC");
        return logEntries.First();
    }
}
