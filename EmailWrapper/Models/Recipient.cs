using System.Globalization;
using System.Text.RegularExpressions;

namespace EmailWrapper.Models;

public class Recipient
{
    public required string Email { get; set; }

    public static List<Recipient> Create(List<string> emails)
    {
        List<Recipient> recipients = [];

        foreach (string email in emails)
        {
            recipients.Add(new Recipient
            {
                Email = email
            });
        }

        return recipients;
    }

    public bool IsEmailStructureValid()
    {
        // Validation is performed based on the following article:
        // https://learn.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
        if (string.IsNullOrWhiteSpace(Email)) return false;

        try
        {
            // Normalize the domain
            Email = Regex.Replace(
                input: Email,
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
                input: Email,
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
