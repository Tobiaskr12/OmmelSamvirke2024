using System.Reflection;
using AutoFixture.Kernel;
using DomainModules.Emails.Entities;
using DomainModules.Newsletters.Entities;

namespace ServiceModules.Tests.Config.Entities.Emails;

public class RecipientSpecimenBuilder : IEntitySpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo propertyInfo && propertyInfo.DeclaringType == typeof(Recipient))
        {
            switch (propertyInfo.Name)
            {
                case nameof(Recipient.EmailAddress):
                    return $"{Guid.NewGuid()}@example.com";
                case nameof(Recipient.NewsletterSubscriptionConfirmations):
                    return new List<NewsletterSubscriptionConfirmation>();
                case nameof(Recipient.NewsletterUnsubscribeConfirmations):
                    return new List<NewsletterUnsubscribeConfirmation>();
            }
        }
        return new NoSpecimen();
    }
}
