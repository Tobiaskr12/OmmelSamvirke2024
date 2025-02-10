using Azure;
using Azure.Communication.Email;
using FluentResults;
using Microsoft.Extensions.Configuration;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DTOs.Emails;
using OmmelSamvirke.Infrastructure.Errors;
using OmmelSamvirke.SupportModules.Logging.Interfaces;

namespace OmmelSamvirke.Infrastructure.Emails;

public class AzureEmailServiceWrapper : IExternalEmailServiceWrapper
{
    private readonly ILoggingHandler _logger;
    private readonly EmailClient _emailClient;

    public AzureEmailServiceWrapper(IConfiguration configuration, ILoggingHandler logger)
    {
        string? connectionString = configuration.GetSection("AcsConnectionString").Value;
        if (string.IsNullOrEmpty(connectionString))
            throw new Exception("No connection string found for Azure Communication Services");
        
        _emailClient = new EmailClient(connectionString);
        _logger = logger;
    }
    
    public async Task<Result<EmailSendingStatus>> SendAsync(Email email, bool useBcc = false, CancellationToken cancellationToken = default)
    {
        try
        {
            EmailMessage emailMessage = ConvertEmailToAzureEmailMessage(email, useBcc);
            await _emailClient.SendAsync(WaitUntil.Started, emailMessage, cancellationToken);

            var sendingStatus = new EmailSendingStatus(email, SendingStatus.NotStarted, []);
            return Result.Ok(sendingStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not send email via Azure");
            return Result.Fail(ErrorMessages.AzureEmailSendingFailed);
        }
    }
    
    public static SendingStatus ConvertAzureStatusToSendingStatus(EmailSendStatus status)
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
    
    private static EmailMessage ConvertEmailToAzureEmailMessage(Email email, bool useBcc)
    {
        EmailMessage emailMessage;
        
        if (useBcc)
        {
            emailMessage= new EmailMessage(
                senderAddress: email.SenderEmailAddress,
                recipients: new EmailRecipients(
                    bcc: email.Recipients.Select(recipient => new EmailAddress(recipient.EmailAddress))
                ),
                content: new EmailContent(subject: email.Subject)
                {
                    Html = email.HtmlBody,
                    PlainText = email.PlainTextBody,
                }
            );
        } 
        else
        {
            emailMessage = new EmailMessage(
                senderAddress: email.SenderEmailAddress,
                recipients: new EmailRecipients(
                    to: email.Recipients.Select(recipient => new EmailAddress(recipient.EmailAddress))
                ),
                content: new EmailContent(subject: email.Subject)
                {
                    Html = email.HtmlBody,
                    PlainText = email.PlainTextBody
                }
            );
        }

        List<EmailAttachment> emailAttachments = email.Attachments.Select(attachment => new EmailAttachment(
            attachment.Name,
            attachment.ContentType.MediaType,
            BinaryData.FromBytes(attachment.BinaryContent ?? throw new InvalidOperationException("Email attachment cannot be empty"))
        )).ToList();

        foreach (EmailAttachment emailAttachment in emailAttachments)
        {
            emailMessage.Attachments.Add(emailAttachment);
        }

        return emailMessage;
    }
}
