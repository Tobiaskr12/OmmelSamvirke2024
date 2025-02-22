using Contracts.DataAccess;
using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters;
using DomainModules.Emails.Entities;
using FluentResults;
using ServiceModules.Newsletters.Queries;
using TestHelpers;

namespace ServiceModules.Tests.Newsletters.Queries;

[TestFixture, Category("IntegrationTest")]
public class GetPaginatedEmailsQueryHandlerTests
{
    private IntegrationTestingHelper _integrationTesting;
    private IRepository<Email> _emailRepository;
    private GetPaginatedEmailsQueryHandler _handler;
    private List<Email> _emails;

    [SetUp]
    public async Task SetUp()
    {
        _integrationTesting = new IntegrationTestingHelper();
        await _integrationTesting.ResetDatabase();

        _emailRepository = _integrationTesting.GetService<IRepository<Email>>();
        _handler = new GetPaginatedEmailsQueryHandler(_emailRepository);

        // Create a list of 50 emails
        _emails = [];
        for (int i = 1; i <= 50; i++)
        {
            _emails.Add(new Email
            {
                SenderEmailAddress = $"sender{i}@example.com",
                Subject = "Newsletter Subject",
                HtmlBody = "<p>Content</p>",
                PlainTextBody = "Content",
                Recipients = [],
                Attachments = [],
                IsNewsletter = i % 2 == 0
            });
        }

        await _emailRepository.AddAsync(_emails);
    }

    [Test]
    public async Task Handle_DefaultPagination_ReturnsPageSize20_OrderedByDateCreatedDesc()
    {
        var query = new GetPaginatedEmailsQuery();

        // Act
        Result<PaginatedResult<Email>> result = await _handler.Handle(query, CancellationToken.None);
        PaginatedResult<Email>? paginatedResult = result.Value;
        Email expectedFirst = _emails.OrderByDescending(e => e.DateCreated ?? DateTime.MinValue).First();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(paginatedResult.Items, Has.Count.EqualTo(20));
            Assert.That(paginatedResult.Items.First().Id, Is.EqualTo(expectedFirst.Id));
            Assert.That(paginatedResult.TotalCount, Is.EqualTo(_emails.Count));
        });
    }
}
