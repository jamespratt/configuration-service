using System;

namespace ConfigurationService.Hosting.Providers.Git;

/// <summary>
/// Options for <see cref="GitProvider"/>.
/// </summary>
public class GitProviderOptions
{
    /// <summary>
    /// URI for the remote repository.
    /// </summary>
    public string RepositoryUrl { get; set; }

    /// <summary>
    /// Username for authentication.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Password for authentication.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// The name of the branch to checkout. When unspecified the remote's default branch will be used instead.
    /// </summary>
    public string Branch { get; set; }

    /// <summary>
    /// Local path to clone into.
    /// </summary>
    public string LocalPath { get; set; }

    /// <summary>
    /// The search string to use as a filter against the names of files. Defaults to all files ('*').
    /// </summary>
    public string SearchPattern { get; set; }

    /// <summary>
    /// The interval to check for for remote changes. Defaults to 60 seconds.
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(60);
}