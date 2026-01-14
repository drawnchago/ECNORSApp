using Microsoft.Maui.Storage;
using System.Diagnostics;

namespace ECNORSApp.Services;

public interface IFileLoggerService
{
    Task InfoAsync(string message);
    Task WarnAsync(string message);
    Task ErrorAsync(string message, Exception? ex = null);
    Task<string> GetLogFilePathAsync();
    Task ExportToDownloadsAsync();

}

public sealed class FileLoggerService : IFileLoggerService
{
    private readonly string _logPath;

    public FileLoggerService()
    {
        // ✔ Ruta válida en Android / Windows / iOS
        var baseDir = FileSystem.AppDataDirectory;

        var dir = Path.Combine(baseDir, "ECNORSLogs");
        Directory.CreateDirectory(dir);

        _logPath = Path.Combine(dir, "ecnors-log.txt");

        if (!File.Exists(_logPath))
        {
            File.WriteAllText(_logPath, $"Log creado: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");
        }

        Debug.WriteLine($"LOG PATH => {_logPath}");
    }

    public Task InfoAsync(string message) => WriteAsync("INFO", message);
    public Task WarnAsync(string message) => WriteAsync("WARN", message);
    public Task ErrorAsync(string message, Exception? ex = null) => WriteAsync("ERROR", message, ex);

    private async Task WriteAsync(string level, string message, Exception? ex = null)
    {
        try
        {
            var line =
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}" +
                (ex != null ? $"\n{ex}\n" : "\n");

            await File.AppendAllTextAsync(_logPath, line);
        }
        catch (Exception e)
        {
            Debug.WriteLine("FILE LOGGER ERROR: " + e);
        }
    }

    public Task<string> GetLogFilePathAsync() => Task.FromResult(_logPath);

    public async Task ExportToDownloadsAsync()
    {
#if ANDROID
        var downloads = Android.OS.Environment
            .GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)
            .AbsolutePath;

        var target = Path.Combine(downloads, "ecnors-log.txt");
        File.Copy(_logPath, target, true);
#endif
    }

}
