using DomainModules.Errors;
using DomainModules.Reservations.Entities;
using FluentValidation;

namespace DomainModules.Reservations.Validators;

public class BlockedReservationTimeSlotValidator : AbstractValidator<BlockedReservationTimeSlot>
{
    public BlockedReservationTimeSlotValidator()
    {
        RuleFor(x => x.StartTime)
            .GreaterThanOrEqualTo(_ => DateTime.UtcNow)
            .WithMessage(ErrorMessages.BlockedReservationTimeSlot_StartTime_InPast);

        RuleFor(x => x.EndTime)
            .GreaterThanOrEqualTo(x => x.StartTime.AddHours(1))
            .WithMessage(ErrorMessages.BlockedReservationTimeSlot_EndTime_MustBeAfterStart);
    }
}
