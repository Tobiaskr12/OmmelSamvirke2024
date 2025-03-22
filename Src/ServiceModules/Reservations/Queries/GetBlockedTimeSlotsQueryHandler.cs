using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Reservations;
using DomainModules.Reservations.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Reservations.Queries;

public class GetBlockedTimeSlotsQueryHandler : IRequestHandler<GetBlockedTimeSlotsQuery, Result<List<BlockedReservationTimeSlot>>>
{
    private readonly IRepository<BlockedReservationTimeSlot> _blockedTimeSlotRepository;

    public GetBlockedTimeSlotsQueryHandler(IRepository<BlockedReservationTimeSlot> blockedTimeSlotRepository)
    {
        _blockedTimeSlotRepository = blockedTimeSlotRepository;
    }

    public async Task<Result<List<BlockedReservationTimeSlot>>> Handle(GetBlockedTimeSlotsQuery request, CancellationToken cancellationToken)
    {
        DateTime endTime = request.StartTime.Add(request.TimeSpan);
        Result<List<BlockedReservationTimeSlot>> result = await _blockedTimeSlotRepository.FindAsync(b => 
            b.StartTime >= request.StartTime && b.StartTime <= endTime,
            cancellationToken: cancellationToken
        );
        
        return result.IsFailed 
            ? Result.Fail<List<BlockedReservationTimeSlot>>(ErrorMessages.GenericErrorWithRetryPrompt) 
            : Result.Ok(result.Value);
    }
}
