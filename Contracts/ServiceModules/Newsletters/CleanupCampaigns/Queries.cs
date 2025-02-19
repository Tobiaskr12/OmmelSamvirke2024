using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Newsletters.CleanupCampaigns;

public record GetNewsletterGroupsCleanupCampaignByIdQuery(int Id, bool ReadOnly) : IRequest<Result<NewsletterGroupsCleanupCampaign>>;

public record GetAllNewsletterGroupsCleanupCampaignsQuery : IRequest<Result<List<NewsletterGroupsCleanupCampaign>>>;
