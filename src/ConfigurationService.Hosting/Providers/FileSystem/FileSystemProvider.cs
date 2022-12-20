using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ConfigurationService.Hosting.Providers.FileSystem;

public class FileSystemProvider : IProvider
{
    private readonly ILogger<FileSystemProvider> _logger;

    private readonly FileSystemProviderOptions _providerOptions;
    private FileSystemWatcher _fileSystemWatcher;
    private Func<IEnumerable<string>, Task> _onChange;

    public string Name => "File System";

    public FileSystemProvider(ILogger<FileSystemProvider> logger, FileSystemProviderOptions providerOptions)
    {
        _logger = logger;
        _providerOptions = providerOptions;

        if (string.IsNullOrWhiteSpace(_providerOptions.Path))
        {
            throw new ProviderOptionNullException(nameof(_providerOptions.Path));
        }
    }

    public Task Watch(Func<IEnumerable<string>, Task> onChange, CancellationToken cancellationToken = default)
    {
        _onChange = onChange;
        _fileSystemWatcher.EnableRaisingEvents = true;
        return Task.CompletedTask;
    }

    public void Initialize()
    {
        _logger.LogInformation("Initializing {Name} provider with options {@Options}", Name, new
        {
            _providerOptions.Path,
            _providerOptions.SearchPattern,
            _providerOptions.IncludeSubdirectories
        });

        if (_providerOptions.Username != null && _providerOptions.Password != null)
        {
            var credentials = new NetworkCredential(_providerOptions.Username, _providerOptions.Password, _providerOptions.Domain);
            var uri = new Uri(_providerOptions.Path);
            _ = new CredentialCache
            {
                {new Uri($"{uri.Scheme}://{uri.Host}"), "Basic", credentials}
            };
        }

        _fileSystemWatcher = new FileSystemWatcher
        {
            Path = _providerOptions.Path,
            Filter = _providerOptions.SearchPattern,
            IncludeSubdirectories = _providerOptions.IncludeSubdirectories,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
        };

        _fileSystemWatcher.Created += FileSystemWatcher_Changed;
        _fileSystemWatcher.Changed += FileSystemWatcher_Changed;
    }

    public async Task<byte[]> GetConfiguration(string name)
    {
        string path = Path.Combine(_providerOptions.Path, name);

        if (!File.Exists(path))
        {
            _logger.LogInformation("File does not exist at {Path}", path);
            return null;
        }

        return await File.ReadAllBytesAsync(path);
    }

    public async Task<string> GetHash(string name)
    {
        var bytes = await GetConfiguration(name);

        return Hasher.CreateHash(bytes);
    }

    public Task<IEnumerable<string>> ListPaths()
    {
        _logger.LogInformation("Listing files at {Path}", _providerOptions.Path);

        var searchOption = _providerOptions.IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = Directory.EnumerateFiles(_providerOptions.Path, _providerOptions.SearchPattern ?? "*", searchOption).ToList();
        files = files.Select(GetRelativePath).ToList();

        _logger.LogInformation("{Count} files found", files.Count);

        return Task.FromResult<IEnumerable<string>>(files);
    }

    private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("Detected file change at {FullPath}", e.FullPath);

        var filename = GetRelativePath(e.FullPath);
        _onChange(new[] { filename });
    }

    private string GetRelativePath(string fullPath)
    {
        return Path.GetRelativePath(_providerOptions.Path, fullPath);
    }
}