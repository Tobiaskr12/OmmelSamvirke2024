using System.Reflection;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.DomainModules.Emails.Constants;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.ServiceModules.Emails.Sending.Commands;

namespace OmmelSamvirke.ServiceModules.Errors;

public class ErrorLogger : IErrorLogger
{
    private readonly ILogger _logger;
    private readonly IMediator _mediator;
    private readonly List<Recipient> _developerEmails;
    
    public ErrorLogger(ILogger logger, IConfiguration config, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
        _developerEmails = [];
        
        // In non-prod, don't send error emails
        string? executionEnvironment = config.GetSection("ExecutionEnvironment").Value;
        if (executionEnvironment != "Prod") return;
        
        // In prod, send error emails to developers listed in config
        string? developerEmailsSection = config.GetSection("DeveloperEmails").Value;
        string[]? developerEmails = developerEmailsSection?.Split(",");

        if (developerEmails is null)
        {
            _logger.LogError("Cannot initialize error logger, because no developer emails are configured.");
            throw new Exception();
        }

        foreach (string email in developerEmails)
        {
            _developerEmails.Add(new Recipient { EmailAddress = email });
        }
    }

    public async Task LogException(
        Exception exception,
        CancellationToken cancellationToken = default,
        string callingMethodName = "",
        string callingFilePath = "",
        int callerLineNumber = 0)
    {
        string callingAssemblyName = Assembly.GetCallingAssembly().GetName().Name ?? "Unknown Assembly";
        
        var emailLog = new Email
        {
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Subject = $"[Exception] in method: {callingMethodName}",
            HtmlBody = "Empty.html", // TODO - populate from some kind of template
            PlainTextBody = "Empty.html", // TODO - populate from some kind of template
            Recipients = _developerEmails,
            Attachments = []
        };
        
        await _mediator.Send(new SendEmailCommand(emailLog), cancellationToken);
        
        _logger.LogError(
            exception,
            "[Exception] in method: {callingMethodName}. Error Message: \"{errorMessage}\".",
            callingMethodName,
            exception.Message);
    }

    public async Task LogValidationError(
        object invalidValue,
        string validationErrorMessage,
        CancellationToken cancellationToken = default,
        string callingMethodName = "",
        string callingFilePath = "",
        int callerLineNumber = 0)
    {
        string callingAssemblyName = Assembly.GetCallingAssembly().GetName().Name ?? "Unknown Assembly";

        var emailLog = new Email
        {
            SenderEmailAddress = ValidSenderEmailAddresses.Auto,
            Subject = $"[Validation Error] in method: {callingMethodName}",
            HtmlBody = "Empty.html", // TODO - populate from some kind of template
            PlainTextBody = "Empty.html", // TODO - populate from some kind of template
            Recipients = _developerEmails,
            Attachments = []
        };
        
        await _mediator.Send(new SendEmailCommand(emailLog), cancellationToken);
        
        _logger.LogWarning(
            "[Validation Error] in method: {callingMethodName}. Error Message: \"{errorMessage}\". Invalid value: \"{invalidValue}\"",
            callingMethodName,
            validationErrorMessage,
            invalidValue.ToString());
    }
}
