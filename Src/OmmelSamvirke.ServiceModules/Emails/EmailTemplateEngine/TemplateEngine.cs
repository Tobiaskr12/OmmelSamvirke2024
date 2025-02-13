using System.Text;
using Contracts.ServiceModules.Emails;
using Contracts.SupportModules.Logging;
using FluentResults;
using HtmlAgilityPack;

namespace OmmelSamvirke.ServiceModules.Emails.EmailTemplateEngine;

public partial class TemplateEngine : IEmailTemplateEngine
{
    private readonly ILoggingHandler _logger;
    private readonly string _templatesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Emails", "EmailTemplateEngine", "Templates");
    private readonly string _partialsBaseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Emails", "EmailTemplateEngine", "Partials");
    private string _htmlBody = string.Empty;
    private string _plainTextBody = string.Empty;
    private string _subject = string.Empty;

    public TemplateEngine(ILoggingHandler logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// This constructor should only be used by the EmailTemplatePreviewGuide program so it can reference
    /// the source files directly and listen for changes.
    /// </summary>
    public TemplateEngine(ILoggingHandler logger, string baseDirectory)
    {
        _logger = logger;
        _templatesDirectory = Path.Combine(baseDirectory, "Emails", "EmailTemplateEngine", "Templates");
        _partialsBaseDirectory = Path.Combine(baseDirectory, "Emails", "EmailTemplateEngine", "Partials");
    }
    
    public Result GenerateBodiesFromTemplate(string templateName, params (string key, string value)[] parameters)
    {
        try
        {
            string safeTemplateName = templateName
                                          .Replace("/", Path.DirectorySeparatorChar.ToString())
                                          .Replace("\\", Path.DirectorySeparatorChar.ToString());
            string templateFilePath = Path.Combine(_templatesDirectory, safeTemplateName);
            
            _htmlBody = File.ReadAllText(templateFilePath);
            
            // Insert partials
            _htmlBody = PartialRegex().Replace(_htmlBody, match =>
            {
                string partialName = match.Groups[1].Value;

                string safePartialName = partialName.Replace("/", Path.DirectorySeparatorChar.ToString());
                string partialFilePath = Path.Combine(_partialsBaseDirectory, safePartialName + ".html");
            
                if (!File.Exists(partialFilePath))
                {
                    _logger.LogWarning($"Could not find partials file {partialFilePath}");
                    return string.Empty;
                }
                
                return File.ReadAllText(partialFilePath);
            });

            // Replace parameters
            foreach ((string key, string value) param in parameters)
            {
                _htmlBody = _htmlBody.Replace("{{" + param.key + "}}", param.value);
            }
            
            // Extract email subject from headlines
            var doc = new HtmlDocument();
            doc.LoadHtml(_htmlBody);
            
            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//head/title");
            _subject = titleNode?.InnerText ?? string.Empty;
            
            _plainTextBody = GeneratePlainTextBody();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex);
            return Result.Fail("Failed to generate email bodies"); // TODO - Fix localization
        }
    }
    
    public Result GenerateBodiesFromHtml(string htmlContent, params (string key, string value)[] parameters)
    {
        try
        {
            _htmlBody = htmlContent;
            
            foreach ((string key, string value) param in parameters)
            {
                _htmlBody = _htmlBody.Replace("{{" + param.key + "}}", param.value);
            }
            
            _plainTextBody = GeneratePlainTextBody();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex);
            return Result.Fail("Failed to generate email bodies"); // TODO - Fix localization
        }
    }

    public string GetHtmlBody()
    {
        return _htmlBody;
    }

    public string GetPlainTextBody()
    {
        return _plainTextBody;
    }

    public string GetSubject()
    {
        return _subject;
    }

    private string GeneratePlainTextBody()
    {
        _htmlBody = WhiteSpaceRegex().Replace(_htmlBody, " ");
        _htmlBody = HtmlTagsWithSpaceRegex().Replace(_htmlBody, "><");
        _htmlBody = _htmlBody.Replace("\r\n", "");
        
        var htmlDoc = new HtmlDocument();
        var plainTextBuilder = new StringBuilder();
        
        htmlDoc.LoadHtml(_htmlBody);
        ProcessHtmlNode(htmlDoc.DocumentNode, plainTextBuilder);
        
        // Remove newline character at the end if there is one
        var plainTextBody = plainTextBuilder.ToString();
        if (plainTextBody.EndsWith("\r\n"))
        {
            plainTextBody = plainTextBody[..^2];
        }
        
        return plainTextBody.Trim();
    }
    
    private static void ProcessHtmlNode(HtmlNode node, StringBuilder builder)
    {
        foreach (HtmlNode? child in node.ChildNodes)
        {
            switch (child.NodeType)
            {
                case HtmlNodeType.Text:
                    string? text = HtmlEntity.DeEntitize(child.InnerText);
                    // Normalize whitespace
                    text = WhiteSpaceRegex().Replace(text, " ");

                    // Only append if there's meaningful text
                    if (!string.IsNullOrEmpty(text))
                    {
                        builder.Append(text);
                    }
                    break;

                case HtmlNodeType.Element:
                    if (child.Name.Equals("head", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (child.Name.Equals("a", StringComparison.OrdinalIgnoreCase))
                    {
                        string href = child.GetAttributeValue("href", string.Empty).Trim();
                        string linkText = child.InnerText.Trim();

                        if (!string.IsNullOrEmpty(linkText) && !string.IsNullOrEmpty(href))
                        {
                            builder.Append($"{linkText} ({href})");
                        }
                        else if (!string.IsNullOrEmpty(linkText))
                        {
                            builder.Append(linkText);
                        }
                        else if (!string.IsNullOrEmpty(href))
                        {
                            builder.Append(href);
                        }
                    }
                    else if (child.Name.Equals("br", StringComparison.OrdinalIgnoreCase))
                    {
                        builder.Append("\r\n");
                    }
                    else if (child.Name.Equals("p", StringComparison.OrdinalIgnoreCase))
                    {
                        AppendNewLineIfNotFirstLine(builder);
                        ProcessHtmlNode(child, builder);
                        builder.Append("\r\n");
                    }
                    else if (child.Name.Equals("footer", StringComparison.OrdinalIgnoreCase))
                    {
                        AppendNewLineIfNotFirstLine(builder);
                        ProcessHtmlNode(child, builder);
                        builder.Append("\r\n");
                    }
                    else if (child.Name.Equals("ul", StringComparison.OrdinalIgnoreCase) ||
                             child.Name.Equals("ol", StringComparison.OrdinalIgnoreCase))
                    {
                        AppendNewLineIfNotFirstLine(builder);
                        ProcessHtmlNode(child, builder);
                        if (!child.OuterHtml.Contains("/"))
                        {
                            builder.Append("\r\n");
                        }
                    }
                    else if (child.Name.Equals("li", StringComparison.OrdinalIgnoreCase))
                    {
                        builder.Append("- ");
                        ProcessHtmlNode(child, builder);
                        builder.Append("\r\n");
                    }
                    else if (child.Name.StartsWith("h") && child.Name.Length == 2 && char.IsDigit(child.Name[1]))
                    {
                        AppendNewLineIfNotFirstLine(builder);
                        ProcessHtmlNode(child, builder);
                        builder.Append("\r\n");
                    }
                    else if (child.Name.Equals("hr", StringComparison.OrdinalIgnoreCase))
                    {
                        AppendNewLineIfNotFirstLine(builder);
                        builder.Append(new string('-', 40));
                        builder.Append("\r\n");
                    }
                    else if (child.Name.Equals("strong", StringComparison.OrdinalIgnoreCase) || 
                             child.Name.Equals("b", StringComparison.OrdinalIgnoreCase))
                    {
                        ProcessHtmlNode(child, builder);
                    }
                    else if (child.Name.Equals("em", StringComparison.OrdinalIgnoreCase) || 
                             child.Name.Equals("i", StringComparison.OrdinalIgnoreCase))
                    {
                        ProcessHtmlNode(child, builder);
                    }
                    else
                    {
                        ProcessHtmlNode(child, builder);
                    }
                    break;

                case HtmlNodeType.Comment:
                    // Ignore comments
                    break;
                case HtmlNodeType.Document:
                    // Ignore document nodes
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private static void AppendNewLineIfNotFirstLine(StringBuilder builder)
    {
        if (builder.Length > 0)
        {
            builder.Append("\r\n");
        }
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"\s+")]
    private static partial System.Text.RegularExpressions.Regex WhiteSpaceRegex();
    
    [System.Text.RegularExpressions.GeneratedRegex(@"> <")]
    private static partial System.Text.RegularExpressions.Regex HtmlTagsWithSpaceRegex();
    
    [System.Text.RegularExpressions.GeneratedRegex(@"\{\{>\s*(.*?)\s*\}\}")]
    private static partial System.Text.RegularExpressions.Regex PartialRegex();
}
