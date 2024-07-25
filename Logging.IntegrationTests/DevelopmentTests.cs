using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Logging.IntegrationTests;

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

        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Testing.json")
            .AddEnvironmentVariables() 
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
