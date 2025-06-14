﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DomainModules.Errors {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ErrorMessages_da {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ErrorMessages_da() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DomainModules.Errors.ErrorMessages.da", typeof(ErrorMessages_da).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Forsidebilledet skal være et af albummets billeder.
        /// </summary>
        public static string Album_CoverImage_MustBeInImages {
            get {
                return ResourceManager.GetString("Album_CoverImage_MustBeInImages", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Beskrivelsen må højst være 500 tegn lang.
        /// </summary>
        public static string Album_Description_InvalidLength {
            get {
                return ResourceManager.GetString("Album_Description_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Der skal være mindst ét billede i albummet.
        /// </summary>
        public static string Album_Images_InvalidSize {
            get {
                return ResourceManager.GetString("Album_Images_InvalidSize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Albumnavn skal være mellem 3 og 100 tegn.
        /// </summary>
        public static string Album_Name_InvalidLength {
            get {
                return ResourceManager.GetString("Album_Name_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Albumnavn må ikke være tom.
        /// </summary>
        public static string Album_Name_NotEmpty {
            get {
                return ResourceManager.GetString("Album_Name_NotEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Den vedhæftede fil har ikke noget indhold. Filen er derfor ikke gyldig.
        /// </summary>
        public static string Attachment_BinaryContent_IsEmpty {
            get {
                return ResourceManager.GetString("Attachment_BinaryContent_IsEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to En vedhæftet fil må ikke være større end 7.5MB.
        /// </summary>
        public static string Attachment_BinaryContent_TooLarge {
            get {
                return ResourceManager.GetString("Attachment_BinaryContent_TooLarge", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Den vedhæftede fils navn skal være mellem 5-256 tegn langt.
        /// </summary>
        public static string Attachment_Name_InvalidLength {
            get {
                return ResourceManager.GetString("Attachment_Name_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Blob GUID må ikke være tom.
        /// </summary>
        public static string BlobStorageFile_BlobGuid_NotEmpty {
            get {
                return ResourceManager.GetString("BlobStorageFile_BlobGuid_NotEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Indholdstypen må ikke være tom.
        /// </summary>
        public static string BlobStorageFile_ContentType_NotEmpty {
            get {
                return ResourceManager.GetString("BlobStorageFile_ContentType_NotEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Filens navn må ikke være tomt.
        /// </summary>
        public static string BlobStorageFile_FileBaseName_NotEmpty {
            get {
                return ResourceManager.GetString("BlobStorageFile_FileBaseName_NotEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Filtypen indeholder ugyldige tegn. Kun alfanumeriske tegn er tilladt.
        /// </summary>
        public static string BlobStorageFile_FileExtension_Invalid {
            get {
                return ResourceManager.GetString("BlobStorageFile_FileExtension_Invalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Filtypen må ikke være tom.
        /// </summary>
        public static string BlobStorageFile_FileExtension_NotEmpty {
            get {
                return ResourceManager.GetString("BlobStorageFile_FileExtension_NotEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Filstørrelsen skal være større end nul.
        /// </summary>
        public static string BlobStorageFile_FileSize_GreaterThanZero {
            get {
                return ResourceManager.GetString("BlobStorageFile_FileSize_GreaterThanZero", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sluttidspunktet skal være mindst en time efter starttidspunktet.
        /// </summary>
        public static string BlockedReservationTimeSlot_EndTime_MustBeAfterStart {
            get {
                return ResourceManager.GetString("BlockedReservationTimeSlot_EndTime_MustBeAfterStart", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Starttidspunktet kan ikke være i fortiden.
        /// </summary>
        public static string BlockedReservationTimeSlot_StartTime_InPast {
            get {
                return ResourceManager.GetString("BlockedReservationTimeSlot_StartTime_InPast", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Beskrivelsen af en kontaktliste skal være mellem 5-2000 tegn lang.
        /// </summary>
        public static string ContactList_Description_InvalidLength {
            get {
                return ResourceManager.GetString("ContactList_Description_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Navnet på en kontaktliste skal være mellem 3-200 tegn langt.
        /// </summary>
        public static string ContactList_Name_InvalidLength {
            get {
                return ResourceManager.GetString("ContactList_Name_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Emailen kan ikke have mere end 10 vedhæftede filer.
        /// </summary>
        public static string Email_Attachments_InvalidSize {
            get {
                return ResourceManager.GetString("Email_Attachments_InvalidSize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Emailens vedhæftede filer skal være unikke.
        /// </summary>
        public static string Email_Attachments_MustBeUnique {
            get {
                return ResourceManager.GetString("Email_Attachments_MustBeUnique", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Indholdet skal være mindst 20 tegn langt og det må ikke fylde mere end 7MB.
        /// </summary>
        public static string Email_Body_InvalidLength {
            get {
                return ResourceManager.GetString("Email_Body_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Emailen må ikke overstige en størrelse på 7.5MB.
        /// </summary>
        public static string Email_ContentSize_TooLarge {
            get {
                return ResourceManager.GetString("Email_ContentSize_TooLarge", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Emailen skal have mellem 1-50 modtagere.
        /// </summary>
        public static string Email_Recipient_InvalidSize {
            get {
                return ResourceManager.GetString("Email_Recipient_InvalidSize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Emailens modtagere skal være unikke.
        /// </summary>
        public static string Email_Recipients_MustBeUnique {
            get {
                return ResourceManager.GetString("Email_Recipients_MustBeUnique", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Afsendingsadressen er ikke godkendt til af sende emails .
        /// </summary>
        public static string Email_SenderAddress_MustBeApproved {
            get {
                return ResourceManager.GetString("Email_SenderAddress_MustBeApproved", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Afsendingsadressen må ikke være tom.
        /// </summary>
        public static string Email_SenderAddress_MustNotBeEmpty {
            get {
                return ResourceManager.GetString("Email_SenderAddress_MustNotBeEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Emnefeltet skal være mellem 3-80 tegn langt.
        /// </summary>
        public static string Email_Subject_InvalidLength {
            get {
                return ResourceManager.GetString("Email_Subject_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Begivenhedsbeskrivelsen skal være mellem 10 og 5000 tegn.
        /// </summary>
        public static string Event_Description_InvalidLength {
            get {
                return ResourceManager.GetString("Event_Description_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Begivenhedsbeskrivelsen må ikke være tom.
        /// </summary>
        public static string Event_Description_NotEmpty {
            get {
                return ResourceManager.GetString("Event_Description_NotEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Begivenhedens varighed skal være mindst 15 minutter.
        /// </summary>
        public static string Event_Duration_Minimum15Minutes {
            get {
                return ResourceManager.GetString("Event_Duration_Minimum15Minutes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Begivenhedens sluttid skal være efter starttid.
        /// </summary>
        public static string Event_EndTime_MustBeAfterStart {
            get {
                return ResourceManager.GetString("Event_EndTime_MustBeAfterStart", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to En eventkoordinator er påkrævet.
        /// </summary>
        public static string Event_EventCoordinator_NotNull {
            get {
                return ResourceManager.GetString("Event_EventCoordinator_NotNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Begivenhedens lokation skal være mellem 3 og 50 tegn.
        /// </summary>
        public static string Event_Location_InvalidLength {
            get {
                return ResourceManager.GetString("Event_Location_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Begivenhedens lokation må ikke være tom.
        /// </summary>
        public static string Event_Location_NotEmpty {
            get {
                return ResourceManager.GetString("Event_Location_NotEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Hver vedhæftet fil skal have en gyldig URL.
        /// </summary>
        public static string Event_RemoteFile_InvalidUrl {
            get {
                return ResourceManager.GetString("Event_RemoteFile_InvalidUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Vedhæftede filer skal være unikke.
        /// </summary>
        public static string Event_RemoteFiles_MustBeUnique {
            get {
                return ResourceManager.GetString("Event_RemoteFiles_MustBeUnique", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Vedhæftede filer må ikke være null.
        /// </summary>
        public static string Event_RemoteFiles_NotNull {
            get {
                return ResourceManager.GetString("Event_RemoteFiles_NotNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Begivenhedens starttid skal være i fremtiden.
        /// </summary>
        public static string Event_StartTime_MustBeInFuture {
            get {
                return ResourceManager.GetString("Event_StartTime_MustBeInFuture", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Begivenhedens navn skal være mellem 3 og 100 tegn.
        /// </summary>
        public static string Event_Title_InvalidLength {
            get {
                return ResourceManager.GetString("Event_Title_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Begivenhedens navn må ikke være tom.
        /// </summary>
        public static string Event_Title_NotEmpty {
            get {
                return ResourceManager.GetString("Event_Title_NotEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Arrangørens emailadresse er ugyldig.
        /// </summary>
        public static string EventCoordinator_EmailAddress_Invalid {
            get {
                return ResourceManager.GetString("EventCoordinator_EmailAddress_Invalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Arrangøren skal have enten en e-mailadresse eller et telefonnummer.
        /// </summary>
        public static string EventCoordinator_EnsureEmailOrPhoneNumber {
            get {
                return ResourceManager.GetString("EventCoordinator_EnsureEmailOrPhoneNumber", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Arrangørens navn skal være mellem 2 og 100 tegn.
        /// </summary>
        public static string EventCoordinator_Name_InvalidLength {
            get {
                return ResourceManager.GetString("EventCoordinator_Name_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Arrangørens navn må ikke være tomt.
        /// </summary>
        public static string EventCoordinator_Name_NotEmpty {
            get {
                return ResourceManager.GetString("EventCoordinator_Name_NotEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Arrangørens telefonnummer er ugyldigt.
        /// </summary>
        public static string EventCoordinator_PhoneNumber_Invalid {
            get {
                return ResourceManager.GetString("EventCoordinator_PhoneNumber_Invalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Filnavn skal være mellem 1 og 255 tegn.
        /// </summary>
        public static string EventRemoteFile_FileName_InvalidLength {
            get {
                return ResourceManager.GetString("EventRemoteFile_FileName_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Filnavn må ikke være tomt.
        /// </summary>
        public static string EventRemoteFile_FileName_NotEmpty {
            get {
                return ResourceManager.GetString("EventRemoteFile_FileName_NotEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Filstørrelse skal være større end 0 byte.
        /// </summary>
        public static string EventRemoteFile_FileSize_Invalid {
            get {
                return ResourceManager.GetString("EventRemoteFile_FileSize_Invalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Filtype skal være mellem 1 og 100 tegn.
        /// </summary>
        public static string EventRemoteFile_FileType_InvalidLength {
            get {
                return ResourceManager.GetString("EventRemoteFile_FileType_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Filtype må ikke være tom.
        /// </summary>
        public static string EventRemoteFile_FileType_NotEmpty {
            get {
                return ResourceManager.GetString("EventRemoteFile_FileType_NotEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Fil URL skal være en gyldig URL.
        /// </summary>
        public static string EventRemoteFile_Url_Invalid {
            get {
                return ResourceManager.GetString("EventRemoteFile_Url_Invalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Fil URL må ikke være tom.
        /// </summary>
        public static string EventRemoteFile_Url_NotEmpty {
            get {
                return ResourceManager.GetString("EventRemoteFile_Url_NotEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Der opstod en fejl.
        /// </summary>
        public static string GenericError {
            get {
                return ResourceManager.GetString("GenericError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Billedets dato kan ikke ligge i fremtiden.
        /// </summary>
        public static string Image_DateTaken_InFuture {
            get {
                return ResourceManager.GetString("Image_DateTaken_InFuture", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Beskrivelsen må højst være 500 tegn.
        /// </summary>
        public static string Image_Description_InvalidLength {
            get {
                return ResourceManager.GetString("Image_Description_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Billedets lokation må højst være 256 tegn.
        /// </summary>
        public static string Image_Location_InvalidLength {
            get {
                return ResourceManager.GetString("Image_Location_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Upload venligst en billedfil.
        /// </summary>
        public static string Image_OriginalUpload_NotEmpty {
            get {
                return ResourceManager.GetString("Image_OriginalUpload_NotEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Fotografens navn må højst være 100 tegn.
        /// </summary>
        public static string Image_PhotographerName_InvalidLength {
            get {
                return ResourceManager.GetString("Image_PhotographerName_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Titlen må højst være 100 tegn.
        /// </summary>
        public static string Image_Title_InvalidLength {
            get {
                return ResourceManager.GetString("Image_Title_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Beskrivelsen på nyhedsbrevsgruppen skal være mellem 5-500 tegn lang.
        /// </summary>
        public static string NewsletterGroup_Description_InvalidLength {
            get {
                return ResourceManager.GetString("NewsletterGroup_Description_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Navnet på nyhedsbrevsgruppen skal være mellem 3-150 tegn langt.
        /// </summary>
        public static string NewsletterGroup_Name_InvalidLength {
            get {
                return ResourceManager.GetString("NewsletterGroup_Name_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Oprydningskampagnen skal være mindst 2 måneder lang.
        /// </summary>
        public static string NewsletterGroupsCleanupCampaign_CampaignDurationMonths_InvalidDuration {
            get {
                return ResourceManager.GetString("NewsletterGroupsCleanupCampaign_CampaignDurationMonths_InvalidDuration", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Oprydningskampagnen kan ikke have et starttidspunkt der ligger i fortiden.
        /// </summary>
        public static string NewsletterGroupsCleanupCampaign_CampaignStart_MustNotBeInThePast {
            get {
                return ResourceManager.GetString("NewsletterGroupsCleanupCampaign_CampaignStart_MustNotBeInThePast", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Handlingen kunne ikke oprettes fordi den ikke kan sættes til at udløbe om mere end 7 dage.
        /// </summary>
        public static string NewsletterSubscriptionAction_Confirmation_AtMost7DaysInFuture {
            get {
                return ResourceManager.GetString("NewsletterSubscriptionAction_Confirmation_AtMost7DaysInFuture", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Handlingen kunne ikke oprettes fordi en udløbsdato ikke er defineret.
        /// </summary>
        public static string NewsletterSubscriptionAction_Confirmation_NotNull {
            get {
                return ResourceManager.GetString("NewsletterSubscriptionAction_Confirmation_NotNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Handlingen kunne ikke oprettes fordi den er udløbet.
        /// </summary>
        public static string NewsletterSubscriptionAction_ConfirmationTime_NotAfterExpiry {
            get {
                return ResourceManager.GetString("NewsletterSubscriptionAction_ConfirmationTime_NotAfterExpiry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Handlingen kunne ikke oprettes fordi værdien er udefineret.
        /// </summary>
        public static string NewsletterSubscriptionAction_IsConfirmed_NotNull {
            get {
                return ResourceManager.GetString("NewsletterSubscriptionAction_IsConfirmed_NotNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Handlingen kunne ikke oprettes fordi nøglen for handlingen er tom.
        /// </summary>
        public static string NewsletterSubscriptionAction_Token_NotEmpty {
            get {
                return ResourceManager.GetString("NewsletterSubscriptionAction_Token_NotEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Der er mindst én modtager-emailadresse som ikke er gyldig.
        /// </summary>
        public static string Recipient_EmailAddress_MustBeValid {
            get {
                return ResourceManager.GetString("Recipient_EmailAddress_MustBeValid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Navnet på foreningen skal være mellem 3-75 tegn langt.
        /// </summary>
        public static string Reservation_CommunityName_InvalidLength {
            get {
                return ResourceManager.GetString("Reservation_CommunityName_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to E-mailen er ikke gyldig.
        /// </summary>
        public static string Reservation_Email_InvalidStructure {
            get {
                return ResourceManager.GetString("Reservation_Email_InvalidStructure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sluttidspunktet skal være mindst en time efter starttidspunktet.
        /// </summary>
        public static string Reservation_EndTime_MustBeAfterStart {
            get {
                return ResourceManager.GetString("Reservation_EndTime_MustBeAfterStart", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Navn må ikke være tomt.
        /// </summary>
        public static string Reservation_Name_Empty {
            get {
                return ResourceManager.GetString("Reservation_Name_Empty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Navn skal være mellem 3-100 tegn langt.
        /// </summary>
        public static string Reservation_Name_InvalidLength {
            get {
                return ResourceManager.GetString("Reservation_Name_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Telefonnummer må ikke være tomt.
        /// </summary>
        public static string Reservation_PhoneNumber_Empty {
            get {
                return ResourceManager.GetString("Reservation_PhoneNumber_Empty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Telefonnummer skal være mellem 5-20 tegn langt.
        /// </summary>
        public static string Reservation_PhoneNumber_InvalidLength {
            get {
                return ResourceManager.GetString("Reservation_PhoneNumber_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Starttidspunktet kan ikke være i fortiden.
        /// </summary>
        public static string Reservation_StartTIme_InPast {
            get {
                return ResourceManager.GetString("Reservation_StartTIme_InPast", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to E-mailen er ikke gyldig.
        /// </summary>
        public static string ReservationHistory_Email_InvalidStructure {
            get {
                return ResourceManager.GetString("ReservationHistory_Email_InvalidStructure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Nøglen til reservationshistorikken kan ikke være tom.
        /// </summary>
        public static string ReservationHistory_Token_Empty {
            get {
                return ResourceManager.GetString("ReservationHistory_Token_Empty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Navnet på reservationslokationen skal være mellem 3-75 tegn langt.
        /// </summary>
        public static string ReservationLocation_Name_InvalidLength {
            get {
                return ResourceManager.GetString("ReservationLocation_Name_InvalidLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Gentagelsesintervallet skal være større end nul.
        /// </summary>
        public static string ReservationSeries_InvalidInterval {
            get {
                return ResourceManager.GetString("ReservationSeries_InvalidInterval", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Slutdatoen for gentagelsen skal være efter startdatoen.
        /// </summary>
        public static string ReservationSeries_InvalidRecurrenceDates {
            get {
                return ResourceManager.GetString("ReservationSeries_InvalidRecurrenceDates", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Gentagelsesintervallet skal vælges.
        /// </summary>
        public static string ReservationSeries_InvalidRecurrenceType {
            get {
                return ResourceManager.GetString("ReservationSeries_InvalidRecurrenceType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Startdatoen for gentagelsen må ikke være i fortiden.
        /// </summary>
        public static string ReservationSeries_StartDateInPast {
            get {
                return ResourceManager.GetString("ReservationSeries_StartDateInPast", resourceCulture);
            }
        }
    }
}
