using FluentResults;
using MediatR;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DTOs.Emails;

namespace ServiceModulesCircularDependencyBreaker.Emails;

/// <summary>
/// This was copied from the ServiceModule to allow the Logging implementation to send emails
/// </summary>
public record SendEmailCommand(Email Email) : IRequest<Result<EmailSendingStatus>>;
