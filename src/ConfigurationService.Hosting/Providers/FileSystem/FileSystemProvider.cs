using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ConfigurationService.Hosting.Providers.FileSystem
{
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
                throw new ArgumentNullException(nameof(_providerOptions.Path), $"{nameof(_providerOptions.Path)} cannot be NULL or empty.");
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
            _logger.LogInformation("Initializing {Name} provider with options {Options}.", Name, new
            {
                _providerOptions.Path,
                _providerOptions.SearchPattern,
                _providerOptions.IncludeSubdirectories
            });

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

        public byte[] GetFile(string fileName)
        {
            string path = Path.Combine(_providerOptions.Path, fileName);

            if (!File.Exists(path))
            {
                _logger.LogInformation("File does not exit at {path}.", path);
                return null;
            }

            return File.ReadAllBytes(path);
        }

        public string GetHash(string fileName)
        {
            var bytes = GetFile(fileName);

            return Hasher.CreateHash(bytes);
        }

        public IEnumerable<string> ListAllFiles()
        {
            _logger.LogInformation("Listing files at {Path}.", _providerOptions.Path);

            var searchOption = _providerOptions.IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.EnumerateFiles(_providerOptions.Path, _providerOptions.SearchPattern ?? "*", searchOption).ToList();
            files = files.Select(GetRelativePath).ToList();

            _logger.LogInformation("{Count} files found.", files.Count);

            return files;
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("Detected file change at {FullPath}.", e.FullPath);

            var filename = GetRelativePath(e.FullPath);
            _onChange(new[] { filename });
        }

        private string GetRelativePath(string fullPath)
        {
            return Path.GetRelativePath(_providerOptions.Path, fullPath);
        }
    }
}