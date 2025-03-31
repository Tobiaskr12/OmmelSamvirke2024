using DomainModules.Common;
using DomainModules.Errors;
using DomainModules.Reservations.Entities;
using FluentValidation;

namespace DomainModules.Reservations.Validators;

public class ReservationSeriesValidator : AbstractValidator<ReservationSeries>
{
    public ReservationSeriesValidator()
    {
        RuleFor(x => x.RecurrenceType)
            .NotEqual(RecurrenceType.None)
            .WithMessage(ErrorMessages.ReservationSeries_InvalidRecurrenceType);

        RuleFor(x => x.Interval)
            .GreaterThan(0)
            .WithMessage(ErrorMessages.ReservationSeries_InvalidInterval);

        RuleFor(x => x.RecurrenceStartDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage(ErrorMessages.ReservationSeries_StartDateInPast);

        RuleFor(x => x.RecurrenceEndDate)
            .GreaterThan(x => x.RecurrenceStartDate)
            .WithMessage(ErrorMessages.ReservationSeries_InvalidRecurrenceDates);
    }
}
