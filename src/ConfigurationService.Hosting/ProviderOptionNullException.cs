using System;
using System.Runtime.Serialization;

namespace ConfigurationService.Hosting;

[Serializable]
public class ProviderOptionNullException : Exception
{
    public ProviderOptionNullException()
    {
    }

    public ProviderOptionNullException(string name)
        : base($"{name} cannot be NULL or empty.")
    {
    }

    public ProviderOptionNullException(string message, Exception inner)
        : base(message, inner)
    {
    }
    
    protected ProviderOptionNullException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}