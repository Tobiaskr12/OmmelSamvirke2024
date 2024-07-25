using EmailWrapper.Models;
using FluentResults;

namespace EmailWrapper.Interfaces;

public interface IEmailSender
{
    Task<Result> SendEmail(Email email);
    Task<Result> SendEmails(List<Email> emails);
}
