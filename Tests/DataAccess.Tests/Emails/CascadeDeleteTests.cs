using System.Data.Common;
using System.Net.Mime;
using Contracts.SupportModules.Logging;
using FluentResults;
using NSubstitute;
using DataAccess.Base;
using DataAccess.Errors;
using DomainModules.Emails.Entities;
using Microsoft.EntityFrameworkCore;
using TestHelpers;

namespace DataAccess.Tests.Emails;

[TestFixture, Category("IntegrationTests")]
public class CascadeDeleteTests : TestDatabaseFixture
{
    private GenericRepository<Email> _emailRepository;
    private GenericRepository<Attachment> _attachmentRepository;
    private GenericRepository<Recipient> _recipientRepository;
    
    [SetUp]
    public void Setup()
    {
        var logger = Substitute.For<ILoggingHandler>();
        
        _emailRepository = new GenericRepository<Email>(Context, logger);
        _attachmentRepository = new GenericRepository<Attachment>(Context, logger);
        _recipientRepository = new GenericRepository<Recipient>(Context, logger);
    }

    [Test]
    public async Task Delete_Email_CascadesToAttachmentsAndEmailRecipientAssociations()
    {
        // Verify initial data
        int initialEmailCount = (await _emailRepository.GetAllAsync()).Value.Count;
        int initialAttachmentCount = (await _attachmentRepository.GetAllAsync()).Value.Count;
        int initialRecipientCount = (await _recipientRepository.GetAllAsync()).Value.Count;
        long initialEmailRecipientCount = await GetJoinTableCount("EmailRecipients");

        Assert.Multiple(() =>
        {
            Assert.That(initialEmailCount, Is.EqualTo(1));
            Assert.That(initialAttachmentCount, Is.EqualTo(2));
            Assert.That(initialRecipientCount, Is.EqualTo(2));
            Assert.That(initialEmailRecipientCount, Is.EqualTo(2));
        });
        
        Result<Email> emailResult = await _emailRepository.GetByIdAsync(1);
        Email? email = emailResult.Value;

        Result deleteResult = await _emailRepository.DeleteAsync(email);

        Result<Email> getEmailResult = await _emailRepository.GetByIdAsync(1);
        Result<List<Attachment>> attachments = await _attachmentRepository.GetAllAsync();
        Result<List<Recipient>> recipients = await _recipientRepository.GetAllAsync();
        long emailRecipientCount = await GetJoinTableCount("EmailRecipients");
        
        Assert.Multiple(() =>
        {
            Assert.That(deleteResult.IsSuccess, Is.True);

            // Verify that the Email is deleted
            Assert.That(getEmailResult.IsSuccess, Is.False);
            Assert.That(getEmailResult.Errors.First(), Is.InstanceOf<NotFoundError>());

            // Verify that Attachments are also deleted
            Assert.That(attachments.IsSuccess, Is.True);
            Assert.That(attachments.Value, Is.Empty);

            // Verify that Recipients are NOT deleted (since they can be associated with other Emails)
            Assert.That(recipients.IsSuccess, Is.True);
            Assert.That(recipients.Value, Has.Count.EqualTo(2));

            // Verify that the EmailRecipient associations are deleted
            Assert.That(emailRecipientCount, Is.EqualTo(0));
        });
    }
    
    protected override async Task SeedDatabase()
    {
        // Seed Recipients
        var recipients = new List<Recipient>
        {
            new() { Id = 1, EmailAddress = "recipient1@example.com" },
            new() { Id = 2, EmailAddress = "recipient2@example.com" }
        };
        await Context.Set<Recipient>().AddRangeAsync(recipients);

        // Seed Attachments
        var attachments = new List<Attachment>
        {
            new()
            {
                Id = 1,
                Name = "Attachment1",
                ContentPath = new Uri("https://example.com/attachment1"),
                ContentType = new ContentType("application/pdf"),
                BinaryContent = [0x00, 0x01],
                EmailId = 1
            },
            new()
            {
                Id = 2,
                Name = "Attachment2",
                ContentPath = new Uri("https://example.com/attachment2"),
                ContentType = new ContentType("image/png"),
                BinaryContent = [0x02, 0x03],
                EmailId = 1
            }
        };
        await Context.Set<Attachment>().AddRangeAsync(attachments);

        // Seed Email
        var email = new Email
        {
            Id = 1,
            SenderEmailAddress = "sender@example.com",
            Subject = "Test Email",
            HtmlBody = "This is a test email.",
            PlainTextBody = "This is a test email.",
            Recipients = recipients,
            Attachments = attachments
        };
        
        await Context.Set<Email>().AddAsync(email);
    }
    
    private async Task<long> GetJoinTableCount(string tableName)
    {
        if (Context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
        {
            await Context.Database.OpenConnectionAsync();
        }

        await using DbCommand command = Context.Database.GetDbConnection().CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {tableName}";
        command.CommandType = System.Data.CommandType.Text;

        object? result = await command.ExecuteScalarAsync();
        return Convert.ToInt64(result);
    }
}
