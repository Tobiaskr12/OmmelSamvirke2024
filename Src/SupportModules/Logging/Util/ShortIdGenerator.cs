using System.Security.Cryptography;
using Contracts.SupportModules.Logging.Util;

namespace SupportModules.Logging.Util;

public class ShortIdGenerator : IShortIdGenerator
{
    public string Generate()
    {
        byte[] buffer = new byte[6];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToBase64String(buffer)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
