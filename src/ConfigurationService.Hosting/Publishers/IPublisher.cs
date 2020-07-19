using System.Threading.Tasks;

namespace ConfigurationService.Hosting.Publishers
{
    public interface IPublisher
    {
        Task Publish(string topic, string message);
    }
}