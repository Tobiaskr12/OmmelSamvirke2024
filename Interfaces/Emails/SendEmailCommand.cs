using FluentResults;
using MediatR;
using OmmelSamvirke.DomainModules.Emails.Entities;
using OmmelSamvirke.DTOs.Emails;

namespace OmmelSamvirke.Interfaces.Emails;

public record SendEmailCommand(Email Email) : IRequest<Result<EmailSendingStatus>>;
