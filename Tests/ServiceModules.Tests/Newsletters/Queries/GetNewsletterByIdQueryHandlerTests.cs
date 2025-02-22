using AutoFixture;
using Contracts.ServiceModules.Newsletters;
using DomainModules.Emails.Entities;
using FluentResults;

namespace ServiceModules.Tests.Newsletters.Queries;

[TestFixture, Category("IntegrationTests")]
public class GetNewsletterByIdQueryHandlerTests : ServiceTestBase
{
    [Test]
    public async Task Handle_WhenEmailExistsAndIsNewsletter_ReturnsEmail()
    {
        var email = GlobalTestSetup.Fixture.Create<Email>();
        email.IsNewsletter = true;
        await AddTestData(email);
        
        var query = new GetNewsletterByIdQuery(email.Id);
        Result<Email> result = await GlobalTestSetup.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Id, Is.EqualTo(email.Id));
        });
    }

    [Test]
    public async Task Handle_WhenEmailNotFound_ReturnsFailure()
    {
        var query = new GetNewsletterByIdQuery(1);
        Result<Email> result = await GlobalTestSetup.Mediator.Send(query);
        
        Assert.That(result.IsFailed, Is.True);
    }

    [Test]
    public async Task Handle_WhenEmailExistsButNotNewsletter_ReturnsFailure()
    {
        var email = GlobalTestSetup.Fixture.Create<Email>();
        email.IsNewsletter = false;
        await AddTestData(email);

        var query = new GetNewsletterByIdQuery(email.Id);
        Result<Email> result = await GlobalTestSetup.Mediator.Send(query);
        
        Assert.That(result.IsFailed, Is.True);
    }
}
