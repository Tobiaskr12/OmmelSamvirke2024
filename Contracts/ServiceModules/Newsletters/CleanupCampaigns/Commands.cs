using DomainModules.Newsletters.Entities;
using FluentResults;
using MediatR;

namespace Contracts.ServiceModules.Newsletters.CleanupCampaigns;

public record StartNewsletterGroupCleanupCampaignCommand(NewsletterGroupsCleanupCampaign CleanupCampaign) : IRequest<Result<NewsletterGroupsCleanupCampaign>>;

public record ConfirmContinuedNewsletterSubscriptionCommand(Guid RecipientToken) : IRequest<Result>;

public record DeleteCleanupCampaignCommand(int CleanupCampaignId) : IRequest<Result>;