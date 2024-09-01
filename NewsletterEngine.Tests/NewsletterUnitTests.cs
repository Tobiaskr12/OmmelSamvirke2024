namespace NewsletterEngine.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [TestCase("")]
    public void GivenTitleLengthIsValid_WhenCreatingNewsletter_ReturnsOk(string title)
    {
        Assert.Fail();
    }

    [TestCase("")]
    public void GivenBodyContentLengthIsValid_WhenCreatingNewsletter_ReturnsOk(string body)
    {
        Assert.Fail();
    }
    

}