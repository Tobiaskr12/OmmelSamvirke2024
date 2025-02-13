using FluentResults;

namespace Contracts.ServiceModules.Emails;

public interface IEmailTemplateEngine
{
    /// <summary>
    /// Generate an email body from an HTML template. This method supports partials and getting the email subject
    /// directly from the template file.
    /// </summary>
    Result GenerateBodiesFromTemplate(string templateName, params (string key, string value)[] parameters);
    
    /// <summary>
    /// Generate an email body from a pure HTML string. This method does not support partials and getting the
    /// email subject from the HTML.
    /// </summary>
    Result GenerateBodiesFromHtml(string htmlContent, params (string key, string value)[] parameters);
    string GetHtmlBody();
    string GetPlainTextBody();
    string GetSubject();
}
