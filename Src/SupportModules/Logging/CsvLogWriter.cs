using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Contracts.ServiceModules.Emails.EmailTemplateEngine;
using Contracts.ServiceModules.Emails.Sending;
using Contracts.SupportModules.Logging;
using Contracts.SupportModules.Logging.Enums;
using Contracts.SupportModules.Logging.Models;
using MediatR;
using DomainModules.Emails.Constants;
using DomainModules.Emails.Entities;
using FluentResults;
using SupportModules.Logging.Interfaces;

namespace SupportModules.Logging;

public class CsvLogWriter : CsvBufferedWriter<LogEntry>, ILoggingHandler
{
    private readonly IMediator _mediator;
    private readonly Func<IEmailTemplateEngine> _emailTemplateEngineFactory;

    public CsvLogWriter(
        ICorrelationContext correlationContext, 
        ILoggingLocationInfo loggingLocationInfo,
        IMediator mediator,
        Func<IEmailTemplateEngine> emailTemplateEngineFactory) : base(correlationContext, loggingLocationInfo)
    {
        _mediator = mediator;
        _emailTemplateEngineFactory = emailTemplateEngineFactory;
    }

    public void LogInformation(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    ) {
        string assemblyName = SanitizeAssemblyName(Assembly.GetCallingAssembly().GetName().Name ?? "UnknownAssembly");
        EnqueueLog(LogLevel.Information, message, null, filePath, lineNumber, memberName, assemblyName);
    } 

    public void LogWarning(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    )
    {
        string assemblyName = SanitizeAssemblyName(Assembly.GetCallingAssembly().GetName().Name ?? "UnknownAssembly");
        EnqueueLog(LogLevel.Warning, message, null, filePath, lineNumber, memberName, assemblyName);
    }
        

    public void LogDebug(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    )
    {
        string assemblyName = SanitizeAssemblyName(Assembly.GetCallingAssembly().GetName().Name ?? "UnknownAssembly");
        EnqueueLog(LogLevel.Debug, message, null, filePath, lineNumber, memberName, assemblyName);
    }

    public void LogError(
        Exception? ex,
        string? customMessage = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    )
    {
        string assemblyName = SanitizeAssemblyName(Assembly.GetCallingAssembly().GetName().Name ?? "UnknownAssembly");
        string message = customMessage ?? ex?.Message ?? "N/A";
        EnqueueLog(LogLevel.Error, message, ex, filePath, lineNumber, memberName, assemblyName);
    }

    public void LogCritical(
        Exception ex,
        string? customMessage = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    )
    {
        string assemblyName = SanitizeAssemblyName(Assembly.GetCallingAssembly().GetName().Name ?? "UnknownAssembly");
        string message = customMessage ?? ex.Message;
        EnqueueLog(LogLevel.Critical, message, ex, filePath, lineNumber, memberName, assemblyName);
        
        try
        {
            _mediator.Send(new SendEmailCommand(BuildCriticalErrorEmail(ex, customMessage)));
        } catch (Exception ex2) 
        {
            LogError(ex2, "Could not send email when a critical error occurred");
        }
    }

    private void EnqueueLog(
        LogLevel level,
        string message,
        Exception? ex,
        string filePath,
        int lineNumber,
        string memberName,
        string assemblyName
    )
    {
        string serviceName = InferServiceName(filePath);

        var entry = new LogEntry
        {
            Level = level,
            Message = message,
            Exception = GetExceptionInfo(ex),
            SessionId = CorrelationContext.SessionId,
            OperationId = CorrelationContext.OperationId,
            CallerLineNumber = lineNumber,
            CallerMemberName = memberName,
            CallerServiceName = serviceName,
            CallerAssemblyName = assemblyName
        };

        AddEntry(entry);
    }

    private static string SanitizeAssemblyName(string assemblyName)
    {
        if (assemblyName.Contains('.'))
        {
            assemblyName = assemblyName.Split('.')[^1];
        }
        
        return assemblyName;
    }

    private ExceptionInfo? GetExceptionInfo(Exception? exception)
    {
        if (exception is null) return null;

        return new ExceptionInfo
        {
            Type = exception.GetType().FullName ?? "Unknown",
            Message = string.IsNullOrEmpty(exception.Message) ? "None" : exception.Message,
            StackTrace = exception.StackTrace ?? "None",
            InnerException = GetExceptionInfo(exception.InnerException)
        };
    }

    private string EncodeExceptionInfo(ExceptionInfo exceptionInfo)
    {
        string json = JsonSerializer.Serialize(exceptionInfo);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    private Email BuildCriticalErrorEmail(Exception ex, string? customMessage = null)
    {
        IEmailTemplateEngine emailTemplateEngine = _emailTemplateEngineFactory();

        Result generationResult = emailTemplateEngine.GenerateBodiesFromTemplate(Templates.General.CriticalError, 
            ("Timestamp", DateTime.UtcNow.ToString("HH:mm")),
            ("CustomMessage", customMessage ?? "None"),
            ("ExceptionType", ex.GetType().Name),
            ("ExceptionMessage", ex.Message),
            ("StackTrace", ex.StackTrace ?? "None")
        );

        if (generationResult.IsSuccess)
        {
            return new Email
            {
                Recipients = [new Recipient { EmailAddress = "tobiaskristensen12@gmail.com" }],
                Subject = emailTemplateEngine.GetSubject(),
                SenderEmailAddress = ValidSenderEmailAddresses.Auto,
                Attachments = [],
                PlainTextBody = emailTemplateEngine.GetPlainTextBody(),
                HtmlBody = emailTemplateEngine.GetHtmlBody(),
            };
        } else
        {
            throw new Exception("Failed to generate the Critical Error email via the Email Template Engine");
        }
    }

    private static string InferServiceName(string filePath) =>
        string.IsNullOrWhiteSpace(filePath)
            ? "UnknownService"
            : Path.GetFileName(filePath);

    protected override string GetFileName(DateTime now) =>
        Path.Combine(Directory, $"{now:yyyy-MM-dd}-Logs.csv");

    protected override string GetHeaderLine() =>
        "Timestamp,Level,Message,Exception,SessionId,OperationId,CallerLineNumber,CallerMemberName,CallerServiceName,CallerAssemblyName";

    protected override string FormatEntry(LogEntry e)
    {
        string exceptionString = string.Empty;
        if (e.Exception is not null) { 
            exceptionString = EncodeExceptionInfo(e.Exception);
        }

        return string.Join(",",
            e.Timestamp.ToString("o"),
            e.Level,
            EscapeCsv(e.Message),
            EscapeCsv(exceptionString),
            EscapeCsv(e.SessionId),
            EscapeCsv(e.OperationId),
            e.CallerLineNumber,
            EscapeCsv(e.CallerMemberName),
            EscapeCsv(e.CallerServiceName),
            EscapeCsv(e.CallerAssemblyName)
        );
    }
}
