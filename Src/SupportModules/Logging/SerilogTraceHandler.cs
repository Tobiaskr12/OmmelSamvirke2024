using Contracts.SupportModules.Logging;
using Serilog;
using Serilog.Context;

namespace SupportModules.Logging;

public class SerilogTraceHandler : ITraceHandler
{
    private readonly ICorrelationContext _correlationContext;

    public SerilogTraceHandler(ICorrelationContext correlationContext)
    {
        _correlationContext = correlationContext;
    }

    public void Trace(string operationType, bool isSuccess, long executionTimeMs, string requestName)
    {
        using (LogContext.PushProperty("SessionId", _correlationContext.SessionId))
        using (LogContext.PushProperty("OperationId", _correlationContext.OperationId))
        using (LogContext.PushProperty("OperationType", operationType))
        using (LogContext.PushProperty("IsSuccess", isSuccess))
        using (LogContext.PushProperty("ExecutionTimeMs", executionTimeMs))
        using (LogContext.PushProperty("RequestName", requestName))
        {
            Log.Information(
                "Trace: {RequestName} ({OperationType}) completed. Success: {IsSuccess}, Duration: {ExecutionTimeMs}ms",
                requestName,
                operationType,
                isSuccess,
                executionTimeMs
            );
        }
    }
}
