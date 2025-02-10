namespace OmmelSamvirke.SupportModules.Logging.Interfaces;

public interface ITraceHandler
{
    void Trace(
        string operationType,
        bool isSuccess,
        long executionTimeMs,
        string requestName
    );
}
