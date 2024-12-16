using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.DataAccess.Base;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Errors;

namespace OmmelSamvirke.ServiceModules.Emails.ContactLists.Commands;

public record CreateContactListCommand(ContactList ContactList) : IRequest<Result<ContactList>>;

[UsedImplicitly]
public class CreateContactListCommandValidator : AbstractValidator<CreateContactListCommand>
{
    public CreateContactListCommandValidator(IValidator<ContactList> contactListValidator)
    {
        RuleFor(x => x.ContactList).SetValidator(contactListValidator);
    }
}

public class CreateContactListCommandHandler : IRequestHandler<CreateContactListCommand, Result<ContactList>>
{
    private readonly IRepository<ContactList> _contactListRepository;
    private readonly ILogger _logger;

    public CreateContactListCommandHandler(IRepository<ContactList> contactListRepository, ILogger logger)
    {
        _contactListRepository = contactListRepository;
        _logger = logger;
    }

    public async Task<Result<ContactList>> Handle(CreateContactListCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await _contactListRepository.AddAsync(request.ContactList, cancellationToken);
        }
        catch (Exception ex)
        {
            var errorCode = Guid.NewGuid();
            _logger.LogError("[{code}] - {message}", errorCode, ex.Message);
            return Result.Fail(ErrorMessages.GenericErrorWithErrorCode + errorCode);
        }
    }
}
