using Azure;
using Azure.Communication.Email;
using Contracts.Infrastructure.Emails;
using Contracts.ServiceModules.Emails.DTOs;
using Contracts.ServiceModules.Emails.Enums;
using Contracts.SupportModules.Logging;
using FluentResults;
using Microsoft.Extensions.Configuration;
using DomainModules.Emails.Entities;
using Infrastructure.Errors;

namespace Infrastructure.Emails;

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

    public async Task<Result> SendBatchesAsync(Email email, int batchSize, bool useBcc = false, CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<Recipient[]> batches = email.Recipients.Chunk(batchSize);

            foreach (Recipient[] batch in batches)
            {
                email.Recipients = batch.ToList();
                Result<EmailSendingStatus> sendResult = await SendAsync(email, useBcc, cancellationToken);

                if (sendResult.IsFailed)
                {
                    _logger.LogError(new Exception($"Sending of email batch failed. Total batches: {batch.Length}"));
                }
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not send email via Azure");
            return Result.Fail(ErrorMessages.AzureEmailSendingFailed);
        }
    }
        
    private static EmailMessage ConvertEmailToAzureEmailMessage(Email email, bool useBcc)
    {
        EmailMessage emailMessage;
            
        if (useBcc)
        {
            emailMessage = new EmailMessage(
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

        // Convert each BlobStorageFile attachment into an EmailAttachment.
        List<EmailAttachment> emailAttachments = email.Attachments.Select(file => new EmailAttachment(
            file.FileBaseName,
            file.ContentType,
            BinaryData.FromStream(file.FileContent ?? throw new InvalidOperationException("Email attachment cannot be empty"))
        )).ToList();

        foreach (EmailAttachment attachment in emailAttachments)
        {
            emailMessage.Attachments.Add(attachment);
        }

        return emailMessage;
    }
}
