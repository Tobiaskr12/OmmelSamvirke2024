using OmmelSamvirke.SupportModules.Logging.Interfaces;
using OmmelSamvirke.SupportModules.Logging.Util;

namespace OmmelSamvirke.SupportModules.Logging;

public class CorrelationContext : ICorrelationContext
{
    public string SessionId { get; } = ShortIdGenerator.Generate();
    public string OperationId { get; set; } = "N/A";
}
