using AutoFixture;
using Contracts.ServiceModules.Newsletters.GroupManagement;
using DomainModules.Newsletters.Entities;
using FluentResults;
using ServiceModules.Errors;

namespace ServiceModules.Tests.Newsletters.GroupManagement.Commands;

[TestFixture, Category("UnitTests")]
public class DeleteNewsletterGroupCommandHandlerTests : ServiceTestBase
{
    [Test]
    public async Task WhenDeletingNewsletterGroup_WithValidId_ReturnsSuccess()
    {
        var newsletterGroup = GlobalTestSetup.Fixture.Create<NewsletterGroup>();
        await AddTestData(newsletterGroup);

        var command = new DeleteNewsletterGroupCommand(newsletterGroup.Id);
        Result result = await GlobalTestSetup.Mediator.Send(command);
            
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task WhenNewsletterGroupNotFound_ReturnsFailure()
    {
        var command = new DeleteNewsletterGroupCommand(1);
        Result result = await GlobalTestSetup.Mediator.Send(command);
            
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors[0].Message, Is.EqualTo(ErrorMessages.GenericErrorWithRetryPrompt));
        });
    }
}
