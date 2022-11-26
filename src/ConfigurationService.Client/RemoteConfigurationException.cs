using System;
using System.Runtime.Serialization;

namespace ConfigurationService.Client
{
    [Serializable]
    public class RemoteConfigurationException : Exception
    {
        public RemoteConfigurationException()
        {
        }

        public RemoteConfigurationException(string name)
            : base($"{name} cannot be NULL or empty.")
        {
        }

        public RemoteConfigurationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected RemoteConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}