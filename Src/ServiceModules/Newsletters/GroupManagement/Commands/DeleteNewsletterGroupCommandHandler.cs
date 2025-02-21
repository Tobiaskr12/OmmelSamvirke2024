using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.GroupManagement;
using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Newsletters.GroupManagement.Commands;

public class DeleteNewsletterGroupCommandHandler : IRequestHandler<DeleteNewsletterGroupCommand, Result>
{
    private readonly IRepository<NewsletterGroup> _newsletterGroupRepository;

    public DeleteNewsletterGroupCommandHandler(IRepository<NewsletterGroup> newsletterGroupRepository)
    {
        _newsletterGroupRepository = newsletterGroupRepository;
    }

    public async Task<Result> Handle(DeleteNewsletterGroupCommand request, CancellationToken cancellationToken)
    {
        Result<NewsletterGroup> storedEntityResult = await _newsletterGroupRepository.GetByIdAsync(
            request.NewsletterGroupId, 
            readOnly: false,
            cancellationToken
        );
        
        if (storedEntityResult.IsFailed) return Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt);
        
        NewsletterGroup? storedEntity = storedEntityResult.Value;
        Result deletionResult = await _newsletterGroupRepository.DeleteAsync(storedEntity, cancellationToken);
        
        return deletionResult.IsFailed 
            ? Result.Fail(ErrorMessages.GenericErrorWithRetryPrompt) 
            : Result.Ok();
    }
}
