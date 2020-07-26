using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigurationService.Hosting.Providers
{
    public interface IProvider
    {
        string Name { get; }

        Task Watch(Func<IEnumerable<string>, Task> onChange, CancellationToken cancellationToken = default);

        void Initialize();

        Task<byte[]> GetFile(string fileName);

        Task<string> GetHash(string fileName);

        IEnumerable<string> ListAllFiles();
    }
}