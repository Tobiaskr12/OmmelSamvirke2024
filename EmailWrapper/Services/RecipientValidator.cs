using EmailWrapper.Models;

namespace EmailWrapper.Services;

public static class RecipientValidator
{
    public static bool Validate(Recipient recipient)
    {
        return recipient.IsEmailStructureValid();
    }
    
    public static List<Recipient> GetInvalidRecipients(List<Recipient> recipients)
    {
        List<Recipient> invalidRecipients = [];
        
        foreach (Recipient recipient in recipients)
        {
            if (!recipient.IsEmailStructureValid())
                invalidRecipients.Add(recipient);
        }

        return invalidRecipients;
    }

    public static List<Recipient> GetValidRecipients(List<Recipient> recipients)
    {
        List<Recipient> validRecipients = [];
        
        foreach (Recipient recipient in recipients)
        {
            if (recipient.IsEmailStructureValid())
                validRecipients.Add(recipient);
        }

        return validRecipients;
    }
}
