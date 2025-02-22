using AutoFixture;
using Contracts.ServiceModules.Newsletters.Sending;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;

namespace ServiceModules.Tests.Newsletters.Sending.Commands;

[TestFixture, Category("IntegrationTests")]
public class SendNewsletterCommandHandlerTests : ServiceTestBase
{
    [Test]
    public async Task WhenSendingNewsletter_WithValidNewsletter_ReturnsSuccessAndSendsToGroupRecipients()
    {
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        var email = GlobalTestSetup.Fixture.Create<Email>();
        await AddTestData(newsletterGroup);
        
        var command = new SendNewsletterCommand([newsletterGroup], email);
        Result result = await GlobalTestSetup.Mediator.Send(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(email.IsNewsletter, Is.True);
        });
    }
    
    [Test]
    public async Task WhenSendingNewsletter_WithDuplicatedRecipients_RecipientsAreDeduplicated()
    {
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        newsletterGroup.ContactList.Contacts.Clear();
        
        var recipient1 = GlobalTestSetup.Fixture.Create<Recipient>();
        var recipient2 = GlobalTestSetup.Fixture.Create<Recipient>();
        newsletterGroup.ContactList.Contacts.AddRange([recipient1, recipient1, recipient1, recipient2, recipient2]);
        await AddTestData(newsletterGroup);
        
        var email = GlobalTestSetup.Fixture.Create<Email>();
        
        var command = new SendNewsletterCommand([newsletterGroup], email);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(email.IsNewsletter, Is.True);
            Assert.That(email.Recipients, Has.Count.EqualTo(2));
            Assert.That(email.Recipients, Is.Unique);
        });
    }
}
