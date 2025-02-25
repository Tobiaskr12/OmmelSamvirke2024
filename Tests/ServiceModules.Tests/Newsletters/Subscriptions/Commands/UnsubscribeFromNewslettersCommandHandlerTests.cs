using AutoFixture;
using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.Subscriptions;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MimeKit;

namespace ServiceModules.Tests.Newsletters.Subscriptions.Commands;

[TestFixture, Category("IntegrationTests")]
public class UnsubscribeFromNewslettersCommandHandlerTests : ServiceTestBase
{
    [Test]
    public async Task Handle_NoExistingRecipient_ReturnsOk()
    {
        const string email = "nosuchuser@example.com";
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        await AddTestData(newsletterGroup);
        
        var command = new UnsubscribeFromNewslettersCommand(email, [ newsletterGroup.Id ]);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsSuccess);
    }

    [Test]
    public async Task Handle_UserNotSubscribedToRequestedGroups_ReturnsFail()
    {
        var recipient = GlobalTestSetup.Fixture.Create<Recipient>();
        recipient.EmailAddress = GlobalTestSetup.TestEmailClientOne.EmailAddress;
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        await AddTestData(recipient);
        await AddTestData(newsletterGroup);
        
        var command = new UnsubscribeFromNewslettersCommand(recipient.EmailAddress, [ newsletterGroup.Id ]);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Handle_HappyPath_CreatesUnsubscribeRequest_AndSendsEmail()
    {
        var recipient = GlobalTestSetup.Fixture.Create<Recipient>();
        recipient.EmailAddress = GlobalTestSetup.TestEmailClientOne.EmailAddress;
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        newsletterGroup.ContactList.Contacts.Add(recipient);
        await AddTestData(newsletterGroup);
        
        var command = new UnsubscribeFromNewslettersCommand(recipient.EmailAddress, [ newsletterGroup.Id ]);
        Result result = await GlobalTestSetup.Mediator.Send(command);

        MimeMessage? receivedEmail = await GetLatestEmailAsync(GlobalTestSetup.TestEmailClientOne, "Bekræft din afmelding fra Ommel Samvirkes nyhedsbrev");
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(receivedEmail, Is.Not.Null);
        });
    }

    [Test]
    public async Task Handle_ActiveUnsubscribeExists_SkipsThoseGroups()
    {
        var recipient = GlobalTestSetup.Fixture.Create<Recipient>();
        recipient.NewsletterUnsubscribeConfirmations.Clear();
        recipient.EmailAddress = GlobalTestSetup.TestEmailClientOne.EmailAddress;
        var newsletterGroupOne = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        var newsletterGroupTwo = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        newsletterGroupOne.NewsletterUnsubscribeConfirmations.Clear();
        newsletterGroupTwo.NewsletterUnsubscribeConfirmations.Clear();
        newsletterGroupOne.ContactList.Contacts.Add(recipient);
        newsletterGroupTwo.ContactList.Contacts.Add(recipient);
        await AddTestData([newsletterGroupOne, newsletterGroupTwo]);

        var confirmedUnsubscribeRequest = new NewsletterUnsubscribeConfirmation
        {
            Recipient = recipient,
            ConfirmationExpiry = DateTime.UtcNow.AddDays(7),
            IsConfirmed = true,
            ConfirmationTime = DateTime.UtcNow.AddDays(-1),
            NewsletterGroups = [newsletterGroupOne],
        };
        await AddTestData(confirmedUnsubscribeRequest);
        
        var command = new UnsubscribeFromNewslettersCommand(recipient.EmailAddress, [ newsletterGroupOne.Id, newsletterGroupTwo.Id ]);
        Result result = await GlobalTestSetup.Mediator.Send(command);

        List<NewsletterUnsubscribeConfirmation> allUnsubscribeRequests = 
            (await GetService<IRepository<NewsletterUnsubscribeConfirmation>>().GetAllAsync()).Value;
        
        MimeMessage? receivedEmail = await GetLatestEmailAsync(GlobalTestSetup.TestEmailClientOne, "Bekræft din afmelding fra Ommel Samvirkes nyhedsbrev");
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(allUnsubscribeRequests, Has.Count.EqualTo(2));
            Assert.That(receivedEmail, Is.Not.Null);
        });
    }
}
