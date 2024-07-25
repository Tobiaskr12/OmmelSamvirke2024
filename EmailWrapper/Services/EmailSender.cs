using Azure;
using Azure.Communication.Email;
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
        string? connectionString = configuration.GetConnectionString("AcsConnectionString");
        if (string.IsNullOrEmpty(connectionString))
            throw new Exception("No connection string found for Azure Communication Services");
        
        _emailClient = new EmailClient(connectionString);
    }
    
    public async Task<Result> SendEmail(Email email)
    {
        try
        {
            await _emailClient.SendAsync(
                wait: WaitUntil.Started,
                senderAddress: "donotreply@9ea353ff-2cff-45ce-abbd-a63d077c3f07.azurecomm.net",
                recipientAddress: email.Recipients.First().Email,
                email.Subject,
                htmlContent: email.Body
            );
            
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError("Error while sending an email {message}", e.Message);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> SendEmails(List<Email> emails)
    {
        List<Email> failedEmails = [];
        
        foreach (Email email in emails)
        {
            Result result = await SendEmail(email);
            if (result.IsFailed)
            {
                failedEmails.Add(email);
            }
        }

        if (failedEmails.Count == 0) return Result.Ok();
        
        _logger.LogError(
            "The emails with the following subjects failed sending: {emails}",
            failedEmails.Select(x => x.Subject)
        );
            
        return Result.Fail($"{failedEmails.Count} failed sending");
    }
}
