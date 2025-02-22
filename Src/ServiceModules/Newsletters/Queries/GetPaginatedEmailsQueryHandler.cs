using Contracts.DataAccess.Base;
using Contracts.ServiceModules;
using Contracts.ServiceModules.Newsletters;
using DomainModules.Emails.Entities;
using FluentResults;
using MediatR;
using ServiceModules.Errors;

namespace ServiceModules.Newsletters.Queries;

public class GetPaginatedEmailsQueryHandler : IRequestHandler<GetPaginatedEmailsQuery, Result<PaginatedResult<Email>>>
{
    private readonly IRepository<Email> _emailRepository;
    public GetPaginatedEmailsQueryHandler(IRepository<Email> emailRepository)
    {
        _emailRepository = emailRepository;
    }

    public async Task<Result<PaginatedResult<Email>>> Handle(GetPaginatedEmailsQuery request, CancellationToken cancellationToken)
    {
        // Retrieve all emails.
        Result<List<Email>> allEmailsQuery = await _emailRepository.GetAllAsync(cancellationToken: cancellationToken);
        if (allEmailsQuery.IsFailed || allEmailsQuery.Value is null || allEmailsQuery.Value.Count == 0) 
            return Result.Fail(ErrorMessages.GenericNotFound);
        
        List<Email> allEmails = allEmailsQuery.Value;
        
        // Order emails by DateCreated descending
        List<Email> orderedEmails = allEmails.OrderByDescending(e => e.DateCreated ?? DateTime.MinValue).ToList();

        int totalCount = orderedEmails.Count;
        List<Email> pagedEmails = orderedEmails
                                  .Skip((request.Page - 1) * request.PageSize)
                                  .Take(request.PageSize)
                                  .ToList();

        var result = new PaginatedResult<Email>
        {
            Items = pagedEmails,
            TotalCount = totalCount
        };

        return Result.Ok(result);
    }
}
