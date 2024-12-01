using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.SupportModules.Logging;
using OmmelSamvirke.SupportModules.SecretsManager;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace OmmelSamvirke.SupportModules.Tests.Logging;

public class DevelopmentTests
{
    private StringWriter _output;
    private ILogger _logger;

    [SetUp]
    public void SetUp()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        
        // Redirect console output to a StringWriter for testing
        _output = new StringWriter();
        Console.SetOut(_output);

        // While the ASPNETCORE_ENVIRONMENT is set to development, we still use the testing database
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddKeyVaultSecrets(ExecutionEnvironment.Testing)
            .Build();
        
        _logger = AppLoggerFactory.CreateLogger(config);
    }

    [Test]
    public void GivenConfiguredConsoleLogger_WhenLoggingInfo_ConsoleContainsLoggedMessage()
    {
        const string logMessage = "This is a test log message";
        _logger.LogInformation(logMessage);

        // Reset console output
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        
        var loggedOutput = _output.ToString();
        Assert.That(loggedOutput, Is.SupersetOf(logMessage));
    }
    
    [Test]
    public void GivenConfiguredConsoleLogger_WhenLoggingInfo_ConsoleTextDoesNotMatchRandomString()
    {
        const string logMessage = "This is a test log message";
        _logger.LogInformation(logMessage);

        // Reset console output
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        
        var loggedOutput = _output.ToString();
        Assert.That(loggedOutput, Is.Not.SupersetOf("This is a random string"));
    }

    [TearDown]
    public void TearDown()
    {
        _output.Dispose();
    }
}
