using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters;
using DomainModules.Emails.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Newsletters.Queries;

public class GetNewsletterByIdQueryHandler : IRequestHandler<GetNewsletterByIdQuery, Result<Email>>
{
    private readonly IRepository<Email> _emailRepository;
    public GetNewsletterByIdQueryHandler(IRepository<Email> emailRepository)
    {
        _emailRepository = emailRepository;
    }

    public async Task<Result<Email>> Handle(GetNewsletterByIdQuery request, CancellationToken cancellationToken)
    {
        Result<Email> emailResult = await _emailRepository.GetByIdAsync(request.Id, cancellationToken: cancellationToken);
        if (emailResult.IsFailed || emailResult.Value == null)
        {
            return Result.Fail<Email>(ErrorMessages.GenericNotFound);
        }
        
        return !emailResult.Value.IsNewsletter 
            ? Result.Fail<Email>(ErrorMessages.EmailIsNotANewsletter) 
            : Result.Ok(emailResult.Value);
    }
}
