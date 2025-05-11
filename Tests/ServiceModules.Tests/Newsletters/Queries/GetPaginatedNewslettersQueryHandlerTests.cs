using AutoFixture;
using Contracts.DataAccess;
using Contracts.ServiceModules.Newsletters;
using DomainModules.Emails.Entities;
using FluentResults;

namespace ServiceModules.Tests.Newsletters.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetPaginatedNewslettersQueryHandlerTests : ServiceTestBase
{
    [Test]
    public async Task Handle_DefaultPagination_ReturnsPageSize20_OrderedByDateCreatedDesc()
    {
        List<Email> emails = [];
        for (int i = 0; i < 50; i++)
        {
            var email = GlobalTestSetup.Fixture.Create<Email>();
            email.IsNewsletter = true;
            emails.Add(email);
        }
        await AddTestData(emails);
        
        var query = new GetPaginatedNewslettersQuery();
        Result<PaginatedResult<Email>> result = await GlobalTestSetup.Mediator.Send(query);
        
        PaginatedResult<Email>? paginatedResult = result.Value;
        Email expectedFirst = emails.OrderByDescending(e => e.DateCreated ?? DateTime.MinValue).First();
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(paginatedResult.Items, Has.Count.EqualTo(20));
            Assert.That(paginatedResult.Items.First().Id, Is.EqualTo(expectedFirst.Id));
            Assert.That(paginatedResult.ItemsCount, Is.EqualTo(emails.Count));
        });
    }
}
