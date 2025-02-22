using System.Diagnostics;
using Contracts.DataAccess.Base;
using Contracts.SupportModules.Logging;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;
using FluentResults;
using Microsoft.Azure.Functions.Worker;

namespace TimerTriggers.Emails;

public class DailyContactListAnalyticsFunction
{
    private readonly ILoggingHandler _logger;
    private readonly ITraceHandler _tracer;
    private readonly IRepository<ContactList> _contactListRepository;
    private readonly IRepository<NewsletterGroup> _newsletterGroupRepository;
    private readonly IRepository<DailyContactListAnalytics> _dailyContactListAnalyticsRepository;

    public DailyContactListAnalyticsFunction(
        ILoggingHandler logger,
        ITraceHandler tracer,
        IRepository<ContactList> contactListRepository,
        IRepository<NewsletterGroup> newsletterGroupRepository,
        IRepository<DailyContactListAnalytics> dailyContactListAnalyticsRepository)
    {
        _logger = logger;
        _tracer = tracer;
        _contactListRepository = contactListRepository;
        _newsletterGroupRepository = newsletterGroupRepository;
        _dailyContactListAnalyticsRepository = dailyContactListAnalyticsRepository;
    }

    [Function("DailyContactListAnalyticsFunction")]
    public async Task Run([TimerTrigger("0 0 3 * * *")] TimerInfo myTimer)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation($"ContactListAnalyticsCron Timer trigger executed at: {DateTime.UtcNow}");

            Result<List<DailyContactListAnalytics>> contactListAnalyticsResult = await GetContactListSubscribersYesterday();
            if (contactListAnalyticsResult.IsSuccess)
            {
                _logger.LogInformation(
                    $"Successfully gathered analytics for contacts in contact lists yesterday. ContactList count: {contactListAnalyticsResult.Value.Count}"
                );
            }
            else
            {
                throw new Exception($"Error retrieving contact list analytics from yesterday: {contactListAnalyticsResult.Errors}");
            }

            Result<List<DailyContactListAnalytics>> saveResult = await _dailyContactListAnalyticsRepository.AddAsync(contactListAnalyticsResult.Value);
            if (saveResult.IsSuccess)
            {
                _logger.LogInformation("Successfully completed execution");
                _tracer.Trace("TimerTrigger", isSuccess: true, sw.ElapsedMilliseconds, "DailyContactListAnalyticsFunction");
            }
            else
            {
                throw new Exception("Analysis completed successfully, but saving the analysis failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex);
            _tracer.Trace("TimerTrigger", isSuccess: false, sw.ElapsedMilliseconds, "DailyContactListAnalyticsFunction");
        }
    }

    private async Task<Result<List<DailyContactListAnalytics>>> GetContactListSubscribersYesterday()
    {
        try
        {
            DateTime yesterday = DateTime.UtcNow.AddDays(-1);
            List<DailyContactListAnalytics> contactListSubscribers = [];
            
            Result<List<ContactList>> queryResult = await _contactListRepository.FindAsync(cl => 
                cl.DateCreated >= StartOfDay(yesterday) &&
                cl.DateCreated <= EndOfDay(yesterday)
            );

            if (queryResult.IsFailed)
            {
                return Result.Fail<List<DailyContactListAnalytics>>(queryResult.Errors);
            }
            
            // Get all newsletter groups
            Result<List<NewsletterGroup>> newsletterGroupsQuery = await _newsletterGroupRepository.GetAllAsync();
            if (newsletterGroupsQuery.IsFailed)
            {
                return Result.Fail<List<DailyContactListAnalytics>>(newsletterGroupsQuery.Errors);
            }

            List<NewsletterGroup>? newsletterGroups = newsletterGroupsQuery.Value;
            
            foreach (ContactList contactList in queryResult.Value)
            {
                bool isNewsletter = newsletterGroups.Any(x => x.ContactList.Id == contactList.Id);
                contactListSubscribers.Add(new DailyContactListAnalytics
                {
                    Date = yesterday.Date,
                    ContactListName = contactList.Name,
                    TotalContacts = contactList.Contacts.Count,
                    IsNewsletter = isNewsletter
                });
            }

            return Result.Ok(contactListSubscribers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex);
            return Result.Fail<List<DailyContactListAnalytics>>(ex.Message);
        }
    }
    
    private static DateTime EndOfDay(DateTime date)
    {
        return date.Date.AddDays(1).AddTicks(-1);
    }

    private static DateTime StartOfDay(DateTime date)
    {
        return date.Date;
    }
}
