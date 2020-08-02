using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigurationService.Hosting
{
    public interface IConfigurationService
    {
        Task InitializeProvider(CancellationToken cancellationToken = default);

        Task OnChange(IEnumerable<string> paths);

        Task PublishChanges(IEnumerable<string> paths);
    }
}