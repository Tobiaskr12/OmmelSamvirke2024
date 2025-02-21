using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.GroupManagement;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using NSubstitute;
using ServiceModules.Newsletters.GroupManagement.Queries;
using TestHelpers;

namespace ServiceModules.Tests.Newsletters.Subscriptions.Queries;

public class GetAllNewsletterGroupsQueryHandlerTests
{
    private IRepository<NewsletterGroup> _newsletterGroupRepository;
    private GetAllNewsletterGroupsQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _newsletterGroupRepository = Substitute.For<IRepository<NewsletterGroup>>();
        _handler = new GetAllNewsletterGroupsQueryHandler(_newsletterGroupRepository);
    }

    [Test]
    public async Task WhenGroupsExist_ReturnsAllGroups()
    {
        var groups = new List<NewsletterGroup>
        {
            new()
            {
                Id = 1, 
                Name = "Group 1", 
                Description = "Desc 1", 
                ContactList = new ContactList
                {
                    Id = 1, 
                    Name = "CL1", 
                    Description = "CL Desc 1"
                }
            },
            new()
            {
                Id = 2, 
                Name = "Group 2", 
                Description = "Desc 2", 
                ContactList = new ContactList
                {
                    Id = 2,
                    Name = "CL2", 
                    Description = "CL Desc 2"
                }
            }
        };

        _newsletterGroupRepository.GetAllAsync()
                                  .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(groups));

        var query = new GetAllNewsletterGroupsQuery();
        Result<List<NewsletterGroup>> result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task WhenNoGroupsExist_ReturnsEmptyList()
    {
        var groups = new List<NewsletterGroup>();

        _newsletterGroupRepository.GetAllAsync()
                                  .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(groups));

        var query = new GetAllNewsletterGroupsQuery();
        Result<List<NewsletterGroup>> result = await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);
        });
    }
}
