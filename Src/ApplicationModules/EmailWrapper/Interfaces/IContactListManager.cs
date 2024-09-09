using EmailWrapper.DTOs;
using FluentResults;

namespace EmailWrapper.Interfaces;

public interface IContactListManager
{
    Result RemoveRecipientFromAllContactLists(RecipientDto recipient);
    Result RemoveRecipientFromContactList(RecipientDto recipient);
    
    Result<ContactListDto> AddRecipientToContactList(RecipientDto recipient, ContactListDto contactList);
    Result<IEnumerable<ContactListDto>> AddRecipientToContactLists(RecipientDto recipient, IList<ContactListDto> contactLists);
    
    Result<ContactListDto> GetContactList(int id);
    Result<IEnumerable<ContactListDto>> GetContactLists(IEnumerable<int> ids);

    Result DeleteContactList(int id);
    Result DeleteContactLists(IEnumerable<int> ids);
}
