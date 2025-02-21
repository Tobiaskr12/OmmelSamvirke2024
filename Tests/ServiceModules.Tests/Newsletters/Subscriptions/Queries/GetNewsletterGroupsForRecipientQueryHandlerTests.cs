using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.GroupManagement;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using NSubstitute;
using ServiceModules.Errors;
using ServiceModules.Newsletters.GroupManagement.Queries;
using TestHelpers;

namespace ServiceModules.Tests.Newsletters.Subscriptions.Queries;

public class GetNewsletterGroupsForRecipientQueryHandlerTests
{
    private IRepository<NewsletterGroup> _newsletterGroupRepository;
    private GetNewsletterGroupsForRecipientQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _newsletterGroupRepository = Substitute.For<IRepository<NewsletterGroup>>();
        _handler = new GetNewsletterGroupsForRecipientQueryHandler(_newsletterGroupRepository);
    }

    [Test]
    public async Task WhenRecipientIsSubscribedToGroups_ReturnsOnlyThoseGroups()
    {
        var recipient = new Recipient { Id = 1, EmailAddress = "test@example.com" };

        var groupSubscribed = new NewsletterGroup
        {
            Id = 1,
            Name = "Subscribed Group",
            Description = "Group Desc",
            ContactList = new ContactList
            {
                Id = 10,
                Name = "CL Subscribed",
                Description = "CL Desc",
                Contacts = [recipient]
            }
        };

        var groupNotSubscribed = new NewsletterGroup
        {
            Id = 2,
            Name = "Not Subscribed Group",
            Description = "Group Desc",
            ContactList = new ContactList
            {
                Id = 20,
                Name = "CL Not Subscribed",
                Description = "CL Desc",
                Contacts = [new Recipient { Id = 101, EmailAddress = "other@example.com" }]
            }
        };

        var groups = new List<NewsletterGroup> { groupSubscribed, groupNotSubscribed };

        _newsletterGroupRepository.GetAllAsync()
                                  .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(groups));

        var query = new GetNewsletterGroupsForRecipientQuery(recipient.EmailAddress);
        Result<List<NewsletterGroup>> result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Count.EqualTo(1));
            Assert.That(result.Value.First().Id, Is.EqualTo(groupSubscribed.Id));
        });
    }

    [Test]
    public async Task WhenNoGroupsFoundForRecipient_ReturnsEmptyList()
    {
        var groups = new List<NewsletterGroup>
        {
            new()
            {
                Id = 1,
                Name = "Group 1",
                Description = "Desc",
                ContactList = new ContactList
                {
                    Id = 10,
                    Name = "CL1",
                    Description = "Desc",
                    Contacts = [new Recipient { Id = 101, EmailAddress = "other@example.com" }]
                }
            }
        };

        _newsletterGroupRepository.GetAllAsync()
                                  .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(groups));

        var query = new GetNewsletterGroupsForRecipientQuery("notfound@example.com");
        Result<List<NewsletterGroup>> result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);
        });
    }

    [Test]
    public async Task WhenRepositoryFails_ReturnsFailure()
    {
        _newsletterGroupRepository.GetAllAsync()
                                  .ReturnsForAnyArgs(MockHelpers.FailedAsyncResult<List<NewsletterGroup>>());

        var query = new GetNewsletterGroupsForRecipientQuery("test@example.com");
        Result<List<NewsletterGroup>> result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed, Is.True);
            Assert.That(result.Errors[0].Message, Is.EqualTo(ErrorMessages.GenericErrorWithRetryPrompt));
        });
    }
}
