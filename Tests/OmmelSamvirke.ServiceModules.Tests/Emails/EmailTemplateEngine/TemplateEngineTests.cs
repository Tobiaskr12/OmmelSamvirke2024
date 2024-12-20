using OmmelSamvirke.ServiceModules.Emails.EmailTemplateEngine;

namespace OmmelSamvirke.ServiceModules.Tests.Emails.EmailTemplateEngine;

[TestFixture]
public class TemplateEngineTests
{
    private readonly string _templatesDirectory;

    public TemplateEngineTests()
    {
        // Set up the Templates directory relative to the current directory
        _templatesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        if (!Directory.Exists(_templatesDirectory))
        {
            Directory.CreateDirectory(_templatesDirectory);
        }
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // Clean up the Templates directory after each test
        if (Directory.Exists(_templatesDirectory))
        {
            Directory.Delete(_templatesDirectory, true);
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

    #region GenerateHtmlBody Tests

    [Test]
    public void GenerateHtmlBody_ReplacesSinglePlaceholder()
    {
        const string templateName = "single_placeholder.html";
        const string templateContent = "<p>Hello, {{UserName}}!</p>";
        const string expectedHtml = "<p>Hello, John Doe!</p>";
        CreateTemplate(templateName, templateContent);

        string result = TemplateEngine.GenerateHtmlBody(templateName, ("UserName", "John Doe"));

        Assert.That(result, Is.EqualTo(expectedHtml));
    }

    [Test]
    public void GenerateHtmlBody_ReplacesMultiplePlaceholders()
    {
        const string templateName = "multiple_placeholders.html";
        const string templateContent = "<p>Hello, {{FirstName}} {{LastName}}!</p><p>Your email is {{Email}}.</p>";
        const string expectedHtml = "<p>Hello, Jane Doe!</p><p>Your email is jane.doe@example.com.</p>";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GenerateHtmlBody(templateName,
            ("FirstName", "Jane"),
            ("LastName", "Doe"),
            ("Email", "jane.doe@example.com"));
        
        Assert.That(result, Is.EqualTo(expectedHtml));
    }

    [Test]
    public void GenerateHtmlBody_NoPlaceholders()
    {
        const string templateName = "no_placeholders.html";
        const string templateContent = "<p>Welcome to our service!</p>";
        const string expectedHtml = "<p>Welcome to our service!</p>";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GenerateHtmlBody(templateName);
        
        Assert.That(result, Is.EqualTo(expectedHtml));
    }

    [Test]
    public void GenerateHtmlBody_PlaceholderNotFound()
    {
        const string templateName = "missing_placeholder.html";
        const string templateContent = "<p>Hello, {{UserName}}!</p>";
        const string expectedHtml = "<p>Hello, {{UserName}}!</p>";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GenerateHtmlBody(templateName);

        Assert.That(result, Is.EqualTo(expectedHtml));
    }

    [Test]
    public void GenerateHtmlBody_MultiplePlaceholdersSameKey()
    {
        const string templateName = "multiple_same_placeholders.html";
        const string templateContent = "<p>{{Greeting}}, {{UserName}}!</p><p>{{Greeting}} again!</p>";
        const string expectedHtml = "<p>Hello, John Doe!</p><p>Hello again!</p>";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GenerateHtmlBody(templateName, ("Greeting", "Hello"), ("UserName", "John Doe"));
        
        Assert.That(result, Is.EqualTo(expectedHtml));
    }

    [Test]
    public void GenerateHtmlBody_EmptyTemplate()
    {
        const string templateName = "empty_template.html";
        const string templateContent = "";
        const string expectedHtml = "";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GenerateHtmlBody(templateName);
        
        Assert.That(result, Is.EqualTo(expectedHtml));
    }

    #endregion

    #region GeneratePlainTextBody Tests

    [Test]
    public void GeneratePlainTextBody_SimpleParagraph()
    {
        const string templateName = "simple_paragraph.html";
        const string templateContent = "<p>Hello, {{UserName}}!</p>";
        const string expectedPlainText = "Hello, John Doe!";
        CreateTemplate(templateName, templateContent);

        string result = TemplateEngine.GeneratePlainTextBody(templateName, ("UserName", "John Doe"));

        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_Link()
    {
        const string templateName = "link.html";
        const string templateContent = "<p>Visit our <a href=\"https://www.example.com\">website</a> for more info.</p>";
        const string expectedPlainText = "Visit our \"website\": \"https://www.example.com\" for more info.";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName);
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_ImageIsRemoved()
    {
        const string templateName = "image_without_alt.html";
        const string templateContent = "<p>Here is our logo:</p><img src=\"logo.png\">";
        const string expectedPlainText = "Here is our logo:";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName);
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_LineBreak()
    {
        const string templateName = "line_break.html";
        const string templateContent = "<p>First line.<br>Second line.</p>";
        const string expectedPlainText = "First line.\r\nSecond line.";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName);
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_UnorderedList()
    {
        const string templateName = "unordered_list.html";
        const string templateContent = "<ul><li>Item 1</li><li>Item 2</li><li>Item 3</li></ul>";
        const string expectedPlainText = "- Item 1\r\n- Item 2\r\n- Item 3";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName);
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_OrderedList()
    {
        const string templateName = "ordered_list.html";
        const string templateContent = "<ol><li>First</li><li>Second</li><li>Third</li></ol>";
        const string expectedPlainText = "- First\r\n- Second\r\n- Third";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName);
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_Headlines()
    {
        const string templateName = "headlines.html";
        const string templateContent = "<h1>Title</h1><h2>Subtitle</h2><h3>Section</h3>";
        const string expectedPlainText = "Title\r\n\r\nSubtitle\r\n\r\nSection";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName);
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_BoldAndItalic()
    {
        const string templateName = "bold_italic.html";
        const string templateContent = "<p>This is <strong>bold</strong> and this is <em>italic</em>.</p>";
        const string expectedPlainText = "This is bold and this is italic.";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName);
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_NestedElements()
    {
        const string templateName = "nested_elements.html";
        const string templateContent = "<p><strong>Hello, <em>{{UserName}}</em>!</strong></p>";
        const string expectedPlainText = "Hello, Jane!";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName, ("UserName", "Jane"));

        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_Blockquote()
    {
        const string templateName = "blockquote.html";
        const string templateContent = "<p>Quote:</p><blockquote>\"This is a quoted text.\"</blockquote>";
        const string expectedPlainText = "Quote:\r\n\"This is a quoted text.\"";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName);
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_HorizontalRule()
    {
        const string templateName = "horizontal_rule.html";
        const string templateContent = "<p>Above the line.</p><hr><p>Below the line.</p>";
        const string expectedPlainText = "Above the line.\r\n\r\n----------------------------------------\r\n\r\nBelow the line.";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName);
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_CodeBlock()
    {
        const string templateName = "code_block.html";
        const string templateContent = "<p>Here is some code:</p><pre>public static void Main() { Console.WriteLine(\"Hello\"); }</pre>";
        const string expectedPlainText = "Here is some code:\r\npublic static void Main() { Console.WriteLine(\"Hello\"); }";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName);
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_InlineCode()
    {
        const string templateName = "inline_code.html";
        const string templateContent = "<p>Use the <code>Console.WriteLine</code> method.</p>";
        const string expectedPlainText = "Use the Console.WriteLine method.";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName);
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_ComplexTemplate()
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
        const string expectedPlainText = "Welcome, Jane!\r\n\r\nWe're excited to have you in our community.\r\n\r\nPlease visit our \"website\": \"https://www.example.com\" to get started.\r\n\r\n- Feature 1\r\n- Feature 2\r\n- Feature 3\r\n\r\nBest regards,\r\nTeam\r\n\r\n----------------------------------------";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName, ("UserName", "Jane"));
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_MissingTemplateFile()
    {
        const string templateName = "nonexistent.html";
        
        Assert.That(() => TemplateEngine.GeneratePlainTextBody(templateName), Throws.TypeOf<FileNotFoundException>());
    }

    [Test]
    public void GeneratePlainTextBody_PlaceholderAdjacentToTags()
    {
        const string templateName = "placeholder_adjacent.html";
        const string templateContent = "<p>Hello,{{UserName}}!</p><p>Your email:{{Email}}</p>";
        const string expectedPlainText = "Hello,Jane!\r\n\r\nYour email:jane.doe@example.com";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName,
            ("UserName", "Jane"),
            ("Email", "jane.doe@example.com"));
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_MultipleSamePlaceholder()
    {
        const string templateName = "multiple_same_placeholder.html";
        const string templateContent = "<p>{{Greeting}}, {{UserName}}!</p><p>{{Greeting}} again!</p>";
        const string expectedPlainText = "Hello, Jane!\r\n\r\nHello again!";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName,
            ("Greeting", "Hello"),
            ("UserName", "Jane"));
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_EmptyTemplate()
    {
        const string templateName = "empty_plain_text.html";
        const string templateContent = "";
        const string expectedPlainText = "";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName);

        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_WhitespaceNormalization()
    {
        const string templateName = "whitespace.html";
        const string templateContent = "<p>   Hello,   {{UserName}}!   </p>";
        const string expectedPlainText = "Hello, Jane!";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName, ("UserName", "Jane"));
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_MalformedHtml_SelfClosingTags()
    {
        const string templateName = "malformed_self_closing.html";
        const string templateContent = "<p>Hello, {{UserName}}!<br><br>Welcome!</p>";
        const string expectedPlainText = "Hello, Jane!\r\n\r\nWelcome!";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName, ("UserName", "Jane"));
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_HtmlEntities()
    {
        const string templateName = "html_entities.html";
        const string templateContent = "<p>Symbols: &amp; &lt; &gt; &quot; &#39;</p>";
        const string expectedPlainText = "Symbols: & < > \" '";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName);

        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_TemplateWithNoHtml()
    {
        const string templateName = "no_html.html";
        const string templateContent = "This is a plain text template with {{Placeholder}}.";
        const string expectedPlainText = "This is a plain text template with Value.";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName, ("Placeholder", "Value"));
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_TemplateWithScriptAndStyleTags()
    {
        const string templateName = "script_style.html";
        const string templateContent = @"
            <html>
                <head>
                    <style>
                        body { font-family: Arial; }
                    </style>
                    <script>
                        console.log('Hello');
                    </script>
                </head>
                <body>
                    <p>Hello, {{UserName}}!</p>
                </body>
            </html>
        ";
        const string expectedPlainText = "Hello, John!";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName, ("UserName", "John"));
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_TemplateWithCustomTags()
    {
        const string templateName = "custom_tags.html";
        const string templateContent = "<custom>Custom content: {{Content}}</custom>";
        const string expectedPlainText = "Custom content: Sample Content";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName, ("Content", "Sample Content"));
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_TemplateWithUnclosedTags()
    {
        const string templateName = "unclosed_tags.html";
        const string templateContent = "<p>Paragraph without closing tag<p>Another paragraph.</p>";
        const string expectedPlainText = "Paragraph without closing tag\r\n\r\nAnother paragraph.";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName);
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_TemplateWithMultipleParagraphs()
    {
        const string templateName = "multiple_paragraphs.html";
        const string templateContent = "<p>First paragraph.</p><p>Second paragraph.</p><p>Third paragraph.</p>";
        const string expectedPlainText = "First paragraph.\r\n\r\nSecond paragraph.\r\n\r\nThird paragraph.";
        CreateTemplate(templateName, templateContent);

        string result = TemplateEngine.GeneratePlainTextBody(templateName);

        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_TemplateWithMultipleHeadlines()
    {
        const string templateName = "multiple_headlines.html";
        const string templateContent = "<h1>Main Title</h1><h2>Sub Title</h2><h3>Section</h3>";
        const string expectedPlainText = "Main Title\r\n\r\nSub Title\r\n\r\nSection";
        CreateTemplate(templateName, templateContent);

        string result = TemplateEngine.GeneratePlainTextBody(templateName);
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_TemplateWithCodeBlock()
    {
        const string templateName = "code_block.html";
        const string templateContent = "<p>Here is some code:</p><pre>public static void Main() { Console.WriteLine(\"Hello\"); }</pre>";
        const string expectedPlainText = "Here is some code:\r\npublic static void Main() { Console.WriteLine(\"Hello\"); }";
        CreateTemplate(templateName, templateContent);

        string result = TemplateEngine.GeneratePlainTextBody(templateName);

        Assert.That(result, Is.EqualTo(expectedPlainText));
    }

    [Test]
    public void GeneratePlainTextBody_TemplateWithInlineCode()
    {
        const string templateName = "inline_code.html";
        const string templateContent = "<p>Use the <code>Console.WriteLine</code> method.</p>";
        const string expectedPlainText = "Use the Console.WriteLine method.";
        CreateTemplate(templateName, templateContent);

        string result = TemplateEngine.GeneratePlainTextBody(templateName);
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }
    
    [Test]
    public void GeneratePlainTextBody_TemplateWithMultipleLineBreaks()
    {
        const string templateName = "multiple_line_breaks.html";
        const string templateContent = "<p>Line 1<br><br>Line 2<br>Line 3</p>";
        const string expectedPlainText = "Line 1\r\n\r\nLine 2\r\nLine 3";
        CreateTemplate(templateName, templateContent);
        
        string result = TemplateEngine.GeneratePlainTextBody(templateName);
        
        Assert.That(result, Is.EqualTo(expectedPlainText));
    }
    #endregion
}

