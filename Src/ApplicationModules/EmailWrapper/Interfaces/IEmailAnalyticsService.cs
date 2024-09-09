using EmailWrapper.DTOs;

namespace EmailWrapper.Interfaces;

public interface IEmailAnalyticsService
{
    SentEmailsReportDto GetSentEmailsReport(DateTime reportStartTimeUtc, DateTime reportEndTimeUtc);
    ServiceLimitsDto GetServiceLimits();
}
