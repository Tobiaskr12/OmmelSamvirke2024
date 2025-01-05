using FluentResults;

namespace OmmelSamvirke.ServiceModules.Emails.EmailTemplateEngine;

public interface IEmailTemplateEngine
{
    Result GenerateBodiesFromTemplate(string templateName, params (string key, string value)[] parameters);
    Result GenerateBodiesFromHtml(string htmlContent, params (string key, string value)[] parameters);
    string GetHtmlBody();
    string GetPlainTextBody();
}
