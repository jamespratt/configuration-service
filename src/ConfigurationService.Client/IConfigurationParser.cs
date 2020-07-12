using System.Collections.Generic;
using System.IO;

namespace ConfigurationService.Client
{
    public interface IConfigurationParser
    {
        IDictionary<string, string> Parse(Stream input);
    }
}