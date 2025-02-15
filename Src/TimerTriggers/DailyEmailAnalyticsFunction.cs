using Contracts.DataAccess.Base;
using Contracts.SupportModules.Logging;
using FluentResults;
using Microsoft.Azure.Functions.Worker;
using DomainModules.Emails.Entities;

namespace TimerTriggers;

public class DailyEmailAnalyticsFunction
{
    private readonly ILoggingHandler _logger;
    private readonly IRepository<Email> _emailRepository;
    private readonly IRepository<DailyEmailAnalytics> _dailyEmailAnalyticsRepository;

    public DailyEmailAnalyticsFunction(
        ILoggingHandler logger,
        IRepository<Email> emailRepository,
        IRepository<DailyEmailAnalytics> dailyEmailAnalyticsRepository)
    {
        _logger = logger;
        _emailRepository = emailRepository;
        _dailyEmailAnalyticsRepository = dailyEmailAnalyticsRepository;
    }

    [Function("DailyEmailAnalyticsFunction")]
    public async Task Run([TimerTrigger("0 0 3 * * *")] TimerInfo myTimer)
    {
        try
        {
            _logger.LogInformation($"EmailAnalyticsCron Timer trigger executed at: {DateTime.UtcNow}");

            // Calculate number of emails sent yesterday
            Result<int> emailsSentResult = await GetNumberOfEmailsSentYesterday();
            if (emailsSentResult.IsSuccess)
            {
                _logger.LogInformation(
                    $"Successfully calculated number of emails sent yesterday. Sent emails: {emailsSentResult.Value}"
                );
            }
            else
            {
                throw new Exception($"Error retrieving number of emails sent yesterday: {emailsSentResult.Errors}");
            }

            // Calculate number of recipients of emails sent yesterday
            Result<int> emailRecipientsResult = await GetNumberOfEmailRecipientsYesterday();
            if (emailRecipientsResult.IsSuccess)
            {
                _logger.LogInformation(
                    "Successfully calculated number of recipients of emails sent yesterday. Recipients: {emailRecipientsResult.Value}"
                );
            }
            else
            {
                throw new Exception($"Error retrieving number of email recipients yesterday: {emailRecipientsResult.Errors}");
            }
            
            // Save analysis results
            var dailyEmailAnalytics = new DailyEmailAnalytics
            {
                Date = StartOfDay(DateTime.UtcNow.AddDays(-1)),
                SentEmails = emailsSentResult.Value,
                TotalRecipients = emailRecipientsResult.Value,
            };
            Result<DailyEmailAnalytics> saveResult = await _dailyEmailAnalyticsRepository.AddAsync(dailyEmailAnalytics);
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

    private async Task<Result<int>> GetNumberOfEmailsSentYesterday()
    {
        try
        {
            DateTime yesterday = DateTime.UtcNow.AddDays(-1);
            Result<List<Email>> emailsResult = await _emailRepository.FindAsync(email =>
                email.DateCreated >= StartOfDay(yesterday) &&
                email.DateCreated <= EndOfDay(yesterday)
            );

            if (emailsResult.IsFailed)
            {
                return Result.Fail<int>(emailsResult.Errors);
            }

            int count = emailsResult.Value.Count;
            return Result.Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex);
            return Result.Fail<int>(ex.Message);
        }
    }

    private async Task<Result<int>> GetNumberOfEmailRecipientsYesterday()
    {
        try
        {
            DateTime yesterday = DateTime.UtcNow.AddDays(-1);
            Result<List<Email>> emailsResult = await _emailRepository.FindAsync(email =>
                email.DateCreated >= StartOfDay(yesterday) &&
                email.DateCreated <= EndOfDay(yesterday)
            );

            if (emailsResult.IsFailed)
            {
                return Result.Fail<int>(emailsResult.Errors);
            }
                
            int totalRecipients = emailsResult.Value.Sum(email => email.Recipients.Count);
            return Result.Ok(totalRecipients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex);
            return Result.Fail<int>(ex.Message);
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
