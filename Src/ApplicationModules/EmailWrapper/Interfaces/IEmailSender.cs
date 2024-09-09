using EmailWrapper.DTOs;
using FluentResults;

namespace EmailWrapper.Interfaces;

public interface IEmailSender
{
    Task<Result> SendEmail(EmailDto email);
    Task<Result> SendEmails(List<EmailDto> emails);
}
