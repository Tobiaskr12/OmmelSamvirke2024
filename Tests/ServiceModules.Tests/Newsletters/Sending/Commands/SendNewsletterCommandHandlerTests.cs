using Contracts.Infrastructure.Emails;
using Contracts.ServiceModules.Emails.DTOs;
using Contracts.ServiceModules.Newsletters.Sending;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using NSubstitute;
using ServiceModules.Newsletters.Sending.Commands;
using TestHelpers;

namespace ServiceModules.Tests.Newsletters.Sending.Commands;

public class SendNewsletterCommandHandlerTests
{
    private IExternalEmailServiceWrapper _emailServiceWrapper;
    private SendNewsletterCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _emailServiceWrapper = Substitute.For<IExternalEmailServiceWrapper>();
        _handler = new SendNewsletterCommandHandler(_emailServiceWrapper);
    }

    [Test]
    public async Task WhenSendingNewsletter_WithValidNewsletter_ReturnsSuccessAndSendsToGroupRecipients()
    {
        // Arrange
        var recipientFromGroup = new Recipient { Id = 100, EmailAddress = "grouprecipient@example.com" };
        var contactList = new ContactList 
        { 
            Id = 1, 
            Name = "Group ContactList", 
            Description = "Desc", 
            Contacts = [recipientFromGroup]
        };
        var newsletterGroup = new NewsletterGroup 
        { 
            Id = 10, 
            Name = "Group 1", 
            Description = "Group Desc", 
            ContactList = contactList 
        };
        var email = new Email 
        { 
            Id = 50, 
            SenderEmailAddress = "sender@example.com", 
            Subject = "Newsletter Subject", 
            HtmlBody = "<p>Newsletter Content</p>", 
            PlainTextBody = "Newsletter Content", 
            Recipients = [], 
            Attachments = []
        };

        _emailServiceWrapper
            .SendBatchesAsync(email, ServiceLimits.RecipientsPerEmail, useBcc: true, Arg.Any<CancellationToken>())
            .Returns(Result.Ok());

        // Act
        Result<EmailSendingStatus> result = await _handler.Handle(new SendNewsletterCommand([newsletterGroup], email), CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task WhenSendingEmailFails_ReturnsFailure()
    {
        // Arrange
        var recipientFromGroup = new Recipient { Id = 200, EmailAddress = "group2@example.com" };
        var contactList = new ContactList 
        { 
            Id = 2, 
            Name = "Another ContactList", 
            Description = "Desc", 
            Contacts = [recipientFromGroup]
        };
        var newsletterGroup = new NewsletterGroup 
        { 
            Id = 20, 
            Name = "Group 2", 
            Description = "Another Desc", 
            ContactList = contactList 
        };
        var email = new Email 
        { 
            Id = 60, 
            SenderEmailAddress = "sender@example.com", 
            Subject = "Newsletter Subject", 
            HtmlBody = "<p>Newsletter Content</p>", 
            PlainTextBody = "Newsletter Content", 
            Recipients = [], 
            Attachments = []
        };

        _emailServiceWrapper
            .SendBatchesAsync(email, ServiceLimits.RecipientsPerEmail, useBcc: true, Arg.Any<CancellationToken>())
            .Returns(MockHelpers.FailedAsyncResult());

        // Act
        Result<EmailSendingStatus> result = await _handler.Handle(new SendNewsletterCommand([newsletterGroup], email), CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed, Is.True);
    }
}
