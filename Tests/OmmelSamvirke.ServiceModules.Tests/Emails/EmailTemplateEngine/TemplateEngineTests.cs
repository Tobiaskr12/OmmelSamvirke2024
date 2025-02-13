using FluentResults;
using OmmelSamvirke.Interfaces.Emails;
using OmmelSamvirke.ServiceModules.Emails.EmailTemplateEngine;
using OmmelSamvirke.SupportModules.Logging.Interfaces;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.EmailTemplateEngine;

[TestFixture, Category("UnitTests")]
public class TemplateEngineTests
{
    private string _templatesDirectory;
    private string _partialsDirectory;
    private IEmailTemplateEngine _emailTemplateEngine;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Set up the Templates directory relative to the current directory
        _templatesDirectory = Path.Combine(".", "Emails", "EmailTemplateEngine", "Templates");
        if (!Directory.Exists(_templatesDirectory))
        {
            Directory.CreateDirectory(_templatesDirectory);
        }
        
        // Set up the Partials directory relative to the current directory
        _partialsDirectory = Path.Combine(".", "Emails", "EmailTemplateEngine", "Partials");
        if (!Directory.Exists(_partialsDirectory))
        {
            Directory.CreateDirectory(_partialsDirectory);
        }
        
        var logger = NSubstitute.Substitute.For<ILoggingHandler>();
        _emailTemplateEngine = new TemplateEngine(logger);
    }

    // Clean up the Templates directory after each test.
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // Clean up Templates
        if (Directory.Exists(_templatesDirectory))
        {
            string[] files = Directory.GetFiles(_templatesDirectory, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                // Assumption: test files start with a lower-case character
                if (char.IsLower(fileName[0]))
                {
                    File.Delete(file);
                }
            }

            // Remove now-empty subdirectories
            foreach (string dir in Directory.GetDirectories(_templatesDirectory, "*", SearchOption.AllDirectories))
            {
                if (!Directory.EnumerateFileSystemEntries(dir).Any())
                {
                    Directory.Delete(dir, false);
                }
            }
        }
    
        // Clean up Partials
        if (Directory.Exists(_partialsDirectory))
        {
            foreach (string filePath in Directory.GetFiles(_partialsDirectory, "*", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileName(filePath);
                // Assumption: test partial files start with a lower-case letter
                if (char.IsLower(fileName[0]))
                {
                    File.Delete(filePath);
                }
            }

            // Remove now-empty subdirectories
            foreach (string dir in Directory.GetDirectories(_partialsDirectory, "*", SearchOption.AllDirectories))
            {
                if (!Directory.EnumerateFileSystemEntries(dir).Any())
                {
                    Directory.Delete(dir, false);
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
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, content);
    }
    
    /// <summary>
    /// Helper method to create a partial file with specified content.
    /// </summary>
    /// <param name="fileName">Name of the partial file.</param>
    /// <param name="content">HTML content of the partial.</param>
    private void CreatePartial(string fileName, string content)
    {
        string filePath = Path.Combine(_partialsDirectory, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
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
    
    [Test]
    public void GenerateBodiesFromTemplate_PartialWhitespaceVariations()
    {
        CreatePartial("test.html", "Hello from partial!");
        
        const string templateName = "partial_whitespace.html";
        const string templateContent = """
                                       <p>{{>test}}</p>
                                       <p>{{> test}}</p>
                                       <p>{{>   test}}</p>
                                       <p>{{>test   }}</p>
                                       <p>{{>   test   }}</p>
                                       """;
        CreateTemplate(templateName, templateContent);
        
        string expectedHtml = "<p>Hello from partial!</p><p>Hello from partial!</p><p>Hello from partial!</p><p>Hello from partial!</p><p>Hello from partial!</p>".Trim();
        
        Result result = _emailTemplateEngine.GenerateBodiesFromTemplate(templateName);

        string actualHtml = _emailTemplateEngine.GetHtmlBody().Trim();
        string actualPlain = _emailTemplateEngine.GetPlainTextBody().Trim(); 

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(actualHtml, Is.EqualTo(expectedHtml));
            
            int occurrences = actualPlain.Split("Hello from partial!").Length - 1;
            Assert.That(occurrences, Is.EqualTo(5), "Expected partial text to appear 5 times in plain text.");
        });
    }

    [Test]
    public void GenerateBodiesFromTemplate_OnePartialInTemplate()
    {
        CreatePartial("single.html", "<strong>Single partial content</strong>");
        
        const string templateName = "one_partial.html";
        const string templateContent = "<div>{{>single}}</div>";
        CreateTemplate(templateName, templateContent);
        
        const string expectedHtml = "<div><strong>Single partial content</strong></div>";
        const string expectedPlain = "Single partial content";
        
        Result result = _emailTemplateEngine.GenerateBodiesFromTemplate(templateName);
        string actualHtml = _emailTemplateEngine.GetHtmlBody();
        string actualPlain = _emailTemplateEngine.GetPlainTextBody();

        PerformAssertions(result, actualHtml, expectedHtml, actualPlain, expectedPlain);
    }

    [Test]
    public void GenerateBodiesFromTemplate_MultiplePartialsInTemplate()
    {
        CreatePartial("header.html", "<h1>Header partial</h1>");
        CreatePartial("footer.html", "<footer>Footer partial</footer>");
        
        const string templateName = "multiple_partials.html";
        const string templateContent = """
            <body>
                {{>header}}
                <p>Main content</p>
                {{>footer}}
            </body>
            """;
        CreateTemplate(templateName, templateContent);
        
        string expectedHtml = """
            <body><h1>Header partial</h1><p>Main content</p><footer>Footer partial</footer></body>
            """.Trim();
        
        string expectedPlain = """
            Header partial

            Main content

            Footer partial
            """.Trim();
        
        Result result = _emailTemplateEngine.GenerateBodiesFromTemplate(templateName);
        string actualHtml = _emailTemplateEngine.GetHtmlBody().Trim();
        string actualPlain = _emailTemplateEngine.GetPlainTextBody().Trim();

        PerformAssertions(result, actualHtml, expectedHtml, actualPlain, expectedPlain);
    }

    [Test]
    public void GenerateBodiesFromTemplate_PartialInRootDirectory()
    {
        CreatePartial("rootPartial.html", "Root partial content");
        
        const string templateName = "root_partial_test.html";
        const string templateContent = "<p>{{>rootPartial}}</p>";
        CreateTemplate(templateName, templateContent);

        const string expectedHtml = "<p>Root partial content</p>";
        const string expectedPlain = "Root partial content";
        
        Result result = _emailTemplateEngine.GenerateBodiesFromTemplate(templateName);
        string actualHtml = _emailTemplateEngine.GetHtmlBody();
        string actualPlain = _emailTemplateEngine.GetPlainTextBody();

        PerformAssertions(result, actualHtml, expectedHtml, actualPlain, expectedPlain);
    }

    [Test]
    public void GenerateBodiesFromTemplate_PartialInSubdirectory()
    {
        // Create a partial in subdirectory: "Partials/subdir/test.html"
        CreatePartial(Path.Combine("subdir", "test.html"), "Subdirectory partial content");
        
        const string templateName = "subdir_partial_test.html";
        const string templateContent = "<p>{{> subdir/test}}</p>";
        CreateTemplate(templateName, templateContent);

        const string expectedHtml = "<p>Subdirectory partial content</p>";
        const string expectedPlain = "Subdirectory partial content";
        
        Result result = _emailTemplateEngine.GenerateBodiesFromTemplate(templateName);
        string actualHtml = _emailTemplateEngine.GetHtmlBody();
        string actualPlain = _emailTemplateEngine.GetPlainTextBody();

        PerformAssertions(result, actualHtml, expectedHtml, actualPlain, expectedPlain);
    }

    [Test]
    public void GenerateBodiesFromTemplate_ExtractEmailSubjectFromTemplate()
    {
        const string templateName = "multiple_partials.html";
        const string templateContent = """
                                       <!DOCTYPE html>
                                       <html>
                                           <head>   
                                                <title>Test subject</title>
                                           </head>
                                           <body>
                                               <p>Email content</p>
                                           </body>
                                       </html>
                                       """;
        
        CreateTemplate(templateName, templateContent);
        Result result = _emailTemplateEngine.GenerateBodiesFromTemplate(templateName);
        string subject = _emailTemplateEngine.GetSubject();
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(subject, Is.EqualTo("Test subject"));
        });
    }
    
    [Test]
    public void GenerateBodiesFromTemplate_SingleSubdirectory()
    {
        const string subDir = "subdir";
        const string fileName = "testTemplate.html";
        string relativePath = Path.Combine(subDir, fileName);  // "subdir/testTemplate.html"
        
        const string templateContent = """
                                           <html>
                                           <head><title>Subdir Title</title></head>
                                           <body>
                                               <p>Hello from single-level subdirectory!</p>
                                           </body>
                                           </html>
                                       """;
        CreateTemplate(relativePath, templateContent);
        
        Result result = _emailTemplateEngine.GenerateBodiesFromTemplate(Path.Combine(subDir, fileName));
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(_emailTemplateEngine.GetHtmlBody(), Does.Contain("Hello from single-level subdirectory!"));
            Assert.That(_emailTemplateEngine.GetSubject(), Is.EqualTo("Subdir Title"));
        });
    }

    [Test]
    public void GenerateBodiesFromTemplate_TwoLevelSubdirectory()
    {
        const string subDirLevel1 = "subdir1";
        const string subDirLevel2 = "subdir2";
        const string fileName = "testTemplate.html";
        string path = Path.Combine(subDirLevel1, subDirLevel2, fileName); 
        const string templateContent = """
                                           <html>
                                           <head><title>Two-Level Subdir Title</title></head>
                                           <body>
                                               <p>Hello from two-level subdirectory!</p>
                                           </body>
                                           </html>
                                       """;
        
        CreateTemplate(path, templateContent);
        Result result = _emailTemplateEngine.GenerateBodiesFromTemplate(path);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess);
            Assert.That(_emailTemplateEngine.GetHtmlBody(), Does.Contain("Hello from two-level subdirectory!"));
            Assert.That(_emailTemplateEngine.GetSubject(), Is.EqualTo("Two-Level Subdir Title"));
        });
    }
}
