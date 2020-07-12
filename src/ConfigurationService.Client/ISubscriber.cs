using System;

namespace ConfigurationService.Client
{
    public interface ISubscriber
    {
        string Name { get; }

        void Subscribe(string topic, Action<object> handler);
    }
}