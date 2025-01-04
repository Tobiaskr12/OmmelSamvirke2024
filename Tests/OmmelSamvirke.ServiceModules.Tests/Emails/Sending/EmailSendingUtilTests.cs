using System.Linq.Expressions;
using NSubstitute;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DataAccess.Errors;
using OmmelSamvirke.DomainModules.Emails.Entities;
using FluentResults;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.ServiceModules.Emails.Sending;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.Sending;

[TestFixture, Category("UnitTests")]
public class EmailSendingUtilTests
{
    private IRepository<Recipient> _recipientRepository;

    [SetUp]
    public void Setup()
    {
        _recipientRepository = Substitute.For<IRepository<Recipient>>();
    }

    [Test]
    public async Task FetchAndReplaceExistingRecipients_AllRecipientsExist_ShouldReplaceAll()
    {
        var email = new Email
        {
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Attachments = [],
            HtmlBody = "This is a test email",
            PlainTextBody = "This is a test email",
            Subject = "This is a test subject",
            Recipients =
            [
                CreateRecipient("existing1@example.com"),
                CreateRecipient("existing2@example.com")
            ]
        };

        var existingRecipients = new List<Recipient>
        {
            CreateRecipient("existing1@example.com", 1),
            CreateRecipient("existing2@example.com", 2)
        };

        _recipientRepository.FindAsync(
                Arg.Any<Expression<Func<Recipient, bool>>>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Ok(existingRecipients));
        
        Result result = await EmailSendingUtil.FetchAndReplaceExistingRecipients(email, _recipientRepository, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(email.Recipients, Has.Count.EqualTo(2));
            Assert.That(email.Recipients[0], Is.SameAs(existingRecipients[0]));
            Assert.That(email.Recipients[1], Is.SameAs(existingRecipients[1]));
        });
    }

    [Test]
    public async Task FetchAndReplaceExistingRecipients_NoRecipientsExist_ShouldKeepAllNew()
    {
        var email = new Email
        {
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Attachments = [],
            HtmlBody = "This is a test email",
            PlainTextBody = "This is a test email",
            Subject = "This is a test subject",
            Recipients =
            [
                CreateRecipient("new1@example.com"),
                CreateRecipient("new2@example.com")
            ]
        };

        var existingRecipients = new List<Recipient>();

        _recipientRepository.FindAsync(
                Arg.Any<Expression<Func<Recipient, bool>>>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Ok(existingRecipients));
        
        Result result = await EmailSendingUtil.FetchAndReplaceExistingRecipients(email, _recipientRepository, CancellationToken.None);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(email.Recipients, Has.Count.EqualTo(2));
            Assert.That(email.Recipients.All(r => r.Id == 0), Is.True);
        });
    }

    [Test]
    public async Task FetchAndReplaceExistingRecipients_SomeRecipientsExist_ShouldReplaceOnlyExisting()
    {
        var email = new Email
        {
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Attachments = [],
            HtmlBody = "This is a test email",
            PlainTextBody = "This is a test email",
            Subject = "This is a test subject",
            Recipients =
            [
                CreateRecipient("existing@example.com"),
                CreateRecipient("new@example.com")
            ]
        };

        var existingRecipients = new List<Recipient>
        {
            CreateRecipient("existing@example.com", 1)
        };

        _recipientRepository.FindAsync(
                Arg.Any<Expression<Func<Recipient, bool>>>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Ok(existingRecipients));
        
        Result result = await EmailSendingUtil.FetchAndReplaceExistingRecipients(email, _recipientRepository, CancellationToken.None);
        Recipient firstRecipient = email.Recipients[0];
        Recipient secondRecipient = email.Recipients[1];
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(email.Recipients, Has.Count.EqualTo(2));
            Assert.That(firstRecipient, Is.SameAs(existingRecipients[0]));
            Assert.That(secondRecipient.Id, Is.EqualTo(0));
            Assert.That(secondRecipient.EmailAddress, Is.EqualTo("new@example.com"));
        });
    }

    [Test]
    public async Task FetchAndReplaceExistingRecipients_RepositoryFails_ShouldReturnFailure()
    {
        var email = new Email
        {
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Attachments = [],
            HtmlBody = "This is a test email",
            PlainTextBody = "This is a test email",
            Subject = "This is a test subject",
            Recipients = [CreateRecipient("any@example.com")]
        };

        var repositoryError = new DatabaseError("Database connection failed.");

        _recipientRepository.FindAsync(
                Arg.Any<Expression<Func<Recipient, bool>>>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Fail<List<Recipient>>(repositoryError));
        
        Result result = await EmailSendingUtil.FetchAndReplaceExistingRecipients(email, _recipientRepository, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.That(result.Errors[0].Message, Is.EqualTo("Database connection failed."));
            
            Assert.That(email.Recipients, Has.Count.EqualTo(1));
            Assert.That(email.Recipients[0].Id, Is.EqualTo(0));
            Assert.That(email.Recipients[0].EmailAddress, Is.EqualTo("any@example.com"));
        });
    }

    [Test]
    public async Task FetchAndReplaceExistingRecipients_EmptyRecipients_ShouldReturnSuccess()
    {
        var email = new Email
        {
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Attachments = [],
            HtmlBody = "This is a test email",
            PlainTextBody = "This is a test email",
            Subject = "This is a test subject",
            Recipients = [] 
        };

        List<Recipient> existingRecipients = [];

        _recipientRepository.FindAsync(
                Arg.Any<Expression<Func<Recipient, bool>>>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Ok(existingRecipients));
        
        Result result = await EmailSendingUtil.FetchAndReplaceExistingRecipients(email, _recipientRepository, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(email.Recipients, Is.Empty);
        });
    }

    [Test]
    public async Task FetchAndReplaceExistingRecipients_DuplicateEmailsInEmail_ShouldReplaceAllOccurrences()
    {
        var email = new Email
        {
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Attachments = [],
            HtmlBody = "This is a test email",
            PlainTextBody = "This is a test email",
            Subject = "This is a test subject",
            Recipients =
            [
                CreateRecipient("duplicate@example.com"),
                CreateRecipient("duplicate@example.com"),
                CreateRecipient("unique@example.com")
            ]
        };

        var existingRecipients = new List<Recipient>
        {
            CreateRecipient("duplicate@example.com", 1)
            // "unique@example.com" does not exist
        };

        _recipientRepository.FindAsync(
                Arg.Any<Expression<Func<Recipient, bool>>>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Ok(existingRecipients));
        
        Result result = await EmailSendingUtil.FetchAndReplaceExistingRecipients(email, _recipientRepository, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(email.Recipients.Count, Is.EqualTo(3));
            
            // Both duplicates should be replaced with the same existing recipient instance
            Assert.That(email.Recipients[0], Is.SameAs(existingRecipients[0]));
            Assert.That(email.Recipients[1], Is.SameAs(existingRecipients[0]));
            Assert.That(email.Recipients[2].Id, Is.EqualTo(0));
            Assert.That(email.Recipients[2].EmailAddress, Is.EqualTo("unique@example.com"));
        });
    }

    [Test]
    public async Task FetchAndReplaceExistingRecipients_MultipleExistingRecipients_ShouldReplaceCorrectOnes()
    {
        var email = new Email
        {
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Attachments = [],
            HtmlBody = "This is a test email",
            PlainTextBody = "This is a test email",
            Subject = "This is a test subject",
            Recipients =
            [
                CreateRecipient("existing1@example.com"),
                CreateRecipient("new1@example.com"),
                CreateRecipient("existing2@example.com"),
                CreateRecipient("new2@example.com")
            ]
        };

        var existingRecipients = new List<Recipient>
        {
            CreateRecipient("existing1@example.com", 1),
            CreateRecipient("existing2@example.com", 2)
        };

        _recipientRepository.FindAsync(
                Arg.Any<Expression<Func<Recipient, bool>>>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Ok(existingRecipients));
        
        Result result = await EmailSendingUtil.FetchAndReplaceExistingRecipients(email, _recipientRepository, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(email.Recipients, Has.Count.EqualTo(4));
            
            Assert.That(email.Recipients[0], Is.SameAs(existingRecipients[0]));
            Assert.That(email.Recipients[1].Id, Is.EqualTo(0));
            Assert.That(email.Recipients[1].EmailAddress, Is.EqualTo("new1@example.com"));
            Assert.That(email.Recipients[2], Is.SameAs(existingRecipients[1]));
            Assert.That(email.Recipients[3].Id, Is.EqualTo(0));
            Assert.That(email.Recipients[3].EmailAddress, Is.EqualTo("new2@example.com"));
        });
    }
        
    private static Recipient CreateRecipient(string emailAddress, int id = 0)
    {
        return new Recipient
        {
            Id = id,
            EmailAddress = emailAddress
        };
    }
}
