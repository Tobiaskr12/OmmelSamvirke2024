using DomainModules.Emails.Entities;
using DomainModules.Emails.Validators;
using DomainModules.Errors;
using DomainModules.Newsletters.Entities;
using DomainModules.Newsletters.Validators;
using FluentValidation.Results;

namespace DomainModules.Tests.Newsletters;

[TestFixture, Category("UnitTests")]
public class NewsletterGroupsCleanupCampaignTests
{
    private NewsletterGroupsCleanupCampaignValidator _validator;
    private NewsletterGroupsCleanupCampaign _baseValidCampaign;

    [SetUp]
    public void SetUp()
    {
        var recipientValidator = new RecipientValidator();
        _validator = new NewsletterGroupsCleanupCampaignValidator(recipientValidator);

        _baseValidCampaign = new NewsletterGroupsCleanupCampaign
        {
            UnconfirmedRecipients = [ 
                new Recipient { EmailAddress = "test1@example.com" },
                new Recipient { EmailAddress = "test2@example.com" } 
            ],
            CampaignStart = DateTime.UtcNow.AddHours(1),
            CampaignDurationMonths = 3
        };
    }

    [Test]
    public void ValidCampaign_PassesValidation()
    {
        NewsletterGroupsCleanupCampaign campaign = _baseValidCampaign;
        ValidationResult? result = _validator.Validate(campaign);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Campaign_StartInPast_FailsValidation()
    {
        NewsletterGroupsCleanupCampaign campaign = _baseValidCampaign;
        campaign.CampaignStart = DateTime.UtcNow.AddHours(-1);
        ValidationResult? result = _validator.Validate(campaign);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.NewsletterGroupsCleanupCampaign_CampaignStart_MustNotBeInThePast)
            ), Is.True);
        });
    }

    [TestCase(0)]
    [TestCase(1)]
    public void Campaign_DurationLessThanTwo_FailsValidation(int duration)
    {
        NewsletterGroupsCleanupCampaign campaign = _baseValidCampaign;
        campaign.CampaignDurationMonths = duration;
        ValidationResult? result = _validator.Validate(campaign);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.NewsletterGroupsCleanupCampaign_CampaignDurationMonths_InvalidDuration)
            ), Is.True);
        });
    }

    [Test]
    public void Campaign_InvalidUncleanedRecipient_FailsValidation()
    {
        NewsletterGroupsCleanupCampaign campaign = _baseValidCampaign;
        // Invalidate a recipient by setting an empty email address
        campaign.UnconfirmedRecipients[0].EmailAddress = "";
        ValidationResult? result = _validator.Validate(campaign);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(x =>
                x.ErrorMessage.Equals(ErrorMessages.Recipient_EmailAddress_MustBeValid)
            ), Is.True);
        });
    }
}
