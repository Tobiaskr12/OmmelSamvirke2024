using Contracts.DataAccess.Base;
using Contracts.ServiceModules.Newsletters.GroupManagement;
using DomainModules.Newsletters.Entities;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;

namespace ServiceModules.Newsletters.GroupManagement.Commands;

[UsedImplicitly]
public class CreateNewsletterGroupCommandValidator : AbstractValidator<CreateNewsletterGroupCommand>
{
    public CreateNewsletterGroupCommandValidator(IValidator<NewsletterGroup> newsletterGroupValidator)
    {
        RuleFor(x => x.NewsletterGroup).SetValidator(newsletterGroupValidator);
    }
}

public class CreateNewsletterGroupCommandHandler : IRequestHandler<CreateNewsletterGroupCommand, Result<NewsletterGroup>>
{
    private readonly IRepository<NewsletterGroup> _newsletterGroupRepository;

    public CreateNewsletterGroupCommandHandler(IRepository<NewsletterGroup> newsletterGroupRepository)
    {
        _newsletterGroupRepository = newsletterGroupRepository;
    }
    
    public async Task<Result<NewsletterGroup>> Handle(CreateNewsletterGroupCommand request, CancellationToken cancellationToken)
    {
        return await _newsletterGroupRepository.AddAsync(request.NewsletterGroup, cancellationToken);
    }
}
