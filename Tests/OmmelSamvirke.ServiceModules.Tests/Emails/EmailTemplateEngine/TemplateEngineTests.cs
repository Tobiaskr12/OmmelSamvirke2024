using FluentResults;
using Microsoft.Extensions.Logging;
using OmmelSamvirke.ServiceModules.Emails.EmailTemplateEngine;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.EmailTemplateEngine;

[TestFixture]
public class TemplateEngineTests
{
    private string _templatesDirectory;
    private IEmailTemplateEngine _emailTemplateEngine;
    private ILogger _logger;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Set up the Templates directory relative to the current directory
        _templatesDirectory = Path.Combine(".", "Emails", "EmailTemplateEngine", "Templates");
        if (!Directory.Exists(_templatesDirectory))
        {
            Directory.CreateDirectory(_templatesDirectory);
        }
        
        _logger = NSubstitute.Substitute.For<ILogger>();
        _emailTemplateEngine = new TemplateEngine(_logger);
    }

    // Clean up the Templates directory after each test.
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // Assumption: Each test file start with a lower-case character.
        // All production files start with an upper-case character.
        if (Directory.Exists(_templatesDirectory))
        {
            string[] files = Directory.GetFiles(_templatesDirectory);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                if (char.IsLower(fileName[0]))
                {
                    File.Delete(file);
                }
            }
        }
    }

    /// <summary>
    /// Helper method to create a template file with specified content.
    /// </summary>
    /// <param name="fileName">Name of the template file.</param>
    /// <param name="content">HTML content of the template.</param>
    private void CreateTemplate(string fileName, string content)
    {
        string filePath = Path.Combine(_templatesDirectory, fileName);
        File.WriteAllText(filePath, content);
    }

    /// <summary>
    /// Helper method for performing assertions since there are many repeated assertion blocks
    /// </summary>
    private static void PerformAssertions(Result result, string actualHtml, string expectedHtml, string actualPlainText, string expectedPlainText)
    {
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(actualHtml, Is.EqualTo(expectedHtml));
            Assert.That(actualPlainText, Is.EqualTo(expectedPlainText));
        });    
    }

    [Test]
    public void GenerateBodiesFromTemplate_ReplacesSinglePlaceholder()
    {
        const string templateName = "single_placeholder.html";
        const string templateContent = "<p>Hello, {{UserName}}!</p>";
        const string expectedHtml = "<p>Hello, John Doe!</p>";
        const string expectedPlain = "Hello, John Doe!";
        CreateTemplate(templateName, templateContent);

        Result result = _emailTemplateEngine.GenerateBodiesFromTemplate(templateName, ("UserName", "John Doe"));
        string actualHtml = _emailTemplateEngine.GetHtmlBody();
        string actualPlain = _emailTemplateEngine.GetPlainTextBody();
        
        PerformAssertions(result, actualHtml, expectedHtml, actualPlain, expectedPlain);
    }

    [Test]
    public void GenerateBodiesFromTemplate_MultiplePlaceholders()
    {
        const string templateName = "multiple_placeholders.html";
        const string templateContent = "<p>Hello, {{FirstName}} {{LastName}}!</p><p>Your email is {{Email}}.</p>";
        const string expectedHtml = "<p>Hello, Jane Doe!</p><p>Your email is jane.doe@example.com.</p>";
        const string expectedPlain = """
                                    Hello, Jane Doe!
                                    
                                    Your email is jane.doe@example.com.
                                    """;

        CreateTemplate(templateName, templateContent);
        Result result = _emailTemplateEngine.GenerateBodiesFromTemplate(
            templateName,
            ("FirstName", "Jane"),
            ("LastName", "Doe"),
            ("Email", "jane.doe@example.com")
        );
        string actualHtml = _emailTemplateEngine.GetHtmlBody();
        string actualPlain = _emailTemplateEngine.GetPlainTextBody();

        PerformAssertions(result, actualHtml, expectedHtml, actualPlain, expectedPlain);
    }

    [Test]
    public void GenerateBodiesFromHtml_MultiplePlaceholders()
    {
        const string htmlContent = "<p>Hello, {{FirstName}} {{LastName}}!</p><p>Your email is {{Email}}.</p>";
        const string expectedHtml = "<p>Hello, Jane Doe!</p><p>Your email is jane.doe@example.com.</p>";
        const string expectedPlain = """
                                    Hello, Jane Doe!
                                    
                                    Your email is jane.doe@example.com.
                                    """;

        Result result = _emailTemplateEngine.GenerateBodiesFromHtml(
            htmlContent,
            ("FirstName", "Jane"),
            ("LastName", "Doe"),
            ("Email", "jane.doe@example.com")
        );
        string actualHtml = _emailTemplateEngine.GetHtmlBody();
        string actualPlain = _emailTemplateEngine.GetPlainTextBody();

        PerformAssertions(result, actualHtml, expectedHtml, actualPlain, expectedPlain);
    }

    [Test]
    public void GenerateBodiesFromTemplate_NoPlaceholders()
    {
        const string templateName = "no_placeholders.html";
        const string templateContent = "<p>Welcome to our service!</p>";
        const string expectedHtml = "<p>Welcome to our service!</p>";
        const string expectedPlain = "Welcome to our service!";

        CreateTemplate(templateName, templateContent);
        Result result = _emailTemplateEngine.GenerateBodiesFromTemplate(templateName);
        string actualHtml = _emailTemplateEngine.GetHtmlBody();
        string actualPlain = _emailTemplateEngine.GetPlainTextBody();

        PerformAssertions(result, actualHtml, expectedHtml, actualPlain, expectedPlain);
    }

    [Test]
    public void GenerateBodiesFromHtml_NoPlaceholders()
    {
        const string htmlContent = "<p>Welcome to our service!</p>";
        const string expectedHtml = "<p>Welcome to our service!</p>";
        const string expectedPlain = "Welcome to our service!";

        Result result = _emailTemplateEngine.GenerateBodiesFromHtml(htmlContent);
        string actualHtml = _emailTemplateEngine.GetHtmlBody();
        string actualPlain = _emailTemplateEngine.GetPlainTextBody();

        PerformAssertions(result, actualHtml, expectedHtml, actualPlain, expectedPlain);
    }

    [Test]
    public void GenerateBodiesFromTemplate_PlaceholderNotFound()
    {
        const string templateName = "missing_placeholder.html";
        const string templateContent = "<p>Hello, {{UserName}}!</p>";
        // Expect no replacement for a missing parameter => remain the same
        const string expectedHtml = "<p>Hello, {{UserName}}!</p>";
        const string expectedPlain = "Hello, {{UserName}}!";

        CreateTemplate(templateName, templateContent);
        // Not providing the parameter "UserName"
        Result result = _emailTemplateEngine.GenerateBodiesFromTemplate(templateName);
        string actualHtml = _emailTemplateEngine.GetHtmlBody();
        string actualPlain = _emailTemplateEngine.GetPlainTextBody();

        PerformAssertions(result, actualHtml, expectedHtml, actualPlain, expectedPlain);
    }

    [Test]
    public void GenerateBodiesFromHtml_PlaceholderNotFound()
    {
        const string htmlContent = "<p>Hello, {{UserName}}!</p>";
        const string expectedHtml = "<p>Hello, {{UserName}}!</p>";
        const string expectedPlain = "Hello, {{UserName}}!";

        // Not providing the parameter "UserName"
        Result result = _emailTemplateEngine.GenerateBodiesFromHtml(htmlContent);
        string actualHtml = _emailTemplateEngine.GetHtmlBody();
        string actualPlain = _emailTemplateEngine.GetPlainTextBody();

        PerformAssertions(result, actualHtml, expectedHtml, actualPlain, expectedPlain);
    }

    [Test]
    public void GenerateBodiesFromTemplate_EmptyTemplate()
    {
        const string templateName = "empty_template.html";
        const string templateContent = "";
        const string expectedHtml = "";
        const string expectedPlain = "";

        CreateTemplate(templateName, templateContent);
        Result result = _emailTemplateEngine.GenerateBodiesFromTemplate(templateName);
        string actualHtml = _emailTemplateEngine.GetHtmlBody();
        string actualPlain = _emailTemplateEngine.GetPlainTextBody();

        PerformAssertions(result, actualHtml, expectedHtml, actualPlain, expectedPlain);
    }

    [Test]
    public void GenerateBodiesFromHtml_EmptyTemplate()
    {
        const string htmlContent = "";
        const string expectedHtml = "";
        const string expectedPlain = "";

        Result result = _emailTemplateEngine.GenerateBodiesFromHtml(htmlContent);
        string actualHtml = _emailTemplateEngine.GetHtmlBody();
        string actualPlain = _emailTemplateEngine.GetPlainTextBody();

        PerformAssertions(result, actualHtml, expectedHtml, actualPlain, expectedPlain);
    }

    [Test]
    public void GenerateBodiesFromTemplate_NestedElements()
    {
        const string templateName = "nested_elements.html";
        const string templateContent = "<p><strong>Hello, <em>{{UserName}}</em>!</strong></p>";
        const string expectedHtml = "<p><strong>Hello, <em>Jane</em>!</strong></p>";
        const string expectedPlain = "Hello, Jane!";

        CreateTemplate(templateName, templateContent);
        Result result = _emailTemplateEngine.GenerateBodiesFromTemplate(templateName, ("UserName", "Jane"));
        string actualHtml = _emailTemplateEngine.GetHtmlBody();
        string actualPlain = _emailTemplateEngine.GetPlainTextBody();

        PerformAssertions(result, actualHtml, expectedHtml, actualPlain, expectedPlain);
    }

    [Test]
    public void GenerateBodiesFromHtml_NestedElements()
    {
        const string htmlContent = "<p><strong>Hello, <em>{{UserName}}</em>!</strong></p>";
        const string expectedHtml = "<p><strong>Hello, <em>Jane</em>!</strong></p>";
        const string expectedPlain = "Hello, Jane!";

        Result result = _emailTemplateEngine.GenerateBodiesFromHtml(htmlContent, ("UserName", "Jane"));
        string actualHtml = _emailTemplateEngine.GetHtmlBody();
        string actualPlain = _emailTemplateEngine.GetPlainTextBody();

        PerformAssertions(result, actualHtml, expectedHtml, actualPlain, expectedPlain);
    }

    [Test]
    public void GenerateBodiesFromTemplate_ComplexTemplate()
    {
        const string templateName = "complex_template.html";
        const string templateContent = """
            <h1>Welcome, {{UserName}}!</h1>
            <p>We're excited to have you in our <strong>community</strong>.</p>
            <p>Please visit our <a href="https://www.example.com">website</a> to get started.</p>
            <ul>
                <li>Feature 1</li>
                <li>Feature 2</li>
                <li>Feature 3</li>
            </ul>
            <p>Best regards,<br>Team</p>
            <img src="logo.png" alt="Company Logo">
            <hr>
            """;
        const string expectedHtml = """
            <h1>Welcome, Jane!</h1><p>We're excited to have you in our <strong>community</strong>.</p><p>Please visit our <a href="https://www.example.com">website</a> to get started.</p><ul><li>Feature 1</li><li>Feature 2</li><li>Feature 3</li></ul><p>Best regards,<br>Team</p><img src="logo.png" alt="Company Logo"><hr>
            """;
        const string expectedPlain = """
            Welcome, Jane!

            We're excited to have you in our community.

            Please visit our website (https://www.example.com) to get started.

            - Feature 1
            - Feature 2
            - Feature 3

            Best regards,
            Team

            ----------------------------------------
            """;

        CreateTemplate(templateName, templateContent);
        Result result = _emailTemplateEngine.GenerateBodiesFromTemplate(templateName, ("UserName", "Jane"));
        string actualHtml = _emailTemplateEngine.GetHtmlBody();
        string actualPlain = _emailTemplateEngine.GetPlainTextBody();

        PerformAssertions(result, actualHtml, expectedHtml, actualPlain, expectedPlain);
    }

    [Test]
    public void GenerateBodiesFromHtml_ComplexTemplate()
    {
        const string htmlContent = """
            <h1>Welcome, {{UserName}}!</h1>
            <p>We're excited to have you in our <strong>community</strong>.</p>
            <p>Please visit our <a href="https://www.example.com">website</a> to get started.</p>
            <ul>
                <li>Feature 1</li>
                <li>Feature 2</li>
                <li>Feature 3</li>
            </ul>
            <p>Best regards,<br>Team</p>
            <img src="logo.png" alt="Company Logo">
            <hr>
            """;
        const string expectedHtml = """
            <h1>Welcome, Jane!</h1><p>We're excited to have you in our <strong>community</strong>.</p><p>Please visit our <a href="https://www.example.com">website</a> to get started.</p><ul><li>Feature 1</li><li>Feature 2</li><li>Feature 3</li></ul><p>Best regards,<br>Team</p><img src="logo.png" alt="Company Logo"><hr>
            """;
        const string expectedPlain = """
            Welcome, Jane!

            We're excited to have you in our community.

            Please visit our website (https://www.example.com) to get started.

            - Feature 1
            - Feature 2
            - Feature 3

            Best regards,
            Team

            ----------------------------------------
            """;

        Result result = _emailTemplateEngine.GenerateBodiesFromHtml(htmlContent, ("UserName", "Jane"));
        string actualHtml = _emailTemplateEngine.GetHtmlBody();
        string actualPlain = _emailTemplateEngine.GetPlainTextBody();

        PerformAssertions(result, actualHtml, expectedHtml, actualPlain, expectedPlain);
    }
}
