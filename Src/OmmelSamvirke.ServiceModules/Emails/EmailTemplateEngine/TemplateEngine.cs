using System.Text;
using HtmlAgilityPack;

namespace OmmelSamvirke.ServiceModules.Emails.EmailTemplateEngine;

public static partial class TemplateEngine
{
    private static readonly string TemplatesDirectory = Path.Combine(".", "Emails", "EmailTemplateEngine", "Templates");
    
    public static string GenerateHtmlBody(string templateFileName, params (string key, string value)[] parameters)
    {
        string htmlTemplate = File.ReadAllText(Path.Combine(TemplatesDirectory, templateFileName));

        foreach ((string key, string value) param in parameters)
        {
            htmlTemplate = htmlTemplate.Replace("{{" + param.key + "}}", param.value);
        }
        
        return htmlTemplate;
    }

    public static string GeneratePlainTextBody(string htmlContent, params (string key, string value)[] parameters)
    {
        htmlContent = WhiteSpaceRegex().Replace(htmlContent, " ");
        htmlContent = HtmlTagsWithSpaceRegex().Replace(htmlContent, "><");
        htmlContent = htmlContent.Replace("\r\n", "");
    
        foreach ((string key, string value) param in parameters)
        {
            htmlContent = htmlContent.Replace("{{" + param.key + "}}", param.value);
        }
        
        var htmlDoc = new HtmlDocument();
        var plainTextBuilder = new StringBuilder();
        
        htmlDoc.LoadHtml(htmlContent);
        ProcessHtmlNode(htmlDoc.DocumentNode, plainTextBuilder);
        
        // Remove newline character at the end if there is one
        var plainTextBody = plainTextBuilder.ToString();
        if (plainTextBody.EndsWith("\r\n"))
        {
            plainTextBody = plainTextBody[..^2];
        }
        
        return plainTextBody.Trim();
    }

    public static string GeneratePlainTextBodyFromTemplate(string templateFileName, params (string key, string value)[] parameters)
    {
        string htmlTemplate = File.ReadAllText(Path.Combine(TemplatesDirectory, templateFileName));
        return GeneratePlainTextBody(htmlTemplate, parameters);
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
}
