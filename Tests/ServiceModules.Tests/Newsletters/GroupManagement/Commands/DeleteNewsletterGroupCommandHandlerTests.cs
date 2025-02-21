using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.GroupManagement;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using NSubstitute;
using ServiceModules.Errors;
using ServiceModules.Newsletters.GroupManagement.Commands;
using TestHelpers;

namespace ServiceModules.Tests.Newsletters.GroupManagement.Commands;

public class DeleteNewsletterGroupCommandHandlerTests
{
    private IRepository<NewsletterGroup> _newsletterGroupRepository;
    private DeleteNewsletterGroupCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _newsletterGroupRepository = Substitute.For<IRepository<NewsletterGroup>>();
        _handler = new DeleteNewsletterGroupCommandHandler(_newsletterGroupRepository);
    }

    [Test]
    public async Task WhenDeletingNewsletterGroup_WithValidId_ReturnsSuccess()
    {
        var storedNewsletterGroup = new NewsletterGroup
        {
            Id = 1,
            Name = "Test Group",
            Description = "Test Description",
            ContactList = new ContactList
            {
                Name = "Test Contact List",
                Description = "Test Contact Description"
            }
        };

        _newsletterGroupRepository
            .GetByIdAsync(1, readOnly: false, CancellationToken.None)
            .Returns(MockHelpers.SuccessAsyncResult(storedNewsletterGroup));

        _newsletterGroupRepository
            .DeleteAsync(storedNewsletterGroup, CancellationToken.None)
            .Returns(Result.Ok());

        var command = new DeleteNewsletterGroupCommand(1);
        Result result = await _handler.Handle(command, CancellationToken.None);
            
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task WhenNewsletterGroupNotFound_ReturnsFailure()
    {
        _newsletterGroupRepository
            .GetByIdAsync(1, readOnly: false, CancellationToken.None)
            .Returns(MockHelpers.FailedAsyncResult<NewsletterGroup>());

        var command = new DeleteNewsletterGroupCommand(1);
        Result result = await _handler.Handle(command, CancellationToken.None);
            
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors[0].Message, Is.EqualTo(ErrorMessages.GenericErrorWithRetryPrompt));
        });
    }

    [Test]
    public async Task WhenDeletionFails_ReturnsFailure()
    {
        var storedNewsletterGroup = new NewsletterGroup
        {
            Id = 1,
            Name = "Test Group",
            Description = "Test Description",
            ContactList = new ContactList
            {
                Name = "Test Contact List",
                Description = "Test Contact Description"
            }
        };

        _newsletterGroupRepository
            .GetByIdAsync(1, readOnly: false, CancellationToken.None)
            .Returns(MockHelpers.SuccessAsyncResult(storedNewsletterGroup));

        _newsletterGroupRepository
            .DeleteAsync(storedNewsletterGroup, CancellationToken.None)
            .Returns(MockHelpers.FailedAsyncResult());

        var command = new DeleteNewsletterGroupCommand(1);
        Result result = await _handler.Handle(command, CancellationToken.None);
            
        Assert.That(result.IsFailed, Is.True);
    }
}
