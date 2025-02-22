using AutoFixture;
using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.GroupManagement;
using DomainModules.Newsletters.Entities;
using FluentResults;
using ServiceModules.Errors;

namespace ServiceModules.Tests.Newsletters.GroupManagement.Commands;

[TestFixture, Category("IntegrationTests")]
public class UpdateNewsletterGroupCommandHandlerTests : ServiceTestBase
{
    [Test]
    public async Task WhenUpdatingNewsletterGroup_WithValidData_ReturnsUpdatedGroup()
    {
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        await AddTestData(newsletterGroup);

        Result<NewsletterGroup> newsletterGroupQuery = await GetService<IRepository<NewsletterGroup>>().GetByIdAsync(newsletterGroup.Id);
        NewsletterGroup updatedNewsletterGroup = newsletterGroupQuery.Value;
        updatedNewsletterGroup.Name = "New and updated name!";
        updatedNewsletterGroup.Name = "This is a new description!";

        var command = new UpdateNewsletterGroupCommand(updatedNewsletterGroup);
        Result<NewsletterGroup> result = await GlobalTestSetup.Mediator.Send(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Name, Is.EqualTo(updatedNewsletterGroup.Name));
            Assert.That(result.Value.Description, Is.EqualTo(updatedNewsletterGroup.Description));
        });
    }

    [Test]
    public async Task WhenNewsletterGroupNotFound_ReturnsFailure()
    {
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        
        var command = new UpdateNewsletterGroupCommand(newsletterGroup);
        Result<NewsletterGroup> result = await GlobalTestSetup.Mediator.Send(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors[0].Message, Is.EqualTo(ErrorMessages.GenericErrorWithRetryPrompt));
        });
    }
}
