using Azure;
using Azure.Communication.Email;
using FluentResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DTOs.Emails;
using OmmelSamvirke.Infrastructure.Errors;

namespace OmmelSamvirke.Infrastructure.Emails;

public class AzureEmailServiceWrapper : IExternalEmailServiceWrapper
{
    private readonly ILogger<AzureEmailServiceWrapper> _logger;
    private readonly EmailClient _emailClient;

    public AzureEmailServiceWrapper(IConfiguration configuration, ILogger<AzureEmailServiceWrapper> logger)
    {
        string? connectionString = configuration.GetConnectionString("AcsConnectionString");
        if (string.IsNullOrEmpty(connectionString))
            throw new Exception("No connection string found for Azure Communication Services");
        
        _emailClient = new EmailClient(connectionString);
        _logger = logger;
    }
    
    public async Task<Result<EmailSendingStatus>> SendAsync(Email email, CancellationToken cancellationToken = default)
    {
        try
        {
            EmailMessage emailMessage = ConvertEmailToAzureEmailMessage(email);
            EmailSendOperation sendOperation = await _emailClient.SendAsync(WaitUntil.Started, emailMessage, cancellationToken);

            if (!sendOperation.HasValue) throw new Exception("The send operation did not produce a value");

            var sendingStatus = new EmailSendingStatus(
                email,
                ConvertAzureStatusToSendingStatus(sendOperation.Value.Status),
                []
            );
            return Result.Ok(sendingStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not send email via Azure: {message}", ex.Message);
            return Result.Fail(ErrorMessages.AzureEmailSendingFailed);
        }
    }
    
    private static EmailMessage ConvertEmailToAzureEmailMessage(Email email)
    {
        var emailMessage = new EmailMessage(
            senderAddress: email.SenderEmailAddress,
            recipients: new EmailRecipients(
                to: email.Recipients.Select(recipient => new EmailAddress(recipient.EmailAddress))
            ),
            content: new EmailContent(subject: email.Subject)
            {
                Html = email.Body
            }
        );

        List<EmailAttachment> emailAttachments = email.Attachments.Select(attachment => new EmailAttachment(
            attachment.Name,
            attachment.ContentType.Name,
            BinaryData.FromBytes(attachment.BinaryContent ?? throw new InvalidOperationException("Email attachment cannot be empty"))
        )).ToList();

        foreach (EmailAttachment emailAttachment in emailAttachments)
        {
            emailMessage.Attachments.Add(emailAttachment);
        }

        return emailMessage;
    }

    private static SendingStatus ConvertAzureStatusToSendingStatus(EmailSendStatus status)
    {
        return status.ToString() switch
        {
            "NotStarted" => SendingStatus.NotStarted,
            "Running" => SendingStatus.Running,
            "Succeeded" => SendingStatus.Succeeded,
            "Failed" => SendingStatus.Failed,
            "Canceled" => SendingStatus.Canceled,
            _ => SendingStatus.Unknown
        };
    }
}
