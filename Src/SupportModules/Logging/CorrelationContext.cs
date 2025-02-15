using Contracts.SupportModules.Logging;
using Contracts.SupportModules.Logging.Util;

namespace SupportModules.Logging;

public class CorrelationContext : ICorrelationContext
{
    public CorrelationContext(IShortIdGenerator shortIdGenerator)
    {
        SessionId = shortIdGenerator.Generate();
    }

    public string SessionId { get; }
    public string OperationId { get; set; } = "N/A";
}
