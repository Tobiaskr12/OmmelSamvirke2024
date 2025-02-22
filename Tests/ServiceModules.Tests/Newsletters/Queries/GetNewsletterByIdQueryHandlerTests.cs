using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters;
using DomainModules.Emails.Entities;
using FluentResults;
using NSubstitute;
using ServiceModules.Newsletters.Queries;

namespace ServiceModules.Tests.Newsletters.Queries;

[TestFixture, Category("UnitTests")]
public class GetNewsletterByIdQueryHandlerTests
{
    private IRepository<Email> _emailRepository;
    private GetNewsletterByIdQueryHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _emailRepository = Substitute.For<IRepository<Email>>();
        _handler = new GetNewsletterByIdQueryHandler(_emailRepository);
    }

    [Test]
    public async Task Handle_WhenEmailExistsAndIsNewsletter_ReturnsEmail()
    {
        // Arrange
        var email = new Email
        {
            Id = 1,
            SenderEmailAddress = "sender@example.com",
            Subject = "Newsletter Subject",
            HtmlBody = "<p>Content</p>",
            PlainTextBody = "Content",
            Recipients = [],
            Attachments = [],
            IsNewsletter = true,
            DateCreated = DateTime.UtcNow
        };

        _emailRepository
            .GetByIdAsync(1, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Ok(email)));

        var query = new GetNewsletterByIdQuery(1);

        // Act
        Result<Email> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Id, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Handle_WhenEmailNotFound_ReturnsFailure()
    {
        // Arrange
        _emailRepository
            .GetByIdAsync(Arg.Any<int>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Fail<Email>("Not found")));

        var query = new GetNewsletterByIdQuery(1);

        // Act
        Result<Email> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed, Is.True);
    }

    [Test]
    public async Task Handle_WhenEmailExistsButNotNewsletter_ReturnsFailure()
    {
        // Arrange
        var email = new Email
        {
            Id = 1,
            SenderEmailAddress = "sender@example.com",
            Subject = "Regular Email Subject",
            HtmlBody = "<p>Content</p>",
            PlainTextBody = "Content",
            Recipients = [],
            Attachments = [],
            IsNewsletter = false,
            DateCreated = DateTime.UtcNow
        };

        _emailRepository.GetByIdAsync(1, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Ok(email)));

        var query = new GetNewsletterByIdQuery(1);

        // Act
        Result<Email> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed, Is.True);
    }
}
