using System;

namespace ConfigurationService.Providers.Git
{
    public class GitProviderOptions
    {
        public string RepositoryUrl { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Branch { get; set; }

        public string LocalPath { get; set; }

        public string SearchPattern { get; set; }

        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(60);
    }
}