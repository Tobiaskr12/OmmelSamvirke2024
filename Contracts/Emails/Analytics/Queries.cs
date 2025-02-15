using Contracts.DataAccess.Emails.Enums;
using Contracts.Emails.Analytics.Models;
using FluentResults;
using MediatR;
using DomainModules.Emails.Entities;

namespace Contracts.Emails.Analytics.Queries;

public record DailyContactListAnalyticsQuery(DateTime Date) : IRequest<Result<List<DailyContactListAnalytics>>>;

public record DailyEmailAnalyticsQuery(DateTime Date) : IRequest<Result<DailyEmailAnalytics?>>;

public record EmailsSentInIntervalQuery(DateTime StartTime, ServiceLimitInterval Interval) : IRequest<Result<int>>;

public record LoadVsServiceLimitReportQuery(DateTime StartTime, ServiceLimitInterval Interval) : IRequest<Result<LoadVsServiceLimitReport>>;
