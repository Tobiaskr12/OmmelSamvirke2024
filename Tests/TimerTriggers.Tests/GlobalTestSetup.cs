using AutoFixture;
using Bootstrapper;
using Contracts.SupportModules.SecretsManager;
using DataAccess.Base;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceModules.Tests.Config;
using SupportModules.SecretsManager;

namespace TimerTriggers.Tests;

[SetUpFixture]
public class GlobalTestSetup
{
    public static IConfigurationRoot Configuration { get; private set; } = null!;
    public static ServiceProvider ServiceProvider  { get; private set; } = null!;
    public static OmmelSamvirkeDbContext DbContext { get; private set; } = null!;
    public static IMediator Mediator { get; private set; } = null!;
    public static TestEmailClient TestEmailClientOne { get; private set; } = null!;
    public static TestEmailClient TestEmailClientTwo { get; private set; } = null!;
    public static Fixture Fixture { get; private set; } = null!;
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var services = new ServiceCollection();
        Configuration = new ConfigurationBuilder()
                        .AddKeyVaultSecrets(ExecutionEnvironment.Testing)
                        .Build();
        
        services.AddSingleton<IConfiguration>(Configuration);
        services.AddSingleton(Configuration); // Adds as IConfigurationRoot
        
        services.InitializeAllServices(Configuration, ExecutionEnvironment.Testing);
        
        ServiceProvider = services.BuildServiceProvider();
        
        Mediator = ServiceProvider.GetService<IMediator>() ?? throw new Exception("Mediator service not found");
        DbContext = ServiceProvider.GetService<OmmelSamvirkeDbContext>() ?? throw new Exception("DB Context not found");

        SetupEmailClients();

        Fixture = FixtureFactory.CreateFixture();
    }

    private static void SetupEmailClients()
    {
        SetupEmailTestAccount(
            testClient: out TestEmailClient client1,
            emailAddress: "ommelsamvirketest1@gmail.com",
            passwordConfigSection: "EmailTestClientOneAppPassword"
        );

        SetupEmailTestAccount(
            testClient: out TestEmailClient client2,
            emailAddress: "ommelsamvirketest2@gmail.com",
            passwordConfigSection: "EmailTestClientTwoAppPassword"
        );

        TestEmailClientOne = client1;
        TestEmailClientTwo = client2;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        ServiceProvider.Dispose();
    }
    
    private static void SetupEmailTestAccount(out TestEmailClient testClient, string emailAddress, string passwordConfigSection)
    {
        string? accountPassword = Configuration[passwordConfigSection];
        if (accountPassword is null)
            throw new Exception($"Cannot read the password for the email test account at the config section {passwordConfigSection}");

        testClient = new TestEmailClient
        {
            EmailAddress = emailAddress,
            AccountPassword = accountPassword,
            ImapHost = "imap.gmail.com",
            ImapPort = 993
        };
    }
    
    public class TestEmailClient
    {
        public required string EmailAddress { get; init; }
        public required string AccountPassword { get; init; }
        public required string ImapHost { get; init; }
        public required int ImapPort { get; init; }
    }
}
