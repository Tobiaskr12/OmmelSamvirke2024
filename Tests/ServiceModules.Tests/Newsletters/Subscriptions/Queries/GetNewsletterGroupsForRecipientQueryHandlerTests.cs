using AutoFixture;
using Contracts.ServiceModules.Newsletters.GroupManagement;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;

namespace ServiceModules.Tests.Newsletters.Subscriptions.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetNewsletterGroupsForRecipientQueryHandlerTests : ServiceTestBase
{
    [Test]
    public async Task WhenRecipientIsSubscribedToGroups_ReturnsOnlyThoseGroups()
    {
        var recipient = GlobalTestSetup.Fixture.Create<Recipient>();
        var newsletterGroup1 = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        var newsletterGroup2 = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        newsletterGroup1.ContactList.Contacts.Add(recipient);
        await AddTestData([newsletterGroup1, newsletterGroup2]);

        var query = new GetNewsletterGroupsForRecipientQuery(recipient.EmailAddress);
        Result<List<NewsletterGroup>> result = await GlobalTestSetup.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Count.EqualTo(1));
            Assert.That(result.Value.First().Id, Is.EqualTo(newsletterGroup1.Id));
        });
    }

    [Test]
    public async Task WhenNoGroupsFoundForRecipient_ReturnsEmptyList()
    {
        var recipient = GlobalTestSetup.Fixture.Create<Recipient>();

        var query = new GetNewsletterGroupsForRecipientQuery(recipient.EmailAddress);
        Result<List<NewsletterGroup>> result = await GlobalTestSetup.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);
        });
    }
}
