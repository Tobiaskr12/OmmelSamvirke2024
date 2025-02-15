using FluentResults;
using DataAccess.Errors;
using DomainModules.Emails.Entities;

namespace DataAccess.Tests.Common;

[TestFixture, Category("IntegrationTests")]
public class InsertTests : GenericRepositoryTestsBase
{
    [Test]
    public async Task AddAsync_ValidEntity_AddsEntity()
    {
        const string senderEmailAddress = "auto@ommelsamvirke.com";
        const string subject = "New Email";
        const string body = "This is a new email.";
        var newEmail = new Email
        {
            SenderEmailAddress = senderEmailAddress,
            Subject = subject,
            HtmlBody = body,
            PlainTextBody = body,
            Recipients = [],
            Attachments = []
        };
        
        Result<Email> addResult = await EmailRepository.AddAsync(newEmail);

        Assert.Multiple(() =>
        {
            Assert.That(addResult.IsSuccess, Is.True);
            Assert.That(addResult.Value.SenderEmailAddress, Is.EqualTo(senderEmailAddress));
            Assert.That(addResult.Value.Subject, Is.EqualTo(subject));
            Assert.That(addResult.Value.HtmlBody, Is.EqualTo(body));
            Assert.That(addResult.Value.Id, Is.Not.Default);
            Assert.That(addResult.Value.DateCreated, Is.Not.Null);
            Assert.That(addResult.Value.DateCreated, Is.Not.Default);
            Assert.That(addResult.Value.DateModified, Is.Not.Null);
            Assert.That(addResult.Value.DateModified, Is.Not.Default);
        });
    }

    [Test]
    public async Task AddAsync_InvalidEntity_ReturnsDatabaseError()
    {
        var newEmail = new Email
        {
            Subject = null!,
            HtmlBody = null!,
            PlainTextBody = null!,
            SenderEmailAddress = null!,
            Recipients = null!,
            Attachments = null!
        };
        
        Result<Email> addResult = await EmailRepository.AddAsync(newEmail);

        Assert.Multiple(() =>
        {
            Assert.That(addResult.IsSuccess, Is.False);
            Assert.That(addResult.Errors.First(), Is.InstanceOf<DatabaseError>());
        });
    }
    
    [Test]
    public async Task AddAsync_DatabaseException_ReturnsDatabaseError()
    {
        await Context.DisposeAsync(); // Simulate database exception
        var newEmail = new Email
        {
            SenderEmailAddress = "new_sender@example.com",
            Subject = "New Email",
            HtmlBody = "This is a new email.",
            PlainTextBody = "This is a new email.",
            Recipients = [],
            Attachments = []
        };
        
        Result<Email> result = await EmailRepository.AddAsync(newEmail);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.First(), Is.InstanceOf<DatabaseError>());
        });
    }
    
    [Test]
    public async Task AddAsync_ValidEntities_AddsEntities()
    {
        List<Email> newEmails =
        [
            new Email
            {
                SenderEmailAddress = "bulk1@example.com",
                Subject = "Bulk Email 1",
                HtmlBody = "This is the first bulk email.",
                PlainTextBody = "This is the first bulk email.",
                Recipients = [],
                Attachments = []
            },

            new Email
            {
                SenderEmailAddress = "bulk2@example.com",
                Subject = "Bulk Email 2",
                HtmlBody = "This is the second bulk email.",
                PlainTextBody = "This is the second bulk email.",
                Recipients = [],
                Attachments = []
            }
        ];

        Result<List<Email>> addResult = await EmailRepository.AddAsync(newEmails);
        Result<List<Email>> allEmailsResult = await EmailRepository.GetAllAsync();

        Assert.Multiple(() =>
        {
            Assert.That(addResult.IsSuccess, Is.True);
            Assert.That(addResult.Value, Has.Count.EqualTo(2));
            foreach (Email email in addResult.Value)
            {
                Assert.That(email.Id, Is.Not.Default);
                Assert.That(email.DateCreated, Is.Not.Null);
                Assert.That(email.DateModified, Is.Not.Null);
            }
            
            Assert.That(allEmailsResult.IsSuccess, Is.True);
            Assert.That(allEmailsResult.Value, Has.Count.EqualTo(2 + SeedData.SeedEmailCount));
        });
    }

    [Test]
    public async Task AddAsync_SomeInvalidEntities_ReturnsPartialSuccess()
    {
        List<Email> newEmails =
        [
            new Email
            {
                SenderEmailAddress = "valid1@example.com",
                Subject = "Valid Bulk Email 1",
                HtmlBody = "This is a valid bulk email.",
                PlainTextBody = "This is a valid bulk email.",
                Recipients = [],
                Attachments = []
            },

            new Email
            {
                SenderEmailAddress = null!, // Invalid entity
                Subject = "Invalid Bulk Email",
                HtmlBody = "This email has invalid data.",
                PlainTextBody = "This email has invalid data.",
                Recipients = null!,
                Attachments = null!
            }
        ];

        Result<List<Email>> addResult = await EmailRepository.AddAsync(newEmails);

        Assert.Multiple(() =>
        {
            Assert.That(addResult.IsSuccess, Is.False);
            Assert.That(addResult.Errors, Is.Not.Empty);
        });
    }

    [Test]
    public async Task AddAsync_BulkDatabaseException_ReturnsDatabaseError()
    {
        await Context.DisposeAsync(); // Simulate database exception
        List<Email> newEmails =
        [
            new Email
            {
                SenderEmailAddress = "bulk_error1@example.com",
                Subject = "Bulk Error Email 1",
                HtmlBody = "This is a bulk email that will fail.",
                PlainTextBody = "This is a bulk email that will fail.",
                Recipients = [],
                Attachments = []
            },

            new Email
            {
                SenderEmailAddress = "bulk_error2@example.com",
                Subject = "Bulk Error Email 2",
                HtmlBody = "This is another bulk email that will fail.",
                PlainTextBody = "This is another bulk email that will fail.",
                Recipients = [],
                Attachments = []
            }
        ];

        Result<List<Email>> result = await EmailRepository.AddAsync(newEmails);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.First(), Is.InstanceOf<DatabaseError>());
        });
    }
}
