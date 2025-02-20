using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Emails.DTOs;
using Contracts.ServiceModules.Emails.EmailTemplateEngine;
using Contracts.ServiceModules.Emails.Enums;
using Contracts.ServiceModules.Emails.Sending;
using Contracts.SupportModules.Logging;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;
using NSubstitute;
using TimerTriggers.Newsletters;
using TestHelpers;

namespace TimerTriggers.Tests.Newsletters;

[TestFixture, Category("UnitTests")]
public class DailyCleanupCampaignFunctionTests
{
    private IRepository<NewsletterGroupsCleanupCampaign> _campaignRepository;
    private IRepository<ContactList> _contactListRepository;
    private IRepository<NewsletterGroup> _newsletterGroupRepository;
    private IMediator _mediator;
    private IEmailTemplateEngine _templateEngine;
    private ILoggingHandler _logger;
    private ITraceHandler _tracer;
    private DailyCleanupCampaignFunction _function;

    [SetUp]
    public void Setup()
    {
        _campaignRepository = Substitute.For<IRepository<NewsletterGroupsCleanupCampaign>>();
        _contactListRepository = Substitute.For<IRepository<ContactList>>();
        _newsletterGroupRepository = Substitute.For<IRepository<NewsletterGroup>>();
        _mediator = Substitute.For<IMediator>();
        _templateEngine = Substitute.For<IEmailTemplateEngine>();
        _logger = Substitute.For<ILoggingHandler>();
        _tracer = Substitute.For<ITraceHandler>();
        _function = new DailyCleanupCampaignFunction(
            _campaignRepository,
            _contactListRepository,
            _newsletterGroupRepository,
            _mediator,
            _templateEngine,
            _logger,
            _tracer);
    }

    #region Active Campaign Retrieval

    [Test]
    public async Task Run_When_FindAsyncFails_ThrowsExceptionAndLogsError()
    {
        // Arrange: simulate repository failure when retrieving active campaigns.
        _campaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.FailedAsyncResult<List<NewsletterGroupsCleanupCampaign>>());

        // Act
        await _function.Run(null!);

        // Assert: Verify that LogError and Trace (with failure) are called.
        _logger.ReceivedWithAnyArgs(1).LogError(default!);
        _tracer.ReceivedWithAnyArgs(1).Trace(default!, false, default, default!);
    }

    [Test]
    public async Task Run_When_NoActiveCampaignsFound_LogsInfoAndExits()
    {
        // Arrange: return an empty list for active campaigns.
        _campaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign>()));

        // Act
        await _function.Run(null!);

        // Assert: Verify LogInformation and successful Trace.
        _logger.ReceivedWithAnyArgs(1).LogInformation(default!);
        _tracer.ReceivedWithAnyArgs(1).Trace(default!, true, default, default!);
    }

    #endregion

    #region Campaign Start and Reminder Emails

    [Test]
    public async Task Run_When_CampaignNotStarted_UpdatesCampaignToStartedAndSendsReminders()
    {
        // Arrange: Create an active campaign that is not yet started and requires sending reminders.
        DateTime now = DateTime.UtcNow;
        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = now.AddDays(-10),
            CampaignDurationMonths = 1,
            IsCampaignStarted = false,
            LastReminderSent = null,
            UncleanedRecipients = [],
            CleanedRecipients = []
        };

        _campaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign> { campaign }));

        // Simulate successful update when starting campaign.
        _campaignRepository
            .UpdateAsync(campaign)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(campaign));

        // Act
        await _function.Run(null!);

        // Assert: Campaign should be marked as started and LastReminderSent updated.
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(campaign.IsCampaignStarted, Is.True);
            Assert.That(campaign.LastReminderSent, Is.Not.Null);
            await _campaignRepository.ReceivedWithAnyArgs(2).UpdateAsync(campaign);
            _logger.ReceivedWithAnyArgs(1).LogInformation(default!);
            _tracer.ReceivedWithAnyArgs(1).Trace(default!, true, default, default!);
        });
    }

    [Test]
    public async Task Run_When_UpdateCampaignToStartedFails_LogsErrorAndTracesFailure()
    {
        // Arrange: Active campaign not started.
        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = DateTime.UtcNow.AddDays(-10),
            CampaignDurationMonths = 1,
            IsCampaignStarted = false,
            LastReminderSent = null,
            UncleanedRecipients = [],
            CleanedRecipients = []
        };

        _campaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign> { campaign }));

        // Simulate failure when updating campaign status.
        _campaignRepository
            .UpdateAsync(campaign)
            .ReturnsForAnyArgs(MockHelpers.FailedAsyncResult<NewsletterGroupsCleanupCampaign>());

        // Act
        await _function.Run(null!);

        // Assert: Verify that LogError and Trace with failure were called.
        _logger.ReceivedWithAnyArgs(1).LogError(default!);
        _tracer.ReceivedWithAnyArgs(1).Trace(default!, false, default, default!);
    }

    [Test]
    public async Task Run_When_SendReminderEmailsThrowsException_LogsErrorAndTracesFailure()
    {
        // Arrange: Active campaign that forces sending reminder emails.
        var recipient = new Recipient { Token = Guid.NewGuid(), EmailAddress = "user@example.com" };
        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = DateTime.UtcNow.AddDays(-40),
            CampaignDurationMonths = 2,
            IsCampaignStarted = true,
            LastReminderSent = null,
            UncleanedRecipients = [recipient],
            CleanedRecipients = []
        };

        _campaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign> { campaign }));

        // Simulate exception in generating email bodies.
        _templateEngine
            .GenerateBodiesFromTemplate(default!, Arg.Any<(string, string)[]>())
            .ReturnsForAnyArgs(_ => throw new Exception("Template error"));

        // Act
        await _function.Run(null!);

        // Assert: Verify that LogError and Trace with failure were called.
        _logger.ReceivedWithAnyArgs(1).LogError(default!);
        _tracer.ReceivedWithAnyArgs(1).Trace(default!, false, default, default!);
    }

    // Additional test to verify that reminder emails are sent correctly.
    [Test]
    public async Task Run_When_ReminderDue_SendsReminderEmailsSuccessfully()
    {
        // Arrange: Create an active campaign that is already started and due for a reminder.
        DateTime now = DateTime.UtcNow;
        var recipient = new Recipient { Token = Guid.NewGuid(), EmailAddress = "reminder@example.com" };
        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = now.AddDays(-20),
            CampaignDurationMonths = 2,
            IsCampaignStarted = true,
            LastReminderSent = now.AddMonths(-2), // Ensure reminder is due.
            UncleanedRecipients = [recipient],
            CleanedRecipients = []
        };

        _campaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign> { campaign }));

        // Simulate successful email template generation.
        _templateEngine
            .GenerateBodiesFromTemplate(default!, Arg.Any<(string, string)[]>())
            .ReturnsForAnyArgs(Result.Ok());
        _templateEngine.GetSubject().Returns("Cleanup Reminder");
        _templateEngine.GetHtmlBody().Returns("<html>Reminder</html>");
        _templateEngine.GetPlainTextBody().Returns("Reminder");

        // Simulate successful mediator call for sending email.
        _mediator
            .Send(Arg.Any<SendEmailCommand>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new EmailSendingStatus(
                new Email
                {
                    Attachments = [],
                    Subject = "Test email",
                    Recipients = [new Recipient { EmailAddress = "test@example.com" }],
                    HtmlBody = "<h1>This is a test</h1>",
                    PlainTextBody = "This is a test",
                    SenderEmailAddress = ValidSenderEmailAddresses.Auto
                },
                SendingStatus.Succeeded, [])));

        // Simulate successful update.
        _campaignRepository
            .UpdateAsync(campaign)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(campaign));

        // Act
        await _function.Run(null!);

        // Assert: Verify that reminder email sending was attempted.
        await _mediator.ReceivedWithAnyArgs(1).Send(Arg.Any<SendEmailCommand>(), Arg.Any<CancellationToken>());
        // Verify that campaign's LastReminderSent was updated.
        Assert.That(campaign.LastReminderSent, Is.Not.Null);
        _logger.ReceivedWithAnyArgs(1).LogInformation(default!);
        _tracer.ReceivedWithAnyArgs(1).Trace(default!, true, Arg.Any<long>(), default!);
    }

    #endregion

    #region Campaign End and Unsubscription

    [Test]
    public async Task Run_When_CampaignHasEnded_And_UnsubscribeFails_LogsErrorAndTracesFailure()
    {
        // Arrange: Create a campaign that has ended.
        DateTime now = DateTime.UtcNow;
        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = now.AddMonths(-2),
            CampaignDurationMonths = 1, // Ended one month ago.
            IsCampaignStarted = true,
            LastReminderSent = now.AddMonths(-1),
            UncleanedRecipients = [new Recipient { Id = 1, EmailAddress = "fail@example.com" }],
            CleanedRecipients = []
        };

        _campaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign> { campaign }));

        // Simulate newsletter groups retrieval succeeds.
        var contactList = new ContactList
        {
            Id = 1,
            Name = "Test Contact List",
            Description = "Test",
            Contacts = [new Recipient { Id = 1, EmailAddress = "fail@example.com" }]
        };
        var newsletterGroup = new NewsletterGroup
        {
            Name = "Test Group",
            Description = "Group Desc",
            ContactList = contactList
        };
        _newsletterGroupRepository
            .GetAllAsync(default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroup> { newsletterGroup }));

        // Simulate contact list retrieval succeeds.
        _contactListRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<ContactList> { contactList }));

        // Simulate failure on updating the contact list (unsubscribe fails).
        _contactListRepository
            .UpdateAsync(contactList)
            .ReturnsForAnyArgs(MockHelpers.FailedAsyncResult<ContactList>());

        // Act
        await _function.Run(null!);

        // Assert: Verify that LogError and Trace with failure were called.
        _logger.ReceivedWithAnyArgs(1).LogError(default!);
        _tracer.ReceivedWithAnyArgs(1).Trace(default!, false, Arg.Any<long>(), default!);
    }

    [Test]
    public async Task Run_When_CampaignHasEnded_And_UnsubscribeSucceeds_CompletesSuccessfully()
    {
        // Arrange: Create a campaign that has ended.
        DateTime now = DateTime.UtcNow;
        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = now.AddMonths(-2),
            CampaignDurationMonths = 1, // Ended one month ago.
            IsCampaignStarted = true,
            LastReminderSent = now.AddMonths(-1),
            UncleanedRecipients = [new Recipient { Id = 1, EmailAddress = "success@example.com" }],
            CleanedRecipients = []
        };

        _campaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign> { campaign }));

        // Simulate newsletter groups retrieval succeeds.
        var contactList = new ContactList
        {
            Id = 1,
            Name = "Test Contact List",
            Description = "Test",
            Contacts = [new Recipient { Id = 1, EmailAddress = "success@example.com" }]
        };
        var newsletterGroup = new NewsletterGroup
        {
            Name = "Test Group",
            Description = "Group Desc",
            ContactList = contactList
        };
        _newsletterGroupRepository
            .GetAllAsync(default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroup> { newsletterGroup }));

        // Simulate contact list retrieval succeeds.
        _contactListRepository
            .FindAsync(default!)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<ContactList> { contactList }));

        // Simulate unsubscribe succeeds by having UpdateAsync succeed.
        _contactListRepository
            .UpdateAsync(contactList)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(contactList));

        // Configure the template engine to succeed in generating email bodies.
        _templateEngine
            .GenerateBodiesFromTemplate(default!, Arg.Any<(string, string)[]>())
            .ReturnsForAnyArgs(Result.Ok());
        _templateEngine.GetSubject().Returns("Unsubscribe Notice");
        _templateEngine.GetHtmlBody().Returns("<html>Unsubscribed</html>");
        _templateEngine.GetPlainTextBody().Returns("Unsubscribed");

        // Configure mediator.Send for sending email to succeed.
        _mediator
            .Send(Arg.Any<SendEmailCommand>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new EmailSendingStatus(
                new Email
                {
                    Attachments = [],
                    Subject = "Test email",
                    Recipients = [new Recipient { EmailAddress = "test@example.com" }],
                    HtmlBody = "<h1>This is a test</h1>",
                    PlainTextBody = "This is a test",
                    SenderEmailAddress = ValidSenderEmailAddresses.Auto
                },
                SendingStatus.Succeeded,
                [])));

        // Act
        await _function.Run(null!);

        // Assert: Since unsubscribe succeeded, no LogError should be called.
        _logger.DidNotReceiveWithAnyArgs().LogError(default!);
    }

    #endregion

    #region No-Action Scenario

    [Test]
    public async Task Run_When_NoReminderDueAndCampaignNotEnded_DoesNothing()
    {
        // Arrange: Active campaign already started and reminder recently sent.
        DateTime now = DateTime.UtcNow;
        var campaign = new NewsletterGroupsCleanupCampaign
        {
            CampaignStart = now.AddDays(-20),
            CampaignDurationMonths = 2,
            IsCampaignStarted = true,
            LastReminderSent = now.AddDays(-15),
            UncleanedRecipients = [],
            CleanedRecipients = []
        };
        _campaignRepository
            .FindAsync(default!, cancellationToken: default)
            .ReturnsForAnyArgs(MockHelpers.SuccessAsyncResult(new List<NewsletterGroupsCleanupCampaign> { campaign }));

        // Act
        await _function.Run(null!);

        // Assert: Verify that UpdateAsync was not called and no reminder log is recorded.
        await _campaignRepository.DidNotReceive().UpdateAsync(campaign);
        _logger.DidNotReceiveWithAnyArgs().LogInformation(default!);
    }

    #endregion
}
