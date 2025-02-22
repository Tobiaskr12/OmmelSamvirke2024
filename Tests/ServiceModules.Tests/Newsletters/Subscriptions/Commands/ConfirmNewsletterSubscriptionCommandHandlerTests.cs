using AutoFixture;
using Contracts.ServiceModules.Newsletters.Subscriptions;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;

namespace ServiceModules.Tests.Newsletters.Subscriptions.Commands;

[TestFixture, Category("IntegrationTests")]
public class ConfirmNewsletterSubscriptionCommandHandlerTests : ServiceTestBase
{
    private NewsletterGroup _newsletterGroup;
    
    [SetUp]
    public async Task SetUp()
    {
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        await AddTestData(newsletterGroup);

        _newsletterGroup = newsletterGroup;
    }
    
    [Test]
    public async Task Handle_HappyPath_ReturnsSuccess()
    {
        var confirmation = GlobalTestSetup.Fixture.Create<NewsletterSubscriptionConfirmation>();
        PrepareConfirmation(confirmation);
        await AddTestData(confirmation);

        var command = new ConfirmNewsletterSubscriptionCommand(confirmation.ConfirmationToken);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsSuccess);
    }

    [Test]
    public async Task Handle_TokenNotFound_ReturnsFailure()
    {
        var command = new ConfirmNewsletterSubscriptionCommand(Guid.NewGuid());
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Handle_TokenExpired_ReturnsFailure()
    {
        var confirmation = GlobalTestSetup.Fixture.Create<NewsletterSubscriptionConfirmation>();
        PrepareConfirmation(confirmation);
        confirmation.ConfirmationExpiry = DateTime.UtcNow.AddDays(-1);
        await AddTestData(confirmation);

        var command = new ConfirmNewsletterSubscriptionCommand(confirmation.ConfirmationToken);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsFailed);
    }

    [Test]
    public async Task Handle_AlreadyConfirmed_ReturnsFailure()
    {
        var confirmation = GlobalTestSetup.Fixture.Create<NewsletterSubscriptionConfirmation>();
        PrepareConfirmation(confirmation);
        confirmation.ConfirmationExpiry = DateTime.UtcNow.AddDays(5);
        confirmation.IsConfirmed = true;
        await AddTestData(confirmation);

        var command = new ConfirmNewsletterSubscriptionCommand(confirmation.ConfirmationToken);
        Result result = await GlobalTestSetup.Mediator.Send(command);
        
        Assert.That(result.IsFailed);
    }

    private void PrepareConfirmation(NewsletterSubscriptionConfirmation confirmation)
    {
        Recipient recipient = _newsletterGroup.ContactList.Contacts.First();
        confirmation.Recipient = recipient;
        confirmation.ConfirmationExpiry = DateTime.UtcNow.AddDays(5);
        confirmation.NewsletterGroups = [_newsletterGroup];
        confirmation.IsConfirmed = false;
    }
}
