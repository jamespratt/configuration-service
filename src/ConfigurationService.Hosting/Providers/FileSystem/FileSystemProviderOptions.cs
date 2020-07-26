namespace ConfigurationService.Hosting.Providers.FileSystem
{
    /// <summary>
    /// Options for <see cref="FileSystemProvider"/>.
    /// </summary>
    public class FileSystemProviderOptions
    {
        /// <summary>
        /// Path to the configuration files.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The search string to use as a filter against the names of files. Defaults to all files ('*').
        /// </summary>
        public string SearchPattern { get; set; }

        /// <summary>
        /// Includes the current directory and all its subdirectories.
        /// </summary>
        public bool IncludeSubdirectories { get; set; }

        /// <summary>
        /// Username for authentication.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password for authentication.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Domain for authentication.
        /// </summary>
        public string Domain { get; set; }
    }
}