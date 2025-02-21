using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.GroupManagement;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using NSubstitute;
using ServiceModules.Newsletters.GroupManagement.Commands;
using TestHelpers;

namespace ServiceModules.Tests.Newsletters.GroupManagement.Commands;

public class CreateNewsletterGroupCommandHandlerTests
{
    private IRepository<NewsletterGroup> _newsletterGroupRepository;
    private CreateNewsletterGroupCommandHandler _handler;
    
    [SetUp]
    public void Setup()
    {
        _newsletterGroupRepository = Substitute.For<IRepository<NewsletterGroup>>();
        _handler = new CreateNewsletterGroupCommandHandler(_newsletterGroupRepository);
    }

    [Test]
    public async Task WhenCreatingNewsletterGroup_TheGroupIsReturnedWithAnId()
    {
        var testNewsletterGroup = new NewsletterGroup
        {
            Name = "This is a test",
            Description = "This is a test description",
            ContactList = new ContactList
            {
                Name = "This is a test",
                Description = "This is a test contact list"
            }
        };

        _newsletterGroupRepository
            .AddAsync(testNewsletterGroup, CancellationToken.None)
            .Returns(MockHelpers.SuccessAsyncResult(testNewsletterGroup));

        Result<NewsletterGroup> result = await _handler.Handle(
            new CreateNewsletterGroupCommand(testNewsletterGroup), 
            CancellationToken.None);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.Name, Is.EqualTo(testNewsletterGroup.Name));
            Assert.That(result.Value.ContactList, Is.EqualTo(testNewsletterGroup.ContactList));
        });
    }
}
