using EmailWrapper.Models;

namespace EmailWrapper.Interfaces;

public interface IRecipientService
{
    Recipient GetRecipientByEmail(string email);
    Recipient GetRecipientById(int id);
    IEnumerable<Recipient> GetRecipientsById(IEnumerable<int> id);

    Recipient SaveRecipient(Recipient recipient);
    Recipient SaveRecipients(IEnumerable<Recipient> recipients);

    Recipient DeleteRecipient(Recipient recipient);
    Recipient DeleteRecipients(IEnumerable<Recipient> recipients);
}
