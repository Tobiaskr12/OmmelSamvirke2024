using DataAccess.Base;
using DomainModules.Common;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;

namespace ServiceModules.Tests;

public abstract class ServiceTestBase
{
    [SetUp]
    public async Task Setup()
    {
        await ResetDatabase();
    }
    
    protected static T GetService<T>() where T : class => GlobalTestSetup.ServiceProvider.GetService<T>() ?? throw new Exception($"Service of type {typeof(T).Name} not found");
    
    protected static async Task ResetDatabase()
    {
        var dbContext = GetService<OmmelSamvirkeDbContext>();
        if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
        
        if (GlobalTestSetup.Configuration.GetSection("ExecutionEnvironment").Value == "Test")
        {
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.MigrateAsync();
            dbContext.ChangeTracker.Clear();
        }
    }

    protected static async Task AddTestData<T>(T entity) where T : BaseEntity
    {
        await GlobalTestSetup.DbContext.AddAsync(entity);
        await GlobalTestSetup.DbContext.SaveChangesAsync();
    }
    
    protected static async Task AddTestData<T>(List<T> entities) where T : BaseEntity
    {
        await GlobalTestSetup.DbContext.AddRangeAsync(entities);
        await GlobalTestSetup.DbContext.SaveChangesAsync();
    }
    
    protected static async Task<MimeMessage?> GetLatestEmailAsync(GlobalTestSetup.TestEmailClient testEmailClient, string subjectIdentifier)
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
            IList<UniqueId> ids = await inbox.SearchAsync(SearchQuery.All);
            if (ids.Any())
            {
                // Grab the most recent message.
                MimeMessage message = await inbox.GetMessageAsync(ids.Last());
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
}
