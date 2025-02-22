using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.GroupManagement;
using DomainModules.Newsletters.Entities;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using ErrorMessages = ServiceModules.Errors.ErrorMessages;

namespace ServiceModules.Newsletters.GroupManagement.Commands;

[UsedImplicitly]
public class UpdateNewsletterGroupCommandValidator : AbstractValidator<UpdateNewsletterGroupCommand>
{
    public UpdateNewsletterGroupCommandValidator(IValidator<NewsletterGroup> newsletterGroupValidator)
    {
        RuleFor(x => x.NewsletterGroup).SetValidator(newsletterGroupValidator);
    }
}

public class UpdateNewsletterGroupCommandHandler : IRequestHandler<UpdateNewsletterGroupCommand, Result<NewsletterGroup>>
{
    private readonly IRepository<NewsletterGroup> _newsletterGroupRepository;

    public UpdateNewsletterGroupCommandHandler(IRepository<NewsletterGroup> newsletterGroupRepository)
    {
        _newsletterGroupRepository = newsletterGroupRepository;
    }
    
    public async Task<Result<NewsletterGroup>> Handle(UpdateNewsletterGroupCommand request, CancellationToken cancellationToken)
    {
        Result<NewsletterGroup> storedEntityQuery = await _newsletterGroupRepository.GetByIdAsync(
            request.NewsletterGroup.Id,
            readOnly: false,
            cancellationToken
        );

        if (storedEntityQuery.IsFailed) return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        NewsletterGroup? storedEntity = storedEntityQuery.Value;

        storedEntity.Name = request.NewsletterGroup.Name;
        storedEntity.Description = request.NewsletterGroup.Description;
        
        return await _newsletterGroupRepository.UpdateAsync(storedEntity, cancellationToken);
    }
}
