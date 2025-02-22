using AutoFixture;
using Contracts.ServiceModules.Emails.Sending;
using FluentResults;
using DomainModules.Emails.Entities;

namespace ServiceModules.Tests.Emails.Sending.Queries;

[TestFixture, Category("IntegrationTests")]
public class RecipientsValidationQueryTests : ServiceTestBase
{
    [Test]
    public async Task RecipientsValidation_AllValid_ReturnsSuccessWithValidRecipientsOnly()
    {
        List<Recipient> recipients =
        [
            GlobalTestSetup.Fixture.Create<Recipient>(),
            GlobalTestSetup.Fixture.Create<Recipient>()
        ];

        var query = new RecipientsValidationQuery(recipients);
        Result<(List<Recipient> ValidRecipients, List<Recipient> InvalidRecipients)> result = await GlobalTestSetup.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.ValidRecipients, Is.EquivalentTo(recipients));
            Assert.That(result.Value.InvalidRecipients, Is.Empty);
        });
    }
    
    [Test]
    public async Task RecipientsValidation_AllInvalid_ReturnsSuccessWithInvalidRecipientsOnly()
    {
        var recipients = new List<Recipient>
        {
            new() { EmailAddress = "invalid-email" },
            new() { EmailAddress = "another-invalid-email" }
        };
        
        var query = new RecipientsValidationQuery(recipients);
        Result<(List<Recipient> ValidRecipients, List<Recipient> InvalidRecipients)> result = await GlobalTestSetup.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.ValidRecipients, Is.Empty);
            Assert.That(result.Value.InvalidRecipients, Is.EquivalentTo(recipients));
        });
    }
    
    [Test]
    public async Task RecipientsValidation_MixedRecipients_ReturnsValidAndInvalidLists()
    {
        var validRecipient = new Recipient { EmailAddress = "valid@example.com" };
        var invalidRecipient = new Recipient { EmailAddress = "invalid-email" };
        var recipients = new List<Recipient> { validRecipient, invalidRecipient };
        
        var query = new RecipientsValidationQuery(recipients);

        Result<(List<Recipient> ValidRecipients, List<Recipient> InvalidRecipients)> result = await GlobalTestSetup.Mediator.Send(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.ValidRecipients, Is.EquivalentTo(new List<Recipient> { validRecipient }));
            Assert.That(result.Value.InvalidRecipients, Is.EquivalentTo(new List<Recipient> { invalidRecipient }));
        });
    }
}
