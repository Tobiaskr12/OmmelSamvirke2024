using Azure;
using Azure.Communication.Email;
using EmailWrapper.Constants;
using EmailWrapper.Interfaces;
using EmailWrapper.Models;
using FluentResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EmailWrapper.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger _logger;
    private readonly EmailClient _emailClient;
    
    public EmailSender(IConfiguration configuration, ILogger logger)
    {
        _logger = logger;
        string? connectionString = configuration.GetSection("AcsConnectionString").Value;
        if (string.IsNullOrEmpty(connectionString))
            throw new Exception("No connection string found for Azure Communication Services");
        
        _emailClient = new EmailClient(connectionString);
    }
    
    public async Task<Result> SendEmail(Email email)
    {
        try
        {
            EmailMessage emailMessage = ConvertEmailToAzureEmailMessage(email);
            await _emailClient.SendAsync(WaitUntil.Started, emailMessage);
            
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError("Error while sending an email: {message}", e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> SendEmails(List<Email> emails)
    {
        List<Email> failedEmails = [];
        
        foreach (Email email in emails)
        {
            Result sendingResult = await SendEmail(email);
            if (sendingResult.IsFailed)
            {
                failedEmails.Add(email);
            }
        }

        if (failedEmails.Count == 0) return Result.Ok();
        
        _logger.LogError(
            "The emails with the following subjects failed sending: {emails}",
            failedEmails.Select(x => x.Subject)
        );

        var error = new Error(
            $"{failedEmails.Count} emails failed sending. The emails can be extracted from this error's metadata"
        );
        failedEmails.ForEach(email => error.Metadata.Add(email.Id.ToString(), email));
        
        return Result.Fail(error);
    }

    private static EmailMessage ConvertEmailToAzureEmailMessage(Email email)
    {
        var emailMessage = new EmailMessage(
            senderAddress: email.SenderEmailAddress,
            recipients: new EmailRecipients(
                to: email.Recipients.Select(recipient => new EmailAddress(recipient.Email))
            ),
            content: new EmailContent(subject: email.Subject)
            {
                Html = email.Body
            }
        );

        List<EmailAttachment> emailAttachments = email.Attachments.Select(attachment => new EmailAttachment(
            attachment.Name,
            attachment.ContentType.Name,
            BinaryData.FromStream(attachment.ContentStream)
        )).ToList();

        foreach (EmailAttachment emailAttachment in emailAttachments)
        {
            emailMessage.Attachments.Add(emailAttachment);
        }

        return emailMessage;
    }
}
