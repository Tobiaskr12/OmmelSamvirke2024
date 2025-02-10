using FluentResults;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Emails.Sending.Queries;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.Sending.Queries;

[TestFixture, Category("UnitTests")]
public class RecipientsValidationQueryTests
{
    private RecipientsValidationQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _handler = new RecipientsValidationQueryHandler();
    }

    /// <summary>
    /// This test assumes that a recipient is considered valid when its EmailAddress is in a valid format.
    /// </summary>
    [Test]
    public async Task RecipientsValidation_AllValid_ReturnsSuccessWithValidRecipientsOnly()
    {
        var recipients = new List<Recipient>
        {
            new() { EmailAddress = "valid1@example.com" },
            new() { EmailAddress = "valid2@example.com" }
        };
        var query = new RecipientsValidationQuery(recipients);

        Result<(List<Recipient> ValidRecipients, List<Recipient> InvalidRecipients)> result =
            await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.ValidRecipients, Is.EquivalentTo(recipients));
            Assert.That(result.Value.InvalidRecipients, Is.Empty);
        });
    }

    /// <summary>
    /// This test assumes that a recipient is considered invalid when its EmailAddress is not in a valid format.
    /// </summary>
    [Test]
    public async Task RecipientsValidation_AllInvalid_ReturnsSuccessWithInvalidRecipientsOnly()
    {
        var recipients = new List<Recipient>
        {
            new() { EmailAddress = "invalid-email" },
            new() { EmailAddress = "another-invalid-email" }
        };
        var query = new RecipientsValidationQuery(recipients);

        Result<(List<Recipient> ValidRecipients, List<Recipient> InvalidRecipients)> result =
            await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.ValidRecipients, Is.Empty);
            Assert.That(result.Value.InvalidRecipients, Is.EquivalentTo(recipients));
        });
    }

    /// <summary>
    /// Tests a mix of valid and invalid recipients.
    /// </summary>
    [Test]
    public async Task RecipientsValidation_MixedRecipients_ReturnsValidAndInvalidLists()
    {
        var validRecipient = new Recipient { EmailAddress = "valid@example.com" };
        var invalidRecipient = new Recipient { EmailAddress = "invalid-email" };

        var recipients = new List<Recipient> { validRecipient, invalidRecipient };
        var query = new RecipientsValidationQuery(recipients);

        Result<(List<Recipient> ValidRecipients, List<Recipient> InvalidRecipients)> result =
            await _handler.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(result.Value.ValidRecipients, Is.EquivalentTo(new List<Recipient> { validRecipient }));
            Assert.That(result.Value.InvalidRecipients, Is.EquivalentTo(new List<Recipient> { invalidRecipient }));
        });
    }

    /// <summary>
    /// This test forces an exception to occur (by passing null) and verifies that the handler returns a failure result.
    /// </summary>
    [Test]
    public async Task RecipientsValidation_ExceptionThrown_ReturnsFailure()
    {
        var query = new RecipientsValidationQuery(null!);

        Result<(List<Recipient> ValidRecipients, List<Recipient> InvalidRecipients)> result =
            await _handler.Handle(query, CancellationToken.None);


        Assert.That(result.IsFailed);
    }
}
