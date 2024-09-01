using OmmelSamvirke.ErrorHandling.Enums;
using OmmelSamvirke.ErrorHandling.Interfaces;

namespace EmailWrapper.Errors;

public static class ContactListErrors
{
    public enum Enums
    {
        InvalidNameLength,
        InvalidDescriptionLength
    }
    
    public static void RegisterErrorMessages(IErrorTranslationService? errorTranslationService)
    {
        errorTranslationService
            // Name
            .ForError(Enums.InvalidNameLength)
            .WithTranslation(SupportedErrorLanguage.English, "The name of the contact list must be between 3-200 characters long")
            .WithTranslation(SupportedErrorLanguage.Danish, "Navnet på en kontaktliste skal være mellem 3-200 tegn langt")
            
            // Description
            .ForError(Enums.InvalidDescriptionLength)
            .WithTranslation(SupportedErrorLanguage.English, "The description of a contact list must be between 5-2000 characters long")
            .WithTranslation(SupportedErrorLanguage.Danish, "Beskrivelsen af en kontaktliste skal være mellem 5-2000 tegn lang");
    }
}
