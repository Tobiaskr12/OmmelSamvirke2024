using Contracts.SupportModules.Logging;
using FluentResults;
using NSubstitute;
using DataAccess.Base;
using DomainModules.BlobStorage.Entities;
using DomainModules.Emails.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Tests.Emails;

[TestFixture, Category("IntegrationTests")]
public class CascadeDeleteTests : TestDatabaseFixture
{
    private GenericRepository<Email> _emailRepository;
    private GenericRepository<BlobStorageFile> _fileRepository;
    private GenericRepository<Recipient> _recipientRepository;
    
    [SetUp]
    public void Setup()
    {
        var logger = Substitute.For<ILoggingHandler>();
        
        _emailRepository = new GenericRepository<Email>(Context, logger);
        _fileRepository = new GenericRepository<BlobStorageFile>(Context, logger);
        _recipientRepository = new GenericRepository<Recipient>(Context, logger);
    }

    [Test]
    public async Task Delete_Email_CascadesToBlobStorageFilesAndEmailRecipientAssociations()
    {
        // Verify initial data
        int initialEmailCount = (await _emailRepository.GetAllAsync()).Value.Count;
        int initialFileCount = (await _fileRepository.GetAllAsync()).Value.Count;
        int initialRecipientCount = (await _recipientRepository.GetAllAsync()).Value.Count;
        long initialEmailRecipientCount = await GetJoinTableCount("Join_EmailRecipients");

        Assert.Multiple(() =>
        {
            Assert.That(initialEmailCount, Is.EqualTo(1));
            Assert.That(initialFileCount, Is.EqualTo(2));
            Assert.That(initialRecipientCount, Is.EqualTo(2));
            Assert.That(initialEmailRecipientCount, Is.EqualTo(2));
        });
        
        Result<Email> emailResult = await _emailRepository.GetByIdAsync(1, readOnly: false);
        Email? email = emailResult.Value;

        Result deleteResult = await _emailRepository.DeleteAsync(email);

        Result<Email> getEmailResult = await _emailRepository.GetByIdAsync(1);
        Result<List<BlobStorageFile>> files = await _fileRepository.GetAllAsync();
        Result<List<Recipient>> recipients = await _recipientRepository.GetAllAsync();
        long emailRecipientCount = await GetJoinTableCount("Join_EmailRecipients");
        
        Assert.Multiple(() =>
        {
            Assert.That(deleteResult.IsSuccess, Is.True);
            Assert.That(getEmailResult.IsSuccess, Is.False);
            Assert.That(files.Value, Is.Empty);
            Assert.That(recipients.Value, Has.Count.EqualTo(2));
            Assert.That(emailRecipientCount, Is.EqualTo(0));
        });
    }
    
    protected override async Task SeedDatabase()
    {
        var recipients = new List<Recipient>
        {
            new() { Id = 1, EmailAddress = "recipient1@example.com" },
            new() { Id = 2, EmailAddress = "recipient2@example.com" }
        };
        await Context.Set<Recipient>().AddRangeAsync(recipients);

        var files = new List<BlobStorageFile>
        {
            new()
            {
                Id = 1,
                FileBaseName = "BlobFile1",
                FileExtension = "pdf",
                ContentType = "application/pdf",
                FileContent = null
            },
            new()
            {
                Id = 2,
                FileBaseName = "BlobFile2",
                FileExtension = "png",
                ContentType = "image/png",
                FileContent = null
            }
        };
        await Context.Set<BlobStorageFile>().AddRangeAsync(files);

        var email = new Email
        {
            Id = 1,
            SenderEmailAddress = "sender@example.com",
            Subject = "Test Email",
            HtmlBody = "This is a test email.",
            PlainTextBody = "This is a test email.",
            Recipients = recipients,
            Attachments = files
        };
        
        await Context.Set<Email>().AddAsync(email);
    }
    
    private async Task<long> GetJoinTableCount(string tableName)
    {
        if (Context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
        {
            await Context.Database.OpenConnectionAsync();
        }

        await using var command = Context.Database.GetDbConnection().CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {tableName}";
        command.CommandType = System.Data.CommandType.Text;

        object? result = await command.ExecuteScalarAsync();
        return Convert.ToInt64(result);
    }
}
