using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.GroupManagement;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using NSubstitute;
using ServiceModules.Errors;
using ServiceModules.Newsletters.GroupManagement.Commands;

namespace ServiceModules.Tests.Newsletters.GroupManagement.Commands;

public class UpdateNewsletterGroupCommandHandlerTests
{
    private IRepository<NewsletterGroup> _newsletterGroupRepository;
    private UpdateNewsletterGroupCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _newsletterGroupRepository = Substitute.For<IRepository<NewsletterGroup>>();
        _handler = new UpdateNewsletterGroupCommandHandler(_newsletterGroupRepository);
    }

    [Test]
    public async Task WhenUpdatingNewsletterGroup_WithValidData_ReturnsUpdatedGroup()
    {
        var storedNewsletterGroup = new NewsletterGroup
        {
            Id = 1,
            Name = "Old Name",
            Description = "Old Description",
            ContactList = new ContactList
            {
                Name = "Old Contact List",
                Description = "Old Contact Description"
            }
        };

        var updatedNewsletterGroup = new NewsletterGroup
        {
            Id = 1,
            Name = "New Name",
            Description = "New Description",
            ContactList = new ContactList
            {
                Name = "New Contact List",
                Description = "New Contact Description"
            }
        };

        _newsletterGroupRepository
            .GetByIdAsync(updatedNewsletterGroup.Id, readOnly: false, CancellationToken.None)
            .Returns(Task.FromResult(Result.Ok(storedNewsletterGroup)));

        _newsletterGroupRepository
            .UpdateAsync(Arg.Any<NewsletterGroup>(), CancellationToken.None)
            .Returns(Task.FromResult(Result.Ok(updatedNewsletterGroup)));

        var command = new UpdateNewsletterGroupCommand(updatedNewsletterGroup);

        Result<NewsletterGroup> result = await _handler.Handle(command, CancellationToken.None);

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
        var updatedNewsletterGroup = new NewsletterGroup
        {
            Id = 1,
            Name = "New Name",
            Description = "New Description",
            ContactList = new ContactList
            {
                Name = "New Contact List",
                Description = "New Contact Description"
            }
        };

        _newsletterGroupRepository
            .GetByIdAsync(updatedNewsletterGroup.Id, readOnly: false, CancellationToken.None)
            .Returns(Task.FromResult(Result.Fail<NewsletterGroup>("Not found")));

        var command = new UpdateNewsletterGroupCommand(updatedNewsletterGroup);

        Result<NewsletterGroup> result = await _handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors[0].Message, Is.EqualTo(ErrorMessages.GenericErrorWithRetryPrompt));
        });
    }

    [Test]
    public async Task WhenUpdateFails_ReturnsFailure()
    {
        var storedNewsletterGroup = new NewsletterGroup
        {
            Id = 1,
            Name = "Old Name",
            Description = "Old Description",
            ContactList = new ContactList
            {
                Name = "Old Contact List",
                Description = "Old Contact Description"
            }
        };

        var updatedNewsletterGroup = new NewsletterGroup
        {
            Id = 1,
            Name = "New Name",
            Description = "New Description",
            ContactList = new ContactList
            {
                Name = "New Contact List",
                Description = "New Contact Description"
            }
        };

        _newsletterGroupRepository
            .GetByIdAsync(updatedNewsletterGroup.Id, readOnly: false, CancellationToken.None)
            .Returns(Task.FromResult(Result.Ok(storedNewsletterGroup)));

        _newsletterGroupRepository
            .UpdateAsync(Arg.Any<NewsletterGroup>(), CancellationToken.None)
            .Returns(Task.FromResult(Result.Fail<NewsletterGroup>("Update error")));

        var command = new UpdateNewsletterGroupCommand(updatedNewsletterGroup);

        Result<NewsletterGroup> result = await _handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors[0].Message, Is.EqualTo("Update error"));
        });
    }
}
