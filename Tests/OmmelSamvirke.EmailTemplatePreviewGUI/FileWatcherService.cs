using Microsoft.AspNetCore.SignalR;

namespace OmmelSamvirke.EmailTemplatePreviewGUI;

public class FileWatcherService : IHostedService, IDisposable
{
    private readonly ILogger<FileWatcherService> _logger;
    private readonly IHubContext<FileChangeHub> _hubContext;
    private FileSystemWatcher? _watcher;
    private string? _currentFilePath;
    private readonly object _lock = new();

    public FileWatcherService(ILogger<FileWatcherService> logger, IHubContext<FileChangeHub> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _logger.LogInformation("FileWatcherService started.");
        }

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
                string directory = Path.GetDirectoryName(filePath) ?? throw new Exception("Cannot watch file, because the file path is invalid");
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

                // Notify clients about the new file being watched
                _hubContext.Clients.All.SendAsync("FileSelected", _currentFilePath);
            }
            else
            {
                _logger.LogWarning($"File {filePath} does not exist.");
                _hubContext.Clients.All.SendAsync("FileSelectionFailed", $"File {filePath} does not exist.");
            }
        }
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation($"File {e.FullPath} has been modified.");
        NotifyClients($"{e.FullPath}");
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        _logger.LogInformation($"File renamed from {e.OldFullPath} to {e.FullPath}.");
        NotifyClients($"File renamed from {e.OldFullPath} to {e.FullPath}.");
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation($"File {e.FullPath} has been deleted.");
        NotifyClients($"File {e.FullPath} has been deleted.");
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation($"File {e.FullPath} has been created.");
        NotifyClients($"File {e.FullPath} has been created.");
    }

    private void NotifyClients(string message)
    {
        _hubContext.Clients.All.SendAsync("FileChanged", message);
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
        GC.SuppressFinalize(this);
        _watcher?.Dispose();
    }
}
