namespace NewsletterEngine.Tests;

public class NewsletterAttachmentUnitTests
{
    [SetUp]
    public void Setup()
    {
    }

    [TestCase("")]
    public void GivenTitleLengthIsValid_WhenCreatingNewsletterAttachment_ReturnsOk(string title)
    {
        Assert.Fail();
    }

    [TestCase("")]
    public void GivenPdfContentIsFromValidUrl_WhenCreatingNewsletterAttachment_ReturnsOk(string url)
    {
        Assert.Fail();
    }

    [TestCase(new byte[0])]
    public void GivenPdfContentIsFromByteArray_WhenCreatingNewsletterAttachment_ReturnsOk(byte[] pdfContent)
    {
        Assert.Fail();
    }
}