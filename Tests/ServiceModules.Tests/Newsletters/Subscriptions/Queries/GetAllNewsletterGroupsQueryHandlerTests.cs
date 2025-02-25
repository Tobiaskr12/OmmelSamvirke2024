using AutoFixture;
using Contracts.ServiceModules.Newsletters.GroupManagement;
using DomainModules.Newsletters.Entities;
using FluentResults;

namespace ServiceModules.Tests.Newsletters.Subscriptions.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetAllNewsletterGroupsQueryHandlerTests : ServiceTestBase
{
    [Test]
    public async Task WhenGroupsExist_ReturnsAllGroups()
    {
        var newsletterGroup1 = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        var newsletterGroup2 = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        await AddTestData([newsletterGroup1, newsletterGroup2]);
        
        var query = new GetAllNewsletterGroupsQuery();
        Result<List<NewsletterGroup>> result = await GlobalTestSetup.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task WhenNoGroupsExist_ReturnsEmptyList()
    {
        var query = new GetAllNewsletterGroupsQuery();
        Result<List<NewsletterGroup>> result = await GlobalTestSetup.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Count.EqualTo(0));
        });
    }
}
