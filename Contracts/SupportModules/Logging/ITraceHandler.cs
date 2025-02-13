namespace Contracts.SupportModules.Logging;

public interface ITraceHandler
{
    void Trace(
        string operationType,
        bool isSuccess,
        long executionTimeMs,
        string requestName
    );
}
