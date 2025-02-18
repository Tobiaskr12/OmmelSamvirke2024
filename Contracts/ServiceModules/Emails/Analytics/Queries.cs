using Contracts.DataAccess.Emails.Enums;
using Contracts.ServiceModules.Emails.Analytics.Models;
using DomainModules.Emails.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Emails.Analytics;

public record DailyContactListAnalyticsQuery(DateTime Date) : IRequest<Result<List<DailyContactListAnalytics>>>;

public record DailyEmailAnalyticsQuery(DateTime Date) : IRequest<Result<DailyEmailAnalytics?>>;

public record EmailsSentInIntervalQuery(DateTime StartTime, ServiceLimitInterval Interval) : IRequest<Result<int>>;

public record LoadVsServiceLimitReportQuery(DateTime StartTime, ServiceLimitInterval Interval) : IRequest<Result<LoadVsServiceLimitReport>>;
