using System.Reflection;
using OmmelSamvirke.SupportModules.Logging.Interfaces;
using OmmelSamvirke.SupportModules.SecretsManager;

namespace OmmelSamvirke.SupportModules.Logging;

public class LoggingLocationInfo : ILoggingLocationInfo
{
    private readonly ExecutionEnvironment _executionEnvironment;

    public LoggingLocationInfo(ExecutionEnvironment executionEnvironment)
    {
        _executionEnvironment = executionEnvironment;
    }
    
    public string GetLoggingDirectoryPath()
    {
        string path = Path.Combine(_executionEnvironment == ExecutionEnvironment.Production 
            ? Path.Combine(Assembly.GetExecutingAssembly().Location, "Logs") 
            : Path.GetTempPath(), "OmmelSamvirkeLogs");
        
        Directory.CreateDirectory(path);

        return path;
    }
}
