using Contracts.ServiceModules.Emails.DTOs;
using FluentResults;
using MediatR;
using OmmelSamvirke.DomainModules.Emails.Entities;

namespace Contracts.ServiceModules.Emails;

public record SendEmailCommand(Email Email) : IRequest<Result<EmailSendingStatus>>;
