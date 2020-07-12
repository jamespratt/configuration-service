using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigurationService.Providers
{
    public interface IProvider
    {
        string Name { get; }

        Task Watch(Func<IEnumerable<string>, Task> onChange, CancellationToken cancellationToken = default);

        void Initialize();

        byte[] GetFile(string fileName);

        string GetHash(string fileName);

        IEnumerable<string> ListChangedFiles();

        IEnumerable<string> ListAllFiles();

        void Update();
    }
}