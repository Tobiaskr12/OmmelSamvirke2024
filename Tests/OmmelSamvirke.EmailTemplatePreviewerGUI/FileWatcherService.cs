using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OmmelSamvirke.EmailTemplatePreviewGUI.Services;

public class FileWatcherService : IHostedService, IDisposable
{
    private readonly ILogger<FileWatcherService> _logger;
    private FileSystemWatcher _watcher;
    private string _currentFilePath;
    private readonly object _lock = new object();

    // Event to notify subscribers about file changes
    public event Action<string> FileChanged;

    public FileWatcherService(ILogger<FileWatcherService> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("FileWatcherService started.");
        // Initialize watcher if needed or wait until a file is set to watch
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets the file to watch. If a watcher is already active, it will be stopped.
    /// </summary>
    /// <param name="filePath">Full path of the file to watch.</param>
    public void SetFileToWatch(string filePath)
    {
        lock (_lock)
        {
            if (_currentFilePath == filePath)
            {
                _logger.LogInformation($"Already watching {filePath}");
                return;
            }

            // Stop existing watcher
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
                _logger.LogInformation($"Stopped watching {_currentFilePath}");
            }

            // Set up new watcher
            if (File.Exists(filePath))
            {
                string directory = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileName(filePath);

                _watcher = new FileSystemWatcher(directory, fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
                };

                _watcher.Changed += OnChanged;
                _watcher.Renamed += OnRenamed;
                _watcher.Deleted += OnDeleted;
                _watcher.Created += OnCreated;
                _watcher.EnableRaisingEvents = true;

                _currentFilePath = filePath;
                _logger.LogInformation($"Started watching {filePath}");

                // Notify subscribers about the new file being watched
                FileChanged?.Invoke($"Started watching {filePath}");
            }
            else
            {
                _logger.LogWarning($"File {filePath} does not exist.");
                FileChanged?.Invoke($"File selection failed: {filePath} does not exist.");
            }
        }
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation($"File {e.FullPath} has been modified.");
        NotifySubscribers($"File Modified: {e.FullPath}");
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        _logger.LogInformation($"File renamed from {e.OldFullPath} to {e.FullPath}.");
        NotifySubscribers($"File Renamed from {e.OldFullPath} to {e.FullPath}");
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation($"File {e.FullPath} has been deleted.");
        NotifySubscribers($"File Deleted: {e.FullPath}");
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation($"File {e.FullPath} has been created.");
        NotifySubscribers($"File Created: {e.FullPath}");
    }

    private void NotifySubscribers(string message)
    {
        // Ensure thread safety by invoking on the main thread if necessary
        FileChanged?.Invoke(message);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
                _logger.LogInformation($"Stopped watching {_currentFilePath}");
            }
        }

        _logger.LogInformation("FileWatcherService stopped.");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}