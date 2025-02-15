using Contracts.SupportModules.Logging;
using Contracts.SupportModules.Logging.Models;
using SupportModules.Logging.Interfaces;

namespace SupportModules.Logging;

public class CsvTraceWriter : CsvBufferedWriter<TraceEntry>, ITraceHandler
{
    public CsvTraceWriter(ICorrelationContext correlationContext, ILoggingLocationInfo loggingLocationInfo)
        : base(correlationContext, loggingLocationInfo) { }

    public void Trace(string operationType, bool isSuccess, long executionTimeMs, string requestName)
    {
        var entry = new TraceEntry
        {
            OperationType = operationType,
            IsSuccess = isSuccess,
            ExecutionTimeMs = executionTimeMs,
            RequestName = requestName,
            SessionId = CorrelationContext.SessionId,
            OperationId = CorrelationContext.OperationId
        };

        AddEntry(entry);
    }

    protected override string GetFileName(DateTime now) =>
        Path.Combine(Directory, $"{now:yyyy-MM-dd}-Traces.csv");

    protected override string GetHeaderLine() =>
        "Timestamp,OperationType,IsSuccess,ExecutionTimeMs,RequestName,SessionId,OperationId";

    protected override string FormatEntry(TraceEntry e)
    {
        return string.Join(",",
            e.Timestamp.ToString("o"),
            EscapeCsv(e.OperationType),
            e.IsSuccess,
            e.ExecutionTimeMs,
            EscapeCsv(e.RequestName),
            EscapeCsv(e.SessionId),
            EscapeCsv(e.OperationId)
        );
    }
}
