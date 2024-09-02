using System.Text;

namespace OmmelSamvirke.ErrorHandling.Models;

public class Error
{
    public readonly string Message;
    public readonly int StatusCode;
    private readonly string? _stackTrace;
    
    public Error(string message, int statusCode, string? stackTrace = null)
    {
        Message = message;
        StatusCode = statusCode;
        _stackTrace = stackTrace;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Error Message: {Message}");
        sb.AppendLine($"Status Code: {StatusCode}");
        if (_stackTrace is null)
        {
            sb.AppendLine("Stack Trace: None");
        }
        else
        {
            sb.AppendLine($"Stack Trace: {_stackTrace}");
        }

        return sb.ToString();
    }
}
