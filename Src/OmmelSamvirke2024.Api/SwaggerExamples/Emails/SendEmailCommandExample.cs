using JetBrains.Annotations;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.Interfaces.Emails;
using Swashbuckle.AspNetCore.Filters;

namespace OmmelSamvirke2024.Api.SwaggerExamples.Emails;

[UsedImplicitly]
public class SendEmailCommandExample : IExamplesProvider<SendEmailCommand>
{
    public SendEmailCommand GetExamples()
    {
        return new SendEmailCommand(
            new Email
            {
                Subject = "Test email subject",
                HtmlBody = "This is a test email",
                PlainTextBody = "This is a test email",
                Attachments = [],
                Recipients =
                [
                    new Recipient
                    {
                        EmailAddress = "test@example.com"
                    }
                ],
                SenderEmailAddress = "test@ommelsamvirke.com"
            }
        );
    }
}
