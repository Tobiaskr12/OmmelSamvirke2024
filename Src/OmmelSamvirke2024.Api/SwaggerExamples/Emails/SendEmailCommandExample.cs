using Emails.Domain.Entities;
using Emails.Services.Features.Sending.Commands;
using JetBrains.Annotations;
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
                Body = "This is a test email",
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
