using System.Globalization;
using System.Text.RegularExpressions;
using OmmelSamvirke.DomainModules.Emails.Entities;

namespace OmmelSamvirke.ServiceModules.Emails.Util;

public static class RecipientValidator
{
    public static bool Validate(Recipient recipient)
    {
        return IsEmailStructureValid(recipient);
    }
    
    public static List<Recipient> GetInvalidRecipients(List<Recipient> recipients)
    {
        List<Recipient> invalidRecipients = [];
        
        foreach (Recipient recipient in recipients)
        {
            if (!IsEmailStructureValid(recipient))
                invalidRecipients.Add(recipient);
        }

        return invalidRecipients;
    }

    public static List<Recipient> GetValidRecipients(List<Recipient> recipients)
    {
        List<Recipient> validRecipients = [];
        
        foreach (Recipient recipient in recipients)
        {
            if (IsEmailStructureValid(recipient))
                validRecipients.Add(recipient);
        }

        return validRecipients;
    }
    
    private static bool IsEmailStructureValid(Recipient recipient)
    {
        // Validation is performed based on the following article:
        // https://learn.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
        if (string.IsNullOrWhiteSpace(recipient.EmailAddress)) return false;

        try
        {
            // Normalize the domain
            recipient.EmailAddress = Regex.Replace(
                input: recipient.EmailAddress,
                pattern: "(@)(.+)$", DomainMapper,
                options: RegexOptions.None, 
                matchTimeout: TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();

                // Pull out and process domain name (throws ArgumentException on invalid)
                string domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(
                input: recipient.EmailAddress,
                pattern: @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                options: RegexOptions.IgnoreCase,
                matchTimeout: TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}
