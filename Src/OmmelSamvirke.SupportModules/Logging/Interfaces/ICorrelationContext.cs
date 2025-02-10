namespace OmmelSamvirke.SupportModules.Logging.Interfaces;

public interface ICorrelationContext
{
    string SessionId { get; }
    string OperationId { get; set; }
}
