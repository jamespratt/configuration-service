using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ConfigurationService.Hosting;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.Extensions.Logging;

namespace ConfigurationService.Providers.Git
{
    public class GitProvider : IProvider
    {
        private readonly ILogger<GitProvider> _logger;

        private readonly GitProviderOptions _providerOptions;
        private readonly CredentialsHandler _credentialsHandler;

        public string Name => "Git";

        public GitProvider(ILogger<GitProvider> logger, GitProviderOptions providerOptions)
        {
            _logger = logger;
            _providerOptions = providerOptions;

            if (string.IsNullOrWhiteSpace(_providerOptions.LocalPath))
            {
                throw new ArgumentNullException(nameof(_providerOptions.LocalPath), $"{nameof(_providerOptions.LocalPath)} cannot be NULL or empty.");
            }

            if (string.IsNullOrWhiteSpace(_providerOptions.RepositoryUrl))
            {
                throw new ArgumentNullException(nameof(_providerOptions.RepositoryUrl), $"{nameof(_providerOptions.RepositoryUrl)} cannot be NULL or empty.");
            }

            if (string.IsNullOrWhiteSpace(_providerOptions.Username))
            {
                throw new ArgumentNullException(nameof(_providerOptions.Username), $"{nameof(_providerOptions.Username)} cannot be NULL or empty.");
            }

            if (string.IsNullOrWhiteSpace(_providerOptions.Password))
            {
                throw new ArgumentNullException(nameof(_providerOptions.Password), $"{nameof(_providerOptions.Password)} cannot be NULL or empty.");
            }

            _credentialsHandler = (url, user, cred) => new UsernamePasswordCredentials
            {
                Username = _providerOptions.Username,
                Password = _providerOptions.Password
            };
        }

        public async Task Watch(Func<IEnumerable<string>, Task> onChange, CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    List<string> files;

                    var task = Task.Run(ListChangedFiles, cancellationToken);
                    // The git diff operation can sometimes hang.  Force to complete after a minute.
                    if (task.Wait(TimeSpan.FromSeconds(60)))
                    {
                        files = task.Result.ToList();
                    }
                    else
                    {
                        throw new Exception("Attempting to list changed files timed out after 60 seconds.");
                    }

                    if (files.Any())
                    {
                        UpdateLocal();
                        await onChange(files);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unhandled exception occurred while attempting to poll for changes");
                }

                var delayDate = DateTime.UtcNow.Add(_providerOptions.PollingInterval);

                _logger.LogInformation("Next polling period will begin in {PollingInterval:c} at {delayDate}.", _providerOptions.PollingInterval, delayDate);

                await Task.Delay(_providerOptions.PollingInterval, cancellationToken);
            }
        }

        public void Initialize()
        {
            _logger.LogInformation("Initializing {Name} provider with options {Options}.", Name, new {
                _providerOptions.RepositoryUrl,
                _providerOptions.LocalPath,
                _providerOptions.Branch,
                _providerOptions.PollingInterval,
                _providerOptions.SearchPattern
            });

            if (Directory.Exists(_providerOptions.LocalPath))
            {
                _logger.LogInformation("A local repository already exists at {LocalPath}.", _providerOptions.LocalPath);

                _logger.LogInformation("Deleting directory {LocalPath}.", _providerOptions.LocalPath);

                DeleteDirectory(_providerOptions.LocalPath);
            }

            if (!Directory.Exists(_providerOptions.LocalPath))
            {
                _logger.LogInformation("Creating directory {LocalPath}.", _providerOptions.LocalPath);

                Directory.CreateDirectory(_providerOptions.LocalPath);
            }

            var cloneOptions = new CloneOptions
            {
                CredentialsProvider = _credentialsHandler,
                BranchName = _providerOptions.Branch
            };

            _logger.LogInformation("Cloning git repository {RepositoryUrl} to {LocalPath}.", _providerOptions.RepositoryUrl, _providerOptions.LocalPath);

            var path = Repository.Clone(_providerOptions.RepositoryUrl, _providerOptions.LocalPath, cloneOptions);

            _logger.LogInformation("Repository cloned to {path}.", path);

            using (var repo = new Repository(_providerOptions.LocalPath))
            {
                var hash = repo.Head.Tip.Sha.Substring(0, 6);

                _logger.LogInformation("Current HEAD is [{hash}] '{MessageShort}'.", hash, repo.Head.Tip.MessageShort);
            }
        }

        public byte[] GetFile(string fileName)
        {
            string path = Path.Combine(_providerOptions.LocalPath, fileName);

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

            return Hasher.CreateMD5Hash(bytes);
        }

        public IEnumerable<string> ListAllFiles()
        {
            IList<string> files = new List<string>();

            using (var repo = new Repository(_providerOptions.LocalPath))
            {
                _logger.LogInformation("Listing files in repository at {LocalPath}.", _providerOptions.LocalPath);

                foreach (IndexEntry entry in repo.Index)
                {
                    if (_providerOptions.SearchPattern != null)
                    {
                        var match = WildcardMatch(entry.Path, _providerOptions.SearchPattern);

                        if (match == false)
                        {
                            _logger.LogInformation("File {Path} does not match search pattern {SearchPattern}.",
                                entry.Path, _providerOptions.SearchPattern);

                            continue;
                        }
                    }

                    files.Add(entry.Path);
                }
            }

            _logger.LogInformation("{Count} files found.", files.Count);

            return files;
        }

        private IEnumerable<string> ListChangedFiles()
        {
            Fetch();

            IList<string> files = new List<string>();

            using (var repo = new Repository(_providerOptions.LocalPath))
            {
                _logger.LogInformation("Checking for remote changes on {RemoteName}.", repo.Head.TrackedBranch.RemoteName);

                foreach (TreeEntryChanges entry in repo.Diff.Compare<TreeChanges>(repo.Head.Tip.Tree, repo.Head.TrackedBranch.Tip.Tree))
                {
                    if (entry.Exists)
                    {
                        _logger.LogInformation("File {Path} changed.", entry.Path);

                        if (_providerOptions.SearchPattern != null)
                        {
                            var match = WildcardMatch(entry.Path, _providerOptions.SearchPattern);

                            if (match == false)
                            {
                                _logger.LogInformation("File {Path} does not match search pattern {SearchPattern}.",
                                    entry.Path, _providerOptions.SearchPattern);

                                continue;
                            }
                        }

                        files.Add(entry.Path);
                    }
                    else
                    {
                        _logger.LogInformation("File {Path} no longer exists.", entry.Path);
                    }
                }
            }

            if (files.Count == 0)
            {
                _logger.LogInformation("No tree entry changes were detected.");

                return files;
            }

            _logger.LogInformation("{Count} files changed.", files.Count);

            return files;
        }

        private void UpdateLocal()
        {
            using (var repo = new Repository(_providerOptions.LocalPath))
            {
                var options = new PullOptions
                {
                    FetchOptions = new FetchOptions
                    {
                        CredentialsProvider = _credentialsHandler
                    }
                };

                var signature = new Signature(new Identity("Configuration Service", "Configuration Service"), DateTimeOffset.Now);

                _logger.LogInformation("Pulling changes to local repository.");

                var currentHash = repo.Head.Tip.Sha.Substring(0, 6);

                _logger.LogInformation("Current HEAD is [{currentHash}] '{MessageShort}'.", currentHash, repo.Head.Tip.MessageShort);

                var result = Commands.Pull(repo, signature, options);

                _logger.LogInformation("Merge completed with status {Status}.", result.Status);

                var newHash = result.Commit.Sha.Substring(0, 6);

                _logger.LogInformation("New HEAD is [{newHash}] '{MessageShort}'.", newHash, result.Commit.MessageShort);
            }
        }

        private static void DeleteDirectory(string directory)
        {
            foreach (var subdirectory in Directory.EnumerateDirectories(directory))
            {
                DeleteDirectory(subdirectory);
            }

            foreach (var fileName in Directory.EnumerateFiles(directory))
            {
                var fileInfo = new FileInfo(fileName)
                {
                    Attributes = FileAttributes.Normal
                };

                fileInfo.Delete();
            }

            Directory.Delete(directory);
        }

        private void Fetch()
        {
            using (var repo = new Repository(_providerOptions.LocalPath))
            {
                FetchOptions options = new FetchOptions
                {
                    CredentialsProvider = _credentialsHandler
                };

                foreach (var remote in repo.Network.Remotes)
                {
                    var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);

                    _logger.LogInformation("Fetching from remote {Name} at {Url}.", remote.Name, remote.Url);

                    Commands.Fetch(repo, remote.Name, refSpecs, options, string.Empty);
                }
            }
        }

        private bool WildcardMatch(string value, string searchPattern)
        {
            var expression = "^" + Regex.Escape(searchPattern).Replace("\\*", ".*") + "$";
            return Regex.IsMatch(value, expression, RegexOptions.IgnoreCase);
        }
    }
}