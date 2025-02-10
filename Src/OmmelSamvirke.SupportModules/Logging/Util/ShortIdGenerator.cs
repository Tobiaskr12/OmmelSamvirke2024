using System.Security.Cryptography;

namespace OmmelSamvirke.SupportModules.Logging.Util;

public static class ShortIdGenerator
{
    /// <summary>
    /// Generates a short semi-unique ID.
    /// This generator is only used to identify sessions and operations for logs and traces.
    /// Assuming (at most) 1 million logs and traces the risk of a collision is ~0.002%.
    /// </summary>
    public static string Generate()
    {
        byte[] buffer = new byte[6];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToBase64String(buffer)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
