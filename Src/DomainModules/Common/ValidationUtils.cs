using System.Net.Mail;

namespace DomainModules.Common;

public static class ValidationUtils
{
    public static bool IsEmailStructureValid(string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress))
            return false;
        try
        {
            var addr = new MailAddress(emailAddress);
            if (addr.Address != emailAddress) return false;
            
            int index = emailAddress.LastIndexOf('@');
            string localPart = emailAddress.Substring(0, index);
            string domainPart = emailAddress.Substring(index + 1);
            return !localPart.Contains("..") && !domainPart.Contains("..");
        }
        catch (FormatException)
        {
            return false;
        }
    }
        
    public static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return false;
        
        // Normalize by removing spaces
        string normalized = phoneNumber.Replace(" ", "");
        
        // If 8 digits, assume Danish number
        if (normalized.Length == 8 && normalized.All(char.IsDigit)) return true;
        
        // Otherwise, require a '+' followed by at least 4 digits
        if (normalized.StartsWith('+') && normalized.Length >= 4)
        {
            return normalized.Substring(1).All(char.IsDigit);
        }
        return false;
    }
}
