using EmailWrapper.Errors;
using EmailWrapper.Services;
using FluentResults;
using OmmelSamvirke.ErrorHandling.Helpers;
using OmmelSamvirke.ErrorHandling.Interfaces;
using OmmelSamvirke2024.Domain;

namespace EmailWrapper.Models;

public class ContactList : BaseEntity
{
    private readonly List<Recipient> _contacts = [];
    
    public string Name { get; }
    public string Description { get; }
    public IReadOnlyList<Recipient> Contacts => _contacts;

    internal ContactList(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public Result AddContact(Recipient recipient)
    {
        if (!RecipientValidator.Validate(recipient)) 
            return Result.Fail($"The recipient with the email: '{recipient.Email}' is not valid");
        
        _contacts.Add(recipient);
        return Result.Ok();
    }

    public Result AddContacts(List<Recipient> recipients)
    {
        var operationResult = new Result();

        foreach (Recipient recipient in recipients)
        {
            Result addResult = AddContact(recipient);

            if (addResult.IsSuccess)
            {
                operationResult.WithSuccess($"Successfully added recipient: {recipient.Email}");
            }
            else
            {
                operationResult.WithErrors(addResult.Errors);
            }
        }

        return operationResult;
    }
}

public class ContactListFactory
{
    private readonly IValidator _validator;

    public ContactListFactory(IValidator validator)
    {
        _validator = validator;
    }

    public Result<ContactList> Create(string name, string description, List<Recipient>? recipients = null)
    {
        var contactList = new ContactList(name, description);

        if (recipients is not null)
        {
            // TODO - Check and handle partial success
            contactList.AddContacts(recipients);
        }

        _validator
            .ValidateLength(contactList.Name, 3, 200, ContactListErrors.Enums.InvalidNameLength)
            .ValidateLength(contactList.Description, 5, 2000, ContactListErrors.Enums.InvalidDescriptionLength);

        return ValidationHelper.ValidateAndReturnResult(contactList, _validator);
    }
}
