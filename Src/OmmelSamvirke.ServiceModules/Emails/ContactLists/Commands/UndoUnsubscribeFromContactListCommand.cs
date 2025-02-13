using Contracts.DataAccess.Base;
using FluentResults;
using MediatR;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Emails.ContactLists.Commands;

public record UndoUnsubscribeFromContactListCommand(string EmailAddress, Guid UnsubscribeToken, Guid UndoToken) : IRequest<Result>;

public class UndoUnsubscribeFromContactListCommandHandler : IRequestHandler<UndoUnsubscribeFromContactListCommand, Result>
{
    private readonly IRepository<ContactList> _contactListRepository;
    private readonly IRepository<ContactListUnsubscription> _contactListUnsubscriptionRepository;

    public UndoUnsubscribeFromContactListCommandHandler(
        IRepository<ContactList> contactListRepository,
        IRepository<ContactListUnsubscription> contactListUnsubscriptionRepository)
    {
        _contactListRepository = contactListRepository;
        _contactListUnsubscriptionRepository = contactListUnsubscriptionRepository;
    }
    
    public async Task<Result> Handle(UndoUnsubscribeFromContactListCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find the contact list using the UnsubscribeToken.
            Result<List<ContactList>> contactListQuery = await _contactListRepository.FindAsync(
                x => x.UnsubscribeToken == request.UnsubscribeToken,
                readOnly: false,
                cancellationToken: cancellationToken
            );
            if (contactListQuery.IsFailed || contactListQuery.Value.Count == 0)
            {
                return Result.Fail(ErrorMessages.ContactList_NotFound_UndoUnsubscribe);
            }
            ContactList contactList = contactListQuery.Value.First();

            // Find the unsubscription record that matches the UndoToken, email, and contact list.
            Result<List<ContactListUnsubscription>> unsubscriptionQuery = await _contactListUnsubscriptionRepository.FindAsync(
                x => x.UndoToken == request.UndoToken &&
                     x.EmailAddress == request.EmailAddress &&
                     x.ContactListId == contactList.Id,
                readOnly: false,
                cancellationToken: cancellationToken
            );
            if (unsubscriptionQuery.IsFailed || unsubscriptionQuery.Value.Count == 0)
            {
                return Result.Fail(ErrorMessages.ContactList_UndoTokenNotFound);
            }
            ContactListUnsubscription unsubscriptionRecord = unsubscriptionQuery.Value.First();

            // Check that the unsubscription record is not older than 14 days.
            if (unsubscriptionRecord.DateCreated.HasValue && unsubscriptionRecord.DateCreated.Value.AddDays(14) < DateTime.UtcNow)
            {
                return Result.Fail(ErrorMessages.ContactList_UndoTokenExpired);
            }

            // Re-add the email address if it isn’t already in the contact list.
            if (contactList.Contacts.All(x => x.EmailAddress != request.EmailAddress))
            {
                contactList.Contacts.Add(new Recipient { EmailAddress = request.EmailAddress });
            }

            Result<ContactList> updateResult = await _contactListRepository.UpdateAsync(contactList, cancellationToken);
            if (updateResult.IsSuccess)
            {
                // Delete the unsubscription record so it cannot be reused.
                await _contactListUnsubscriptionRepository.DeleteAsync(unsubscriptionRecord, cancellationToken);
                return Result.Ok();
            }
            return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }
        catch (Exception)
        {
            return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        }
    }
}
