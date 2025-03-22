using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using DomainModules.Reservations.Enums;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Reservations.Commands;

[UsedImplicitly]
public class CreateBlockedReservationTimeSlotCommandValidator : AbstractValidator<CreateBlockedReservationTimeSlotCommand>
{
    public CreateBlockedReservationTimeSlotCommandValidator(IValidator<BlockedReservationTimeSlot> blockedReservationTimeSlotValidator)
    {
        RuleFor(x => x.BlockedReservationTimeSlot).SetValidator(blockedReservationTimeSlotValidator);
    }
}

public class CreateBlockedReservationTimeSlotCommandHandler : IRequestHandler<CreateBlockedReservationTimeSlotCommand, Result<List<BlockedReservationTimeSlot>>>
{
    private readonly IRepository<BlockedReservationTimeSlot> _blockedTimeSlotRepository;

    public CreateBlockedReservationTimeSlotCommandHandler(IRepository<BlockedReservationTimeSlot> blockedTimeSlotRepository)
    {
        _blockedTimeSlotRepository = blockedTimeSlotRepository;
    }

    public async Task<Result<List<BlockedReservationTimeSlot>>> Handle(CreateBlockedReservationTimeSlotCommand request, CancellationToken cancellationToken)
    {
        var createdTimeSlots = new List<BlockedReservationTimeSlot>();

        // If recurrence options are provided and a recurrence type is set, generate multiple time slots
        if (request.RecurrenceOptions != null && request.RecurrenceOptions.RecurrenceType != RecurrenceType.None)
        {
            IEnumerable<DateTime> recurrenceDates = RecurringDatesHelper.GenerateRecurrenceDates(request.RecurrenceOptions);
            foreach (DateTime date in recurrenceDates)
            {
                // Adjust start and end times to the recurrence date while preserving the time-of-day
                TimeSpan timeOffset = request.BlockedReservationTimeSlot.EndTime - request.BlockedReservationTimeSlot.StartTime;
                var newSlot = new BlockedReservationTimeSlot
                {
                    StartTime = new DateTime(date.Year, date.Month, date.Day,
                                              request.BlockedReservationTimeSlot.StartTime.Hour,
                                              request.BlockedReservationTimeSlot.StartTime.Minute,
                                              request.BlockedReservationTimeSlot.StartTime.Second, 
                                              DateTimeKind.Utc),
                    
                    EndTime = new DateTime(date.Year, date.Month, date.Day,
                                            request.BlockedReservationTimeSlot.StartTime.Hour,
                                            request.BlockedReservationTimeSlot.StartTime.Minute,
                                            request.BlockedReservationTimeSlot.StartTime.Second, 
                                            DateTimeKind.Utc).Add(timeOffset)
                };
                
                createdTimeSlots.Add(newSlot);
            }
            
            // Save the created time slots
            Result<List<BlockedReservationTimeSlot>> addResult = await _blockedTimeSlotRepository.AddAsync(createdTimeSlots, cancellationToken);
            if (addResult.IsFailed)
            {
                return Result.Fail<List<BlockedReservationTimeSlot>>(ErrorMessages.GenericErrorWithRetryPrompt);
            }
        }
        else
        {
            // No recurrence: add the provided time slot directly
            Result<BlockedReservationTimeSlot> addResult = await _blockedTimeSlotRepository.AddAsync(request.BlockedReservationTimeSlot, cancellationToken);
            if (addResult.IsFailed)
            {
                return Result.Fail<List<BlockedReservationTimeSlot>>(ErrorMessages.GenericErrorWithRetryPrompt);
            }
            createdTimeSlots.Add(addResult.Value);
        }

        // Return the created time slots
        return Result.Ok(createdTimeSlots);
    }
}
