using Contracts.ServiceModules.Emails.DTOs;
using Contracts.ServiceModules.Newsletters.Sending;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MimeKit;
using TestHelpers;

namespace ServiceModules.Tests.Newsletters.Sending.Commands;

[TestFixture, Category("IntegrationTests")]
public class NewsletterSendingIntegrationTests
{
    private IntegrationTestingHelper _integrationTestingHelper;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _integrationTestingHelper = new IntegrationTestingHelper();
    }
    
    [Test]
    public async Task GivenValidNewsletterIsSentToTwoClients_WhenCheckingEmailClients_TheNewsletterIsDelivered()
    {
        // Arrange
        await _integrationTestingHelper.ResetDatabase();
        var messageGuid = Guid.NewGuid();
        
        // Create recipients from both test email clients
        var recipientOne = new Recipient { EmailAddress = _integrationTestingHelper.TestEmailClientOne.EmailAddress };
        var recipientTwo = new Recipient { EmailAddress = _integrationTestingHelper.TestEmailClientTwo.EmailAddress };
        
        // Create a contact list containing both recipients
        var contactList = new ContactList
        {
            Name = "Test ContactList",
            Description = "Test Description",
            Contacts = [recipientOne, recipientTwo]
        };
        
        // Create a newsletter group that uses this contact list
        var newsletterGroup = new NewsletterGroup
        {
            Name = "Test Newsletter Group",
            Description = "Test Group Description",
            ContactList = contactList
        };
        
        // Create the email that will be sent as part of the newsletter
        var email = new Email
        {
            SenderEmailAddress = ValidSenderEmailAddresses.Newsletter,
            Subject = $"{messageGuid} - Newsletter Test",
            HtmlBody = "<p>This is a test newsletter.</p>",
            PlainTextBody = "This is a test newsletter.",
            Recipients = [],
            Attachments = []
        };
        
        // Act
        Result<EmailSendingStatus> result = await _integrationTestingHelper.Mediator.Send(new SendNewsletterCommand([newsletterGroup], email));
        Assert.That(result.IsSuccess);
        
        // Retrieve the emails from both test email clients using the messageGuid as a subject identifier
        MimeMessage? receivedMessageOne = await _integrationTestingHelper.GetLatestEmailAsync(_integrationTestingHelper.TestEmailClientOne, messageGuid.ToString());
        MimeMessage? receivedMessageTwo = await _integrationTestingHelper.GetLatestEmailAsync(_integrationTestingHelper.TestEmailClientTwo, messageGuid.ToString());
        
        // Assert that both clients have received the newsletter email
        Assert.Multiple(() =>
        {
            Assert.That(receivedMessageOne, Is.Not.Null);
            Assert.That(receivedMessageTwo, Is.Not.Null);
            Assert.That(receivedMessageOne!.Subject, Does.Contain(messageGuid.ToString()));
            Assert.That(receivedMessageTwo!.Subject, Does.Contain(messageGuid.ToString()));
        });
    }
}
