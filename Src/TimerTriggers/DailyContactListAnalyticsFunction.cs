using Contracts.DataAccess.Base;
using Contracts.SupportModules.Logging;
using FluentResults;
using Microsoft.Azure.Functions.Worker;
using DomainModules.Emails.Entities;

namespace TimerTriggers;

public class DailyContactListAnalyticsFunction
{
    private readonly ILoggingHandler _logger;
    private readonly IRepository<ContactList> _contactListRepository;
    private readonly IRepository<DailyContactListAnalytics> _dailyContactListAnalyticsRepository;

    public DailyContactListAnalyticsFunction(
        ILoggingHandler logger,
        IRepository<ContactList> contactListRepository,
        IRepository<DailyContactListAnalytics> dailyContactListAnalyticsRepository)
    {
        _logger = logger;
        _contactListRepository = contactListRepository;
        _dailyContactListAnalyticsRepository = dailyContactListAnalyticsRepository;
    }

    [Function("DailyContactListAnalyticsFunction")]
    public async Task Run([TimerTrigger("0 0 3 * * *")] TimerInfo myTimer)
    {
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
            }
            else
            {
                throw new Exception("Analysis completed successfully, but saving the analysis failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex);
            throw;
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

            foreach (ContactList contactList in queryResult.Value)
            {
                contactListSubscribers.Add(new DailyContactListAnalytics
                {
                    Date = yesterday.Date,
                    ContactListName = contactList.Name,
                    TotalContacts = contactList.Contacts.Count,
                    IsNewsletter = false // TODO - update this when the newsletter module has been implemented
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
