using System.Diagnostics;
using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Emails.DTOs;
using Contracts.ServiceModules.Emails.EmailTemplateEngine;
using Contracts.ServiceModules.Emails.Sending;
using Contracts.SupportModules.Logging;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;
using Microsoft.Azure.Functions.Worker;

namespace TimerTriggers.Newsletters;

public class DailyCleanupCampaignFunction
{
    private readonly IRepository<NewsletterGroupsCleanupCampaign> _cleanupCampaignRepository;
    private readonly IRepository<ContactList> _contactListRepository;
    private readonly IRepository<NewsletterGroup> _newsletterGroupsRepository;
    private readonly IMediator _mediator;
    private readonly IEmailTemplateEngine _templateEngine;
    private readonly ILoggingHandler _logger;
    private readonly ITraceHandler _tracer;

    public DailyCleanupCampaignFunction(
        IRepository<NewsletterGroupsCleanupCampaign> cleanupCampaignRepository,
        IRepository<ContactList> contactListRepository,
        IRepository<NewsletterGroup> newsletterGroupsRepository,
        IMediator mediator,
        IEmailTemplateEngine templateEngine,
        ILoggingHandler logger,
        ITraceHandler tracer)
    {
        _cleanupCampaignRepository = cleanupCampaignRepository;
        _contactListRepository = contactListRepository;
        _newsletterGroupsRepository = newsletterGroupsRepository;
        _mediator = mediator;
        _templateEngine = templateEngine;
        _logger = logger;
        _tracer = tracer;
    }

    [Function("DailyCleanupCampaignFunction")]
    public async Task Run([TimerTrigger("0 0 11 * * *")] TimerInfo myTimer)
    {
        DateTime now = DateTime.UtcNow;
        var sw = Stopwatch.StartNew();

        try
        {
            // Retrieve active cleanup campaigns.
            Result<List<NewsletterGroupsCleanupCampaign>> cleanupCampaignQuery = await _cleanupCampaignRepository.FindAsync(
                x => x.CampaignStart <= now &&
                     now <= x.CampaignStart.AddMonths(x.CampaignDurationMonths),
                readOnly: false);

            if (cleanupCampaignQuery.IsFailed)
            {
                throw new Exception(string.Join("; ", cleanupCampaignQuery.Errors.Select(e => e.Message)));
            }

            List<NewsletterGroupsCleanupCampaign> cleanupCampaigns = cleanupCampaignQuery.Value;
            if (cleanupCampaigns.Count > 1)
            {
                _logger.LogWarning($"DailyCleanupCampaignFunction found {cleanupCampaigns.Count} active cleanup campaigns. There should never be more than one at a time.");
            }

            // Quit if no active cleanup campaign.
            NewsletterGroupsCleanupCampaign? cleanupCampaign = cleanupCampaigns.FirstOrDefault();
            if (cleanupCampaign is null)
            {
                _logger.LogInformation("DailyCleanupCampaignFunction found no active cleanup campaigns.");
                _tracer.Trace("TimerTrigger", isSuccess: true, sw.ElapsedMilliseconds, "DailyCleanupCampaignFunction");
                return;
            }

            DateTime campaignEndTime = cleanupCampaign.CampaignStart.AddMonths(cleanupCampaign.CampaignDurationMonths);

            // Start campaign if not already started.
            if (!cleanupCampaign.IsCampaignStarted)
            {
                cleanupCampaign.IsCampaignStarted = true;
                var updateResult = await _cleanupCampaignRepository.UpdateAsync(cleanupCampaign);
                if (updateResult.IsFailed)
                {
                    throw new Exception("Failed to update campaign status.");
                }
                cleanupCampaign = updateResult.Value;
            }

            // Send reminder emails if needed.
            if (cleanupCampaign.LastReminderSent is null ||
                (cleanupCampaign.LastReminderSent.Value.AddMonths(1) < now && campaignEndTime > now.AddDays(7)))
            {
                Result sendResult = await SendCampaignEmails(cleanupCampaign);
                if (sendResult.IsFailed)
                {
                    throw new Exception(string.Join("; ", sendResult.Errors.Select(e => e.Message)));
                }

                cleanupCampaign.LastReminderSent = now;
                await _cleanupCampaignRepository.UpdateAsync(cleanupCampaign);

                _logger.LogInformation("DailyCleanupCampaignFunction sent reminder emails.");
                _tracer.Trace("TimerTrigger", isSuccess: true, sw.ElapsedMilliseconds, "DailyCleanupCampaignFunction");
                return;
            }

            // If the campaign has ended, unsubscribe all unresponsive recipients.
            if (campaignEndTime < now)
            {
                Result unsubscribeResult = await UnsubscribeUnresponsiveRecipients(cleanupCampaign);
                if (unsubscribeResult.IsFailed)
                {
                    throw new Exception(string.Join("; ", unsubscribeResult.Errors.Select(e => e.Message)));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex);
            _tracer.Trace("TimerTrigger", isSuccess: false, sw.ElapsedMilliseconds, "DailyCleanupCampaignFunction");
        }
    }

    private async Task<Result> SendCampaignEmails(NewsletterGroupsCleanupCampaign cleanupCampaign)
    {
        try
        {
            foreach (Recipient uncleanedRecipient in cleanupCampaign.UncleanedRecipients)
            {
                var generationResult = _templateEngine.GenerateBodiesFromTemplate(Templates.Newsletters.CleanupCampaign,
                    ("MonthsDuration", cleanupCampaign.CampaignDurationMonths.ToString()),
                    ("ConfirmationLink", $"https://www.ommelsamvirke.com/newsletter-cleanup-unsubscribe?token={uncleanedRecipient.Token}"),
                    ("UnsubscribeLink", $"https://www.ommelsamvirke.com/newsletter-cleanup-confirm-subscription?token={uncleanedRecipient.Token}")
                );

                if (generationResult.IsFailed)
                {
                    _logger.LogWarning($"Cleanup campaign email generation failed for recipient {uncleanedRecipient.EmailAddress}: {string.Join(";", generationResult.Errors.Select(e => e.Message))}");
                    continue;
                }

                var email = new Email
                {
                    Subject = _templateEngine.GetSubject(),
                    SenderEmailAddress = ValidSenderEmailAddresses.Newsletter,
                    Attachments = [],
                    Recipients = [uncleanedRecipient],
                    HtmlBody = _templateEngine.GetHtmlBody(),
                    PlainTextBody = _templateEngine.GetPlainTextBody()
                };

                Result<EmailSendingStatus> sendResult = await _mediator.Send(new SendEmailCommand(email));
                if (sendResult.IsFailed)
                {
                    _logger.LogWarning($"Failed sending cleanup campaign email to recipient {uncleanedRecipient.EmailAddress}: {string.Join("; ", sendResult.Errors.Select(e => e.Message))}");
                }
            }
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    private async Task<Result> UnsubscribeUnresponsiveRecipients(NewsletterGroupsCleanupCampaign cleanupCampaign)
    {
        Result<List<NewsletterGroup>> newsletterGroupsResult = await _newsletterGroupsRepository.GetAllAsync();
        if (newsletterGroupsResult.IsFailed)
        {
            return Result.Fail($"Could not perform newsletter cleanup: {string.Join("; ", newsletterGroupsResult.Errors.Select(e => e.Message))}");
        }

        List<NewsletterGroup> newsletterGroups = newsletterGroupsResult.Value;
        List<int> newsletterContactListIds = newsletterGroups.Select(ng => ng.ContactList.Id).ToList();

        Result<List<ContactList>> contactListsResult = await _contactListRepository.FindAsync(
            x => newsletterContactListIds.Contains(x.Id),
            readOnly: false);
        if (contactListsResult.IsFailed)
        {
            return Result.Fail($"Could not get contact lists for newsletter groups: {string.Join("; ", contactListsResult.Errors.Select(e => e.Message))}");
        }

        List<ContactList> contactLists = contactListsResult.Value;

        // Use a copy of the uncleaned recipients list to avoid modification issues during iteration.
        foreach (Recipient uncleanedRecipient in cleanupCampaign.UncleanedRecipients.ToList())
        {
            List<ContactList> affectedContactLists = contactLists
                                                     .Where(cl => cl.Contacts.Any(c => c.Id == uncleanedRecipient.Id))
                                                     .ToList();

            List<NewsletterGroup> affectedNewsletterGroups = newsletterGroups
                                                             .Where(ng => affectedContactLists.Any(cl => cl.Id == ng.ContactList.Id))
                                                             .ToList();

            Result emailGenerationResult = _templateEngine.GenerateBodiesFromTemplate(Templates.Newsletters.CleanupNotice,
                ("UnsubscribedNewsletters", string.Join(", ", affectedNewsletterGroups.Select(ng => ng.Name))));

            if (emailGenerationResult.IsFailed)
            {
                _logger.LogWarning($"Failed generating cleanup email for recipient {uncleanedRecipient.EmailAddress}: {string.Join("; ", emailGenerationResult.Errors.Select(e => e.Message))}");
            }
            else
            {
                var email = new Email
                {
                    SenderEmailAddress = ValidSenderEmailAddresses.Newsletter,
                    Attachments = [],
                    Subject = _templateEngine.GetSubject(),
                    Recipients = [uncleanedRecipient],
                    HtmlBody = _templateEngine.GetHtmlBody(),
                    PlainTextBody = _templateEngine.GetPlainTextBody()
                };

                await _mediator.Send(new SendEmailCommand(email));
            }

            // Remove the recipient from every affected contact list.
            foreach (ContactList contactList in affectedContactLists)
            {
                Recipient? recipientInList = contactList.Contacts.FirstOrDefault(c => c.Id == uncleanedRecipient.Id);
                if (recipientInList is null)
                {
                    throw new Exception($"Could not unsubscribe recipient with id: {uncleanedRecipient.Id} from contact list with id {contactList.Id}");
                }

                contactList.Contacts.Remove(recipientInList);
                Result<ContactList> updateResult = await _contactListRepository.UpdateAsync(contactList);
                if (updateResult.IsFailed)
                {
                    _logger.LogWarning($"Failed updating contact list for recipient id {uncleanedRecipient.Id}: {string.Join("; ", updateResult.Errors.Select(e => e.Message))}");
                }
            }
        }

        return Result.Ok();
    }
}
