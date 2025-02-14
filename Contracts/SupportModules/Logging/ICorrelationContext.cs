namespace Contracts.SupportModules.Logging;

public interface ICorrelationContext
{
    string SessionId { get; }
    string OperationId { get; set; }
}
