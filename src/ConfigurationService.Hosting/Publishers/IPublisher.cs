using System.Threading.Tasks;

namespace ConfigurationService.Publishers
{
    public interface IPublisher
    {
        Task Publish(string topic, string message);
    }
}