using System.Linq.Expressions;
using Contracts.DataAccess.Base;
using Contracts.DataAccess.Emails.Enums;
using FluentResults;
using NSubstitute;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Emails.Analytics.Queries;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.Analytics.Queries;

[TestFixture, Category("UnitTests")]
public class EmailsSentInIntervalQueryTests
{
    private IRepository<Email> _emailRepository;
    private EmailsSentInIntervalQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _emailRepository = Substitute.For<IRepository<Email>>();
        _handler = new EmailsSentInIntervalQueryHandler(_emailRepository);
    }

    [Test]
    public async Task EmailsSentInIntervalQuery_ValidPerMinute_ReturnsEmailCount()
    {
        // Arrange
        DateTime startTime = DateTime.UtcNow.AddMinutes(-10);
        var query = new EmailsSentInIntervalQuery(startTime, ServiceLimitInterval.PerMinute);

        List<Email> emailsInInterval =
        [
            new()
            {
                SenderEmailAddress = "sender1@example.com",
                Subject = "Subject1",
                HtmlBody = "<p>Body1</p>",
                PlainTextBody = "Body1",
                Recipients = [],
                Attachments = [],
                DateCreated = startTime.AddSeconds(10)
            },
            new()
            {
                SenderEmailAddress = "sender2@example.com",
                Subject = "Subject2",
                HtmlBody = "<p>Body2</p>",
                PlainTextBody = "Body2",
                Recipients = [],
                Attachments = [],
                DateCreated = startTime.AddSeconds(20)
            }
        ];

        SetupRepositoryForEmailsInIntervalCall(
            emailsInInterval, 
            testDateInside: startTime.AddSeconds(30), 
            testDateOutside: startTime.AddMinutes(2)
        );

        // Act
        Result<int> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.EqualTo(emailsInInterval.Count));
        });
    }

    [Test]
    public async Task EmailsSentInIntervalQuery_ValidPerHour_ReturnsEmailCount()
    {
        // Arrange
        DateTime startTime = DateTime.UtcNow.AddHours(-2);
        var query = new EmailsSentInIntervalQuery(startTime, ServiceLimitInterval.PerHour);

        List<Email> emailsInInterval =
        [
            new()
            {
                SenderEmailAddress = "sender1@example.com",
                Subject = "Subject1",
                HtmlBody = "<p>Body1</p>",
                PlainTextBody = "Body1",
                Recipients = [],
                Attachments = [],
                DateCreated = startTime.AddMinutes(10)
            },
            new()
            {
                SenderEmailAddress = "sender2@example.com",
                Subject = "Subject2",
                HtmlBody = "<p>Body2</p>",
                PlainTextBody = "Body2",
                Recipients = [],
                Attachments = [],
                DateCreated = startTime.AddMinutes(20)
            },
            new()
            {
                SenderEmailAddress = "sender3@example.com",
                Subject = "Subject3",
                HtmlBody = "<p>Body3</p>",
                PlainTextBody = "Body3",
                Recipients = [],
                Attachments = [],
                DateCreated = startTime.AddMinutes(30)
            }
        ];

        SetupRepositoryForEmailsInIntervalCall(
            emailsInInterval, 
            testDateInside: startTime.AddMinutes(15), 
            testDateOutside: startTime.AddHours(2)
        );

        // Act
        Result<int> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value, Is.EqualTo(emailsInInterval.Count));
        });
    }

    [Test]
    public async Task EmailsSentInIntervalQuery_InvalidInterval_ReturnsFailure()
    {
        DateTime startTime = DateTime.UtcNow.AddMinutes(-10);
        var invalidInterval = (ServiceLimitInterval)99;
        var query = new EmailsSentInIntervalQuery(startTime, invalidInterval);
        
        Result<int> result = await _handler.Handle(query, CancellationToken.None);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors[0].Message, Is.EqualTo(ErrorMessages.ServiceLimits_InvalidEmailInterval));
        });
    }

    [Test]
    public async Task EmailsSentInIntervalQuery_StartTimeInFuture_ReturnsFailure()
    {
        DateTime futureStartTime = DateTime.UtcNow.AddMinutes(5);
        var query = new EmailsSentInIntervalQuery(futureStartTime, ServiceLimitInterval.PerMinute);
        
        Result<int> result = await _handler.Handle(query, CancellationToken.None);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors[0].Message, Is.EqualTo(ErrorMessages.ServiceLimits_InvalidEmailInterval));
        });
    }

    [Test]
    public async Task EmailsSentInIntervalQuery_RepositoryFailure_ReturnsGenericError()
    {
        DateTime startTime = DateTime.UtcNow.AddMinutes(-10);
        var query = new EmailsSentInIntervalQuery(startTime, ServiceLimitInterval.PerMinute);
        _emailRepository.FindAsync(default!).ReturnsForAnyArgs(Result.Fail<List<Email>>(new List<string> { "Database error" }));
        
        Result<int> result = await _handler.Handle(query, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailed);
            Assert.That(result.Errors[0].Message, Is.EqualTo(ErrorMessages.GenericErrorWithRetryPrompt));
        });
    }
    
    private void SetupRepositoryForEmailsInIntervalCall(List<Email> emailsInInterval, DateTime testDateInside, DateTime testDateOutside)
    {
        _emailRepository
            .FindAsync(
                Arg.Is<Expression<Func<Email, bool>>>(expr =>
                    // When the predicate is compiled, an email with DateCreated equal to testDateInside should match
                    expr.Compile().Invoke(new Email
                    {
                        SenderEmailAddress = "dummy",
                        Subject = "dummy",
                        HtmlBody = "dummy",
                        PlainTextBody = "dummy",
                        Recipients = new List<Recipient>(),
                        Attachments = new List<Attachment>(),
                        DateCreated = testDateInside
                    })
                    // And one with DateCreated equal to testDateOutside should not match.
                    &&
                    !expr.Compile().Invoke(new Email
                    {
                        SenderEmailAddress = "dummy",
                        Subject = "dummy",
                        HtmlBody = "dummy",
                        PlainTextBody = "dummy",
                        Recipients = new List<Recipient>(),
                        Attachments = new List<Attachment>(),
                        DateCreated = testDateOutside
                    })
                ),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Result.Ok(emailsInInterval));
    }
}
