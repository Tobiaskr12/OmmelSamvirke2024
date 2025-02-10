using System.Collections.Concurrent;
using OmmelSamvirke.SupportModules.Logging.Interfaces;

namespace OmmelSamvirke.SupportModules.Logging;

/// <summary>
/// Base class for CSV writers that buffers entries and flushes them to a file.
/// </summary>
public abstract class CsvBufferedWriter<T> : IDisposable
{
    private const int MaxBufferSize = 10;
    private readonly TimeSpan _maxTimeBetweenFlush = TimeSpan.FromSeconds(5);

    private readonly ConcurrentQueue<T> _buffer = new();
    private DateTime _lastFlushTime = DateTime.UtcNow;
    private readonly object _flushLock = new();

    private readonly Timer _timer;
    protected readonly string Directory;
    protected readonly ICorrelationContext CorrelationContext;

    protected CsvBufferedWriter(ICorrelationContext correlationContext, ILoggingLocationInfo loggingLocationInfo)
    {
        CorrelationContext = correlationContext;
        Directory = loggingLocationInfo.GetLoggingDirectoryPath();
        // Set up a timer to flush periodically.
        _timer = new Timer(FlushIfNeeded, null, 5000, 5000);
    }

    /// <summary>
    /// Enqueues an entry and flushes if the buffer is full or enough time has passed.
    /// </summary>
    protected void AddEntry(T entry)
    {
        lock (_flushLock)
        {
            _buffer.Enqueue(entry);

            if (_buffer.Count >= MaxBufferSize || DateTime.UtcNow - _lastFlushTime > _maxTimeBetweenFlush)
            {
                FlushBuffer();
            }
        }
    }

    private void FlushIfNeeded(object? state)
    {
        lock (_flushLock)
        {
            if (DateTime.UtcNow - _lastFlushTime > _maxTimeBetweenFlush && !_buffer.IsEmpty)
            {
                FlushBuffer();
            }
        }
    }

    /// <summary>
    /// Flushes the buffer to disk.
    /// </summary>
    private void FlushBuffer()
    {
        var list = new List<T>();
        lock (_flushLock)
        {
            while (_buffer.TryDequeue(out var entry))
            {
                list.Add(entry);
            }
        }
        if (list.Count == 0)
        {
            return;
        }
        _lastFlushTime = DateTime.UtcNow;

        string fileName = GetFileName(_lastFlushTime);
        bool fileExists = File.Exists(fileName);

        using var stream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
        using var writer = new StreamWriter(stream);

        if (!fileExists)
        {
            writer.WriteLine(GetHeaderLine());
        }

        foreach (T entry in list)
        {
            writer.WriteLine(FormatEntry(entry));
        }
    }

    /// <summary>
    /// Gets the full file name (including path) for the given timestamp.
    /// </summary>
    protected abstract string GetFileName(DateTime now);

    /// <summary>
    /// Gets the CSV header line.
    /// </summary>
    protected abstract string GetHeaderLine();

    /// <summary>
    /// Formats the entry as a CSV line.
    /// </summary>
    protected abstract string FormatEntry(T entry);

    /// <summary>
    /// Escapes a string for CSV.
    /// </summary>
    protected static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }
        if (value.Contains(',') || value.Contains('"'))
        {
            value = $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    public void Dispose()
    {
        _timer.Dispose();
        lock (_flushLock)
        {
            FlushBuffer();
        }
        GC.SuppressFinalize(this);
    }
}
