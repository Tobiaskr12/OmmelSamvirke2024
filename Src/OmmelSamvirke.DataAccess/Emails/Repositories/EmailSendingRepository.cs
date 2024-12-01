using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DataAccess.Emails.Enums;
using OmmelSamvirke.DataAccess.Emails.Interfaces;
using OmmelSamvirke.DataAccess.Errors;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.DomainModules.Emails.Entities;

namespace OmmelSamvirke.DataAccess.Emails.Repositories;

public class EmailSendingRepository : IEmailSendingRepository
{
    private readonly OmmelSamvirkeDbContext _context;
    private readonly ILogger _logger;

    public EmailSendingRepository(OmmelSamvirkeDbContext context, ILogger logger)
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
            "CalculateServiceLimitAfterSendingEmails called with - serviceLimitInterval: {interval}, numberOfEmailsToSend: {emailCount}",
            serviceLimitInterval,
            numberOfEmailsToSend);

        try
        {
            int numberOfEmailsSentInInterval;
            double serviceLimit;

            if (numberOfEmailsToSend < 0)
            {
                _logger.LogError("CalculateServiceLimitAfterSendingEmails was called with a negative value");
                return Result.Fail<double>(new Error(ErrorMessages.NumberOfEmailsToSend_Negative));
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
