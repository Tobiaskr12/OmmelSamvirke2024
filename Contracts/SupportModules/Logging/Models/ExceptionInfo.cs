namespace Contracts.SupportModules.Logging.Models;

public class ExceptionInfo
{
    public required string Type { get; set; }
    public required string Message { get; set; }
    public required string StackTrace { get; set; }
    public required ExceptionInfo? InnerException { get; set; }
}
