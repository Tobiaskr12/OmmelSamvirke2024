using AutoFixture;
using Contracts.ServiceModules.Newsletters.Subscriptions;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MimeKit;

namespace ServiceModules.Tests.Newsletters.Subscriptions.Commands;

[TestFixture, Category("IntegrationTests")]
public class SubscribeToNewslettersCommandHandlerTests : ServiceTestBase
{
    [Test]
    public async Task Handle_HappyPath_NewRecipient_ReturnsSuccess_AndSendsConfirmationEmail()
    {
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        await AddTestData(newsletterGroup);
        
        var subscribeCommand = new SubscribeToNewslettersCommand(GlobalTestSetup.TestEmailClientOne.EmailAddress, [newsletterGroup.Id]);
        Result commandResult = await GlobalTestSetup.Mediator.Send(subscribeCommand, CancellationToken.None);
        
        MimeMessage? receivedEmail = await GetLatestEmailAsync(GlobalTestSetup.TestEmailClientOne, subjectIdentifier: "Bekræft din tilmelding til Ommel Samvirkes nyhedsbrev");
        Assert.Multiple(() =>
        {
            
            Assert.That(commandResult.IsSuccess);
            Assert.That(receivedEmail, Is.Not.Null);
            Assert.That(receivedEmail!.HtmlBody, Does.Contain("<a href=\"https://www.ommelsamvirke.com/confirm-subscription?token="));
        });
    }

    [Test]
    public async Task Handle_HappyPath_ExistingRecipient_ReturnsSuccess_AndSendsConfirmationEmail()
    {
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        var recipient = GlobalTestSetup.Fixture.Create<Recipient>();
        recipient.EmailAddress = GlobalTestSetup.TestEmailClientOne.EmailAddress;
        await AddTestData(newsletterGroup);
        await AddTestData(recipient);
        
        var subscribeCommand = new SubscribeToNewslettersCommand(recipient.EmailAddress, [newsletterGroup.Id]);
        Result commandResult = await GlobalTestSetup.Mediator.Send(subscribeCommand, CancellationToken.None);
        
        MimeMessage? receivedEmail = await GetLatestEmailAsync(GlobalTestSetup.TestEmailClientOne, subjectIdentifier: "Bekræft din tilmelding til Ommel Samvirkes nyhedsbrev");
        Assert.Multiple(() =>
        {
            Assert.That(commandResult.IsSuccess);
            Assert.That(receivedEmail, Is.Not.Null);
            Assert.That(receivedEmail!.HtmlBody, Does.Contain("<a href=\"https://www.ommelsamvirke.com/confirm-subscription?token="));
        });
    }

    [Test]
    public async Task Handle_SkipsAlreadySubscribedGroups()
    {
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        var recipient = GlobalTestSetup.Fixture.Create<Recipient>();
        newsletterGroup.ContactList.Contacts.Add(recipient);
        await AddTestData(newsletterGroup);
        
        var subscribeCommand = new SubscribeToNewslettersCommand(recipient.EmailAddress, [newsletterGroup.Id]);
        Result commandResult = await GlobalTestSetup.Mediator.Send(subscribeCommand, CancellationToken.None);
        
        Assert.That(commandResult.IsFailed);
    }

    [Test]
    public async Task Handle_TooManyPendingConfirmations_ReturnsFailure()
    {
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        var recipient = GlobalTestSetup.Fixture.Create<Recipient>();
        recipient.EmailAddress = GlobalTestSetup.TestEmailClientOne.EmailAddress;
        var pendingConfirmations = new List<NewsletterSubscriptionConfirmation>();
        for (int i = 0; i < 5; i++)
        {
            var pendingConfirmation = GlobalTestSetup.Fixture.Create<NewsletterSubscriptionConfirmation>();
            pendingConfirmation.IsConfirmed = false;
            pendingConfirmation.ConfirmationTime = null;
            pendingConfirmation.ConfirmationExpiry = DateTime.UtcNow.AddDays(5);
            pendingConfirmation.Recipient = recipient;
            pendingConfirmation.NewsletterGroups = [newsletterGroup];
            pendingConfirmations.Add(pendingConfirmation);
            
        }
        await AddTestData(pendingConfirmations);
        
        var subscribeCommand = new SubscribeToNewslettersCommand(GlobalTestSetup.TestEmailClientOne.EmailAddress, [newsletterGroup.Id]);
        Result commandResult = await GlobalTestSetup.Mediator.Send(subscribeCommand, CancellationToken.None);
        
        Assert.That(commandResult.IsFailed);
    }
}
