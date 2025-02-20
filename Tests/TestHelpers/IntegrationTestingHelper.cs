using Bootstrapper;
using Contracts.SupportModules.SecretsManager;
using DataAccess.Base;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;
using SupportModules.SecretsManager;

namespace TestHelpers;

public class IntegrationTestingHelper
{
    private IMediator? _mediator;
    private IConfigurationRoot? _configuration;
    private ServiceProvider? _serviceProvider;
    private TestEmailClient? _testEmailClientOne;
    private TestEmailClient? _testEmailClientTwo;
    
    public IMediator Mediator
    {
        get
        {
            if (_mediator is null)
            {
                Setup();
            }
            
            return _mediator!;
        }
        private set => _mediator = value;
    }

    public IConfigurationRoot Configuration
    {
        get
        {
            if (_configuration is null)
            {
                Setup();
            }
            
            return _configuration!;
        }
        private set => _configuration = value;
    }

    public TestEmailClient TestEmailClientOne
    {
        get
        {
            if (_testEmailClientOne is null)
            {
                Setup();
            }

            return _testEmailClientOne!;
        }
        private set => _testEmailClientOne = value;
    }
    
    public TestEmailClient TestEmailClientTwo
    {
        get
        {
            if (_testEmailClientTwo is null)
            {
                Setup();
            }

            return _testEmailClientTwo!;
        }
        private set => _testEmailClientTwo = value;
    }
    
    public T GetService<T>() where T : class => ServiceProvider.GetService<T>() ?? throw new Exception($"Service of type {typeof(T).Name} not found");
    
    public async Task ResetDatabase()
    {
        var dbContext = ServiceProvider.GetService<OmmelSamvirkeDbContext>();
        if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
        
        if (_configuration?.GetSection("ExecutionEnvironment").Value == "Test")
        {
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.MigrateAsync();
            dbContext.ChangeTracker.Clear();
        }
    }
    
    public async Task<MimeMessage?> GetLatestEmailAsync(TestEmailClient testEmailClient, string subjectIdentifier)
    {
        using var client = new ImapClient();
        await client.ConnectAsync(testEmailClient.ImapHost, testEmailClient.ImapPort, SecureSocketOptions.SslOnConnect);
        await client.AuthenticateAsync(testEmailClient.EmailAddress, testEmailClient.AccountPassword);
        IMailFolder? inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadOnly);

        MimeMessage? foundMessage = null;
        DateTime timeout = DateTime.UtcNow.AddMinutes(2);
        while (DateTime.UtcNow < timeout)
        {
            // Search for all messages in the inbox.
            IList<UniqueId> uids = await inbox.SearchAsync(SearchQuery.All);
            if (uids.Any())
            {
                // Grab the most recent message.
                MimeMessage message = await inbox.GetMessageAsync(uids.Last());
                if (message.Subject != null && message.Subject.Contains(subjectIdentifier))
                {
                    foundMessage = message;
                    break;
                }
            }
            await Task.Delay(2500);
        }
        await client.DisconnectAsync(true);
        return foundMessage;
    }

    private ServiceProvider ServiceProvider
    {
        get
        {
            if (_serviceProvider is null)
            {
                Setup();
            }
            
            return _serviceProvider!;
        }
        set => _serviceProvider = value;
    }

    private void Setup()
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
    
    private void SetupEmailTestAccount(out TestEmailClient testClient, string emailAddress, string passwordConfigSection)
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
