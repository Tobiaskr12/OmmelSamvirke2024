namespace Contracts.SupportModules.Logging.Util;

/// <summary>
/// Generates a short semi-unique ID.
/// This generator is only used to identify sessions and operations for logs and traces.
/// Assuming (at most) 1 million logs and traces the risk of a collision is ~0.002%.
/// </summary>
public interface IShortIdGenerator
{
    string Generate();
}
