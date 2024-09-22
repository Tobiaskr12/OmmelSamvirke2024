// using EmailWrapper.Models;
// using EmailWrapper.Util;
//
// namespace EmailWrapper.Tests;
//
// public class RecipientValidationUnitTests
// {
//     private List<Recipient> _validRecipients;
//     private List<Recipient> _invalidRecipients;
//     private List<Recipient> _allRecipients;
//     
//     [SetUp]
//     public void Setup()
//     {
//         _validRecipients = Recipient.Create([
//             "email@example.com", "firstname.lastname@example.com", "email@subdomain.example.com",
//             "firstname+lastname@example.com", "email@123.123.123.123", "email@[123.123.123.123]",
//             "\"email\"@example.com", "1234567890@example.com", "email@example-one.com", "_______@example.com",
//             "email@example.name", "email@example.museum", "email@example.co.jp", "firstname-lastname@example.com",
//         ]);
//
//         _invalidRecipients = Recipient.Create([
//             "plainaddress", "#@%^%#$@#$@#.com", "@example.com", "Joe Smith <email@example.com>",
//             "email@example@example.com", "email@example.com (Joe Smith)", "email@example",
//             "email@-example.com", "email@example..com", "this\\ is\"really\"not\\allowed@example.com"
//         ]);
//
//         _allRecipients = [];
//         _allRecipients.AddRange(_validRecipients);
//         _allRecipients.AddRange(_invalidRecipients);
//     }
//     
//     [Test]
//     public void GivenListOfValidEmails_WhenCheckingTheirValidity_EveryEmailShouldBeAccepted()
//     {
//         var validCount = 0;
//         
//         foreach (Recipient validRecipient in _validRecipients)
//         {
//             if (RecipientValidator.Validate(validRecipient))
//             {
//                 validCount++;
//             }
//             else
//             {
//                 Console.WriteLine($"Valid recipient marked as invalid: {validRecipient.Email}");
//             }
//         }
//         
//         Assert.That(validCount, Is.EqualTo(_validRecipients.Count));
//     }
//     
//     [Test]
//     public void GivenListOfInvalidEmails_WhenCheckingTheirValidity_EveryEmailShouldBeRejected()
//     {
//         var invalidCount = 0;
//         
//         foreach (Recipient invalidRecipient in _invalidRecipients)
//         {
//             if (!RecipientValidator.Validate(invalidRecipient))
//             {
//                 invalidCount++;
//             }
//             else
//             {
//                 Console.WriteLine($"Invalid recipient marked as valid: {invalidRecipient.Email}");
//             }
//         }
//         
//         Assert.That(invalidCount, Is.EqualTo(_invalidRecipients.Count));
//     }
//     
//     [Test]
//     public void GivenListOfEmails_WhenFilteringRecipients_ReturnListOfInvalidRecipients()
//     {
//         List<Recipient> validRecipients = RecipientValidator.GetValidRecipients(_allRecipients);
//         
//         Assert.That(validRecipients, Is.EqualTo(_validRecipients));
//     }
//     
//     [Test]
//     public void GivenListOfEmails_WhenFilteringRecipients_ReturnListOfValidRecipients()
//     {
//         List<Recipient> invalidRecipients = RecipientValidator.GetInvalidRecipients(_invalidRecipients);
//         
//         Assert.That(invalidRecipients, Is.EqualTo(_invalidRecipients));
//     }
// }
