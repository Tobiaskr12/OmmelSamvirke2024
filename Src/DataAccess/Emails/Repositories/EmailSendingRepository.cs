using Contracts.DataAccess.Emails;
using Contracts.DataAccess.Emails.Enums;
using Contracts.SupportModules.Logging;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using DataAccess.Base;
using DataAccess.Errors;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;

namespace DataAccess.Emails.Repositories;

public class EmailSendingRepository : IEmailSendingRepository
{
    private readonly OmmelSamvirkeDbContext _context;
    private readonly ILoggingHandler _logger;

    public EmailSendingRepository(OmmelSamvirkeDbContext context, ILoggingHandler logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<Result<double>> CalculateServiceLimitAfterSendingEmails(
        ServiceLimitInterval serviceLimitInterval,
        int numberOfEmailsToSend,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            $"CalculateServiceLimitAfterSendingEmails called with - serviceLimitInterval: {serviceLimitInterval}, numberOfEmailsToSend: {numberOfEmailsToSend}"
        );

        try
        {
            int numberOfEmailsSentInInterval;
            double serviceLimit;

            if (numberOfEmailsToSend < 0)
            {
                throw new Exception(ErrorMessages.NumberOfEmailsToSend_Negative);
            }

            switch (serviceLimitInterval)
            {
                case ServiceLimitInterval.PerMinute:
                    DateTime oneMinuteAgo = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1));
                    numberOfEmailsSentInInterval = await _context.Set<Email>().CountAsync(e => 
                        e.DateCreated >= oneMinuteAgo,
                        cancellationToken: cancellationToken);
                    serviceLimit = ServiceLimits.EmailsPerMinute;
                    break;
                case ServiceLimitInterval.PerHour:
                    DateTime oneHourAgo = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
                    numberOfEmailsSentInInterval = await _context.Set<Email>().CountAsync(e => 
                        e.DateCreated >= oneHourAgo,
                        cancellationToken: cancellationToken);
                    serviceLimit = ServiceLimits.EmailsPerHour;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(serviceLimitInterval),
                        serviceLimitInterval,
                        ErrorMessages.ServiceLimitInterval_ArgumentException);
            }

            double usagePercentage = (numberOfEmailsSentInInterval + numberOfEmailsToSend) / serviceLimit;
            return Result.Ok(usagePercentage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while calculating email service limit usage");
            return Result.Fail<double>(new DatabaseError($"An error occurred: {ex.Message}"));
        }
    }
}
