using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Emails.ContactLists.Commands;

public record UnsubscribeFromContactListCommand(string EmailAddress, Guid UnsubscribeToken) : IRequest<Result>;

public class UnsubscribeFromContactListCommandHandler : IRequestHandler<UnsubscribeFromContactListCommand, Result>
{
    private readonly ILogger _logger;
    private readonly IMediator _mediator;
    private readonly IRepository<ContactList> _contactListRepository;

    public UnsubscribeFromContactListCommandHandler(
        ILogger logger, 
        IMediator mediator,
        IRepository<ContactList> contactListRepository)
    {
        _logger = logger;
        _mediator = mediator;
        _contactListRepository = contactListRepository;
    }
    
    public async Task<Result> Handle(UnsubscribeFromContactListCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Result<List<ContactList>> contactListQueryResult = await _contactListRepository.FindAsync(x => 
                x.UnsubscribeToken == request.UnsubscribeToken,
                readOnly: false,
                cancellationToken: cancellationToken
            );

            if (contactListQueryResult.IsFailed || contactListQueryResult.Value.Count == 0)
            {
                return Result.Fail(ErrorMessages.ContactList_UnsubscribeTokenEmptyQuery);
            }

            // There should only be one contact list with the provided token, so if more exist, log the warning!
            if (contactListQueryResult.Value.Count > 1)
            {
                _logger.LogWarning(
                    "Multiple contact lists with the same unsubscribe token were found in {command}. Total count: {count}",
                    nameof(UnsubscribeFromContactListCommand),
                    contactListQueryResult.Value.Count
                );
            }

            ContactList contactList = contactListQueryResult.Value.First();
            
            // There should only be 0-1 matches, but if more somehow exist they should also be removed
            List<Recipient> recipients = contactList.Contacts.Where(x => x.EmailAddress == request.EmailAddress).ToList();
            foreach (Recipient recipient in recipients)
            {
                contactList.Contacts.Remove(recipient);
            }

            Result<ContactList> updateResult = await _contactListRepository.UpdateAsync(contactList, cancellationToken);
            if (updateResult.IsSuccess)
            {
                bool recipientStillInContactList = updateResult.Value.Contacts.Any(x => x.EmailAddress == request.EmailAddress);
                return recipientStillInContactList
                    ? Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt)
                    : Result.Ok();
            }

            return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }
        catch (Exception ex)
        {
            _logger.LogError("{errorMessage}", ex.Message);
            return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }
    }
}
