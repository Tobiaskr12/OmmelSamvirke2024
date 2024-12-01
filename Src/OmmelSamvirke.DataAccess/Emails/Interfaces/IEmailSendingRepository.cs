using FluentResults;
using OmmelSamvirke.DataAccess.Emails.Enums;

namespace OmmelSamvirke.DataAccess.Emails.Interfaces;

public interface IEmailSendingRepository
{
    /// <summary>
    /// Calculates the percentage of the service limit used
    /// after sending X amount of emails, where X is the
    /// parameter <paramref name="numberOfEmailsToSend"/>
    /// </summary>
    /// <param name="serviceLimitInterval">
    /// The service limits are partitioned in intervals.
    /// This parameter select which interval to check against.
    /// </param>
    /// <param name="numberOfEmailsToSend">The number of emails to be sent</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>
    /// The percentage of the service limit that would be used after sending the emails.
    /// The usage is returned in percent, written as a double.
    /// </returns>
    Task<Result<double>> CalculateServiceLimitAfterSendingEmails(
        ServiceLimitInterval serviceLimitInterval,
        int numberOfEmailsToSend,
        CancellationToken cancellationToken = default);
}
