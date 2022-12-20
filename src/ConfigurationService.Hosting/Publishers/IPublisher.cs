using System.Threading.Tasks;

namespace ConfigurationService.Hosting.Publishers;

public interface IPublisher
{
    void Initialize();

    Task Publish(string topic, string message);
}